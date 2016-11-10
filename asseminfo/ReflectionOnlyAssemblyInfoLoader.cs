using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssemInfo
{
    public class ReflectionOnlyAssemblyInfoLoader
    {
        private readonly string _currentDirectory;

        public ReflectionOnlyAssemblyInfoLoader()
        {
            _currentDirectory = Environment.CurrentDirectory;

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) =>
            {
                var name = new AssemblyName(args.Name);
                var asmToCheck = Path.GetDirectoryName(_currentDirectory) + "\\" + name.Name + ".dll";
                if (File.Exists(asmToCheck))
                {
                    return Assembly.ReflectionOnlyLoadFrom(asmToCheck);
                }
                return Assembly.ReflectionOnlyLoad(args.Name);
            };
        }

        public void AddAssemblyInCurrentDirectory()
        {
            AddAssemblyInDirectory(Environment.CurrentDirectory);
        }

        public void AddAssemblyInDirectory(string directory, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var filePaths = Directory
                .GetFiles(directory, "*", searchOption)
                .Where(s =>
                {
                    var ext = Path.GetExtension(s);
                    if (ext == null)
                    {
                        return false;
                    }
                    ext = ext.ToLower();
                    return ext == ".dll" || ext == ".exe";
                });
            foreach (var filePath in filePaths)
            {
                AddAssembly(filePath);
            }
        }

        public bool AddAssembly(string filePath)
        {
            try
            {
                Assembly.ReflectionOnlyLoadFrom(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public List<AssemblyInfo> Load()
        {
            var infos = new List<AssemblyInfo>();
            foreach (Assembly a in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
            {
                var cadList = a.GetCustomAttributesData().ToList();
                var minVerAttr = cadList
                    .FirstOrDefault(data => data.AttributeType.FullName.EndsWith("MinimumServerVersionAttribute"))
                    ;
                string minVer = null;
                if (minVerAttr != null)
                {
                    // Attribute 생성자를 사용하고, Named Argument를 안썼다고 가정 -_-
                    // 예) 
                    // [assembly: MiniumServerVersion("2016.2.1.0")]
                    var arg = minVerAttr.ConstructorArguments.FirstOrDefault();
                    if (arg != null && arg.Value != null)
                    {
                        minVer = arg.Value.ToString();
                    }
                }
                var location = GetSafeValueOrDefault(()=>a.Location, "N/A");
                var version = GetSafeValueOrDefault(()=>a.GetName().Version.ToString(), "N/A");
                var fullName = GetSafeValueOrDefault(() => a.FullName, "N/A");
                infos.Add(new AssemblyInfo
                          {
                              Path = location,
                              FullName = fullName,
                              Version = version,
                              MinimumServerVersion = minVer,
                          });
            }
            return infos;
        }

        private T GetSafeValueOrDefault<T>(Func<T> getter, T defaultValue = default(T))
        {
            try
            {
                return getter();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}