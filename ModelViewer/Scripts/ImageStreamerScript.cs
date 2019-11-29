using System;
using System.IO;
using Toys;

namespace ModelViewer
{
    class ImageStreamerScript : ScriptingComponent
    {
        public DynamicFormStream script { get; private set; }
        public Window wndw;

        public void SetDSS(DynamicFormStream dynamicForm)
        {
            script = dynamicForm;
        }

        void PostRender()
        {
            if (wndw == null || script == null)
                return;
            
            try
            {
                if (wndw.stream)
                {
                    var bytes = script.ImageStream.ToArray();
                    wndw.Execute(bytes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }
    }
}
