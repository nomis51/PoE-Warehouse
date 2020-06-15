using PoEW.API.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PoEW.API.Logging {
    public class FileLogger : ILogger {
        private const string LogFilePath = ".\\log.txt";

        public FileLogger() { }

        public void Log(string message, object obj = null) {
            File.AppendAllText(LogFilePath, message + (obj != null ? $" {obj}" : "") + Environment.NewLine);
        }

        public void Log(object obj) {
            File.AppendAllText(LogFilePath, obj.ToString() + Environment.NewLine);
        }
    }
}
