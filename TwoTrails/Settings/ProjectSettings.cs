using FMSC.Core.ComponentModel;
using System.Collections.Generic;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails.Settings
{
    public class ProjectSettings : BaseModel
    {
        private readonly TtProject _Project;

        public TtPolygon LastRetraceTargetPolygon { get; set; }
        public List<Retrace> LastRetrace { get; set; }



        public ProjectSettings(TtProject project)
        {
            _Project = project;
            _Project.HistoryManager.HistoryChanged += HistoryManager_HistoryChanged;
        }

        private void HistoryManager_HistoryChanged(object sender, HistoryEventArgs e)
        {
            if (e.HistoryEventType == HistoryEventType.Commit)
            {
                if (e.CommandInfo.ActionType != DataActionType.RetracePoints)
                {
                    LastRetraceTargetPolygon = null;
                    LastRetrace = null;
                }
            }
        }
    }
}
