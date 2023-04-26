using System;
using System.Collections.Generic;
using System.Text;
using Toys;
using OpenTK.Mathematics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelViewer
{
    class PlayVideoScript : ScriptingComponent
    {
        SceneNode videoCanvasNode;
        Canvas videoCanvas;
        VideoClip video;
        RawImage videoScreen;
        AudioSource source1;
        public bool IsPlaing { get; private set; }
        //Task<VideoClip> task;

        public Material ScreenMaterial;
        void Awake()
        {
            //video = ResourcesManager.LoadAsset<VideoClip>(@"Assets\OreNoImoutoGaKonnaNiKawaiiWakeGaNai．-OP01b-NCBD.mp4");
            //video = ResourcesManager.LoadAsset<VideoClip>(@"Assets\16635998518810.mp4");
            //var task = ResourcesManager.LoadAssetAsync<VideoClip>(@"Assets\16635998518810.mp4");
            var task = ResourcesManager.LoadAssetAsync<VideoClip>(@"Assets\OreNoImoutoGaKonnaNiKawaiiWakeGaNai．-OP01b-NCBD.mp4");
              
            source1 = Node.AddComponent<AudioSource>();
            source1.IsLooping = false;

            //source1.SetAudioClip(video.GetAudionChannel(0));
            //ScreenMaterial.SetTexture(video.TargetTexture, TextureType.Diffuse); 
            //VideoStart();
            
            task.ContinueWith(OnLoad);
        }

        
        void OnLoad(Task<VideoClip> clip)
        {
            video = clip.Result;
            ScreenMaterial.SetTexture(video.TargetTexture, TextureType.Diffuse);
            source1.SetAudioClip(video.GetAudionChannel(0));
            //VideoStart();
        }
        

        public void Reset()
        {
            video.Reset();
            source1.Stop();
        }

        public void Stop()
        {
            video.Stop();
            Reset();
            IsPlaing = false;
        }

        //create floating ui
        void Test()
        {
            videoCanvasNode = new SceneNode();
            videoCanvasNode.Name = "video";
            videoCanvasNode.GetTransform.Position = new Vector3(1, 1.8f, 0);
            CoreEngine.MainScene.AddNode2Root(videoCanvasNode);

            videoCanvas = videoCanvasNode.AddComponent<Canvas>();
            var root = new UIElement();
            videoCanvas.Add2Root(root);
            videoScreen = (RawImage)root.AddComponent<RawImage>();

            var videoAspect = video.Height / (float)video.Width;
            var rect = root.GetTransform;
            rect.anchorMax = new Vector2(0, 0);
            rect.anchorMin = new Vector2(0, 0);
            rect.offsetMin = new Vector2(-200, -200 * videoAspect);
            rect.offsetMax = new Vector2(200, 200 * videoAspect);
            videoCanvas.Mode = Canvas.RenderMode.WorldSpace;
            videoCanvas.Canvas2WorldScale = 0.0025f;
        }

        public void VideoStart()
        {
            video.Play();
            source1.Play();
            IsPlaing = true;
        }

        void Update()
        {
            if (!video || !IsPlaing)
                return;

            video.Update();            
            if (!video.IsPlaing)
            {
                Reset();
                VideoStart();
            }
            
        }
    }
}
