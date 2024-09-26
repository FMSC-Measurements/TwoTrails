using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Utils;

namespace TwoTrails.ViewModels
{
    public class AnalyzeProjectModel : BaseModel
    {
        private const int TOTAL_TASKS = 8;

        public ICommand CloseCommand { get; }

        public bool Analyzing { get; private set; } = true;
        public string AnalyzationText { get; private set; } = "Analyzing";
        public int ProgressComplete { get; private set; }


        public bool HasMiszonnedPoints { get; private set; }
        public bool HasOrphanedQuondams { get; private set; }
        public bool HasMissingChildren { get; private set; }
        public bool HasUnsetPolygonAcc { get; private set; }

        public bool HasEmptyPolygons { get; private set; }
        public bool HasUnusedMetadata { get; private set; }
        public bool HasUnusedGroups { get; private set; }
        public bool HasDuplicateMetadata { get; private set; }



        public AnalyzeProjectModel(Window window, TtProject project)
        {
            CloseCommand = new RelayCommand((x) => window.Close());

            new Thread(() =>
            {
                HasMiszonnedPoints = DataHelper.AnalyzeMiszonedPoints(project.HistoryManager);
                TaskProgress(1);
                OnPropertyChanged(nameof(HasMiszonnedPoints), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasOrphanedQuondams = DataHelper.AnalyzeOrphanedQuondams(project.HistoryManager);
                TaskProgress(2);
                OnPropertyChanged(nameof(HasOrphanedQuondams), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasMissingChildren = DataHelper.AnalyzeChildlessPoints(project.HistoryManager);
                TaskProgress(3);
                OnPropertyChanged(nameof(HasMissingChildren), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasUnsetPolygonAcc = DataHelper.HasUnsetPolygonAccuracies(project.HistoryManager);
                TaskProgress(4);
                OnPropertyChanged(nameof(HasUnsetPolygonAcc), nameof(ProgressComplete));
                Thread.Sleep(100);


                HasEmptyPolygons = DataHelper.AnalyzeEmptyPolygons(project.HistoryManager);
                TaskProgress(5);
                OnPropertyChanged(nameof(HasEmptyPolygons), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasUnusedMetadata = DataHelper.AnalyzeUnusedMetadata(project.HistoryManager);
                TaskProgress(6);
                OnPropertyChanged(nameof(HasUnusedMetadata), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasUnusedGroups = DataHelper.AnalyzeUnusedGroups(project.HistoryManager);
                TaskProgress(7);
                OnPropertyChanged(nameof(HasUnusedGroups), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasDuplicateMetadata = DataHelper.AnalyzeDuplicateMetadata(project.HistoryManager);
                TaskProgress(8);
                OnPropertyChanged(nameof(HasDuplicateMetadata), nameof(ProgressComplete));

                Thread.Sleep(250);


                AnalyzationText = (HasMiszonnedPoints || HasOrphanedQuondams || HasEmptyPolygons || 
                    HasUnusedMetadata || HasUnusedGroups || HasDuplicateMetadata) ? "Issues Found" : "No Issues Found.";
                Analyzing = false;
                OnPropertyChanged(nameof(ProgressComplete), nameof(Analyzing), nameof(AnalyzationText));
            }).Start();
        }

        private void TaskProgress(double task)
        {
            ProgressComplete = (int)Math.Round(task / TOTAL_TASKS * 100d, 0, MidpointRounding.ToEven);
        }
    }
}
