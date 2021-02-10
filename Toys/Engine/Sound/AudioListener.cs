using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace Toys
{
    /// <summary>
    /// can have only one instance
    /// </summary>
    public class AudioListener : Component
    {
        static AudioListener listener;
        public Vector3 direction;
        private AudioListener() : base(typeof(AudioListener))
        {
            
        }

        public static AudioListener GetListener()
        {
            if (!listener)
                listener = new AudioListener();

            return listener;
        }

        internal void Update()
        {
            var pos = Node.GetTransform.Position;
            var up = Vector3.UnitY;
            direction = new Vector3(Node.GetTransform.GlobalTransform.M31, Node.GetTransform.GlobalTransform.M32, Node.GetTransform.GlobalTransform.M33);
            AL.Listener(ALListener3f.Position, ref pos);
            AL.Listener(ALListenerfv.Orientation, ref direction, ref up);
        }

        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
            CoreEngine.aEngine.Listner = this;
        }

        internal override void RemoveComponent()
        {
            Node = null;
            if (CoreEngine.aEngine.Listner == this)
                CoreEngine.aEngine.Listner = null;
        }
        internal override void Unload()
        {
        }
    }
}
