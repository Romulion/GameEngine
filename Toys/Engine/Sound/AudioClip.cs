using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using OpenTK.Audio.OpenAL;

namespace Toys
{
    /// <summary>
    /// Store audion data (File only)
    /// </summary>
    public class AudioClip : Resource
    {
        AudioFileReader audioData;

        internal int Bps { get; private set; }
        internal int SampleRate { get; private set; }
        internal int Channels { get; private set; }
        internal byte[] ByteBuffer { get; private set; }
        public ALFormat SampleFormat { get; private set; }
        public AudioClip(System.IO.Stream stream, string file)
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

            //Console.WriteLine(audioData.Length);
            //Console.WriteLine(audioData.WaveFormat.Encoding);

            GetFormat(Channels, Bps);


            audioData.Dispose();
        }

        public AudioClip(byte[] data, int bps, int sampleRate)
        {
            Bps = bps;
            ByteBuffer = data;
            SampleRate = sampleRate;
            Channels = 1;

            //SampleFormat = ALFormat.MonoFloat32Ext;
            SampleFormat = ALFormat.Mono16;
        }

        public void GetFormat(int channels, int bps)
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

            SampleFormat = result;
        }

        protected override void Unload()
        {
            
        }
    }
}
