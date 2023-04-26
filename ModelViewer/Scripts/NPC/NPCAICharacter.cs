using System;
using System.Collections.Generic;
using System.Text;
using Toys;

namespace ModelViewer
{
    class NPCAICharacter
    {
        NPCNeeds needs;
        public NPCAICharacter()
        {
            needs = new NPCNeeds();
        }
        internal void Update()
        {
            needs.Update();
        }
    }
}
