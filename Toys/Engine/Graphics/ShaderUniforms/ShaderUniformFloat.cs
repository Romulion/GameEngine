﻿using System;
using System.Collections.Generic;
using System.Linq;
namespace Toys
{
    public class ShaderUniformFloat : ShaderUniform 
    {

        Dictionary<MaterialMorpher, UniformModifier> mods = new Dictionary<MaterialMorpher, UniformModifier>();
        string varName;
        float value = 0f;



        public ShaderUniformFloat(string name, string group, Shader program)
        {
            Name = name;
            Group = group;

            if (group != "")
                varName = group + "." + name;
            else
                varName = name;

            this.program = program;
        }

        public override bool CheckCompability(object val)
        {
            return val is float;
        }

        public override void Assign()
        {
            program.ApplyShader();
            program.SetUniform(value, varName);
        }

        public override object GetValue()
        {
            return value;
        }

        public override void AddModifier(MaterialMorpher caller, object val, ModifyType mod)
        {
            if (!CheckCompability(val))
                return;

            if (mods.ContainsKey(caller) &&
                    (mod == ModifyType.Add && RetrieveValue(val) == 0 ||
                    mod == ModifyType.Multiply && RetrieveValue(val) == 1))
                mods.Remove(caller);
            else
                mods[caller] = new UniformModifier(val, mod);
            CalculateFinal();
        }

        public override UniformModifier[] Getmods()
        {
            var query = from mod in mods
                        orderby mod.Value.t descending
                        select mod.Value;

            return query.ToArray();
        }


        protected override void CalculateFinal()
        {
            value = RetrieveValue(defaultValue);
            Console.WriteLine(222);
            foreach (UniformModifier mod in Getmods())
            {
                switch (mod.t)
                {
                    case ModifyType.Add:
                        value += RetrieveValue(mod.value);
                        break;
                    case ModifyType.Multiply:
                        value *= RetrieveValue(mod.value);
                        break;
                    default:
                        break;
                }
            }
            Assign();
        }

        float RetrieveValue(object obj)
        {
            return (float)obj;
        }
    }
}