using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionUC
{
    public delegate void DelCommandSend(int Command, int Option);

    public delegate void DelSerialCommandSend(int Command, byte Option);

    public delegate void DelSerialSettingCommandSend(int Command, string Port);
}
