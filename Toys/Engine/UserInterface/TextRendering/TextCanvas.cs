using OpenTK.Mathematics;

namespace Toys
{
    public enum TextAlignHorizontal
    {
        Left,
        Right,
        Center,
    }

    public enum TextAlignVertical
    {
        Bottom,
        Top,
        Center,
    }
    public class TextCanvas
    {
        /// <summary>
        /// Horizontaly allign to node box
        /// </summary>
        public TextAlignHorizontal alignHorizontal;
        /// <summary>
        /// Vertically allign to node box
        /// </summary>
        public TextAlignVertical alignVertical;
        internal int VAO;
        internal int VBO;
        internal string Text { get { return text; } set { text = value; IsTextUpdated = true; } }
        internal int StringLength;
        public Vector2 Position = Vector2.Zero;
        public float Scale { get { return scale; }  set { scale = value; IsTextUpdated = true; } }
        public Vector3 colour = Vector3.One;
        internal int Length;
        internal float Width = 0;
        internal float Heigth = 0;
        string text = "";
        float scale = 1;
        internal bool IsTextUpdated;
        internal TextCanvas (int vao, int vbo)
        {
            alignHorizontal = TextAlignHorizontal.Left;
            alignVertical = TextAlignVertical.Center;
            VAO = vao;
            VBO = vbo;
        }

        public void CloneTo(TextCanvas obj)
        {
            obj.alignHorizontal = alignHorizontal;
            obj.alignVertical = alignVertical;
            obj.Text = Text;
            obj.StringLength = StringLength;
            obj.Position = Position;
            obj.Scale = Scale;
            obj.colour = colour;
            obj.Length = Length;
            obj.Width = Width;
            obj.Heigth = Heigth;
        }
    }
}