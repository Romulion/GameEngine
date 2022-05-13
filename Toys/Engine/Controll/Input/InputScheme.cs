using System;
using System.Collections.Generic;
using System.Text;

namespace Toys
{
    public class InputScheme
    {
        readonly List<InputTrigger> triggers = new List<InputTrigger>();

        public InputContext ContextType;

        public void AddTrigger(InputTrigger trigger)
        {
            triggers.Add(trigger);
        }

        public void RemoveTrigger(InputTrigger trigger)
        {
            triggers.Remove(trigger);
        }

        public InputTrigger GetTrigger(string name)
        {
            var trigger = triggers.Find(o => o.Name == name);
            return trigger;
        }

        internal void UpdateTriggers()
        {
            for (int i = 0; i < triggers.Count; i++)
                triggers[i].UpdateValue();
        }

        internal void ResetTriggers()
        {
            for (int i = 0; i < triggers.Count; i++)
                triggers[i].Reset();
        }
    }
}
