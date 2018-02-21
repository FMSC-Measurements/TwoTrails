using FMSC.GeoSpatial.MTDC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails
{
    public static class SessionData
    {
        public static string GpsAccuracyReportFile { get; } = Path.Combine(App.TwoTrailsAppDataDir, "GpsTests.xml");
        
        public static GpsAccuracyReport GpsAccuracyReport { get; private set; }

        public static int MakeID { get; set; } = 0;
        public static int ModelID { get; set; } = 0;
        

        public static GpsReportStatus HasGpsAccReport()
        {
            if (GpsAccuracyReport == null)
            {
                if (File.Exists(GpsAccuracyReportFile))
                {
                    if (File.GetCreationTime(GpsAccuracyReportFile) < DateTime.Now.Subtract(TimeSpan.FromDays(1)))
                    {
                        if (!DownloadFile())
                        {
                            if (LoadFromFile())
                                return GpsReportStatus.HasOldReport;
                            else
                                return GpsReportStatus.CantGetReport;
                        }
                    }
                    else
                    {
                        if (!LoadFromFile() && !DownloadFile())
                            return GpsReportStatus.CantGetReport;
                    }
                }
                else
                {
                    if (!DownloadFile())
                        return GpsReportStatus.CantGetReport;
                } 
            }

            return GpsReportStatus.HasReport;
        }

        private static bool LoadFromFile()
        {
            try
            {
                GpsAccuracyReport = GpsAccuracyReport.Load(GpsAccuracyReportFile);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static bool DownloadFile()
        {
            try
            {
                GpsAccuracyReport.DownloadGpsTests(GpsAccuracyReportFile);
                LoadFromFile();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "SessionData:HasReport");
                return false;
            }

            return true;
        }
    }

    public enum GpsReportStatus
    {
        HasReport,
        HasOldReport,
        CantGetReport
    }
}
