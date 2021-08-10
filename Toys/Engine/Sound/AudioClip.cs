using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using OpenTK.Audio.OpenAL;

namespace Toys
{
    public class AudioClip : Resource
    {
        AudioFileReader audioData;

        internal int Bps { get; private set; }
        internal int SampleRate { get; private set; }
        internal int Channels { get; private set; }

        internal byte[] ByteBuffer { get; private set; }

        public AudioClip(string file) : base(typeof(AudioClip))
        {
            audioData = new AudioFileReader(file);
            IWaveProvider sampler;
            Bps = audioData.WaveFormat.BitsPerSample;
            //read data to pointer
            SampleRate = audioData.WaveFormat.SampleRate;

            if (audioData.WaveFormat.BitsPerSample > 16)
            {
                sampler = audioData.ToWaveProvider16();
                ByteBuffer = new byte[audioData.Length * audioData.WaveFormat.BitsPerSample / 16];
                sampler.Read(ByteBuffer, 0, ByteBuffer.Length);
                Bps = 16;
            }
            else
            {
                ByteBuffer = new byte[audioData.Length];
                audioData.Read(ByteBuffer, 0, ByteBuffer.Length);
            }
        }


        public ALFormat GetFormat(int channels, int bps)
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

        internal override void Unload()
        {
            audioData.Dispose();
        }
    }
}
