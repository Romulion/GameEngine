//#define debugTextmap
using System;
using SharpFont;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
	internal class TextRenderer
	{
		Library lib;
		Face face;
		string font = "font.ttf";
		Dictionary<char, Character> chars = new Dictionary<char, Character>();
		Matrix4 projection;
        static int mapSize = 1024;
        Texture2D charmap;
		Shader shdr;
        Vector3 position = Vector3.Zero;
        int x, y, ymax;

        List<TextCanvas> texts = new List<TextCanvas>();
        internal List<TextBox> textBoxes = new List<TextBox>();

        ShaderUniform projUniform;
        ShaderUniform colorUniform;
        ShaderUniform posUniform;

        int ScreenWidth, ScreenHeigth;

        internal TextRenderer()
        {
            lib = new Library();

            try
            {
                using (Stream file = File.OpenRead(font))
                {
                    face = new Face(lib, ReadFont(file), 0);
                }
            }
            catch (Exception)
            {
                string defPath = "Toys.Resourses.Fonts.reddelicious.ttf";
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(ShaderManager)).Assembly;
                face = new Face(lib, ReadFont(assembly.GetManifestResourceStream(defPath)), 0);
            }

            face.SetPixelSizes(0, 30);
            projection = Matrix4.CreateOrthographicOffCenter(0, 800, 0, 600, 0f, -0.01f);
            ShaderManager shdmMgmt = ShaderManager.GetInstance;
            shdmMgmt.LoadShader("text");
            shdr = shdmMgmt.GetShader("text");
            shdr.ApplyShader();
            shdr.SetUniform((int)TextureType.Diffuse, "text");

            foreach (var uniform in shdr.GetUniforms)
            {
                if (uniform.Name == "projection")
                    projUniform = uniform;
                else if (uniform.Name == "position_scale")
                    posUniform = uniform;
                else if (uniform.Name == "textColor")
                    colorUniform = uniform;
            }
            projUniform.SetValue(projection);
            charmap = Texture2D.CreateCharMap(mapSize, mapSize);
        }


        Character GetCharacter(char c)
        {
            if (chars.ContainsKey(c))
                return chars[c];

            //rendering character
            face.LoadChar(c, LoadFlags.Render, LoadTarget.Light);
            FTBitmap bitmap = face.Glyph.Bitmap;

            //calculating position
            if (x + bitmap.Width > mapSize)
            {
                y += ymax;
                x = 0;
            }

            if (y + bitmap.Rows > mapSize)
                throw new Exception("out of memory string");

            if (ymax < bitmap.Rows)
                ymax = bitmap.Rows;

            charmap.AddSubImage(bitmap.Buffer, x, y, bitmap.Width, bitmap.Rows);
            Character ch = new Character(new Vector2(x/ (float)mapSize, y / (float)mapSize),
										 new Vector2(bitmap.Width, bitmap.Rows),
										 new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
										 face.Glyph.Advance.X.Value);
			chars.Add(c, ch);

            x += bitmap.Width;

            bitmap.Dispose();
            
            return ch;
        }

        internal TextCanvas CreateCanvas()
        {
            int VAO = GL.GenVertexArray();
            int VBO = GL.GenBuffer();
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * 4, 0);
            GL.BindVertexArray(0);

            var canvas = new TextCanvas(VAO, VBO);
            texts.Add(canvas);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, 1000 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            return canvas;
        }

        public void UpdateText(TextCanvas canvas)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            charmap.BindTexture();

            float[] vertices = new float[6 * 4* canvas.Text.Length];
            int x = 0;
            int y = 0;
            int i = 0;
            foreach (var c in canvas.Text)
            {
                if (c == 8381)
                    continue;

                var chr = GetCharacter(c);
                float xpos = x + chr.Bearing.X;
                float ypos = y - (chr.Size.Y - chr.Bearing.Y);
                float w = chr.Size.X;
                float h = chr.Size.Y;

                //update text size
                if (canvas.Heigth < ypos + h)
                    canvas.Heigth = ypos + h;
                if (canvas.Width < xpos + w)
                    canvas.Width = xpos + w;

                float[] verts = {
                     xpos,    ypos + h,   chr.Position.X, chr.Position.Y,
                     xpos,     ypos,      chr.Position.X, chr.Position.Y + chr.Size.Y / mapSize,
                     xpos + w, ypos,       chr.Position.X + chr.Size.X / mapSize, chr.Position.Y + chr.Size.Y / mapSize ,
                     xpos,     ypos + h,   chr.Position.X, chr.Position.Y ,
                     xpos + w, ypos,       chr.Position.X + chr.Size.X / mapSize, chr.Position.Y + chr.Size.Y / mapSize ,
                     xpos + w, ypos + h,   chr.Position.X + chr.Size.X / mapSize, chr.Position.Y 
                };

                Array.Copy(verts, 0, vertices, i, verts.Length);
                x += (int)((chr.Advance >> 6) );

                i += 24;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, canvas.VBO);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * 4, vertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            canvas.StringLength = canvas.Text.Length * 6;
        }
        
		byte[] ReadFont(Stream strm)
		{
			byte[] buffer = new byte[16 * 1024];
			using (MemoryStream ms = new MemoryStream())
			{
				int read;
				while ((read = strm.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		internal void Resize(int width, int heigth)
		{
            ScreenWidth = width;
            ScreenHeigth = heigth;
            projection = Matrix4.CreateOrthographicOffCenter(0, width, 0, heigth, 0f, -0.01f);
			projUniform.SetValue(projection);
		}

		internal void RenderText()
		{
            GL.StencilFunc(StencilFunction.Always, 0, 0xFF);
            shdr.ApplyShader();
            foreach (var text in textBoxes)
            {
                //stencil masking
                if (text.Node.MaskCheck == 0)
                    GL.StencilFunc(StencilFunction.Always, 0, 0xFF);
                else
                    GL.StencilFunc(StencilFunction.Equal, text.Node.MaskCheck, 0xFF);

                colorUniform.SetValue(text.textCanvas.colour);
                position = CalculatePosition(text.textCanvas,text.Node.GetTransform);
                position.Z = text.textCanvas.Scale;
                posUniform.SetValue(position);
                GL.ActiveTexture(TextureUnit.Texture0);
                charmap.BindTexture();
                GL.BindVertexArray(text.textCanvas.VAO);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * text.textCanvas.StringLength);
            }
            GL.BindVertexArray(0);
		}

		internal void Unload()
		{
			lib.Dispose();
			face.Dispose();
		}

        private Vector3 CalculatePosition(TextCanvas textCanvas, RectTransform transform)
        {
            Vector3 location = Vector3.UnitZ;

            switch (textCanvas.alignVertical)
            {
                case TextAlignVertical.Bottom:
                    location.Y = transform.Min.Y * 2;
                    break;
                case TextAlignVertical.Top:
                    location.Y = (transform.Max.Y - textCanvas.Heigth * textCanvas.Scale / (float)ScreenHeigth) * 2;
                    break;
                case TextAlignVertical.Center:
                    location.Y = transform.Min.Y + transform.Max.Y - textCanvas.Heigth * textCanvas.Scale / (float)ScreenHeigth;
                    break;
            }

            switch (textCanvas.alignHorizontal)
            {
                case TextAlignHorizontal.Left:
                    location.X = transform.Min.X * 2;
                    break;
                case TextAlignHorizontal.Right:
                    location.X = (transform.Max.X - textCanvas.Width * textCanvas.Scale / (float)ScreenHeigth) * 2;
                    break;
                case TextAlignHorizontal.Center:
                    location.X = transform.Min.X + transform.Max.X - textCanvas.Width * textCanvas.Scale / (float)ScreenWidth;
                    break;
            }
            return location;
        }
	}
}
