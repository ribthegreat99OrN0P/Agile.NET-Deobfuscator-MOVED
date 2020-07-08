using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile.NET_Deobfuscator_DNLIB.Protections
{
    public class AntiTamperModule
    {
        private static int removedInlineAttributes = 0;
        public static void Execute()
        {
            try
            {
                foreach(ModuleDef module in Globals.ASM.Modules)//Go through all the modules in the assembly
                {
                    foreach(TypeDef type in module.GetTypes())//Go through all the types and nested tyoes in the module
                    {
                        foreach(MethodDef method in type.Methods)// Go through all the methods in the type
                        {
                            if(method.ImplAttributes == MethodImplAttributes.NoInlining)//Check if the method has a Immpl attribute of [NoVMLining]
                            {
                                method.ImplAttributes = MethodImplAttributes.IL;//Removes the impl attribute and replaces it with normal IL
                                removedInlineAttributes++;// Add 1 to the count
                            }
                        }
                    }
                }
            }
            catch(Exception ex)//if somethinf goes wrong print the problem
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("[-] Looks like something bad happened in the anti-tamper phase: {0}", ex.Message));
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format("[+] Removed {0} anti-tamper attributes", removedInlineAttributes));
        }
    }
}
