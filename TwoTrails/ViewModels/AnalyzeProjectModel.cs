using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Utils;

namespace TwoTrails.ViewModels
{
    public class AnalyzeProjectModel : BaseModel
    {
        public ICommand CloseCommand { get; }

        public bool Analyzing { get; private set; } = true;
        public string AnalyzationText { get; private set; } = "Analyzing";
        public int ProgressComplete { get; private set; }

        public bool HasMiszonnedPoints { get; private set; }
        public bool HasOrphanedQuondams { get; private set; }
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
                ProgressComplete = 30;
                OnPropertyChanged(nameof(HasMiszonnedPoints), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasOrphanedQuondams = DataHelper.AnalyzeOrphanedQuondams(project.HistoryManager);
                ProgressComplete = 52;
                OnPropertyChanged(nameof(HasOrphanedQuondams), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasEmptyPolygons = DataHelper.AnalyzeEmptyPolygons(project.HistoryManager);
                ProgressComplete = 64;
                OnPropertyChanged(nameof(HasEmptyPolygons), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasUnusedMetadata = DataHelper.AnalyzeUnusedMetadata(project.HistoryManager);
                ProgressComplete = 76;
                OnPropertyChanged(nameof(HasUnusedMetadata), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasUnusedGroups = DataHelper.AnalyzeUnusedGroups(project.HistoryManager);
                ProgressComplete = 88;
                OnPropertyChanged(nameof(HasUnusedGroups), nameof(ProgressComplete));
                Thread.Sleep(100);

                HasDuplicateMetadata = DataHelper.AnalyzeDuplicateMetadata(project.HistoryManager);
                ProgressComplete = 100;
                OnPropertyChanged(nameof(HasDuplicateMetadata), nameof(ProgressComplete));

                Thread.Sleep(250);

                AnalyzationText = (HasMiszonnedPoints || HasOrphanedQuondams || HasEmptyPolygons || 
                    HasUnusedMetadata || HasUnusedGroups || HasDuplicateMetadata) ? "Issues Found" : "No Issues Found.";
                Analyzing = false;
                OnPropertyChanged(nameof(ProgressComplete), nameof(Analyzing), nameof(AnalyzationText));
            }).Start();
        }
    }
}
