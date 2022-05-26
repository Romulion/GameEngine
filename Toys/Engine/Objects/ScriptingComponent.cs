using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    public class ScriptingComponent : Component
    {
        /// <summary>
        /// Check if script is already initialized
        /// </summary>
        public bool IsInstalized { get; internal set; }

        public ScriptingComponent()
        {
            IsInstalized = false;
        }

        protected override void Unload()
        {
        }

        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
            CoreEngine.ScriptingEngine.AddScript(this);
        }

        internal override void RemoveComponent()
        {
            Node = null;
            CoreEngine.ScriptingEngine.RemoveScript(this);
        }

        /// <summary>
        /// method for calling on script destroing
        /// </summary>
        protected virtual void Destroy()
        {

        }
    }
}
