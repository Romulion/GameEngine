using System;
using System.Collections.Generic;
using System.Text;

namespace Toys
{
    public class InputTriggerBool : InputTrigger
    {
        public Func<bool> Val1;
        public Func<bool> Val2;
        public Func<bool> Val3;
        Func<bool> fnk = () => false;

        public InputTriggerBool(string name)
        {
            Name = name;
            
            Val1 = fnk;
            Val2 = fnk;
            Val3 = fnk;
        }

        public bool Value { get; private set; }

        internal override void UpdateValue()
        {
            Value = Val1.Invoke() || Val2.Invoke() || Val3.Invoke();
        }

        internal override void Reset()
        {
            Value = false;
        }
    }
}
