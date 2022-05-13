using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys
{
    class InputTriggerVector2 : InputTrigger
    {
        public Func<Vector2> check;

        public InputTriggerVector2(string name)
        {
            Name = name;
        }
        public Vector2 Value { get; private set; }

        internal override void UpdateValue()
        {
            if (check != null)
                Value = check.Invoke();
        }

        internal override void Reset()
        {
            Value = Vector2.Zero;
        }
    }
}
