using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class Message
    {
        public Action Method { get; private set; }
        public ScriptingComponent obj { get; private set; }

        public Message(ScriptingComponent sc, Action method)
        {
            Method = method;
            obj = sc;
        }
    }
}
