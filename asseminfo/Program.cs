using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace AssemInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication
                      {
                          Name = "asseminfo",
                          FullName = "Assembly Info Lister",
                      };
            app.HelpOption("-?|-h|--help");
            var directory = app.Argument("[directory]", "directory path to search(\".\" for current directory)");
            var pause = app.Option("-p|--pause", "pause after run", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                if (string.IsNullOrEmpty(directory.Value))
                {
                    app.ShowHelp();
                    return 0;
                }
                var result = Run(directory.Value);
                if (pause.HasValue())
                {
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadLine();
                }
                return result;
            });
            
            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException cpe)
            {
                Console.WriteLine("Error : {0}", cpe.Message);
                //throw;
            }
        }

        private static int Run(string directory)
        {
            try
            {
                var directoryFullPath = Path.GetFullPath(directory);
                if (!Directory.Exists(directoryFullPath))
                {
                    Console.Error.WriteLine("No such directory : {0}", directoryFullPath);
                    return -1;
                }
                var assemblyInfoLoader = new ReflectionOnlyAssemblyInfoLoader();
                Console.WriteLine("Loading assembly in directory : {0}", directoryFullPath);
                assemblyInfoLoader.AddAssemblyInDirectory(directoryFullPath);
                var infos = assemblyInfoLoader.Load();
                foreach (var info in infos)
                {
                    var additionalInfo = info.MinimumServerVersion != null ?
                        string.Format("MinimunServerVersion={0}", info.MinimumServerVersion) : string.Empty;
                    Console.WriteLine("{0} : Version={1} {2}",
                        Path.GetFileName(info.Path), info.Version, additionalInfo);
                }
                return 0;
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine(e.Message);
                return -1;
            }
        }
    }
}
