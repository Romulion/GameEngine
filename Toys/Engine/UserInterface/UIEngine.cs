using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
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
        Vector2 clickStartPosition;
        Vector2 dragThresold = new Vector2(5);
        bool dragEnabled = false;

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

        private List<UIElement> SortCanvas(UIElement root, Canvas canvas, int maskID = 0)
        {
            var elements = new List<UIElement>();
            root.ParentCanvas = canvas;
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
                    elements.AddRange(SortCanvas(child, canvas, maskID));
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
                foreach (var root in canvas.GetNodes())
                    elements.AddRange(SortCanvas(root, canvas)); 
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

        internal void DrawUI(Camera camera)
        {
            var projection = Matrix4.CreateOrthographicOffCenter(0, CoreEngine.GetCamera.Width, 0,  CoreEngine.GetCamera.Height, -1, 1);
            var projectionWorld = camera.GetLook * camera.Projection;
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
                    component.Draw(projection);
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
                    if (component.Node.ParentCanvas.Mode == Canvas.RenderMode.WorldSpace)
                        component.Draw(Matrix4.CreateScale(component.Node.ParentCanvas.Canvas2WorldScale) * component.Node.ParentCanvas.Node.GetTransform.GlobalTransform * projectionWorld);
                    //else if (component.Node.ParentCanvas.Mode == Canvas.RenderMode.ScreenSpace)
                    else
                        component.Draw(projection);
                }
            }
        }

        internal void CheckMouse()
        {
            
            if (!GLWindow.gLWindow.IsFocused)
                return;
            var ms = GLWindow.gLWindow.MouseState;
            //Console.WriteLine(ms.Position);
            //var point = GLWindow.gLWindow.PointToClient(new Vector2i((int)ms.Position.X, (int)ms.Position.Y));

            //Vector2 cursorWindowPosition = new Vector2(point.X, point.Y);
            Vector2 cursorWindowPosition = ms.Position;

            //skip out of boundary
            if (cursorWindowPosition.X < 0 || cursorWindowPosition.Y < 0 || cursorWindowPosition.X > GLWindow.gLWindow.Size.X || cursorWindowPosition.Y > GLWindow.gLWindow.Size.Y)
                return;

            //converting y coordinate
            cursorWindowPosition.Y = GLWindow.gLWindow.Size.Y - cursorWindowPosition.Y;

            
            //detect drag for elements not supporting it
            if (!dragEnabled && clicked)
            {
                if (Math.Abs(cursorWindowPosition.X - clickStartPosition.X) > dragThresold.X
                    || Math.Abs(cursorWindowPosition.Y - clickStartPosition.Y) > dragThresold.Y)
                {
                    clickContext = null;
                    dragEnabled = true;
                    clicked = false;
                }
            }
            else if (dragEnabled && clicked)
            {
                //inform active component about mouse position
                clickContext?.PositionUpdate(cursorWindowPosition.X, cursorWindowPosition.Y);
            }


            //inform current element about mouseup
            if (clicked && !ms.IsButtonDown(MouseButton.Left))
            {
                clickContext?.ClickUpState();
                clickContext = null;
                dragEnabled = false;
            }
            else
            {
                
                for (int i = activeButtons.Count - 1; i >= 0; i--)
                {
                    var button = activeButtons[i];
                    if ((button.IsAllowDrag || !dragEnabled)
                        && InMask(button, cursorWindowPosition)
                        && button.Node.GetTransform.GlobalRect.Contains(cursorWindowPosition.X, cursorWindowPosition.Y))
                    {

                        //fill context on click
                        if (!clicked && ms.IsButtonDown(MouseButton.Left))
                        {
                            dragEnabled = button.IsAllowDrag;
                            button.ClickDownState();
                            clickContext = button;
                            clickStartPosition = cursorWindowPosition;
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

            }

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
