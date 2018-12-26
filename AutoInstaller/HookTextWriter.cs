using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AutoInstaller
{
    class HookStringWriter : StringWriter
    {
        public event EventHandler TextChanged;
        public TextWriter hooked;

        public HookStringWriter(TextWriter toHook)
        {
            hooked = toHook;
        }

        protected void OnWrite()
        {
            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void Write(char value)
        {
            base.Write(value);
            hooked.Write(value);
            OnWrite();
        }

        public override void Write(string value)
        {
            base.Write(value);
            hooked.Write(value);
            OnWrite();
        }

        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
            hooked.Write(buffer, index, count);
            OnWrite();
        }
    }
}
