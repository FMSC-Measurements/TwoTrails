using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CommandInfo
    {
        public String Description { get; }
        public DataActionType ActionType { get; }
        public int AffectedItems { get; }

        public CommandInfo(DataActionType actionType,  string description, int adffectedItems = 1)
        {
            ActionType = actionType;

            Description = description ?? "Unknown Command";
            AffectedItems = adffectedItems;
        }
    }
}
