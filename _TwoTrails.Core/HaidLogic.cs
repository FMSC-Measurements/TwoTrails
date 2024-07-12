using System.IO;
using System.Text;

namespace TwoTrails.Core
{
    public static class HaidLogic
    {
        public static PolygonSummary GenerateSummary(ITtManager manager, TtPolygon polygon, bool showPoints = false, bool advancedProcessing = false)
        {
            lock (manager)
            {
                return new PolygonSummary(manager, polygon, true, showPoints, advancedProcessing);
            }
        }

        public static string GenerateSummaryHeader(TtProjectInfo projectInfo, string projectFilePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Project File: { Path.GetFileName(projectFilePath) }");
            sb.AppendLine($"Project Name: { projectInfo.Name }");
            sb.AppendLine($"Region: { projectInfo.Region }");
            sb.AppendLine($"Forest: { projectInfo.Forest }");
            sb.AppendLine($"District: { projectInfo.District }");
            sb.AppendLine($"Description: { projectInfo.Description }");
            sb.AppendLine($"Created On: { projectInfo.CreationDate }");
            sb.AppendLine($"Version: { projectInfo.Version }");
            sb.AppendLine($"Data Version: { projectInfo.DbVersion }");
            sb.AppendLine($"Creation Version: { projectInfo.CreationVersion }");
            sb.AppendLine("\n");
            sb.AppendLine(
                "**** GNSS survey area-error is best lowered by applying the correct\n" +
                "NTDP lesser-canopy-error along roads, trails, and such.\n" + 
                "ALWAYS DO THE POINT ACCURACY ASSIGNMENT FIRST. ****\n\n" +

                "**** AREA-ERROR CANNOT BE LESSENED OR DIVIDED BY TWO UNLESS STRICT CRITERIA IS APPLIED. ****\n" +
                "(See https://github.com/FMSC-Measurements/TwoTrails/wiki/Meeting-Area-Error)\n\n" +

                "**** Points with asterisks are OFF boundary points. ****");
            sb.AppendLine("\n");
            return sb.ToString();
        }
    }
}
