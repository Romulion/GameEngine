using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;

namespace ModelViewer
{
    class LoadUIScript : ScriptingComponent
    {
        public CharControll cc;
        void Awake()
        {
            var canvas = (Canvas)Node.AddComponent<Canvas>();

            var ui0 = new UIElement();
            ui0.GetTransform.anchorMax = new Vector2(0f, 1f);
            ui0.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui0.GetTransform.offsetMax = new Vector2(400, 0);
            ui0.GetTransform.offsetMin = new Vector2(0, -400);
            canvas.Root = ui0;
            canvas.AddObject(ui0);
            var ui7 = new UIElement();
            ui7.GetTransform.anchorMax = new Vector2(0f, 1f);
            ui7.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui7.GetTransform.offsetMax = new Vector2(170, -40);
            ui7.GetTransform.offsetMin = new Vector2(20, -180);
            ui7.SetParent(ui0);
            var mask = (UIMaskComponent)ui7.AddComponent<UIMaskComponent>();

            //scrollbox test
            var ui = new UIElement();
            ui.GetTransform.anchorMax = new Vector2(0f, 1f);
            ui.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui.GetTransform.offsetMax = new Vector2(130, 0);
            ui.GetTransform.offsetMin = new Vector2(0, -200);
            ui.SetParent(ui7);
            var scrollBox = (ScrollBoxComponent)ui.AddComponent<ScrollBoxComponent>();
            scrollBox.Mask = mask;
            scrollBox.ScrollDirection = ScrollMode.Vertical;
            scrollBox.color.W = 0.3f;

            //scrollbar
            var scrollBar = (ScrollBarComponent)ui7.AddComponent<ScrollBarComponent>();
            scrollBar.SetScrollBox(scrollBox);

            var ui1 = new UIElement();
            ui1.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui1.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui1.GetTransform.offsetMax = new Vector2(0, 0);
            ui1.GetTransform.offsetMin = new Vector2(0, -25);
            var image1 = (ButtonComponent)ui1.AddComponent<ButtonComponent>();

            var butLabel1 = (TextBox)ui1.AddComponent<TextBox>();
            butLabel1.textCanvas.colour = new Vector3(1, 0, 0);
            butLabel1.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            butLabel1.textCanvas.alignVertical = TextAlignVertical.Center;

            butLabel1.SetText("path");
            butLabel1.textCanvas.Scale = 0.5f;

            image1.OnClick = () => { cc?.SetDestination(CoreEngine.GetCamera.Node.GetTransform.Position); };
            var ui2 = new UIElement();
            ui2.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui2.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui2.GetTransform.offsetMax = new Vector2(0, -30);
            ui2.GetTransform.offsetMin = new Vector2(0, -55);
            var butLabel2 = (TextBox)ui2.AddComponent<TextBox>();

            butLabel2.SetText("Go/Stop");
            butLabel2.textCanvas.Scale = 0.5f;

            butLabel2.textCanvas.colour = new Vector3(1, 1, 0);
            butLabel2.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            butLabel2.textCanvas.alignVertical = TextAlignVertical.Center;
            var button = (ButtonComponent)ui2.AddComponent<ButtonComponent>();
            button.OnClick = () => { cc?.Go(); };
            //var ISS = (ImageStreamerScript)node.AddComponent<ImageStreamerScript>();
            //ISS.SetDSS(script);
            ui1.SetParent(ui);
            ui2.SetParent(ui);

            var ui3 = new UIElement();
            ui3.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui3.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui3.GetTransform.offsetMax = new Vector2(0, -60);
            ui3.GetTransform.offsetMin = new Vector2(0, -80);
            var slider = (SliderCompoent)ui3.AddComponent<SliderCompoent>();
            ui3.SetParent(ui);

            var ui4 = new UIElement();
            ui4.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui4.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui4.GetTransform.offsetMax = new Vector2(0, -85);
            ui4.GetTransform.offsetMin = new Vector2(0, -110);
            ui4.SetParent(ui);
            var checkbox = (CheckboxComponent)ui4.AddComponent<CheckboxComponent>();
            //checkbox.OnChange = () => Console.WriteLine(444);
            var butLabel4 = (TextBox)ui4.AddComponent<TextBox>();
            butLabel4.textCanvas.colour = new Vector3(1, 0, 0);
            butLabel4.textCanvas.alignHorizontal = TextAlignHorizontal.Left;
            butLabel4.textCanvas.alignVertical = TextAlignVertical.Center;
            butLabel4.textCanvas.colour = Vector3.Zero;
            butLabel4.textCanvas.Scale = 0.7f;
            butLabel4.SetText("checkbox");

            var ui5 = new UIElement();
            ui5.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui5.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui5.GetTransform.offsetMax = new Vector2(0, -115);
            ui5.GetTransform.offsetMin = new Vector2(0, -140);
            ui5.SetParent(ui);
            var input = (TextInputComponent)ui5.AddComponent<TextInputComponent>();
            //debug textures
            /*
            var canvas1 = (Canvas)Node.AddComponent<Canvas>();
            var ui10 = new UIElement();
            ui10.GetTransform.anchorMax = new Vector2(0f, 1f);
            ui10.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui10.GetTransform.offsetMax = new Vector2(400, 100);
            ui10.GetTransform.offsetMin = new Vector2(100, 400);
            canvas1.Root = ui10;

            var img =  ui10.AddComponent<RawImage>();
            img.Material.SetTexture(Node.scene.GetLight.shadowMap, TextureType.Diffuse);
            */
            /*
            var msd = (Animator)Node.GetComponent<Animator>();
            if (msd != null)
                bc = msd.BoneController;
            */

            //MadePath(31);

            //slider1.OnValueChanged = () => { active = false; physics.SetGravity(new Vector3(0, -10, 0)); };    

            Console.WriteLine(1111);

            //save test
            var save = new SaveSystem();
            var ui8 = new UIElement();
            ui8.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui8.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui8.GetTransform.offsetMax = new Vector2(0, -145);
            ui8.GetTransform.offsetMin = new Vector2(0, -170);
            var image5 = (ButtonComponent)ui8.AddComponent<ButtonComponent>();

            var butLabel5 = (TextBox)ui8.AddComponent<TextBox>();
            butLabel5.textCanvas.colour = new Vector3(1, 0, 0);
            butLabel5.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            butLabel5.textCanvas.alignVertical = TextAlignVertical.Center;

            butLabel5.SetText("Save");
            butLabel5.textCanvas.Scale = 0.5f;

            image5.OnClick = () => { save.SaveGame(); };
            var ui6 = new UIElement();
            ui6.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui6.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui6.GetTransform.offsetMax = new Vector2(0, -175);
            ui6.GetTransform.offsetMin = new Vector2(0, -200);
            var butLabel6 = (TextBox)ui6.AddComponent<TextBox>();

            butLabel6.SetText("Load");
            butLabel6.textCanvas.Scale = 0.5f;

            butLabel6.textCanvas.colour = new Vector3(1, 1, 0);
            butLabel6.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            butLabel6.textCanvas.alignVertical = TextAlignVertical.Center;
            var button5 = (ButtonComponent)ui6.AddComponent<ButtonComponent>();
            button5.OnClick = () => { save.LoadGame(); };
            ui6.SetParent(ui);
            ui8.SetParent(ui);
        }


    }
}
