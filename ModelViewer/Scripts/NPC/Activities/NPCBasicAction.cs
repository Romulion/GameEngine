using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViewer
{
    abstract class NPCBasicAction
    {
        public string Name { get; private set; }
        protected NPCBasicAction(string name)
        {
            Name = name;
        }
        public abstract void Start(NPCController controller);
        public abstract void Stop(NPCController controller);
        public abstract bool IsCompleted(NPCController controller);
    }
}
