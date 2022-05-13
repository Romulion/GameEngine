 using System;
using System.Collections.Generic;
using System.Text;

namespace Toys.Engine.Controll.Input
{
    public class InputTriggerFloat : InputTrigger
    {
        public Func<float> check;

        public InputTriggerFloat(string name)
        {
            Name = name;
        }
        public float Value { get; private set; }

        internal override void UpdateValue()
        {
            if (check != null)
                Value = check.Invoke();
        }

        internal override void Reset()
        {
            Value = 0;
        }
    }
}
