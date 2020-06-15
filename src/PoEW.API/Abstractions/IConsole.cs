using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.API.Abstractions {
    public interface IConsole  {
        void PushLine(string text);

        void Clear();
    }
}
