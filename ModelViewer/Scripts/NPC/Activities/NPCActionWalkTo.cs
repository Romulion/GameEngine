using System;
using System.Collections.Generic;
using System.Text;
using Toys;

namespace ModelViewer
{
    class NPCActionWalkTo : NPCBasicAction
    {
        SceneNode actor;
        bool isCompleted = false;
        public NPCActionWalkTo(string name, SceneNode target) : base(name)
        {
            actor = target;
        }

        public override bool IsCompleted(NPCController controller)
        {
            return !controller.navigationController.IsWalking
                && !controller.navigationController.IsRotating
                && isCompleted;
        }

        public override void Start(NPCController controller)
        {
            isCompleted = false;
            controller.navigationController.GoImmedeatly(actor.GetTransform.GlobalPosition, OpenTK.Mathematics.Vector3.Zero,
                () => isCompleted = true, true);
        }

        public override void Stop(NPCController controller)
        {
            if (!controller.navigationController.IsTaskCompleted)
                controller.navigationController.Stop();
        }
    }
}
