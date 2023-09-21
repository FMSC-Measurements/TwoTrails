using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails.Tests
{
    public class VirtualTestBase : IClassFixture<VirtualProject>
    {
        public VirtualProject Project { get; }

        public VirtualTestBase(VirtualProject project)
        {
            Project = project;
        } 
    }
}
