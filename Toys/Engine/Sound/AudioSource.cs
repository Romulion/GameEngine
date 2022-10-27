using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Toys
{
    public class AudioSource : Component
    {
        AudioClip clip;
        int bufferID, sourceID;
        Vector3 dir = Vector3.UnitZ;
        bool looping;
        public bool IsPlaing { get; private set; }
        public AudioSource()
        {
            bufferID = AL.GenBuffer();
            sourceID = AL.GenSource();
            IsPlaing = false;
            looping = false;
        }
        public void SetAudioClip(AudioClip audio)
        {
            AL.SourceStop(sourceID);
            AL.Source(sourceID, ALSourcei.Buffer, 0);
            clip = audio;
            
            //read data to pointer
            var format = clip.SampleFormat;
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(clip.ByteBuffer.Length);
            Marshal.Copy(clip.ByteBuffer, 0, unmanagedPointer, clip.ByteBuffer.Length);
            AL.BufferData(bufferID, format, unmanagedPointer, clip.ByteBuffer.Length, clip.SampleRate);
            Marshal.FreeHGlobal(unmanagedPointer);
            AL.Source(sourceID, ALSourcei.Buffer, bufferID);
            AL.Source(sourceID, ALSourceb.Looping, looping);
            AL.Source(sourceID, ALSource3f.Direction, ref dir);

            AL.Source(sourceID, ALSourcef.MaxDistance, 50);
        }


        public float GetCurrentVolume()
        {

            int sampleCount = (int)(clip.SampleRate * 0.02f);
            int position;
            AL.GetSource(sourceID, ALGetSourcei.ByteOffset, out position);
            float result = 0;

            if (clip.Bps == 8)
                result = clip.ByteBuffer[position] / (float)byte.MaxValue;
            else if (clip.Bps == 16)
            {
                int n = 0;
                for (int i = -sampleCount; i < sampleCount; i++)
                {
                    
                    var pos = position + i * 2;
                    if (pos < 0)
                        continue;
                    if (pos >= clip.ByteBuffer.Length)
                        break;
                    result += BitConverter.ToUInt16(clip.ByteBuffer, pos);
                    n++;
                }

                if (n > 0)
                    result /= (ushort.MaxValue * n);

            }
            else if (clip.Bps == 32)
                result = BitConverter.ToUInt32(clip.ByteBuffer, position) / (float)uint.MaxValue;

            return result;
        }

        public bool IsLooping
        {
            get { return looping; }
            set
            {
                looping = value;
                AL.Source(sourceID, ALSourceb.Looping, looping);
            }
        }

        public void Play()
        {
            AL.SourcePlay(sourceID);
            IsPlaing = true;
        }

        public void Pause()
        {
            AL.SourcePause(sourceID);
            IsPlaing = false;
        }

        public void Stop()
        {
            AL.SourceStop(sourceID);
            IsPlaing = false;
        }


        internal void Update()
        {
            var pos = Node.GetTransform.GlobalPosition;
            AL.Source(sourceID, ALSource3f.Position, ref pos);
            /*
            var fwd = Node.GetTransform.Forward;
            AL.Source(sourceID, ALSource3f.Direction, ref fwd);
            */
            int play;
            AL.GetSource(sourceID, ALGetSourcei.SourceState, out play);
            if (play == (int)ALSourceState.Stopped)
                IsPlaing = false;
        }
        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
            CoreEngine.AudioEngine.AddSource(this);
        }

        internal override void RemoveComponent()
        {
            Node = null;
            CoreEngine.AudioEngine.RemoveSource(this);
            if (IsPlaing)
                Stop();
        }

        protected override void Unload()
        {
            AL.DeleteSource(sourceID);
            AL.DeleteBuffer(bufferID);
        }
    }
}
