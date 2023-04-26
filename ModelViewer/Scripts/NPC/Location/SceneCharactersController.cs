using System;
using System.Collections.Generic;
using System.Timers;
using Toys;

namespace ModelViewer
{
    class SceneCharactersController : ScriptingComponent
    {
        List<NPCController> NPCs;
        readonly Camera player;
        Zone[] zones;
        /// <summary>
        /// Update time in seconds
        /// </summary>
        float updateInterval = 0.5f;
        float time;
        bool triggered;
        Timer updateTimer;

        public SceneCharactersController()
        {
            NPCs = new List<NPCController>();
            player = CoreEngine.GetCamera;
            zones = Zone.LoadFromStream(System.IO.File.OpenRead(@"Assets\Models\Home\zones.obj")).ToArray();
            
            updateTimer = new Timer(updateInterval * 1000);
            updateTimer.Elapsed += UpdateZone;
            updateTimer.AutoReset = true;
            updateTimer.Enabled = false;
            
        }

        public void AddNPC(NPCController npc)
        {
            NPCs.Add(npc);
        }

        void UpdateZone(object sender, ElapsedEventArgs e)
        {
            foreach (var cc in NPCs)
            {
                bool inZone = false;
                var pos = cc.Node.GetTransform.GlobalPosition.Xz;
                if (cc.CurrentZone != null)
                {
                    inZone = cc.CurrentZone.CheckInside(pos);
                }

                if (!inZone)
                {
                    foreach (var zone in zones)
                    {
                        if (zone.CheckInside(pos))
                        {
                            cc.CurrentZone = zone;
                            Console.WriteLine(cc.CurrentZone.Name);
                            break;
                        }
                    }
                }
            }
        }
    }
}
