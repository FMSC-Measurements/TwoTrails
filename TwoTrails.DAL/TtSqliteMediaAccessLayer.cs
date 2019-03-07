using CSUtil.Databases;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
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
            database.ExecuteNonQuery(TwoTrailsMediaSchema.Images.CreateTable);
            database.ExecuteNonQuery(TwoTrailsMediaSchema.Data.CreateTable);
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

        public IEnumerable<TtImage> GetImages(string pointCN = null)
        {
            if (_Database != null)
            {
                String query = String.Format(@"select {1}.{0}, {2} from {1} left join {3} on {1}.{4} = {3}.{4}{5}",
                    TwoTrailsMediaSchema.Media.SelectItems,
                    TwoTrailsMediaSchema.Media.TableName,
                    TwoTrailsMediaSchema.Images.SelectItemsNoCN,
                    TwoTrailsMediaSchema.Images.TableName,
                    TwoTrailsSchema.SharedSchema.CN,
                    pointCN != null ? String.Format (" where {0} = '{1}'",
                        TwoTrailsMediaSchema.Media.PointCN, pointCN) : String.Empty
                );

                using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                {
                    using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                    {
                        if (dr != null)
                        {
                            string cn, pcn, name, file, cmt;
                            bool isExt;
                            MediaType mt;
                            ImageType pt;
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
                                isExt = dr.GetBoolean(7);

                                pt = (ImageType)dr.GetInt32(8);
                                az = dr.GetFloatN(9);
                                pitch = dr.GetFloatN(10);
                                roll = dr.GetFloatN(11);

                                switch (pt)
                                {
                                    case ImageType.Regular:
                                        image = new TtImage(cn, name, file, cmt, date, pcn, isExt, az, pitch, roll);
                                        break;
                                    case ImageType.Panorama:
                                        image = new TtPanorama(cn, name, file, cmt, date, pcn, isExt, az, pitch, roll);
                                        break;
                                    case ImageType.PhotoSphere:
                                        image = new TtPhotoShpere(cn, name, file, cmt, date, pcn, isExt, az, pitch, roll);
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
                                    _Database.Insert(TwoTrailsMediaSchema.Images.TableName, GetImageValues(media as TtImage), conn, trans);
                                    break;
                                case MediaType.Video:
                                    break;
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "MAL:InsertMedia");
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
                                _Database.Update(TwoTrailsMediaSchema.Images.TableName,
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
                        Debug.WriteLine(ex.Message, "MAL:UpdateMedia");
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
                [TwoTrailsMediaSchema.Media.PointCN] = media.PointCN,
                [TwoTrailsMediaSchema.Media.IsExternal] = media.IsExternal
            };
        }

        private Dictionary<string, object> GetImageValues(TtImage image)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = image.CN,
                [TwoTrailsMediaSchema.Images.PicType] = (int)image.PictureType,
                [TwoTrailsMediaSchema.Images.Azimuth] = image.Azimuth,
                [TwoTrailsMediaSchema.Images.Pitch] = image.Pitch,
                [TwoTrailsMediaSchema.Images.Roll] = image.Roll
            };
        }

        private Dictionary<string, object> GetDataValues(string cn, string type, byte[] data)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = cn,
                [TwoTrailsMediaSchema.Data.DataType] = type,
                [TwoTrailsMediaSchema.Data.BinaryData] = data
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
                                _Database.Delete(TwoTrailsMediaSchema.Images.TableName,
                                    $"{TwoTrailsSchema.SharedSchema.CN} = '{media.CN}'",
                                    conn,
                                    trans);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message, "MAL:DeleteMedia");
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


        public void InsertImageData(TtImage image, Stream bitmapDataStream)
        {
            byte[] bdata;

            using (MemoryStream ms = new MemoryStream())
            {
                const int CHUNK_SIZE = 2 * 1024;
                byte[] buffer = new byte[CHUNK_SIZE];
                long bytesRead;

                while ((bytesRead = bitmapDataStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, (int)bytesRead);
                }

                bdata = ms.ToArray();
            }

            if (image.IsExternal)
            {
                using (FileStream stream = File.Create(image.FilePath))
                {
                    stream.Write(bdata, 0, bdata.Length);
                }
            }
            else
            {
                if (_Database != null)
                {
                    using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                    {
                        using (SQLiteTransaction trans = conn.BeginTransaction())
                        {
                            try
                            {
                                _Database.Insert(TwoTrailsMediaSchema.Data.TableName,
                                    GetDataValues(image.CN, Path.GetExtension(image.FilePath).Trim('.'), bdata),
                                    conn,
                                    trans);

                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message, "MAL:InsertImageData");
                                trans.Rollback();
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                    }
                }
            }
        }


        //public void InsertImageData(TtImage image, BitmapImage data)
        //{
        //    byte[] bdata;

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        const int CHUNK_SIZE = 2 * 1024;
        //        byte[] buffer = new byte[CHUNK_SIZE];
        //        long bytesRead;

        //        while ((bytesRead = data.StreamSource.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            ms.Write(buffer, 0, (int)bytesRead);
        //        }

        //        bdata = ms.ToArray();
        //    }

        //    if (image.IsExternal)
        //    {
        //        using (FileStream stream = File.Create(image.FilePath))
        //        {
        //            stream.Write(bdata, 0, bdata.Length);
        //        }
        //    }
        //    else
        //    {
        //        if (_Database != null)
        //        {
        //            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
        //            {
        //                using (SQLiteTransaction trans = conn.BeginTransaction())
        //                {
        //                    try
        //                    {
        //                        _Database.Insert(TwoTrailsMediaSchema.Data.TableName,
        //                            GetDataValues(image.CN, Path.GetExtension(image.FilePath).Trim('.'), bdata),
        //                            conn,
        //                            trans);

        //                        trans.Commit();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Debug.WriteLine(ex.Message, "MAL:InsertImageData");
        //                        trans.Rollback();
        //                    }
        //                    finally
        //                    {
        //                        conn.Close();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public BitmapImage GetImageData(TtImage image)
        //{
        //    BitmapImage bitmap = new BitmapImage();

        //    byte[] data = GetRawImageData(image);

        //    if (data != null)
        //    {
        //        bitmap.BeginInit();
        //        bitmap.StreamSource = new MemoryStream(data);
        //        bitmap.EndInit();
        //        bitmap.Freeze();
        //    }
            
        //    return bitmap;
        //}


        public byte[] GetRawImageData(TtImage image)
        {
            if (_Database != null)
            {
                String query = String.Format(@"select {0} from {1} where {2} = '{3}' limit 1",
                    TwoTrailsMediaSchema.Data.BinaryData,
                    TwoTrailsMediaSchema.Data.TableName,
                    TwoTrailsMediaSchema.SharedSchema.CN,
                    image.CN
                );

                using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                {
                    using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                    {
                        if (dr != null)
                        {
                            if (dr.Read())
                            {
                                return dr.GetBytesEx(0);
                            }
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return null;
        }

        #region Utils
        public void Clean()
        {
            
        }
        #endregion
    }
}
