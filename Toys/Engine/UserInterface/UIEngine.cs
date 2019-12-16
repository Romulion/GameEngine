using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace Toys
{
    class UIEngine
    {
        internal List<Canvas> canvases;
        internal List<VisualComponent> visualComponents;
        internal List<ButtonComponent> buttons;

        List<VisualComponent> activeComponents = new List<VisualComponent>();
        List<ButtonComponent> activeButtons = new List<ButtonComponent>();
        
        public UIEngine()
        {
            canvases = new List<Canvas>();
            visualComponents = new List<VisualComponent>();
            buttons = new List<ButtonComponent>();
        }

        private List<UIElement> SortCanvas(UIElement root)
        {
            var elements = new List<UIElement>();

            if (root.Active)
            {
                elements.Add(root);
                root.UpdateTransform();
                foreach (var child in root.Childs)
                {
                    elements.AddRange(SortCanvas(child));
                }
            }
            return elements;
        }

        internal void UpdateUI()
        {
            var elements = new List<UIElement>();
            foreach (var canvas in canvases)
            {
                if (canvas.Root)
                    elements.AddRange(SortCanvas(canvas.Root));
            }

            activeButtons.Clear();
            activeComponents.Clear();

            foreach (var element in elements)
            {
                foreach (var component in visualComponents)
                {
                    if (component.Node == element)
                        activeComponents.Add(component);
                }

                foreach (var component in buttons)
                {
                    if (component.Node == element)
                        activeButtons.Add(component);
                }

            }
        }

        internal void DrawUI()
        {
            foreach (var component in activeComponents)
                component.Draw();
        }

        internal void CheckMouse()
        {
            if (!GLWindow.gLWindow.Focused)
                return;

            var windowPosition = GLWindow.gLWindow.Location;
            var windowSize = GLWindow.gLWindow.Size;
            MouseState ms = Mouse.GetCursorState();
            Vector2 cursorWindowPosition = new Vector2(ms.X - windowPosition.X, ms.Y - windowPosition.Y);

            //skip out of boundary
            if (cursorWindowPosition.X < 0 || cursorWindowPosition.Y < 0 || cursorWindowPosition.X > windowSize.Width || cursorWindowPosition.Y > windowSize.Height)
                return;

            //normalizing
            cursorWindowPosition.X /= windowSize.Width;
            cursorWindowPosition.Y /= windowSize.Height;
            cursorWindowPosition.Y = 1 - cursorWindowPosition.Y;

            foreach (var button in activeButtons)
            {
                if (button.Node.GetTransform.GlobalRect.Contains(cursorWindowPosition.X, cursorWindowPosition.Y))
                {
                    button.Hover();
                    //button.OnClick();
                }
                else
                    button.Normal();
            }
        }
    }
}
