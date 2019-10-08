﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    public class ScriptingComponent : Component
    {
        public bool IsInstalized { get; internal set; }

        internal ScriptingComponent() : base(typeof(ScriptingComponent))
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
        }
    }
}
