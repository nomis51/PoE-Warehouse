using PoEW.API.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace PoEW.API.Logging {
    public class ConsoleLogger : ILogger {
        IConsole Console;

        public ConsoleLogger(IConsole console) {
            Console = console;
        }

        public void Log(string message, object obj = null) {
            Console.PushLine(message + (obj != null ? obj.ToString() : ""));
        }

        public void Log(object obj) {
            Console.PushLine(obj.ToString());
        }
    }
}
