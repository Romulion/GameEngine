using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;

namespace ModelViewer
{
    class NPCExpression
    {
        public string Name { get; private set; }
        public bool canSpeak = true;
        public Dictionary<string, float> expressionList = new Dictionary<string, float>();

        float eyeLocationX = 0f;
        float eyeLocationY = 0f;

        public NPCExpression(string name)
        {
            Name = name;
        }

        public float EyeLocationX
        {
            get { return eyeLocationX; }
            set { eyeLocationX = CutValue(value, -1, 1); }
        }

        public float EyeLocationY
        {
            get { return eyeLocationY; }
            set { eyeLocationY = CutValue(value, -1, 1); }
        }

        
        
        public void SetExpression(string name, float value)
        {
            expressionList.Add(name, CutValue(value,0,1));
        }



        float CutValue(float value, float min, float max)
        {
            if (value > max)
                value = max;
            else if (value < min)
                value = min;

            return value;
        }

        internal NPCExpression Clone()
        {
            var val = new NPCExpression(Name);
            val.canSpeak = canSpeak;
            val.eyeLocationX = eyeLocationX;
            val.eyeLocationY = eyeLocationY;
            val.expressionList = expressionList;

            return val;
        }
    }
}
