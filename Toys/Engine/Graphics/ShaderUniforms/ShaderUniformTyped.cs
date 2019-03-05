using System;
using System.Collections.Generic;
using OpenTK;
using System.Linq;

namespace Toys
{
    /// <summary>
    /// Availible types:
    /// INT FLOAT VECTOR3 VECTOR4 MATRIX4
    /// 
    /// probably better create 5 classes for these variables
    /// </summary>
    /// <typeparam name="T">INT FLOAT VECTOR3 VECTOR4 MATRIX4</typeparam>
    class ShaderUniformTyped<T> : IShaderUniform where T : struct
    {
        public string Name { get; private set; }
        public string Group { get; private set; }
        public Type type { get; private set; }
        public object defaultValue { get; private set; }

        Dictionary<Morph, UniformModifier> mods = new Dictionary<Morph, UniformModifier>();
        string varName;
        Shader program;
        Action assignAction;
        
        public ShaderUniformTyped(string name,string group,Type type, Shader program)
        {
            Name = name;
            Group = group;

            if (group != "")
                varName = group + "." + name;
            else
                varName = name;

            this.type = type;
            this.program = program;
            FindAssignFunction();
        }

        void FindAssignFunction()
        {
            Dictionary<Type, Action> actionList = new Dictionary<Type, Action>();
            actionList[typeof(int)] = () => AssignInt();
            actionList[typeof(float)] = () => AssignFloat();
            actionList[typeof(Vector3)] = () => AssignVector3();
            actionList[typeof(Vector4)] = () => AssignVector4();
            actionList[typeof(Matrix4)] = () => AssignMatrix4();

            try
            {
                assignAction = actionList[type];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception(String.Format("wrong type passed to uniform: {0}", type));
            }
        }

        public void SetValue(object val)
        {
            
            if (!(val is T))
            { 
                return;
            }
            
            defaultValue = val;
            program.ApplyShader();
            Assign();
        }

        public bool CheckCompability (object val)
        {
            return val is T;
        }

        public void Assign()
        {
            assignAction();
        }

        public void AddModifier(Morph caller, object val, ModifyType mod)
        {
            mods[caller] = new UniformModifier(val, mod);
        }

        public UniformModifier[] Getmods()
        {
            var query = from mod in mods
                        orderby mod.Value.t descending
                        select mod.Value;

            return query.ToArray();
        }


        void CalculateFinal()
        {
            T value = (T)this.defaultValue;
            foreach (UniformModifier mod in Getmods())
            {
                switch (mod.t)
                {
                    case ModifyType.Add:
                        value += (T)mod.value;
                }
            }
        }

        void AssignInt()
        {
            Console.WriteLine(44);
            program.SetUniform((int)defaultValue, varName);
        }

        void AssignFloat()
        {
            program.SetUniform((float)defaultValue, varName);
        }

        void AssignVector3()
        {
            program.SetUniform((Vector3)defaultValue, varName);
        }

        void AssignVector4()
        {
            program.SetUniform((Vector4)defaultValue, varName);
        }

        void AssignMatrix4()
        {
            program.SetUniform((Matrix4)defaultValue, varName);
        }
    }
}
