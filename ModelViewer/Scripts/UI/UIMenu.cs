using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;

namespace ModelViewer
{
    class UIMenu
    {
        public bool IsInMenu { get; private set; }
        RawImage menuBakgrd;
        public Canvas Canvas;
        UIElement parent;
        SceneNode Node;
        int offsetY = 0;
        public UIMenu(SceneNode node)
        {
            Node = node;
            Canvas = Node.AddComponent<Canvas>();
            Canvas.Mode = Canvas.RenderMode.Overlay;
            Canvas.CanvasScale = 1;
            var ui0 = new UIElement();
            //center
            /*
            ui0.GetTransform.anchorMax = new Vector2(0.5f, 0.5f);
            ui0.GetTransform.anchorMin = new Vector2(0.5f, 0.5f);
            ui0.GetTransform.offsetMax = new Vector2(150, 200);
            ui0.GetTransform.offsetMin = new Vector2(-150, -200);
            */
            ui0.GetTransform.anchorMax = new Vector2(0f, 0.5f);
            ui0.GetTransform.anchorMin = new Vector2(0f, 0.5f);
            ui0.GetTransform.offsetMax = new Vector2(320, 200);
            ui0.GetTransform.offsetMin = new Vector2(20, -200);
            Canvas.Add2Root(ui0);

            var ui7 = new UIElement();
            ui7.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui7.GetTransform.anchorMin = new Vector2(0f, 0f);
            ui7.GetTransform.offsetMax = new Vector2(0, 0);
            ui7.GetTransform.offsetMin = new Vector2(0, 30);
            ui7.SetParent(ui0);
            var mask = (UIMaskComponent)ui7.AddComponent<UIMaskComponent>();

            //scrollbox
            var ui = new UIElement();
            ui.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui.GetTransform.anchorMin = new Vector2(0f, 0f);
            ui.GetTransform.offsetMax = new Vector2(-20, 0);
            ui.GetTransform.offsetMin = new Vector2(0, -100);
            ui.SetParent(ui7);
            var scrollBox = (ScrollBoxComponent)ui.AddComponent<ScrollBoxComponent>();
            scrollBox.Mask = mask;
            scrollBox.ScrollDirection = ScrollMode.Vertical;
            scrollBox.color.W = 0.3f;

            //scrollbar
            var scrollBar = (ScrollBarComponent)ui7.AddComponent<ScrollBarComponent>();
            scrollBar.SetScrollBox(scrollBox);

            parent = ui;
        }

        public ButtonComponent UIAdd2List(string label, Action onClick)
        {
            var button = new Toys.UserInterface.ButtonPrefab(label);
            var btnTransf = button.GetTransform;
            btnTransf.anchorMax = new Vector2(1f, 1f);
            btnTransf.anchorMin = new Vector2(0f, 1f);
            btnTransf.offsetMax = new Vector2(0, offsetY);
            btnTransf.offsetMin = new Vector2(0, offsetY - 25);
            button.SetAction(onClick);
            button.SetParent(parent);

            offsetY -= 30;
            return button.component;
        }

        public ButtonComponent UIAddSpecialButton(string label, Action onClick)
        {
            var button = new Toys.UserInterface.ButtonPrefab(label);
            var btnTransf = button.GetTransform;
            btnTransf.anchorMax = new Vector2(1f, 0f);
            btnTransf.anchorMin = new Vector2(0f, 0f);
            btnTransf.offsetMax = new Vector2(0, 25);
            btnTransf.offsetMin = new Vector2(0, 0);
            button.SetAction(onClick);
            button.SetParent(parent);

            return button.component;
        }

    }
}
