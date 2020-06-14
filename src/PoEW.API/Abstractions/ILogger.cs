using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.API.Abstractions {
    public interface ILogger {
        void Log(string message, object obj = null);
        void Log(object obj);
    }
}
