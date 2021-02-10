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
        int bufferID, sourceID, bps;
        int sampleRate = 0;
        byte[] byteBuffer;
        Vector3 dir = Vector3.UnitZ;
        public bool IsPlaing { get; private set; }
        public AudioSource() : base(typeof(AudioSource))
        {
            AL.GenSource();
            bufferID = AL.GenBuffer();
            sourceID = AL.GenSource();
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
            AL.BufferData(bufferID, format, unmanagedPointer, byteBuffer.Length, audioData.WaveFormat.SampleRate);
            Marshal.FreeHGlobal(unmanagedPointer);
            AL.Source(sourceID, ALSourcei.Buffer, bufferID);
            AL.Source(sourceID, ALSourceb.Looping, true);
            AL.Source(sourceID, ALSource3f.Direction, ref dir);

            sampleRate = audioData.WaveFormat.SampleRate;
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
            int sampleCount = (int)(sampleRate * 0.016f);
            int position;
            AL.GetSource(sourceID, ALGetSourcei.ByteOffset, out position);
            float result = 0;

            if (bps == 8)
                result = byteBuffer[position] / (float)byte.MaxValue;
            else if (bps == 16)
            {
                int n = 0;
                for (int i = -sampleCount; i < sampleCount; i++)
                {
                    
                    var pos = position + i * 2;
                    if (pos < 0)
                        continue;
                    if (pos > byteBuffer.Length)
                        break;
                    result += BitConverter.ToUInt16(byteBuffer, pos);
                    n++;
                }

                if (n > 0)
                    result /= ((float)ushort.MaxValue * n);

                Console.WriteLine((float)ushort.MaxValue * n);
            }
            else if (bps == 32)
                result = BitConverter.ToUInt32(byteBuffer, position) / (float)uint.MaxValue;
            return -(float)Math.Log10(result);
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
            //AL.Source(source, ALSource3f.Direction,Node.GetTransform.GetDirection());
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
            AL.DeleteSource(sourceID);
            AL.DeleteBuffer(bufferID);
        }
    }
}
