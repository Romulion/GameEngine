using System;

namespace Toys
{
    public abstract class ShaderUniform
    {
        public Type Type { get; protected set; }
        public object defaultValue { get; protected set; }
        public string Name { get; protected set; }
        public string Group { get; protected set; }

        protected Shader program;
		protected int varId;
        
        public abstract void Assign();
        public abstract bool CheckCompability(object value);
        public abstract object GetValue();

        protected abstract void CalculateFinal();
        public abstract void AddModifier(MaterialMorpher caller, object val, ModifyType mod);
        public abstract UniformModifier[] Getmods();

        public abstract ShaderUniform Clone();

        public void SetValue(object val)
        {
            
            if (!(CheckCompability(val)))
                return;

            defaultValue = val;
            CalculateFinal();
        }
    }
}
