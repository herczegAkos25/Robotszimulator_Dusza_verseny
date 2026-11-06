using System;
using System.Collections.Generic;
using System.Text;

namespace Robotszimulator
{
    public static class AppSettings
    {
        public static bool IsQuietMode { get; set; } = false;
        public static string InputFilePath { get; set; } = string.Empty;
        public static string OutputFilePath { get; set; } = string.Empty;
    }
}
