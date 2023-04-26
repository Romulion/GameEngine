using System;
using System.Collections.Generic;
using System.Text;

namespace ModelViewer
{
    internal class NPCActionCloth : NPCBasicAction
    {
        ClothingType[] clothState;
        public NPCActionCloth(string name, ClothingType[] cloth) : base(name)
        {
            clothState = cloth;
        }

        public override bool IsCompleted(NPCController controller)
        {
            return true;
        }

        public override void Start(NPCController controller)
        {
            foreach(var cloth in clothState)
                controller.ClothingController.RemoveCloth(cloth);
        }

        public override void Stop(NPCController controller)
        {
            foreach (var cloth in clothState)
                controller.ClothingController.PutOn(cloth);
        }
    }
}
