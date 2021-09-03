using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    [Flags]
    public enum ScrollMode
    {
        Vertical = 1,
        Horizontal,
        Both,
    }

    public class ScrollBoxComponent : InteractableComponent
    {
        ShaderUniform shaderUniform;
        ShaderUniform colorMask;
        public UIMaskComponent Mask;

        public Texture2D Texture;
        public Vector4 color;
        Vector2 cursorPrev = Vector2.Zero;
        bool moveInitialized = false;
        ScrollBarComponent scrollBar;

        /// <summary>
        /// Direction where area can be scroled
        /// </summary>
        public ScrollMode ScrollDirection { get; set; }
        internal ButtonStates State { get; private set; }
        public ScrollBoxComponent() : base(typeof(ScrollBoxComponent))
        {
            //Material = defaultMaterial;
            shaderUniform = Material.UniformManager.GetUniform("model");
            colorMask = Material.UniformManager.GetUniform("color_mask");
            color = Vector4.One;
            ScrollDirection = ScrollMode.Vertical;
            base.IsAllowDrag = true;
        }

        static ScrollBoxComponent()
        {
        }

        internal void AddScrollBar(ScrollBarComponent component)
        {
            scrollBar = component;
        }

        internal override void AddComponent(UIElement nod)
        {
            CoreEngine.gEngine.UIEngine.buttons.Add(this);
            base.AddComponent(nod);
        }

        internal override void RemoveComponent()
        {
            CoreEngine.gEngine.UIEngine.buttons.Remove(this);
            base.RemoveComponent();
        }

        internal override void Draw(Matrix4 worldTransform)
        {

            Material.ApplyMaterial();
            colorMask.SetValue(color);
            shaderUniform.SetValue(Node.GetTransform.GlobalTransform * worldTransform);
            if (Texture)
                Texture.BindTexture();
            base.Draw(worldTransform);
        }

        internal override void ClickDownState()
        {
            if (State != ButtonStates.Clicked)
            {
                State = ButtonStates.Clicked;
            }
        }

        internal override void ClickUpState()
        {
            if (State == ButtonStates.Clicked)
            {
                State = ButtonStates.Normal;
                moveInitialized = false;
            }
        }

        internal override void Hover()
        {

        }


        internal override void Normal()
        {
            if (State == ButtonStates.Clicked)
            {
                State = ButtonStates.Normal;
                moveInitialized = false;
            }
        }

        internal override void Unload()
        {
            base.Unload();
        }

        internal override void PositionUpdate(float x, float y)
        {
            float delta = 0;
            //set start position
            if (!moveInitialized)
            {
                cursorPrev.X = x;
                cursorPrev.Y = y;
                moveInitialized = true;
                return;
            }

            //set borders
            Vector2 leftBott,rightTop;
            if (Mask)
            {
                leftBott = Mask.Node.GetTransform.Min;
                rightTop = Mask.Node.GetTransform.Max;
            }
            else
            {
                leftBott = Vector2.Zero;
                rightTop = Vector2.One;
            }
            Vector2 move = Vector2.Zero;
            if (ScrollDirection.HasFlag(ScrollMode.Horizontal))
            {
                move.X = x - cursorPrev.X;
                if (move.X > 0)
                {
                    //check left bound
                    delta = leftBott.X - Node.GetTransform.Min.X;
                    if (delta < 0 || (delta == 0 && move.X < 0))
                        move.X = 0;
                    else
                        move.X = (delta - move.X > 0) ? move.X : delta;
                }
                else
                {
                    //check right bound
                    delta = rightTop.X - Node.GetTransform.Max.X;
                    if (delta > 0)
                        move.X = 0;
                    else
                        move.X = (delta - move.X < 0) ? move.X : delta;
                }
            }
            
            if (ScrollDirection.HasFlag(ScrollMode.Vertical))
            {
                move.Y = y - cursorPrev.Y;
                //check Top bound
                if (move.Y < 0)
                {
                    delta = rightTop.Y - Node.GetTransform.Max.Y;
                    if (delta > 0)
                        move.Y = 0;
                    else
                        move.Y = (delta - move.Y < 0) ? move.Y : delta;
                }
                //check bottom bound
                else
                {
                    delta = leftBott.Y - Node.GetTransform.Min.Y;
                    if (delta < 0)
                        move.Y = 0;
                    else
                        move.Y = (delta - move.Y > 0) ? move.Y : delta;
                }
            }

            //update position
            Node.GetTransform.offsetMax += move;
            Node.GetTransform.offsetMin += move;
            cursorPrev.X = x;
            cursorPrev.Y = y;

            if (scrollBar)
                UpdateScrollBar();
        }

        void UpdateScrollBar()
        {
            Vector2 delta = new Vector2();
            delta.X = Node.GetTransform.GlobalRect.Width - Mask.Node.GetTransform.GlobalRect.Width;
            delta.Y = Node.GetTransform.GlobalRect.Height - Mask.Node.GetTransform.GlobalRect.Height;
            scrollBar.Value = 1 - Node.GetTransform.offsetMax.Y / delta.Y;
        }

        internal void UpdatePositionScrollbox(Vector2 move)
        {
            if (!Mask)
                return;

            var size = new Vector2();
            size.X = Node.GetTransform.GlobalRect.Width;
            size.Y = Node.GetTransform.GlobalRect.Height;
            Vector2 delta = new Vector2();
            delta.X = Node.GetTransform.GlobalRect.Width - Mask.Node.GetTransform.GlobalRect.Width;
            delta.Y = Node.GetTransform.GlobalRect.Height - Mask.Node.GetTransform.GlobalRect.Height;
            //delta
            Vector2 top = new Vector2();
            top.X = size.X;
            Node.GetTransform.offsetMax = top + delta * move;
            Node.GetTransform.offsetMin = Node.GetTransform.offsetMax - size;
        }

        public override VisualComponent Clone()
        {
            var scroll = new ScrollBoxComponent();
            scroll.Material = Material;
            scroll.shaderUniform = shaderUniform;
            scroll.colorMask = colorMask;
            scroll.Mask = Mask;
            scroll.Texture = Texture;
            scroll.color = color;
            scroll.ScrollDirection = ScrollDirection;
            return scroll;
        }
    }
}
