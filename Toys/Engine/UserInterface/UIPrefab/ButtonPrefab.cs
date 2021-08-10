using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;


namespace Toys.UserInterface
{
    public class ButtonPrefab
    {
        public UIElement ui { get; private set; }
        public ButtonComponent component { get; private set; }
        public TextBox label { get; private set; }

        public ButtonPrefab(string text = "")
        {
            ui = new UIElement();
            ui.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui.GetTransform.offsetMax = new Vector2(0, 0);
            ui.GetTransform.offsetMin = new Vector2(0, 0);
            component = (ButtonComponent)ui.AddComponent<ButtonComponent>();

            var label = (TextBox)ui.AddComponent<TextBox>();
            label.textCanvas.colour = new Vector3(1, 0, 0);
            label.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            label.textCanvas.alignVertical = TextAlignVertical.Center;

            label.SetText(text);
            label.textCanvas.Scale = 0.5f;
        }

        public RectTransform GetTransform { 
            get { return ui.GetTransform;  }
        }

        public string Text
        {
            get { return label.Text; }
            set { label.SetText(value); }
        }


        public void SetAction(Action action)
        {
            component.OnClick = action;
        }
        public void SetParent(UIElement parent)
        {
            ui.SetParent(parent);
        }
    }
}
