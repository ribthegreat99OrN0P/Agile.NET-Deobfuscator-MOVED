
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Agile.NET_Deobfuscator_DNLIB
{
    public class Globals
    {
        public static AssemblyDef ASM { get; set; }
        public static Assembly ASMREF { get; set; }
        public static string ASM_PATH { get; set; }
        public static bool IsDLL { get; set; }

    }
}
