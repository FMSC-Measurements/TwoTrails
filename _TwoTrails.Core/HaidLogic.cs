using System.IO;
using System.Text;

namespace TwoTrails.Core
{
    public static class HaidLogic
    {
        public static PolygonSummary GenerateSummary(ITtManager manager, TtPolygon polygon, bool showPoints = false)
        {
            lock (manager)
            {
                return new PolygonSummary(manager, polygon, showPoints);
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
            sb.AppendLine($"Data Version: { projectInfo.Version }");
            sb.AppendLine($"Creation Version: { projectInfo.CreationVersion }");
            sb.AppendLine("\n");
            sb.AppendLine("**** GPS Error can be divided by 2 if an appropriate ANGLE POINT METHOD is used instead of the WALK METHOD ****");
            sb.AppendLine("**** Appropriate means that the boundary legs are reasonably long between verticies where the boundary direction changes by 90 degree angles where possible and changes at least more than 30 degrees most of the time. ****");
            sb.AppendLine("**** If the unit is totally a direction distance-traverse. Use only the traverse contribution area-error. ****");
            sb.AppendLine("**** Points with asterisks are OFF boundary points. ****");
            sb.AppendLine("\n");
            return sb.ToString();
        }
    }
}
