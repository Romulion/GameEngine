using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using OpenTK.Audio.OpenAL;
using OpenTK;
using System.Runtime.InteropServices;

namespace Toys
{
    public class AudioSource : Component
    {
        int buffer, source, bps;
        byte[] byteBuffer;

        public bool IsPlaing { get; private set; }
        public AudioSource() : base(typeof(AudioSource))
        {
            AL.GenBuffers(1, out buffer);
            AL.GenSources(1, out source);
            IsPlaing = false;
        }

        public AudioSource(AudioFileReader audioData): this()
        {
            IWaveProvider sampler;
            bps = audioData.WaveFormat.BitsPerSample;
            if (audioData.WaveFormat.BitsPerSample > 16)
            {
                sampler = audioData.ToWaveProvider16();
                byteBuffer = new byte[audioData.Length * audioData.WaveFormat.BitsPerSample / 16];
                sampler.Read(byteBuffer, 0, byteBuffer.Length);
                bps = 16;
            }
            else
            {
                byteBuffer = new byte[audioData.Length];
                audioData.Read(byteBuffer, 0, byteBuffer.Length);
            }

            //read data to pointer
            var format = GetFormat(audioData.WaveFormat.Channels,bps);
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(byteBuffer.Length);
            Marshal.Copy(byteBuffer, 0, unmanagedPointer, byteBuffer.Length);
            AL.BufferData(buffer, format, unmanagedPointer, byteBuffer.Length, audioData.WaveFormat.SampleRate);
            Marshal.FreeHGlobal(unmanagedPointer);
            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.Source(source, ALSourceb.Looping, true);
        }

        ALFormat GetFormat(int channels, int bps)
        {
            ALFormat result = ALFormat.Mono16;
            if (channels == 2)
            {
                if (bps == 8)
                    result = ALFormat.Stereo8;
                else if (bps == 16)
                    result = ALFormat.Stereo16;
            }
            else if (channels == 1)
            {
                if (bps == 8)
                    result = ALFormat.Mono8;
                else if (bps == 16)
                    result = ALFormat.Mono16;
            }
            
            return result;
        }

        public float GetCurrentVolume()
        {
            int position;
            AL.GetSource(source, ALGetSourcei.ByteOffset, out position);
            float result = 0;

            if (bps == 8)
                result = byteBuffer[position] / (float)byte.MaxValue;
            else if (bps == 16)
            {
                //median from 121 samples
                for (int i = -60; i < 61; i++)
                    result += BitConverter.ToUInt16(byteBuffer, position + i * 16);

                result /= (float)ushort.MaxValue * 121;
            }
            else if (bps == 32)
                result = BitConverter.ToUInt32(byteBuffer, position) / (float)uint.MaxValue;
            return result;
        }

        public void Play()
        {
            AL.SourcePlay(source);
            IsPlaing = true;
        }

        public void Pause()
        {
            AL.SourcePause(source);
            IsPlaing = false;
        }

        public void Stop()
        {
            AL.SourceStop(source);
            IsPlaing = false;
        }


        internal void Update()
        {
            var pos = Node.GetTransform.Position;
            AL.Source(source, ALSource3f.Position, ref pos);
            //AL.Source(source, ALSource3f.Velocity, ref pos);
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
            AL.DeleteSource(source);
            AL.DeleteBuffer(buffer);
        }
    }
}
