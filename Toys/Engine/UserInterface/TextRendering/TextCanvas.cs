using OpenTK;

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
        public TextAlignHorizontal alignHorizontal;
        public TextAlignVertical alignVertical;
        internal int VAO;
        internal int VBO;
        internal string Text = "";
        internal int StringLength;
        public Vector2 Position = Vector2.Zero;
        public float Scale = 1;
        public Vector3 colour = Vector3.One;
        internal int Length;
        internal float Width = 0;
        internal float Heigth = 0;
        internal TextCanvas (int vao, int vbo)
        {
            alignHorizontal = TextAlignHorizontal.Left;
            alignVertical = TextAlignVertical.Center;
            VAO = vao;
            VBO = vbo;
        }
    }
}