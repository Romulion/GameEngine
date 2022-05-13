using System;
using System.Collections.Generic;
using System.Text;

namespace Toys
{
    public abstract class InputTrigger
    {
        public string Name { get; protected set; }

        public bool IsSet { get; protected set; }

        internal abstract void UpdateValue();

        internal abstract void Reset();
    }
}
