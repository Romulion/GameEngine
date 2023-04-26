using System;
using System.Collections.Generic;
using System.Text;
using Toys;

namespace ModelViewer
{
    class NPCActionTalk : NPCBasicAction
    {
        SceneNode actor;
        public NPCActionTalk(string name, SceneNode target) : base(name)
        {
            actor = target;
        }

        public override bool IsCompleted(NPCController controller)
        {
            return true;
        }

        public override void Start(NPCController controller)
        {
            controller.AI.HeadController.LookAt(CoreEngine.GetCamera.Node);
            controller.AI.PlayVoice(117);
            //controller.AI.SetExpression(controller.AI.GetExpressions()[11]);
        }

        public override void Stop(NPCController controller)
        {
            controller.AI.HeadController.Return2Base();
        }
        
    }
}
