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
        internal ScriptingComponent ScriptingObject { get; private set; }

        public Message(ScriptingComponent sc, Action method)
        {
            Method = method;
            ScriptingObject = sc;
        }
    }
}
