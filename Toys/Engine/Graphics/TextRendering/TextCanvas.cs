using OpenTK;

namespace Toys
{
    internal class TextCanvas
    {
        internal int VAO;
        internal int VBO;
        internal string Text;
        internal int Length;
        internal Vector2 Position = Vector2.Zero;
        internal float Scale = 1;
        
        internal TextCanvas (int vao, int vbo)
        {
            VAO = vao;
            VBO = vbo;
        }
    }
}
