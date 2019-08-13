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
		string font = "ARIALUNI.TTF";
		Dictionary<char, Character> chars = new Dictionary<char, Character>();
		Matrix4 projection;
        static int mapSize = 1024;
        Texture charmap;
		Shader shdr;

        int x, y, ymax;

		int VAO, VBO;

        List<TextCanvas> texts = new List<TextCanvas>();
        

        internal TextRenderer()
		{
			lib = new Library();

			string defPath = "Toys.Resourses.Fonts.";
			var assembly = IntrospectionExtensions.GetTypeInfo(typeof(ShaderManager)).Assembly;

			face = new Face(lib, ReadFont(assembly.GetManifestResourceStream(defPath + font)), 0);
            face.SetPixelSizes(0, 48);

			projection = Matrix4.CreateOrthographicOffCenter(0, 800, 0, 600, 0f, -0.01f);
			ShaderManager shdmMgmt = ShaderManager.GetInstance;
			shdmMgmt.LoadShader("text");
			shdr = shdmMgmt.GetShader("text");
			shdr.ApplyShader();
			shdr.SetUniform((int)TextureType.Diffuse, "text");

			shdr.uniforms[0].SetValue(projection);
            charmap = Texture.CreateCharMap(mapSize, mapSize);
            CreateCanvas();
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

            Console.WriteLine("{0} {1} {2} {3} {4}",c, x,y, bitmap.Width, bitmap.Rows);
            x += bitmap.Width;

            bitmap.Dispose();
            
            return ch;
        }

        internal TextCanvas CreateCanvas()
        {
            int VAO = GL.GenVertexArray();
            int VBO = GL.GenBuffer();
            GL.BindVertexArray(VAO);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * 4, 0);
            GL.BindVertexArray(0);

            var canvas = new TextCanvas(VAO, VBO);
            texts.Add(canvas);

            return canvas;
        }

        public void AddText(TextCanvas canvas)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            charmap.BindTexture();
           
            float[] vertices = new float[6 * 4* canvas.text.Length];
            int x = (int)canvas.pos.X;
            int y = (int)canvas.pos.Y;
            int i = 0;
            foreach (var c in canvas.text)
            {

                var chr = GetCharacter(c);
                float xpos = x + chr.Bearing.X * canvas.scale;
                float ypos = y - (chr.Size.Y - chr.Bearing.Y) * canvas.scale;
                float w = chr.Size.X * canvas.scale;
                float h = chr.Size.Y * canvas.scale;

                float[] verts = {
                     xpos,    ypos + h,   chr.position.X, chr.position.Y,
                     xpos,     ypos,      chr.position.X, chr.position.Y + chr.Size.Y / mapSize,
                     xpos + w, ypos,       chr.position.X + chr.Size.X / mapSize, chr.position.Y + chr.Size.Y / mapSize ,
                     xpos,     ypos + h,   chr.position.X, chr.position.Y ,
                     xpos + w, ypos,       chr.position.X + chr.Size.X / mapSize, chr.position.Y + chr.Size.Y / mapSize ,
                     xpos + w, ypos + h,   chr.position.X + chr.Size.X / mapSize, chr.position.Y 
                };

                Array.Copy(verts, 0, vertices, i, 24);
                x += (int)((chr.Advance >> 6) * canvas.scale);

                i += 24;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * 4, vertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
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

		internal void Resize(int Width, int Heigth)
		{
			projection = Matrix4.CreateOrthographicOffCenter(0, Width, 0, Heigth, 0f, -0.01f);
			shdr.uniforms[0].SetValue(projection);
		}

		internal void RenderText()
		{
			var color = Vector3.One;
                   
            shdr.ApplyShader();
            shdr.uniforms[1].SetValue(color);

            GL.ActiveTexture(TextureUnit.Texture0);
            charmap.BindTexture();
            
            foreach (var t in texts)
			{
                GL.BindVertexArray(t.VAO);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * t.length);
            }
#if debugTextmap
            GL.BindVertexArray(VAO);
            float[,] vertics = {
                    { 0,    480,   0, 0},
                    { 0,     0,      0, 1},
                    { 480, 0,       1, 1},
                    { 0,    480,  0, 0},
                    { 480, 0,       1 , 1},
                    { 480, 480,   1 , 0 }
                };

            //chr.texture.BindTexture();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, 6 * 4 * 4, vertics);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
#endif

            GL.BindVertexArray(0);
		}

		internal void Unload()
		{
			lib.Dispose();
			face.Dispose();
		}
	}
}
