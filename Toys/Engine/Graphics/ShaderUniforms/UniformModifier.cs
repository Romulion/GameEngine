namespace Toys
{
    public enum ModifyType {
        Add,
        Multiply
    }  

    public struct UniformModifier
    {
        public object value;
        public ModifyType t;

        public UniformModifier(object val, ModifyType type)
        {
            value = val;
            t = type;
        }
    }
}
