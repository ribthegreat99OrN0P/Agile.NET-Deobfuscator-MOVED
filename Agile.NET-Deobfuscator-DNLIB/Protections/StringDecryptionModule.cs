using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile.NET_Deobfuscator_DNLIB.Protections
{
    public class StringDecryption
    {
        private static int decrypted = 0;
        public static void Execute()
        {
            try
            {
                if (Globals.IsDLL)//Check to see if the file is a dll or not this is important so it can identify the correct names/values
                {
                    byte[] byteInit = GetArrayInitDll();//Get the initialize array for the strings in the dll
                    if (byteInit != null)//if the array does not equal null/nothing
                    {
                        foreach (ModuleDef module in Globals.ASM.Modules)//Go through all the modules in the assembly
                        {
                            foreach (TypeDef type in module.GetTypes())//Go through all the types in the module
                            {
                                foreach (MethodDef method in type.Methods)//Go through all the methods in the type
                                {
                                    if (method.HasBody && method.Body.HasInstructions)//if the method has a valid body and instructions in it
                                    {
                                        for (int i = 0; i < method.Body.Instructions.Count; i++)//Go through all the instructions in the method
                                        {
                                            var Ins = method.Body.Instructions[i];
                                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr &&
                                                method.Body.Instructions[i + 1].OpCode == OpCodes.Call//Here is where we find the encoded string
                                                && method.Body.Instructions[i + 1].Operand.ToString().Contains("2XY="))//This might change in new updates.
                                            {                                                                          //We dont need the extra check for the method name but having it ensures that its accurate.
                                                string operand = method.Body.Instructions[i].Operand.ToString();//Get the encoded string
                                                string decypted = DecryptStringAgile6(operand, byteInit);//decrypt the string with the function

                                                if (decypted != null)//if the returned decrypted string does not equal to null or nothing
                                                {
                                                    method.Body.Instructions[i].Operand = decypted;//replace the encoded string with the decrypted one
                                                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;//Nop the useless call to the decryption method
                                                    decrypted++;//Add 1 to the decrypted count
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Contact for me to update in future update
                    }
                }
                else//if the file is a exe
                {
                    byte[] byteInit = GetArrayInitExe();//Get the initialize array for the strings in the exe
                    if (byteInit != null)//if the array does not equal null/nothing
                    {
                        foreach (ModuleDef module in Globals.ASM.Modules)//Go through all the modules in the assembly
                        {
                            foreach (TypeDef type in module.GetTypes())//Go through all the types and nested types in the module
                            {
                                foreach (MethodDef method in type.Methods)//Go through all the methods in the type
                                {
                                    if (method.HasBody && method.Body.HasInstructions)//Check to see if the method has a valid body with instructions
                                    {
                                        for (int i = 0; i < method.Body.Instructions.Count; i++)//Go through all the instructions in the method
                                        {
                                            var Ins = method.Body.Instructions[i];
                                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr &&
                                                method.Body.Instructions[i + 1].OpCode == OpCodes.Call//Here is where we find the encoded string
                                                && method.Body.Instructions[i + 1].Operand.ToString().Contains("oRM="))//This might change in new updates.
                                            {                                                                          //We dont need the extra check for the method name but having it ensures that its accurate.
                                                string operand = method.Body.Instructions[i].Operand.ToString();//Get the encoded string
                                                string decypted = DecryptStringAgile6(operand, byteInit);//decrypt the encrypted string with the function

                                                if (decypted != null)//if the decrpyted string not equals null or nothing
                                                {
                                                    method.Body.Instructions[i].Operand = decypted;//replace the encoded string with the decrypted one
                                                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;//Nop the useless call to the decryption method
                                                    decrypted++;//Add 1 to the decrypted count
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                    }
                }//EXE
                
            }
            catch (Exception ex)// any problems are printed
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("[-] Looks like something bad happened in the string-decryption phase : {0}", ex.Message));
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format("[+] Decrypted {0} encoded strings", decrypted));
        }
        private static byte[] GetArrayInitExe()
        {
            FieldDef initField = null;
            byte[] byteKey = null;
            foreach (ModuleDef module in Globals.ASM.Modules)//Go through all the modules in the assembly
            {
                foreach (TypeDef type in module.GetTypes())//Go through all the types in the module
                {
                    if (type.Name == "<AgileDotNetRT>")//if the type name is <AgileDotNetRT> we go into it
                    {
                        foreach (FieldDef field in type.Fields)//Go through all the fields in the type
                        {
                            if (field.Name == "pRM=")//if we find a field with this name we set it to our variable.
                            {                        // this name might change in newer updates
                                initField = field;
                            }
                        }
                    }
                }
            }
            if(initField == null)//if it wasnt found its a sign of no string encryption or it failed for other reasons
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[?] Initialize field not found for string decryption");
                return null;
            }
            //First defualt way
            byteKey = initField.InitialValue;//We grab the value using default dnlib
            if(byteKey == null)//if it is equal to null or nothing
            {
                //Reflection alternative we use reflection to get it instead!
                var info = Globals.ASMREF.ManifestModule.ResolveField(initField.MDToken.ToInt32());
                byteKey = (byte[])info.GetValue(null);
            }
            return byteKey;
        }
        private static byte[] GetArrayInitDll()
        {
            FieldDef initField = null;
            byte[] byteKey = null;
            foreach (ModuleDef module in Globals.ASM.Modules)//Go through all the modules in the assembly
            {
                foreach (TypeDef type in module.GetTypes())//Go through all the types and nested types in the module
                {
                    if (type.Name == "<AgileDotNetRT>")//if the type name is equal to <AgileDotNetRT> we go into it
                    {
                        foreach (FieldDef field in type.Fields)//Go through all the fields in the type
                        {
                            if (field.Name == "3XY=")//if the field name is equal to this value we set it to out variable
                            {                        //this value may again change in newer updates
                                initField = field;
                            }
                        }
                    }
                }
            }
            if (initField == null)//if it wasnt found its a sign of no string encryption or it failed for other reasons
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[?] Initialize field not found for string decryption");
                return null;
            }
            //First defualt way
            byteKey = initField.InitialValue;

            if (byteKey == null)
            {
                //Reflection alternative we use reflection to get it instead!
                var info = Globals.ASMREF.ManifestModule.ResolveField(initField.MDToken.ToInt32());
                byteKey = (byte[])info.GetValue(null);
            }
            return byteKey;
        }
        private static string DecryptStringAgile6(string A_0, byte[] arr)//Latest i believe of Agile.NET, let me know if it is not so i can update.
        {                                                                //Simply copied the method from the runtime of agile and simplified it as well as replaced the variables
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < A_0.Length; i++)
            {
                stringBuilder.Append(Convert.ToChar((int)(A_0[i] ^ (char)arr[i % arr.Length])));
            }
            return stringBuilder.ToString();
        }
    }
}
