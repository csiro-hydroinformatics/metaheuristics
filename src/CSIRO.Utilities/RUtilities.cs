using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Environment = System.Environment;

namespace CSIRO.Utilities
{
    [Obsolete("R.NET now offers some facilities to do most if not all all this does", false)]
    public static class RUtilities
    {
        public static string SearchRBinPath()
        {
            string rBinPath = null;
            rBinPath = RBinFolderFromRegistry();
            if (rBinPath == null)
                rBinPath = RBinFolderFromDisk();

            return rBinPath;
        }
        private static void RInstallFromRegistry(out string currentVersion, out string installPath)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\R-core\R\");
            try
            {
                currentVersion = (string)key.GetValue("Current Version");
                installPath = (string)key.GetValue("InstallPath");
            }
            catch (NullReferenceException)
            {
                currentVersion = null;
                installPath = null;
            }

        }

        private static string RBinFolder(string installPath)
        {
            string osFolder = System.Environment.Is64BitOperatingSystem ? "x64" : "i386";

            return Path.Combine(installPath, "bin", osFolder);
        }

        private static string RBinFolderFromRegistry()
        {
            string installPath;
            string ver;
            RInstallFromRegistry(out ver, out installPath);
            string result = (installPath != null) ? RBinFolder(installPath) : null;
            return result;
        }

        private static DirectoryInfo tryRPath(System.Environment.SpecialFolder folder)
        {
            var path = System.Environment.GetFolderPath(folder);
            return new DirectoryInfo(Path.Combine(path, "R"));
        }

        private static string searchHighestVersion(DirectoryInfo rRootPath)
        {
            var rInstances = new List<DirectoryInfo>(rRootPath.GetDirectories("R-*.*"));
            return rInstances[rInstances.Count - 1].FullName;
        }

        private static string RInstallFromDisk()
        {
            DirectoryInfo di;

            di = tryRPath(Environment.SpecialFolder.ProgramFiles);
            if (di.Exists)
                return searchHighestVersion(di);

   
            di = tryRPath(Environment.SpecialFolder.ProgramFilesX86);
            if (di.Exists)
                return searchHighestVersion(di);


            return null;
        }

        private static string RBinFolderFromDisk()
        {
            string installPath = RInstallFromDisk();
            if (installPath == null)
                return null;
            else
                return RBinFolder(installPath);
        }
    }
}
