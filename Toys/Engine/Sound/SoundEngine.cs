using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenTK;

namespace Toys
{
    class SoundEngine
    {
        IntPtr device;
        ContextHandle context;
        string version;
        string vendor;
        string renderer;
        List<AudioSource> sourceList;
        internal AudioListener Listner { get; set; }


        public SoundEngine()
        {
            context = ContextHandle.Zero;
            Initialize();
            sourceList = new List<AudioSource>();
        }

        void Initialize()
        {
            device = Alc.OpenDevice(null);
            unsafe
            {
                context = Alc.CreateContext(device, (int*)null);
                Alc.MakeContextCurrent(context);
            }
            Console.WriteLine(Alc.GetString(IntPtr.Zero, AlcGetString.AllDevicesSpecifier));
            version = AL.Get(ALGetString.Version);
            vendor = AL.Get(ALGetString.Vendor);
            renderer = AL.Get(ALGetString.Extensions);
        }

        internal void AddSource(AudioSource source)
        {
            if (!sourceList.Contains(source))
                sourceList.Add(source);
        }

        internal void RemoveSource(AudioSource source)
        {
            if (sourceList.Contains(source))
                sourceList.Remove(source);
        }

        internal void Update()
        {
            for(int i = 0; i < sourceList.Count; i++)
            {
                if (sourceList[i].Node.Active && sourceList[i].IsPlaing)
                    sourceList[i].Update();
            }
            Listner?.Update();
        }

        public void Dispose()
        {
            if (context != ContextHandle.Zero)
            {
                Alc.MakeContextCurrent(ContextHandle.Zero);
                Alc.DestroyContext(context);
            }
            context = ContextHandle.Zero;

            if (device != IntPtr.Zero)
            {
                Alc.CloseDevice(device);
            }
            device = IntPtr.Zero;
        }
    }
}
