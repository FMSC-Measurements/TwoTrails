using FMSC.Core;

namespace TwoTrails.Core
{
    public interface IDeviceSettings
    {
        bool DeleteExistingPlots { get; }
        bool SplitToIndividualPolys { get; }
        bool DeletePointWarning { get; }
        Distance LogDeckDistance { get; }
        Volume LogDeckVolume { get; }
        double LogDeckCollarWidth { get; }
        double LogDeckLength { get; }
        double LogDeckDefect { get; }
        double LogDeckVoid { get; }
    }
}
