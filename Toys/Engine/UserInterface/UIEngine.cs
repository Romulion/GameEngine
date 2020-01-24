using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
    class UIEngine
    {
        internal List<Canvas> canvases;
        internal List<VisualComponent> visualComponents;
        internal List<InteractableComponent> buttons;

        List<VisualComponent> activeComponents = new List<VisualComponent>();
        List<InteractableComponent> activeButtons = new List<InteractableComponent>();
        List<UIMaskComponent> masks = new List<UIMaskComponent>();
        private InteractableComponent clickContext;
        private bool clicked;
        public bool Busy
        {
            get
            {
                return clickContext != null;
            }
        }
        public UIEngine()
        {
            canvases = new List<Canvas>();
            visualComponents = new List<VisualComponent>();
            buttons = new List<InteractableComponent>();
        }

        private List<UIElement> SortCanvas(UIElement root, int maskID = 0)
        {
            var elements = new List<UIElement>();

            if (root.Active)
            {
                //process mask
                root.MaskCheck = maskID;
                if (root.IsMask)
                {
                    maskID++;
                    var mask = (UIMaskComponent)root.GetComponent<UIMaskComponent>();
                    mask.MaskValue = maskID;
                    masks.Add(mask);
                }

                elements.Add(root);
                root.UpdateTransform();
                foreach (var child in root.Childs)
                {
                    elements.AddRange(SortCanvas(child,maskID));
                }
            }
            return elements;
        }

        internal void UpdateUI()
        {
            masks.Clear();
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
            int currMask = 0;
            GL.StencilFunc(StencilFunction.Always, currMask, 0xFF);
            GL.StencilMask(0x00);
            foreach (var component in activeComponents)
            {

                //draw mask to stencil
                if (component.Node.IsMask)
                {
                    foreach (var elem in masks)
                    {
                        if (elem.Node == component.Node)
                        {
                            
                            GL.StencilFunc(StencilFunction.Always, elem.MaskValue, 0xFF);
                            break;
                        }
                    }
                    GL.StencilMask(0xFF);
                    component.Draw();
                    GL.StencilMask(0x00);
                }
                else
                {
                    //change stencil context
                    if (currMask != component.Node.MaskCheck)
                    {
                        currMask = component.Node.MaskCheck;
                        if (currMask == 0)
                        {
                            GL.StencilFunc(StencilFunction.Always, currMask, 0xFF);
                        }
                        else
                            GL.StencilFunc(StencilFunction.Equal, currMask, 0xFF);
                    }
                    component.Draw();
                }
            }
        }

        internal void CheckMouse()
        {
            if (!GLWindow.gLWindow.Focused)
                return;

            MouseState ms = Mouse.GetCursorState();
            var point = GLWindow.gLWindow.PointToClient(new System.Drawing.Point(ms.X, ms.Y));
            
            Vector2 cursorWindowPosition = new Vector2(point.X, point.Y);

            //skip out of boundary
            if (cursorWindowPosition.X < 0 || cursorWindowPosition.Y < 0 || cursorWindowPosition.X > GLWindow.gLWindow.Width || cursorWindowPosition.Y > GLWindow.gLWindow.Height)
                return;

            //normalizing coordinates
            cursorWindowPosition.X /= GLWindow.gLWindow.Width;
            cursorWindowPosition.Y /= GLWindow.gLWindow.Height;
            cursorWindowPosition.Y = 1 - cursorWindowPosition.Y;

            //inform active component about mouse position
            clickContext?.PositionUpdate(cursorWindowPosition.X, cursorWindowPosition.Y);

            for (int i = activeButtons.Count - 1; i >= 0; i--)
            {
                var button = activeButtons[i];
                if (InMask(button, cursorWindowPosition) 
                    && button.Node.GetTransform.GlobalRect.Contains(cursorWindowPosition.X, cursorWindowPosition.Y))
                {
                    
                    if (!clicked && ms.IsButtonDown(MouseButton.Left))
                    {
                        button.ClickDownState();
                        clickContext = button;
                    }
                    else if (clicked && ms.IsButtonUp(MouseButton.Left))
                    {
                        clickContext?.ClickUpState();

                    }
                    else if (clickContext != button)
                    {
                        button.Hover();
                    }
                    break;
                }
                else if (button != clickContext)
                {
                    button.Normal();
                }
            }

            //clear context on mouse
            if (clicked && ms.IsButtonUp(MouseButton.Left))
                clickContext = null;

            clicked = ms.IsButtonDown(MouseButton.Left);
        }

        bool InMask(InteractableComponent component,Vector2 cursorPos)
        {
            if (component.Node.MaskCheck != 0 
                && !masks[component.Node.MaskCheck - 1].Node.GetTransform.GlobalRect.Contains(cursorPos.X, cursorPos.Y))
                return false;
            return true;
        }
    }
}
