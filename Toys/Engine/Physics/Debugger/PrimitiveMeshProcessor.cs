using System;
using BulletSharp;
using System.Drawing;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Toys
{
	public struct InstanceData
	{
		public Matrix4 WorldTransform;
		public Color Color;
	}



	public class PrimitiveMeshProcessor
	{
		Dictionary<CollisionShape, PrimitiveData> shapes = new Dictionary<CollisionShape, PrimitiveData>();
		List<CollisionShape> removeList = new List<CollisionShape>();
		DynamicsWorld World;

		static Color groundColor = Color.Green;
		static Color activeColor = Color.Orange;
		static Color passiveColor = Color.Red;
		static Color softBodyColor = Color.LightBlue;
		static Color linkColor = Color.Black;

		//int worldMatrixLocation;
		int vertexPositionLocation = 0;
		//int vertexNormalLocation;
		//int vertexColorLocation;

		Shader shaderProgram;
		ShaderUniform worldS;
		ShaderUniform color;
		int VBO, VAO;

		public PrimitiveMeshProcessor(DynamicsWorld world)
		{
			World = world;
			ShaderManager.GetInstance.LoadShader("phys");
			shaderProgram = ShaderManager.GetInstance.GetShader("phys");

			//setting shader uniform references
			foreach (var uni in shaderProgram.GetUniforms)
				if (uni.Name == "world")
					worldS = uni;
				else if (uni.Name == "color")
					color = uni;

			CreateTri();
		}

		void CreateTri()
		{
			float[] vertices = {
				-0.5f, -0.5f, 0.0f,
				 0.5f, -0.5f, 0.0f,
				 0.0f,  0.5f, 0.0f
			};
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			//GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);
			//GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3,0);
			//GL.EnableVertexAttribArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			//GL.BindVertexArray(0);

			var ubm = UniformBufferManager.GetInstance;	
			var ubsp = ubm.GetBuffer("space");
			shaderProgram.SetUBO(ubsp.BufferIndex, "space");
		}

		PrimitiveData CreateShape(CollisionShape shape)
		{
			PrimitiveData shapeData = new PrimitiveData();
			uint[] indices;
			BulletSharp.Math.Vector3[] vertexBuffer = PrimitiveMeshFactory.CreateShape(shape, out indices);

			if (vertexBuffer != null)
			{
				shapeData.VertexCount = vertexBuffer.Length / 2;

				BulletSharp.Math.Vector3[] vertices = new BulletSharp.Math.Vector3[shapeData.VertexCount];
				BulletSharp.Math.Vector3[] normals = new BulletSharp.Math.Vector3[shapeData.VertexCount];

				int i;
				for (i = 0; i < shapeData.VertexCount; i++)
				{
					vertices[i] = vertexBuffer[i * 2];
					normals[i] = vertexBuffer[i * 2 + 1];
				}

				shapeData.SetVertexBuffer(vertices);
			}

			if (indices != null)
			{
				ushort[] indices_s = PrimitiveMeshFactory.CompactIndexBuffer(indices);
				if (indices_s != null)
				{
					shapeData.SetIndexBuffer(indices_s);
				}
				else
				{
					shapeData.SetIndexBuffer(indices);
				}
				shapeData.ElementCount = indices.Length;
			}

			return shapeData;
		}

		public void RemoveShape(CollisionShape shape)
		{
			if (shapes.ContainsKey(shape))
			{
				shapes[shape].Dispose();
				shapes.Remove(shape);
			}
		}

		PrimitiveData InitShapeData(CollisionShape shape)
		{
			PrimitiveData shapeData;

			if (shapes.TryGetValue(shape, out shapeData) == false)
			{
				if (shape.ShapeType == BroadphaseNativeType.SoftBodyShape)
				{
					shapeData = new PrimitiveData();
				}
				else
				{
					shapeData = CreateShape(shape);
				}

				shapes.Add(shape, shapeData);
			}

			return shapeData;
		}

		void InitRigidBodyInstance(CollisionObject colObj, CollisionShape shape, ref BulletSharp.Math.Matrix transform)
		{
			if (shape.ShapeType == BroadphaseNativeType.CompoundShape)
			{
				foreach (CompoundShapeChild child in (shape as CompoundShape).ChildList)
				{
					BulletSharp.Math.Matrix childTransform = child.Transform * transform;
					InitRigidBodyInstance(colObj, child.ChildShape, ref childTransform);
				}
			}
			else
			{
				Color clr;

				switch (shape.ShapeType)
				{
					case BroadphaseNativeType.CapsuleShape:
						clr = Color.Red;
						break;
					case BroadphaseNativeType.BoxShape:
						clr = Color.Green;
						break;
					case BroadphaseNativeType.SphereShape:
						clr = Color.Blue;
						break;
					default:
						clr = Color.Azure;
						break;
						
				}

				InitShapeData(shape).Instances.Add(new InstanceData()
				{
					WorldTransform = transform.Convert(),
					//WorldTransform = MathHelper.Convert(ref transform),
					//Color = "Ground".Equals(colObj.UserObject) ? groundColor :
					//	colObj.ActivationState == ActivationState.ActiveTag ? activeColor : passiveColor
					Color = clr
				});


			}
		}

		public void InitInstancedRender()
		{
			// Clear instance data
			foreach (PrimitiveData shapeData in shapes.Values)
			{
				shapeData.Instances.Clear();
			}

			// Gather instance data
			foreach (var colObj in World.CollisionObjectArray)
			{
				var shape = colObj.CollisionShape;

				if (shape.ShapeType == BroadphaseNativeType.SoftBodyShape)
				{
					//disable softbody
				}
				else
				{
					BulletSharp.Math.Matrix transform;
					colObj.GetWorldTransform(out transform);
					InitRigidBodyInstance(colObj, shape, ref transform);
				}
			}

			foreach (KeyValuePair<CollisionShape, PrimitiveData> shape in shapes)
			{
				PrimitiveData shapeData = shape.Value;

				if (shapeData.Instances.Count == 0)
				{
					removeList.Add(shape.Key);
				}

			}

			// Remove shapes that had no instances
			if (removeList.Count != 0)
			{
				foreach (var shape in removeList)
				{
					shapes.Remove(shape);
				}
				removeList.Clear();
			}
		}

		public void RenderInstanced()
		{
			shaderProgram.ApplyShader();

			//color.SetValue(new Vector4(1));
			//worldS.SetValue(Matrix4.Identity);

			GL.EnableVertexAttribArray(0);

			foreach (PrimitiveData shapeData in shapes.Values)
			{
				//break;
				// Normal buffer
				// Vertex buffer
				GL.BindBuffer(BufferTarget.ArrayBuffer, shapeData.VertexBufferID);
				GL.VertexAttribPointer(vertexPositionLocation, 3, VertexAttribPointerType.Float, false, BulletSharp.Math.Vector3.SizeInBytes, IntPtr.Zero);

				Matrix4 worldMatrix;

				// Index (element) buffer
				if (shapeData.ElementCount != 0)
				{
					
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, shapeData.ElementBufferID);

					foreach (InstanceData instance in shapeData.Instances)
					{
						worldMatrix = instance.WorldTransform;
						color.SetValue(ColorToVec3(instance.Color));
						worldS.SetValue(worldMatrix);
						//GL.UniformMatrix4(worldMatrixLocation, false, ref worldMatrix);
						//GL.Uniform4(vertexColorLocation, instance.Color);
						GL.DrawElements(shapeData.PrimitiveType, shapeData.ElementCount, shapeData.ElementsType, IntPtr.Zero);
					}

					GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
					}
				else
				{
					
					foreach (InstanceData instance in shapeData.Instances)
					{
						worldMatrix = instance.WorldTransform;
						color.SetValue(ColorToVec3(instance.Color));
						worldS.SetValue(worldMatrix);
						//GL.UniformMatrix4(worldMatrixLocation, false, ref worldMatrix);
						//GL.Uniform4(vertexColorLocation, instance.Color);
						GL.DrawArrays(shapeData.PrimitiveType, 0, shapeData.VertexCount);
					}
				}

			}

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.DisableVertexAttribArray(vertexPositionLocation);
		}

		Vector4 ColorToVec3(Color color)
		{
			return new Vector4((color.R / 255.0f),(color.G / 255.0f),(color.R / 255.0f),1);
		}
	}
}
