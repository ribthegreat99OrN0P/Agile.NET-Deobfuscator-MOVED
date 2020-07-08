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

        private static FieldDef testfield = null;
        private static int removed = 0;
        public static void Execute()
        {
            foreach (ModuleDef module in Globals.ASM.Modules)
            {
                foreach (TypeDef type in module.GetTypes())
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody && method.Body.HasInstructions)
                        {
                            for (int i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                
                                if (method.Body.Instructions[i].OpCode == dnlib.DotNet.Emit.OpCodes.Ldsfld)
                                {
                                    object operand = method.Body.Instructions[i].Operand;
                                    
                                    if(operand is FieldDef field)
                                    {
                                        testfield = field;
                                        method.Body.Instructions[i].OpCode = dnlib.DotNet.Emit.OpCodes.Nop;
                                    }
                                }
                                else if(method.Body.Instructions[i].OpCode == dnlib.DotNet.Emit.OpCodes.Call &&
                                    method.Body.Instructions[i].Operand.ToString().Contains("::Invoke"))
                                {
                                    method.Body.Instructions[i].Operand = ResolveDelegate(testfield);
                                    removed++;
                                }
                                
                                /*
                                if (method.Body.Instructions[i].OpCode == dnlib.DotNet.Emit.OpCodes.Ldsfld)
                                {
                                    object operand = method.Body.Instructions[i].Operand;

                                    if (operand is FieldDef field)
                                    {
                                        testfield = field;

                                        method.Body.Instructions[i].OpCode = dnlib.DotNet.Emit.OpCodes.Nop;
                                    }
                                    for (int p = i; p < method.Body.Instructions.Count; p++)
                                    {
                                        if(method.Body.Instructions[p].OpCode == dnlib.DotNet.Emit.OpCodes.Call
                                            && method.Body.Instructions[p].Operand.ToString().Contains("::Invoke"))
                                        {
                                            method.Body.Instructions[p].Operand = ResolveDelegate(testfield);
                                            removed++;
                                        }
                                    }
                                }
                                */
                            }
                        }
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format("[+] Fixed {0} delegates", removed));
        }

        private static MemberRef ResolveDelegate(FieldDef field)
        {
            MemberRef result = null;
            TypeDef deleType = field.DeclaringType;
            if (deleType.BaseType.FullName == "System.MulticastDelegate")
            {
                foreach (MethodDef meth in deleType.Methods)
                {
                    if (meth.HasBody && meth.Body.HasInstructions)
                    {
                        for (int o = 0; o < meth.Body.Instructions.Count; o++)
                        {
                            if (meth.Body.Instructions[o].OpCode == dnlib.DotNet.Emit.OpCodes.Ldc_I4)
                            {
                                int valuetest = meth.Body.Instructions[o].GetLdcI4Value();
                                string name = field.Name;
                                bool flag = false;
                                if (name.EndsWith("%"))
                                {
                                    flag = true;
                                    name = name.TrimEnd(new char[]
                                    {
                                       '%'
                                    });
                                }
                                uint num = BitConverter.ToUInt32(Convert.FromBase64String(name), 0);
                                ModuleDefMD MDMod = (ModuleDefMD)Globals.ASM.ManifestModule;
                                MemberRef solvedMemberRef = MDMod.ResolveMemberRef((uint)((long)num + 167772161L) - 167772160U);
                                result = solvedMemberRef;
                            }
                        }
                    }
                }
            }
            return result;
        }


    }
}
