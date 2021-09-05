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
        public AudioSource() : base(typeof(AudioSource))
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
            var format = clip.GetFormat(clip.Channels, clip.Bps);
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(clip.ByteBuffer.Length);
            Marshal.Copy(clip.ByteBuffer, 0, unmanagedPointer, clip.ByteBuffer.Length);
            AL.BufferData(bufferID, format, unmanagedPointer, clip.ByteBuffer.Length, clip.SampleRate);
            Marshal.FreeHGlobal(unmanagedPointer);
            AL.Source(sourceID, ALSourcei.Buffer, bufferID);
            AL.Source(sourceID, ALSourceb.Looping, looping);
            AL.Source(sourceID, ALSource3f.Direction, ref dir);
        }


        public float GetCurrentVolume()
        {
            int sampleCount = (int)(clip.SampleRate * 0.016f);
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
                    if (pos > clip.ByteBuffer.Length)
                        break;
                    result += BitConverter.ToUInt16(clip.ByteBuffer, pos);
                    n++;
                }

                if (n > 0)
                    result /= ((float)ushort.MaxValue * n);

                Console.WriteLine((float)ushort.MaxValue * n);
            }
            else if (clip.Bps == 32)
                result = BitConverter.ToUInt32(clip.ByteBuffer, position) / (float)uint.MaxValue;
            return -(float)Math.Log10(result);
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
            var pos = Node.GetTransform.Position;
            AL.Source(sourceID, ALSource3f.Position, ref pos);

            int play;
            AL.GetSource(sourceID, ALGetSourcei.SourceState, out play);
            if (play == (int)ALSourceState.Stopped)
                IsPlaing = false;
        }
        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
            CoreEngine.aEngine.AddSource(this);
        }

        internal override void RemoveComponent()
        {
            Node = null;
            CoreEngine.aEngine.RemoveSource(this);
            if (IsPlaing)
                Stop();
        }

        internal override void Unload()
        {
            AL.DeleteSource(sourceID);
            AL.DeleteBuffer(bufferID);
        }
    }
}
