using System;
using System.Collections.Generic;
using System.Text;
using Toys;

namespace ModelViewer
{
    internal class NPCActionEnableTV :  NPCBasicAction
    {
        NPCNavigationSystem navigationSystem;
        LocationState location;
        PlayVideoScript player;
        public NPCActionEnableTV(string name, string TVId) : base(name)
        {
            SceneNode tvnode = CoreEngine.MainScene.FindByName(TVId)[0];
            player = tvnode.GetComponent<PlayVideoScript>();
        }

        public override bool IsCompleted(NPCController controller)
        {
            return player.IsPlaing;
        }

        public override void Start(NPCController controller)
        {
            player.VideoStart();
        }

        public override void Stop(NPCController controller)
        {
            player.Stop();
        }
    }
}
