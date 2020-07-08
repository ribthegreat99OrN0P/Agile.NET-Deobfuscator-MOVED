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
                                //Here we find the(Ldsfld)opcode which has the delegate field operand and we set it to out local variable
                                if (method.Body.Instructions[i].OpCode == dnlib.DotNet.Emit.OpCodes.Ldsfld)
                                {
                                    object operand = method.Body.Instructions[i].Operand;
                                    
                                    if(operand is FieldDef field)
                                    {
                                        testfield = field;
                                        method.Body.Instructions[i].OpCode = dnlib.DotNet.Emit.OpCodes.Nop;//Replace it with Nop because we dont need it anymore
                                    }
                                }
                                //here we find then the Invoke method which executes the process of the delegate and we replace the operand with the actual method
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

        private static MemberRef ResolveDelegate(FieldDef field)//Most of this process was copied from the method from Agile
        {
            MemberRef result = null;
            TypeDef deleType = field.DeclaringType;//Get the type of the field
            if (deleType.BaseType.FullName == "System.MulticastDelegate")//Check to see if the type is a delegate
            {
                foreach (MethodDef meth in deleType.Methods)//Go through all the methods in the type
                {
                    if (meth.HasBody && meth.Body.HasInstructions)//Check to see if the method has a valid body with instructions
                    {
                        for (int o = 0; o < meth.Body.Instructions.Count; o++)//Go through all the instructions in the method
                        {
                            if (meth.Body.Instructions[o].OpCode == dnlib.DotNet.Emit.OpCodes.Ldc_I4)//find the int to use when solving the method
                            {
                                int valuetest = meth.Body.Instructions[o].GetLdcI4Value();//Get the value
                                string name = field.Name;//Get the field name
                                bool flag = false;
                                if (name.EndsWith("%"))//This method is originally from Agile
                                {
                                    flag = true;
                                    name = name.TrimEnd(new char[]
                                    {
                                       '%'
                                    });
                                }
                                uint num = BitConverter.ToUInt32(Convert.FromBase64String(name), 0);
                                ModuleDefMD MDMod = (ModuleDefMD)Globals.ASM.ManifestModule;
                                MemberRef solvedMemberRef = MDMod.ResolveMemberRef((uint)((long)num + 167772161L) - 167772160U);//Resolve the method!
                                result = solvedMemberRef;//set the local variable with the resolved memberef
                            }
                        }
                    }
                }
            }
            return result;//return the result
        }


    }
}
