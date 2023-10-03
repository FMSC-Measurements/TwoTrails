using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CommandInfo
    {
        public String Description { get; }
        public Type AffectedType { get; }
        public int AffectedItems { get; }

        public CommandInfo(Type affectedType,  string description, int adffectedItems = 1)
        {
            AffectedType = affectedType;

            Description = description ?? "Unknown Command";
            AffectedItems = adffectedItems;
        }
    }
}
