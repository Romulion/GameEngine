﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Toys
{
    class ShaderUniformVector2 : ShaderUniform
    {
        Dictionary<MaterialMorpher, UniformModifier> mods = new Dictionary<MaterialMorpher, UniformModifier>();
        string varName;
        Vector2 value = Vector2.Zero;



        public ShaderUniformVector2(string name, string group, Shader program, int id)
        {
            Name = name;
            Group = group;
            varId = id;

            if (group != "")
                varName = group + "." + name;
            else
                varName = name;

            this.program = program;
        }

        public override bool CheckCompability(object val)
        {
            return val is Vector2;
        }

        public override void Assign()
        {
            program.ApplyShader();
            program.SetUniform(value, varId);
        }

        public override void AddModifier(MaterialMorpher caller, object val, ModifyType mod)
        {
            if (!CheckCompability(val))
                return;

            if (mods.ContainsKey(caller) &&
                    (mod == ModifyType.Add && RetrieveValue(val) == Vector2.Zero ||
                    mod == ModifyType.Multiply && RetrieveValue(val) == Vector2.One))
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

        public override object GetValue()
        {
            return value;
        }

        protected override void CalculateFinal()
        {
            value = RetrieveValue(defaultValue);
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

        Vector2 RetrieveValue(object obj)
        {
            return (Vector2)obj;
        }
    }
}