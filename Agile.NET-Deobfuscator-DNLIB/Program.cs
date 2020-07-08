
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using dnlib.DotNet;
using Agile.NET_Deobfuscator_DNLIB.Protections;
using dnlib.DotNet.Writer;
using System.IO;

namespace Agile.NET_Deobfuscator_DNLIB
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "Agile.NET-Deobfuscator-(DNLIB BASED) Coded by N0P";
            try
            {
                if (args.Length == 0)//if user did not drag and drop file, open a file dialogue to choose from
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Title = "Select a file";//set dialogue title
                    ofd.Filter = "Executable Files (*.exe)|*.exe|Dll Files (*.dll)|*.dll";//lets user select if file is .exe or .dll file
                    ofd.RestoreDirectory = true;//Get the recent directory in which the dialogue was last used in
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        Globals.ASM = AssemblyDef.Load(ofd.FileName);//Load file using dnlib
                        Globals.ASMREF = Assembly.UnsafeLoadFrom(ofd.FileName);//Load file using reflection
                        Globals.ASM_PATH = ofd.FileName;//load file path to use later when saving file
                        IsDLL(ofd.FileName);//Check if file is a dll/exe
                        LoadAsmRef(ofd.FileName);//load all the referenced assemblies in the selected file
                    }
                }
                else
                {
                    Globals.ASM = AssemblyDef.Load(args[0]);//Load file using dnlib
                    Globals.ASMREF = Assembly.UnsafeLoadFrom(args[0]);//Load file using reflection
                    Globals.ASM_PATH = args[0];//load file path for later when saving file
                    IsDLL(args[0]);//Check if file is a dll/exe
                    LoadAsmRef(args[0]);//load all the referenced assemblies in the selected file
                }
                Begin();//Start the process
            }
            catch (Exception ex)//If there are any problems loading the file print the problem
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);

            }
            Console.ReadKey();//Keeps the file from not instantly closing after everything is finished.(Closes once user enters key)
        }
        static void LoadAsmRef(string filename)
        {
            var asmResolver = new AssemblyResolver();
            var modCtx = new ModuleContext(asmResolver);
            asmResolver.DefaultModuleContext = modCtx;
            asmResolver.EnableTypeDefCache = true;

            Globals.ASM.ManifestModule.Location = filename;
            var asmRefs = Globals.ASM.ManifestModule.GetAssemblyRefs().ToList();
            Globals.ASM.ManifestModule.Context = modCtx;
            foreach (var asmRef in asmRefs)
            {
                if (asmRef == null)
                    continue;
                var asma = asmResolver.Resolve(asmRef.FullName, Globals.ASM.ManifestModule);
                ((AssemblyResolver)Globals.ASM.ManifestModule.Context.AssemblyResolver).AddToCache(asma);
            }
        }
        static void IsDLL(string file)
        {
            //Simple filename check, you can implement advanced checking with dnlib
            if (file.Contains(".dll"))
                Globals.IsDLL = true;
            else
                Globals.IsDLL = false;
        }
        static void Begin()
        {
            //NOTE: For some odd reason, the proxy-delegate fixer fails on some methods like the constructor/entrypoint i yet have not fixed this but will try fix soon. But it works for every other method
            ProxyDelegatesModule.Execute();//Execute the process of cleaning delegates
            ControlFlowModule.Execute();//Execute the process of cleaning controlflow
            StringDecryption.Execute();//Execute the process of decrypting strings
            AntiTamperModule.Execute();//Execute the process of removing anti-tamper attributes

            ModuleWriterOptions options = new ModuleWriterOptions(Globals.ASM.ManifestModule);
            options.MetadataOptions.Flags |= MetadataFlags.PreserveAll;//Preserve/keep all the tokens
            options.MetadataLogger = DummyLogger.NoThrowInstance; //Prevents any errors coming.
            options.MetadataOptions.PreserveHeapOrder(Globals.ASM.ManifestModule, true);//Will help if file is protected with KoiVM + Agile.NET which keeps the #Koi Stream.
            
            if (Globals.IsDLL)//Checks to see if file is dll
                Globals.ASM.Write(Path.GetFileNameWithoutExtension(Globals.ASM_PATH) + "-Deobfuscated.dll", options);//save file as dll
            else
                Globals.ASM.Write(Path.GetFileNameWithoutExtension(Globals.ASM_PATH) + "-Deobfuscated.exe", options);//save file as exe
                
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[+] Finished.");
        }
    }
}
