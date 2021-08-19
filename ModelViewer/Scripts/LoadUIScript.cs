﻿using System;
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

            

            var btn2 = new Toys.UserInterface.ButtonPrefab("path");
            var btnTrtans2 = btn2.GetTransform;
            btnTrtans2.anchorMax = new Vector2(1f, 1f);
            btnTrtans2.anchorMin = new Vector2(0f, 1f);
            btnTrtans2.offsetMax = new Vector2(0, -0);
            btnTrtans2.offsetMin = new Vector2(0, -25);
            btn2.SetAction(() => { cc?.SetDestination(CoreEngine.GetCamera.Node.GetTransform.GlobalTransform.ExtractTranslation()); });
            btn2.SetParent(ui);

            
            var btn3 = new Toys.UserInterface.ButtonPrefab("Go/Stop");
            var btnTrtans3 = btn3.GetTransform;
            btnTrtans3.anchorMax = new Vector2(1f, 1f);
            btnTrtans3.anchorMin = new Vector2(0f, 1f);
            btnTrtans3.offsetMax = new Vector2(0, -30);
            btnTrtans3.offsetMin = new Vector2(0, -55);
            btn3.SetAction(() => { cc?.Go(); });
            btn3.SetParent(ui);            

            /*
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
            */
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

            //save test
            var save = new SaveSystem();

            var btn = new Toys.UserInterface.ButtonPrefab("Save");
            var btnTrtans = btn.GetTransform;
            btnTrtans.anchorMax = new Vector2(1f, 1f);
            btnTrtans.anchorMin = new Vector2(0f, 1f);
            btnTrtans.offsetMax = new Vector2(0, -60);
            btnTrtans.offsetMin = new Vector2(0, -85);
            btn.SetAction(() => { save.SaveGame(); });
            btn.SetParent(ui);

            var btn1 = new Toys.UserInterface.ButtonPrefab("Load");
            var btnTrtans1 = btn1.GetTransform;
            btnTrtans1.anchorMax = new Vector2(1f, 1f);
            btnTrtans1.anchorMin = new Vector2(0f, 1f);
            btnTrtans1.offsetMax = new Vector2(0, -90);
            btnTrtans1.offsetMin = new Vector2(0, -115);
            btn1.SetAction(() => { save.LoadGame(); });
            btn1.SetParent(ui);
        }


    }
}