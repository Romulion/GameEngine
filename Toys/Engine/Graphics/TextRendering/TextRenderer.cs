using System;
using SharpFont;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Toys
{
	internal class TextRenderer
	{
		Library lib;
		Face face;
		string font = "ARIALUNI.TTF";
		Dictionary<char, Character> chars = new Dictionary<char, Character>();
		Matrix4 projection;

		Shader shdr;

		int VAO, VBO;

		internal TextRenderer()
		{
			lib = new Library();

			string defPath = "Toys.Resourses.Fonts.";
			var assembly = IntrospectionExtensions.GetTypeInfo(typeof(ShaderManager)).Assembly;

			face = new Face(lib, ReadFont(assembly.GetManifestResourceStream(defPath + font)), 0);
			face.SetPixelSizes(0, 48);
			LoadCharacters();

			//projection = Matrix4.CreateOrthographic(800, 600, 1f, -1f);
			//projection = Matrix4.CreateOrthographic(800, 600, 0f, -0.01f);
			projection = Matrix4.CreateOrthographicOffCenter(0, 800, 0, 600, 0f, -0.01f);
			ShaderManager shdmMgmt = ShaderManager.GetInstance;
			shdmMgmt.LoadShader("text");
			shdr = shdmMgmt.GetShader("text");
			shdr.ApplyShader();
			shdr.SetUniform((int)TextureType.Diffuse, "text");

			shdr.uniforms[0].SetValue(projection);
			foreach (var un in shdr.uniforms)
				Console.WriteLine(un.Name);
			CreateCanvas();
			var vec = new Vector4(25, 25, 0, 1);
			Console.WriteLine(projection * vec);

			Console.WriteLine(vec * projection);
		}

		void LoadCharacters()
		{
			for (char c = (char)0; c < 128; c++)
			{
				
				face.LoadChar(c, LoadFlags.Render, LoadTarget.Light);


				FTBitmap bitmap = null;
				Texture tex = null;
				//Console.WriteLine(c);

				bitmap = face.Glyph.Bitmap;

				/*
					for (int i = 0; i < bitmap.Width * bitmap.Rows; i += 1)
					{
						Console.WriteLine(c);
						Console.WriteLine("{0} : {1} {2}", i, bitmap.Width, bitmap.Rows);
						Marshal.ReadByte(bitmap.Buffer, i);
					}
*/
				tex = Texture.CreateChar(c, bitmap.Buffer, bitmap.Width, bitmap.Rows);
				Character ch = new Character(tex,
										 new Vector2(bitmap.Width, bitmap.Rows),
										 new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
										 face.Glyph.Advance.X.Value);
				chars.Add(c, ch);
				bitmap.Dispose();
			}
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

		void CreateCanvas()
		{
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();

			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, 4 * 6 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * 4, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
		}

		internal void RenderText(string text, int x, int y, float scale)
		{
			var color = Vector3.One;
			shdr.ApplyShader();
			shdr.uniforms[1].SetValue(color);
			//shdr.SetUniform(projection, "projection");
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindVertexArray(VAO);
			//Console.WriteLine( GL.GetError());
			foreach (var c in text)
			{
				if (!chars.ContainsKey(c))
					continue;
				
				var chr = chars[c];
				float xpos = x + chr.Bearing.X * scale;
				float ypos = y - (chr.Size.Y - chr.Bearing.Y) * scale;
				float w = chr.Size.X * scale;
				float h = chr.Size.Y * scale;

				float[,] vertices = {
					{ xpos,    ypos + h,   0.0f, 0.0f},
					{ xpos,     ypos,       0.0f, 1.0f },
					{ xpos + w, ypos,       1.0f, 1.0f },
					{ xpos,     ypos + h,   0.0f, 0.0f },
					{ xpos + w, ypos,       1.0f, 1.0f },
					{ xpos + w, ypos + h,   1.0f, 0.0f }
				};

				chr.texture.BindTexture();
				GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, 6 * 4 * 4, vertices);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
				x += (int)((chr.Advance >> 6) * scale);
				//break;
			}

			GL.BindVertexArray(0);
		}

		internal void Unload()
		{
			lib.Dispose();
			face.Dispose();
		}
	}
}
