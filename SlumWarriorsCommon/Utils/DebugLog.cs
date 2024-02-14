using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlumWarriorsCommon.Utils
{
    public class DebugLog
    {
        private static string logDir = $"{Environment.CurrentDirectory}\\debug.log";
        private static StreamWriter writer = new StreamWriter(logDir);

        public static void WriteLine(string tag, string message)
        {
            if (!File.Exists(logDir))
                File.CreateText(logDir);

            writer.WriteLine($"{tag} - {message}");
        }
    }
}
