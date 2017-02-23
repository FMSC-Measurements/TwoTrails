using CSUtil;
using CSUtil.Databases;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using System.Diagnostics;

namespace TwoTrails.DAL
{
    public class TtSqliteMediaAccessLayer : ITtMediaLayer
    {
        public String FilePath { get; }
        
        private SQLiteDatabase _Database;


        public TtSqliteMediaAccessLayer(String filePath)
        {
            FilePath = filePath;
            _Database = new SQLiteDatabase(FilePath);
        }

        private TtSqliteMediaAccessLayer(SQLiteDatabase database)
        {
            this.FilePath = database.FileName;
            this._Database = database;
        }

        public static TtSqliteMediaAccessLayer Create(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            SQLiteDatabase database = new SQLiteDatabase(filePath);

            database.ExecuteNonQuery(TwoTrailsMediaSchema.Media.CreateTable);
            database.ExecuteNonQuery(TwoTrailsMediaSchema.Pictures.CreateTable);
            database.ExecuteNonQuery(TwoTrailsMediaSchema.Info.CreateTable);

            database.Insert(TwoTrailsMediaSchema.Info.TableName,
                new Dictionary<string, string>()
                {
                    [TwoTrailsMediaSchema.Info.TtMediaDbSchemaVersion] = TwoTrailsMediaSchema.SchemaVersion.ToString()
                });

            return new TtSqliteMediaAccessLayer(database);
        }

        public bool RequiresUpgrade
        {
            get { return GetDataVersion() < TwoTrailsMediaSchema.SchemaVersion; }
        }



        public Version GetDataVersion()
        {
            String query = $"select {TwoTrailsMediaSchema.Info.TtMediaDbSchemaVersion} from {TwoTrailsMediaSchema.Info.TableName} limit 1";

            Version version = null;

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        if (dr.Read())
                        {
                            version = new Version(dr.GetString(0));
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            return version;
        }

        public IEnumerable<TtImage> GetPictures(string pointCN)
        {
            if (_Database != null)
            {
                String query = String.Format(@"select {1}.{0}, {2} from {1} left join {3} on {1}.{4} = {3}.{4}",
                    TwoTrailsMediaSchema.Media.SelectItems,
                    TwoTrailsMediaSchema.Media.TableName,
                    TwoTrailsMediaSchema.Pictures.SelectItemsNoCN,
                    TwoTrailsMediaSchema.Pictures.TableName,
                    TwoTrailsSchema.SharedSchema.CN
                );

                using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                {
                    using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                    {
                        if (dr != null)
                        {
                            string cn, pcn, name, file, cmt;
                            MediaType mt;
                            PictureType pt;
                            DateTime date;
                            float? az, pitch, roll;

                            TtImage image = null;

                            while (dr.Read())
                            {
                                cn = dr.GetString(0);
                                pcn = dr.GetString(1);
                                mt = (MediaType)dr.GetInt32(2);
                                name = dr.GetString(3);
                                file = dr.GetStringN(4);
                                date = TtCoreUtils.ParseTime(dr.GetString(5));
                                cmt = dr.GetStringN(6);

                                pt = (PictureType)dr.GetInt32(7);
                                az = dr.GetFloatN(8);
                                pitch = dr.GetFloatN(9);
                                roll = dr.GetFloatN(10);

                                switch (pt)
                                {
                                    case PictureType.Regular:
                                        image = new TtImage(cn, name, file, cmt, date, pcn, az, pitch, roll);
                                        break;
                                    case PictureType.Panorama:
                                        image = new TtPanorama(cn, name, file, cmt, date, pcn, az, pitch, roll);
                                        break;
                                    case PictureType.PhotoSphere:
                                        image = new TtPhotoShpere(cn, name, file, cmt, date, pcn, az, pitch, roll);
                                        break;
                                    default:
                                        throw new Exception("Unknown Image Type");
                                }

                                yield return image;
                            }

                            dr.Close();
                        }
                    }

                    conn.Close();
                }
            }
        }

        public bool InsertMedia(TtMedia media)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        if (_Database.Insert(TwoTrailsMediaSchema.Media.TableName, GetMediaValues(media), conn, trans) > 0)
                        {
                            switch (media.MediaType)
                            {
                                case MediaType.Picture:
                                    _Database.Insert(TwoTrailsMediaSchema.Pictures.TableName, GetImageValues(media as TtImage), conn, trans);
                                    break;
                                case MediaType.Video:
                                    break;
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertMedia");
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return true;
        }

        public bool UpdateMedia(TtMedia media)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Update(TwoTrailsMediaSchema.Media.TableName,
                            GetMediaValues(media),
                            $"{TwoTrailsSchema.SharedSchema.CN} = '{media.CN}'",
                            conn,
                            trans);

                        switch (media.MediaType)
                        {
                            case MediaType.Picture:
                                _Database.Update(TwoTrailsMediaSchema.Pictures.TableName,
                                    GetImageValues(media as TtImage),
                                    $"{TwoTrailsSchema.SharedSchema.CN} = '{media.CN}'",
                                    conn,
                                    trans);
                                break;
                            case MediaType.Video:
                                break;
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateMedia");
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return true;
        }


        private Dictionary<string, object> GetMediaValues(TtMedia media)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = media.CN,
                [TwoTrailsMediaSchema.Media.Name] = media.Name,
                [TwoTrailsMediaSchema.Media.FilePath] = media.FilePath,
                [TwoTrailsMediaSchema.Media.Comment] = media.Comment,
                [TwoTrailsMediaSchema.Media.CreationTime] = media.TimeCreated.ToString(Consts.DATE_FORMAT),
                [TwoTrailsMediaSchema.Media.MediaType] = (int)media.MediaType,
                [TwoTrailsMediaSchema.Media.PointCN] = media.PointCN
            };
        }

        private Dictionary<string, object> GetImageValues(TtImage image)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = image.CN,
                [TwoTrailsMediaSchema.Pictures.PicType] = (int)image.PictureType,
                [TwoTrailsMediaSchema.Pictures.Azimuth] = image.Azimuth,
                [TwoTrailsMediaSchema.Pictures.Pitch] = image.Pitch,
                [TwoTrailsMediaSchema.Pictures.Roll] = image.Roll
            };
        }

        public bool DeleteMedia(TtMedia media)
        {
            if (_Database != null)
            {
                using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                {
                    using (SQLiteTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            _Database.Delete(TwoTrailsMediaSchema.Media.TableName,
                                $"{TwoTrailsSchema.SharedSchema.CN} = '{media.CN}'",
                                conn,
                                trans);

                            if (media.MediaType == MediaType.Picture)
                            {
                                _Database.Delete(TwoTrailsMediaSchema.Pictures.TableName,
                                    $"{TwoTrailsSchema.SharedSchema.CN} = '{media.CN}'",
                                    conn,
                                    trans);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message, "DAL:DeleteMedia");
                            trans.Rollback();
                            return false;
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                } 
            }

            return true;
        }


        #region Utils
        public void Clean()
        {
            
        }
        #endregion
    }
}
