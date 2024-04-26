using System;
using System.Diagnostics;
using System.IO;
using TwoTrails.Utils;

namespace TwoTrails
{
    public static class SessionData
    {
        public static string GpsAccuracyReportFile { get; } = Path.Combine(App.TwoTrailsAppDataDir, "GpsTests.xml");
        
        public static GpsAccuracyReport GpsAccuracyReport { get; private set; }

        public static String MakeID { get; set; }
        public static String ModelID { get; set; }

        public static DateTime? LastWebCheck { get; private set; }
        

        public static GpsReportStatus HasGpsAccReport()
        {
            if (GpsAccuracyReport == null)
            {
                if (File.Exists(GpsAccuracyReportFile))
                {
                    if (File.GetCreationTime(GpsAccuracyReportFile) < DateTime.Now.Subtract(TimeSpan.FromDays(1)))
                    {
                        return CheckFromWeb();
                    }
                    else
                    {
                        if (!LoadGpsAccuracyReportFromFile() && !DownloadGpsAccuracyReportFile())
                            return GpsReportStatus.CantGetReport;
                    }
                }
                else
                {
                    return CheckFromWeb();
                } 
            }

            return GpsReportStatus.HasReport;
        }

        private static GpsReportStatus CheckFromWeb()
        {
            if (LastWebCheck == null || LastWebCheck < DateTime.Now.Subtract(TimeSpan.FromMinutes(15)))
            {
                if (!DownloadGpsAccuracyReportFile())
                {
                    LastWebCheck = DateTime.Now;

                    if (LoadGpsAccuracyReportFromFile())
                        return GpsReportStatus.HasOldReport;
                    else
                        return GpsReportStatus.CantGetReport;
                }
                else
                {
                    return GpsReportStatus.HasReport;
                }
            }
            else
            {
                return GpsReportStatus.CantGetReport;
            }
        }

        private static bool LoadGpsAccuracyReportFromFile()
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

        private static bool DownloadGpsAccuracyReportFile()
        {
            try
            {
                GpsAccuracyReport.DownloadGpsTests(GpsAccuracyReportFile);
                return LoadGpsAccuracyReportFromFile();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "SessionData:DownloadGpsAccuracyReportFile");
                return false;
            }
        }
    }

    public enum GpsReportStatus
    {
        HasReport,
        HasOldReport,
        CantGetReport
    }
}
