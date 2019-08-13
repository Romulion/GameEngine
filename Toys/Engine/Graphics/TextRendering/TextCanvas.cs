using OpenTK;

namespace Toys
{
    internal class TextCanvas
    {
        internal int VAO;
        internal int VBO;
        internal string text;
        internal int length;
        internal Vector2 pos = Vector2.Zero;
        internal float scale = 1;
        
        internal TextCanvas (int vao, int vbo)
        {
            VAO = vao;
            VBO = vbo;
        }
    }
}
