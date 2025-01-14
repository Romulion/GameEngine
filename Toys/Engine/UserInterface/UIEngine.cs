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
        internal List<InteractiveComponent> buttons;

        internal List<Canvas> activeCanvases = new List<Canvas>();

        private InteractiveComponent clickContext;
        private bool clicked;
        Vector2 clickStartPosition;
        Vector2 dragThresold = new Vector2(10);
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
            buttons = new List<InteractiveComponent>();
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
                    canvas.masks.Add(mask);
                    
                }
                elements.Add(root);
                root.UpdateTransform(canvas.CanvasScale);
                foreach (var child in root.Childs)
                {
                    elements.AddRange(SortCanvas(child, canvas, maskID));
                }
            }
            return elements;
        }

        internal void UpdateUI()
        {
            activeCanvases.Clear();
            foreach (var canvas in canvases)
            {
                var elements = new List<UIElement>();
                canvas.masks.Clear();
                canvas.activeButtons.Clear();
                canvas.activeComponents.Clear();
                if (canvas.IsActive)
                {
                    activeCanvases.Add(canvas);
                    foreach (var root in canvas.GetNodes())
                        elements.AddRange(SortCanvas(root, canvas));

                    foreach (var element in elements)
                    {
                        foreach (var component in visualComponents)
                        {
                            if (component.Node == element)
                                canvas.activeComponents.Add(component);
                        }

                        foreach (var component in buttons)
                        {
                            if (component.Node == element)
                                canvas.activeButtons.Add(component);
                        }
                    }
                    
                }
            }
            CheckMouse();
        }

        internal void DrawWorldUI(Camera camera)
        {
            GL.Disable(EnableCap.DepthTest);
            foreach (var canvas in activeCanvases)
            {
                if (canvas.Mode != Canvas.RenderMode.WorldSpace)
                    continue;
                DrawUI(camera, canvas);
            }
            GL.Enable(EnableCap.DepthTest);
        }

        internal void DrawScreenUI(Camera camera)
        {
            foreach (var canvas in activeCanvases)
            {
                if (canvas.Mode != Canvas.RenderMode.Overlay)
                    continue;
                DrawUI(camera, canvas);
            }
        }

        void DrawUI(Camera camera, Canvas canvas)
        {
            GL.Clear(ClearBufferMask.StencilBufferBit);
            var projection = Matrix4.CreateOrthographicOffCenter(0, CoreEngine.GetCamera.Width, 0,  CoreEngine.GetCamera.Height, -1, 1);
            var projectionWorld = camera.GetLook * camera.Projection;
            int currMask = 0;
            GL.StencilFunc(StencilFunction.Always, currMask, 0xFF);
            GL.StencilMask(0x00);
            foreach (var component in canvas.activeComponents)
            {

                //draw mask to stencil
                if (component.Node.IsMask)
                {
                    foreach (var elem in canvas.masks)
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
                    if (canvas.Mode == Canvas.RenderMode.WorldSpace)
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
            cursorWindowPosition.Y = CoreEngine.GfxEngine.Height - cursorWindowPosition.Y;

            
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
                if (clickContext != null)
                {
                    //inform active component about mouse position
                    var drag = cursorWindowPosition / clickContext.Node.ParentCanvas.CanvasScale;
                    clickContext.PositionUpdate(drag.X, drag.Y);
                }
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
                foreach (var canvas in activeCanvases)
                {
                    for (int i = canvas.activeButtons.Count - 1; i >= 0; i--)
                    {
                        
                        var button = canvas.activeButtons[i];
                        Console.WriteLine(button.Node.GetTransform.GlobalRect.Location + "" + (button.Node.GetTransform.GlobalRect.Location + button.Node.GetTransform.GlobalRect.Size));
                        Console.WriteLine(cursorWindowPosition);
                        if ((button.IsAllowDrag || !dragEnabled)
                            && InMask(button, cursorWindowPosition, canvas)
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
                                //Console.WriteLine(button);
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

            }

            clicked = ms.IsButtonDown(MouseButton.Left);
        }

        bool InMask(InteractiveComponent component,Vector2 cursorPos, Canvas canvas)
        {
            if (component.Node.MaskCheck != 0 
                && !canvas.masks[component.Node.MaskCheck - 1].Node.GetTransform.GlobalRect.Contains(cursorPos.X, cursorPos.Y))
                return false;
            return true;
        }
    }
}
