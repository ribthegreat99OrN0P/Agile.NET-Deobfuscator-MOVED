using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Reflection;
using System.Reflection.Emit;

namespace Agile.NET_Deobfuscator_DNLIB.Protections
{
    public class ProxyDelegatesModule
    {

        private static bool flag = false;
        private static int removed = 0;
        public static void Execute()
        {
            foreach (ModuleDef module in Globals.ASM.Modules)//Go through all the modules in the assembly
            {
                foreach (TypeDef type in module.GetTypes())//go through all the types and nested types in the module
                {
                    foreach (MethodDef method in type.Methods)//Go trhough all the methods in the type
                    {
                        if (method.HasBody && method.Body.HasInstructions)//Check to see if the method has a valid body with instructions in it
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)//Go through all the instructions in the method
                            {
                                if (method.Body.Instructions[i].OpCode == dnlib.DotNet.Emit.OpCodes.Call &&
                               method.Body.Instructions[i].Operand.ToString().Contains("::Invoke"))
                                {

                                    var op = method.Body.Instructions[i].Operand;
                                    if (op is MethodDef mm)
                                    {
                                        MemberRef solved = gettt(mm);

                                        method.Body.Instructions[i].Operand = solved;
                                        removed++;
                                    }
                                }
                                if (method.Body.Instructions[i].OpCode == dnlib.DotNet.Emit.OpCodes.Ldsfld)
                                {
                                    var op2 = method.Body.Instructions[i].Operand;
                                    if (op2 is FieldDef ff && ff.DeclaringType.BaseType.FullName == "System.MulticastDelegate")
                                    {
                                        method.Body.Instructions[i].OpCode = dnlib.DotNet.Emit.OpCodes.Nop;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format("[+] Fixed {0} delegates", removed));
        }

        private static MemberRef gettt(MethodDef met)
        {
            TypeDef decType = met.DeclaringType;
            if (decType.BaseType.FullName == "System.MulticastDelegate")
            {
                foreach (MethodDef method in decType.Methods)
                {
                    if (method.HasBody && method.Body.HasInstructions)
                    {
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].IsLdcI4())
                            {

                                int value = method.Body.Instructions[i].GetLdcI4Value();
                                FieldDef fieldDele = getfieldofdelegate(decType);

                                string name = fieldDele.Name;
                                if (name.EndsWith("%"))
                                {
                                    flag = true;
                                    name = name.TrimEnd(new char[] { '%' });
                                }
                                uint num = BitConverter.ToUInt32(Convert.FromBase64String(name), 0);
                                ModuleDefMD MDMod = (ModuleDefMD)Globals.ASM.ManifestModule;
                                MemberRef solvedMemberRef = MDMod.ResolveMemberRef((uint)((long)num + 167772161L) - 167772160U);
                                return solvedMemberRef;
                            }
                        }
                    }
                }
            }
            return null;
        }
        private static FieldDef getfieldofdelegate(TypeDef type)
        {
            foreach (var item in type.Fields)
            {
                return item;
            }
            return null;
        }


    }
}
