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
            IsInstalized = true;
        }

        internal override void Unload()
        {

        }

        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
            CoreEngine.sEngine.AddScript(this);
        }

        internal override void RemoveComponent()
        {
            Node = null;
            CoreEngine.sEngine.RemoveScript(this);
            Destroy();
        }

        /// <summary>
        /// method for calling on script destroing
        /// </summary>
        protected virtual void Destroy()
        {

        }
    }
}
