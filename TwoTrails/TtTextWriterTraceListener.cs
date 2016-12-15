using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails
{
    public class TtTextWriterTraceListener : TextWriterTraceListener
    {
        public TtTextWriterTraceListener(TextWriter writer) : base(writer) { }
        public TtTextWriterTraceListener(String fileName) : base(fileName) { }
        public TtTextWriterTraceListener(TextWriter writer, String name) : base(writer, name) { }
        public TtTextWriterTraceListener(String fileName, String name) : base(fileName, name) { }


        public override void Write(String message)
        {
            base.Write(String.Format("[{0}] {1}", DateTime.Now, message));
            base.Flush();
        }


        public override void WriteLine(String message)
        {
            base.WriteLine(String.Format("[{0}] {1}", DateTime.Now, message));
            base.Flush();
        }
    }
}
