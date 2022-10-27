using System;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Audio;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading.Tasks;


namespace Toys
{
    public class VideoClip : Resource
    {
        IntPtr frameBufferFront;
        IntPtr frameBufferBack;

        AudioClip[] channels;

        bool isFrameUpdated;
        MediaFile videoFile;
        public readonly int Height;
        public readonly int Width;
        public bool HasAudio { get; private set; }
        public bool HasVideo { get; private set; }

        public float FrameRate { get; private set; }
        public bool IsPlaing { get; private set; }

        readonly Timer timer;
        //Time time;
        int channelCount;
        volatile bool isDecoderBusy;
        int frameCount = 0;
        public TextureDynamic TargetTexture { get; private set; }

        
        internal VideoClip(System.IO.Stream stream, string path) : base(false)
        {
            
            //var options = new MediaOptions();
            videoFile = MediaFile.Open(stream);
            var metadata = videoFile.Video.Info;
            Height = metadata.FrameSize.Height;
            Width = metadata.FrameSize.Width;
            HasAudio = videoFile.HasAudio;
            HasVideo = videoFile.HasVideo;
            FrameRate = (float)metadata.AvgFrameRate;
            //Logger.Info(metadata.IsVariableFrameRate);
            //Logger.Info(metadata.PixelFormat);
            //Logger.Info(metadata.Duration);
            Logger.Error(444);
            frameBufferFront = Marshal.AllocHGlobal(Height * Width * 3);
            frameBufferBack = Marshal.AllocHGlobal(Height * Width * 3);
            //prepase timer sync
            timer = new Timer(1000 / FrameRate);
            timer.Elapsed += SwapFrames;
            timer.AutoReset = true;

            //TargetTexture = new TextureDynamic(Width, Height);

            SetupInitialData();


            if (GLWindow.gLWindow.CheckContext)
                TargetTexture = new TextureDynamic(Width, Height);
            else 
                CoreEngine.ActiveCore.AddNotyfyTask(() => TargetTexture = new TextureDynamic(Width, Height)).WaitOne(); ;
        }

        public void Play()
        {
            timer.Start();
            timer.AutoReset = true;
            IsPlaing = true;
        }

        public void Stop()
        {
            timer.Stop();
            timer.AutoReset = false;
            IsPlaing = false;
        }

        void SwapFrames(object sender, ElapsedEventArgs e)
        {
            //time.Stop();
            //skip swap if previous frame not processed
            if (isDecoderBusy)
                return;
            isDecoderBusy = true;
            //swap buffers
            var tempBuffer = frameBufferFront;
            frameBufferFront = frameBufferBack;
            frameBufferBack = tempBuffer;
            isFrameUpdated = true;
            //try get next frame to back buffer and stop if media file ends
            //due to bug if videostream comes to last frame its imposible to reset position
            if (!videoFile.Video.TryGetNextFrame(frameBufferBack, Width * 3))
            {
                //Reset();
                Stop();
            }
            else
                frameCount++;
            isDecoderBusy = false;
        }

        void SetupInitialData()
        {
            //get first 2 frames
            //videoFile.Video.TryGetFrame(TimeSpan.FromSeconds(0), frameBufferFront, Width * 3);
            Reset();

            
            //Audio
            channelCount = videoFile.Audio.Info.NumChannels;
            byte[][] samples = new byte[channelCount][];

            
            channels = new AudioClip[channelCount];
            //Read full audio track
            AudioData data;
            for (int i = 0; i < channelCount; i++)
            {
                samples[i] = new byte[videoFile.Audio.Info.SamplesPerFrame * (int)videoFile.Audio.Info.NumberOfFrames * 2];
            }

            var tempBuffer = new short[videoFile.Audio.Info.SamplesPerFrame];
            int offset = 0;
            
            while (videoFile.Audio.TryGetNextFrame(out data))
            {
                
                var temp = data.GetSampleData();
                for (int i = 0; i < channelCount; i++)
                {
                    //convert to 16 bit unsigned
                    for (int n = 0; n < temp[i].Length; n++)
                        tempBuffer[n] = (short)(short.MaxValue * temp[i][n] + 1);

                    Buffer.BlockCopy(tempBuffer, 0, samples[i], offset, videoFile.Audio.Info.SamplesPerFrame * 2);
                }
                
                offset += videoFile.Audio.Info.SamplesPerFrame * 2;
                
            }

            
            for (int i = 0; i < channelCount; i++)
            {
                channels[i] = new AudioClip(samples[i], 16, videoFile.Audio.Info.SampleRate);
            }
        }

        public AudioClip GetAudionChannel(int channelId)
        {
            if (!HasAudio || channelId + 1 > channelCount)
                return null;

            return channels[channelId];
        }

        public void Reset()
        {
            videoFile.Video.TryGetFrame(TimeSpan.Zero, frameBufferFront, Width * 3);
            videoFile.Video.TryGetNextFrame(frameBufferBack, Width * 3);
            isFrameUpdated = true;
            frameCount = 2;
        }


        /// <summary>
        /// Add this to update video output texture
        /// * main thread only
        /// </summary>
        public void Update()
        {
            if (isFrameUpdated && !isDestroyed && GLWindow.gLWindow.CheckContext)
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
