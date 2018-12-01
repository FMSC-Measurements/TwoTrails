namespace TwoTrails.Core
{
    public interface IDeviceSettings
    {
        bool DeleteExistingPlots { get; }
        bool SplitToIndividualPolys { get; }
    }
}
