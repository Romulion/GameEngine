using System;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Audio;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Timers;


namespace Toys
{
    public class VideoDecoder : Resource
    {
        IntPtr frameBufferFront;
        IntPtr frameBufferBack;
        public AudioClip channel1 { get; private set; }
        public AudioClip channel2 { get; private set; }
        bool isFrameUpdated;
        MediaFile videoFile;
        public int Height;
        public int Width;
        Timer timer;
        Time time;
        int channelCount;
        public TextureDynamic TargetTexture { get; private set; }

        public VideoDecoder(string path) : base(false)
        {
            var options = new MediaOptions();
            videoFile = MediaFile.Open(path);
            var metadata = videoFile.Video.Info;
            Height = metadata.FrameSize.Height;
            Width = metadata.FrameSize.Width;

            //Logger.Info(metadata.IsVariableFrameRate);
            //Logger.Info(metadata.PixelFormat);
            //Logger.Info(metadata.Duration);

            frameBufferFront = Marshal.AllocHGlobal(Height * Width * 3);
            frameBufferBack = Marshal.AllocHGlobal(Height * Width * 3);

            //prepase timer sync
            timer = new Timer(1000 / metadata.AvgFrameRate);
            timer.Elapsed += SwapFrames;
            timer.AutoReset = true;

            TargetTexture = new TextureDynamic(Width, Height);

            SetupInitialData();

            time = new Time();
        }

        public void Play()
        {
            timer.Start();
            timer.AutoReset = true;
            time.Start();
        }

        public void Stop()
        {
            timer.Stop();
            timer.AutoReset = false;
            
            Logger.Info(time.Stop());
        }

        void SwapFrames(object sender, ElapsedEventArgs e)
        {
            //swap buffers
            var tempBuffer = frameBufferFront;
            frameBufferFront = frameBufferBack;
            frameBufferBack = tempBuffer;
            isFrameUpdated = true;

            //try get next frame to back buffer and stop if media file ends
            if (!videoFile.Video.TryGetNextFrame(frameBufferBack, Width * 3))
            {
                Stop();
            }
        }

        void SetupInitialData()
        {
            //get first 2 frames
            videoFile.Video.TryGetNextFrame(frameBufferFront, Width * 3);
            videoFile.Video.TryGetNextFrame(frameBufferBack, Width * 3);
            isFrameUpdated = true;

            //Audio
            channelCount = videoFile.Audio.Info.NumChannels;
            byte[][] samples = new byte[channelCount][];

            //Read full audio track
            AudioData data;
            for (int i = 0; i < channelCount; i++)
            {
                samples[i] = new byte[videoFile.Audio.Info.SamplesPerFrame * (int)videoFile.Audio.Info.NumberOfFrames * 2];
            }

            Logger.Info(videoFile.Audio.Info.SampleRate);
            var tempBuffer = new short[videoFile.Audio.Info.SamplesPerFrame];
            int offset = 0;
            while (videoFile.Audio.TryGetNextFrame(out data))
            {
                var temp = data.GetSampleData();
                //foreach (var sample in temp[0])
                //    Console.WriteLine(sample);
                for (int i = 0; i < channelCount; i++)
                {
                    //Buffer.BlockCopy(temp[i], 0, samples[i], offset, videoFile.Audio.Info.SamplesPerFrame * 4);
                    //convert to 16 bit unsigned
                    for (int n = 0; n < temp[i].Length; n++)
                    {
                        tempBuffer[n] = (short)(short.MaxValue * temp[i][n] + 1);
                        //Console.WriteLine(temp[i][n]);
                    }

                    Buffer.BlockCopy(tempBuffer, 0, samples[i], offset, videoFile.Audio.Info.SamplesPerFrame * 2);
                }

                /*
                var bits = data.GetChannelData(0);
                for (int n = 0; n < bits.Length; n++)
                {
                    var bytes = BitConverter.GetBytes(bits[n]);
                    Array.Reverse(bytes, 0, bytes.Length);
                    Console.WriteLine(BitConverter.ToSingle(bytes));
                }
                */

                offset += videoFile.Audio.Info.SamplesPerFrame * 2;
                
            }

            channel1 = new AudioClip(samples[0], 16, videoFile.Audio.Info.SampleRate);
            channel2 = new AudioClip(samples[1], 16, videoFile.Audio.Info.SampleRate);
        }

        public void Update()
        {
            if (isFrameUpdated && !isDestroyed)
            {
                TargetTexture.UpdateTexture(frameBufferFront);
                isFrameUpdated = false;
            }
        }

        protected override void Unload()
        {
            Stop();
            Marshal.FreeHGlobal(frameBufferFront);
            Marshal.FreeHGlobal(frameBufferBack);
            timer.Dispose();
            videoFile.Dispose();
        }

        /*
        unsafe void ConvertYUV2RGB(IntPtr frameConvBuffer, byte[] YUVFrame)
        {
            int numOfPixel = Width * Height;
            int positionOfV = numOfPixel;
            int positionOfU = numOfPixel / 4 + numOfPixel;
            byte[] rgb = new byte[numOfPixel * 3];

            int R = 0;
            int G = 1;
            int B = 2;

            for (int i = 0; i < Height; i++)
            {
                int startY = i * Width;
                int step = (i / 2) * (Width / 2);
                int startU = positionOfU + step;
                int startV = positionOfV + step;

                for (int j = 0; j < Width; j++)
                {
                    int Y = startY + j;
                    int U = startU + j / 2;
                    int V = startV + j / 2;
                    int index = Y * 3;

                    float r = YUVFrame[Y] + 1.370705f * (YUVFrame[V] - 128);
                    float g = YUVFrame[Y] - 0.698001f * (YUVFrame[U] - 128) - 0.337633f * (YUVFrame[V] - 128);
                    float b = YUVFrame[Y] + 1.732446f * (YUVFrame[U] - 128);

                    r = OpenTK.Mathematics.MathHelper.Clamp(r, 0, 255);
                    g = OpenTK.Mathematics.MathHelper.Clamp(g, 0, 255);
                    b = OpenTK.Mathematics.MathHelper.Clamp(b, 0, 255);



                    Marshal.WriteByte(frameConvBuffer, index + R, (byte)r); 
                    Marshal.WriteByte(frameConvBuffer, index + G, (byte)g);
                    Marshal.WriteByte(frameConvBuffer, index + B, (byte)b);


                    rgb[index + R] = (byte)r;
                    rgb[index + G] = (byte)g;
                    rgb[index + B] = (byte)b;
                    
                }
            }
        }

        
        Bitmap CreateBitmap(byte[] RGBFrame, int width, int height)
        {
            PixelFormat pxFormat = PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(width, height, pxFormat);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pxFormat);
            IntPtr pNative = bmpData.Scan0;
            Marshal.Copy(RGBFrame, 0, pNative, RGBFrame.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }
        

        void SaveTextureRGBFromPtr(IntPtr ptr, int height,int width)
        {
            var bdata = new byte[height * width * 3];
            Marshal.Copy(ptr, bdata, 0, bdata.Length);
            CreateBitmap(bdata, width, height).Save("test2.png");
        }
        */
        /*
        void Test()
        {
            var bdata = new byte[frameSize.Height * frameSize.Width * 3];
            //Span<byte> buffer = new Span<byte>(bdata);
            /*
            PixelFormat pxFormat = PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(frameSize.Width, frameSize.Height, pxFormat);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, frameSize.Width, frameSize.Height), ImageLockMode.ReadWrite, pxFormat);
            IntPtr pNative = bmpData.Scan0;
            
            //prepare next frame
            if (videoFile.Video.TryGetFrame(TimeSpan.FromSeconds(14.4), frameRawBuffer, frameSize.Width * 3))
            {
                Marshal.Copy(frameRawBuffer, bdata, 0, bdata.Length);
                //CreateBitmap(bdata, frameSize.Width, frameSize.Height).Save("test.png");
            }

            //bmp.UnlockBits(bmpData);
            //bmp.Save("test1.png");
        }
        */
    }
}
