﻿using System;
using BulletSharp;
using BulletSharp.Math;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;

namespace Toys
{
	/*
	 * Code taken from BulletSharp demo framework sources
	 * 
	 * 
	 */

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionedNormal
	{
		public Vector3 Position;

		public PositionedNormal(Vector3 pos)
		{
			Position = pos;
		}

		public PositionedNormal(ref Vector3 pos)
		{
			Position = pos;
		}	}

	public static class PrimitiveMeshFactory
	{
		public static Vector3[] CreateShape(CollisionShape shape, out uint[] indices)
		{
			switch (shape.ShapeType)
			{
				case BroadphaseNativeType.BoxShape:
					indices = null;
					return CreateBox(shape as BoxShape);
				case BroadphaseNativeType.Box2DShape:
					indices = null;
					return CreateBox2DShape(shape as Box2DShape);
				case BroadphaseNativeType.CapsuleShape:
					return CreateCapsule(shape as CapsuleShape, out indices);
				case BroadphaseNativeType.Convex2DShape:
					return CreateShape((shape as Convex2DShape).ChildShape, out indices);
				case BroadphaseNativeType.ConvexHullShape:
					indices = null;
					return CreateConvexHull(shape as ConvexHullShape);
				case BroadphaseNativeType.ConeShape:
					return CreateCone(shape as ConeShape, out indices);
				case BroadphaseNativeType.CylinderShape:
					return CreateCylinder(shape as CylinderShape, out indices);
				case BroadphaseNativeType.GImpactShape:
					indices = null;
					return CreateTriangleMesh((shape as GImpactMeshShape).MeshInterface);
				case BroadphaseNativeType.MultiSphereShape:
					return CreateMultiSphere(shape as MultiSphereShape, out indices);
				case BroadphaseNativeType.SphereShape:
					return CreateSphere(shape as SphereShape, out indices);
				case BroadphaseNativeType.StaticPlaneShape:
					return CreateStaticPlane(shape as StaticPlaneShape, out indices);
				case BroadphaseNativeType.TerrainShape:
					return CreateHeightFieldTerrainShape(shape as HeightfieldTerrainShape, out indices);
				case BroadphaseNativeType.TriangleMeshShape:
					indices = null;
					return CreateTriangleMesh((shape as TriangleMeshShape).MeshInterface);
				case BroadphaseNativeType.UniformScalingShape:
					indices = null;
					return CreateUniformScalingShape(shape as UniformScalingShape, out indices);
				case BroadphaseNativeType.ScaledTriangleMeshShape:
					indices = null;
					return CreateScaledBvhTriangleMeshShape(shape as ScaledBvhTriangleMeshShape, out indices);

			}
			if (shape is PolyhedralConvexShape)
			{
				return CreatePolyhedralConvexShape((shape as PolyhedralConvexShape), out indices);
			}
			throw new NotImplementedException();
		}

		public static Vector3[] CreateScaledBvhTriangleMeshShape(ScaledBvhTriangleMeshShape shape, out uint[] indices)
		{
			Vector3[] vert = CreateShape(shape.ChildShape, out indices);
			var scale = shape.LocalScaling;

			return ScaleShape(vert, ref scale);
		}

		public static Vector3[] CreateUniformScalingShape(UniformScalingShape shape, out uint[] indices)
		{
			Vector3[] vert = CreateShape(shape.ChildShape, out indices);
			var scale = new Vector3(shape.UniformScalingFactor);

			return ScaleShape(vert, ref scale);
		}

		public static Vector3[] ScaleShape(Vector3[] shape, ref Vector3 scaling)
		{
			for (int i = 0; i < shape.Length; i++)
			{
				shape[i] *= scaling;
			}
			return shape;
		}

		public static ushort[] CompactIndexBuffer(uint[] indices)
		{
			if (indices.Length > 65535)
			{
				throw new ArgumentOutOfRangeException(nameof(indices));
			}
			var ib = new ushort[indices.Length];
			for (int i = 0; i < ib.Length; i++)
			{
				ib[i] = (ushort)indices[i];
			}
			return ib;
		}

		public static Vector3[] CreateBox(BoxShape shape)
		{
			Vector3 size = shape.HalfExtentsWithMargin;
			Vector3[] vertices = new Vector3[36 * 2];
			Vector3 normal;
			int v = 0;

			for (int j = 0; j < 3; j++)
			{
				for (int i = 1; i != -3; i -= 2)
				{
					normal = RotateYAxisUp(0, i, 0, j);
					vertices[v++] = RotateYAxisUp(i, i, i, j) * size;
					vertices[v++] = normal;
					vertices[v++] = RotateYAxisUp(1, i, -1, j) * size;
					vertices[v++] = normal;
					vertices[v++] = RotateYAxisUp(-1, i, 1, j) * size;
					vertices[v++] = normal;
					vertices[v++] = RotateYAxisUp(-i, i, -i, j) * size;
					vertices[v++] = normal;
					vertices[v++] = RotateYAxisUp(-1, i, 1, j) * size;
					vertices[v++] = normal;
					vertices[v++] = RotateYAxisUp(1, i, -1, j) * size;
					vertices[v++] = normal;
				}
			}

			return vertices;
		}

		private static Vector3[] CreateBox2DShape(Box2DShape box2DShape)
		{
			Vector3Array v = box2DShape.Vertices;
			return new Vector3[12]
			{
				v[0], Vector3.UnitZ,
				v[1], Vector3.UnitZ,
				v[2], Vector3.UnitZ,
				v[0], Vector3.UnitZ,
				v[2], Vector3.UnitZ,
				v[3], Vector3.UnitZ,
			};
		}

		public static Vector3[] CreateCapsule(CapsuleShape shape, out uint[] indices)
		{
			int up = shape.UpAxis;
			float radius = shape.Radius;
			float cylinderHalfHeight = shape.HalfHeight;

			int slices = (int)(radius * 5.0f);
			int stacks = (int)(radius * 5.0f);
			slices = 2 * MinMax(slices, 3, 16);
			stacks = 2 * MinMax(stacks, 2, 16) + 1;

			int vertexCount = 2 + slices * (stacks - 1);
			int indexCount = 6 * slices * (stacks - 1);

			var vertices = new Vector3[vertexCount * 2];
			indices = new uint[indexCount];


			// Vertices
			// Top and bottom
			const int topVertexIndex = 0;
			const int bottomVertexIndex = 1;
			float apex = cylinderHalfHeight + radius;

			vertices[0] = RotateYAxisUp(0, -apex, 0, up);
			vertices[1] = RotateYAxisUp(-Vector3.UnitY, up);
			vertices[2] = RotateYAxisUp(0, apex, 0, up);
			vertices[3] = RotateYAxisUp(Vector3.UnitY, up);

			// Stacks
			int v = 4;
			float hAngle = 0;
			float vAngle = -(float)Math.PI / 2;
			float hAngleStep = (float)Math.PI * 2 / slices;
			float vAngleStep = (float)Math.PI / stacks;
			Vector3 cylinderOffset = RotateYAxisUp(0, -cylinderHalfHeight, 0, up);
			for (int j = 0; j < stacks - 1; j++)
			{
				float prevAngle = vAngle;
				vAngle += vAngleStep;

				if (vAngle > 0 && prevAngle <= 0)
				{
					cylinderOffset = -cylinderOffset;
				}

				for (int k = 0; k < slices; k++)
				{
					hAngle += hAngleStep;

					Vector3 sphereVertex = RotateYAxisUp(
						(float)Math.Cos(vAngle) * (float)Math.Sin(hAngle),
						(float)Math.Sin(vAngle),
						(float)Math.Cos(vAngle) * (float)Math.Cos(hAngle),
						up);
					vertices[v++] = sphereVertex * radius + cylinderOffset;
					vertices[v++] = Vector3.Normalize(sphereVertex);
				}
			}


			// Indices
			// Top cap
			int i = 0;
			uint index = 2;
			for (int k = 0; k < slices; k++)
			{
				indices[i++] = index++;
				indices[i++] = topVertexIndex;
				indices[i++] = index;
			}
			indices[i - 1] = 2;

			// Stacks
			int sliceDiff = slices * 3;
			for (int j = 0; j < stacks - 2; j++)
			{
				for (int k = 0; k < slices; k++)
				{
					indices[i] = indices[i - sliceDiff + 2];
					indices[i + 1] = index++;
					indices[i + 2] = indices[i - sliceDiff];
					i += 3;
				}

				for (int k = 0; k < slices; k++)
				{
					indices[i] = indices[i - sliceDiff + 1];
					indices[i + 1] = indices[i - sliceDiff];
					indices[i + 2] = indices[i - sliceDiff + 4];
					i += 3;
				}
				indices[i - 1] = indices[i - sliceDiff];
			}

			// Bottom cap
			index--;
			for (int k = 0; k < slices; k++)
			{
				indices[i++] = index--;
				indices[i++] = bottomVertexIndex;
				indices[i++] = index;
			}
			indices[i - 1] = indices[i - sliceDiff];

			return vertices;
		}

		private static Vector3 RotateYAxisUp(Vector3 vector, int upAxis)
		{
			switch (upAxis)
			{
				case 0:
					return new Vector3(vector.Y, vector.Z, vector.X);
				case 1:
					return vector;
				default:
					return new Vector3(vector.Z, vector.X, vector.Y);
			}
		}

		private static Vector3 RotateYAxisUp(float x, float y, float z, int upAxis)
		{
			switch (upAxis)
			{
				case 0:
					return new Vector3(y, z, x);
				case 1:
					return new Vector3(x, y, z);
				default:
					return new Vector3(z, x, y);
			}
		}

		public static Vector3[] CreateCone(ConeShape shape, out uint[] indices)
		{
			int up = shape.ConeUpIndex;
			float radius = shape.Radius;
			float halfHeight = shape.Height / 2 + shape.Margin;

			const int numSteps = 10;
			const float angleStep = (2 * (float)Math.PI) / numSteps;

			const int vertexCount = 2 + 6 * numSteps;
			const int indexCount = (4 * numSteps + 2) * 3;

			var vertices = new Vector3[vertexCount * 2];
			indices = new uint[indexCount];

			int i = 0, v = 0;
			uint index = 0;
			uint baseIndex;
			Vector3 normal;

			// Base
			normal = RotateYAxisUp(-Vector3.UnitY, up);

			baseIndex = index;
			vertices[v++] = RotateYAxisUp(0, -halfHeight, 0, up);
			vertices[v++] = normal;

			vertices[v++] = RotateYAxisUp(0, -halfHeight, radius, up);
			vertices[v++] = normal;
			index += 2;

			for (int j = 1; j < numSteps; j++)
			{
				float x = radius * (float)Math.Sin(j * angleStep);
				float z = radius * (float)Math.Cos(j * angleStep);

				vertices[v++] = RotateYAxisUp(x, -halfHeight, z, up);
				vertices[v++] = normal;

				indices[i++] = baseIndex;
				indices[i++] = index;
				indices[i++] = index - 1;
				index++;
			}
			indices[i++] = baseIndex;
			indices[i++] = baseIndex + 1;
			indices[i++] = index - 1;


			// Side
			normal = RotateYAxisUp(0, 0, radius, up);
			normal.Normalize();

			baseIndex = index;
			vertices[v++] = RotateYAxisUp(0, halfHeight, 0, up);
			vertices[v++] = normal;

			vertices[v++] = RotateYAxisUp(0, -halfHeight, radius, up);
			vertices[v++] = normal;
			index += 2;

			for (int j = 1; j < numSteps + 1; j++)
			{
				float x = radius * (float)Math.Sin(j * angleStep);
				float z = radius * (float)Math.Cos(j * angleStep);

				normal = RotateYAxisUp(x, 0, z, up);
				normal.Normalize();

				vertices[v++] = RotateYAxisUp(0, halfHeight, 0, up);
				vertices[v++] = normal;

				vertices[v++] = RotateYAxisUp(x, -halfHeight, z, up);
				vertices[v++] = normal;

				indices[i++] = index - 2;
				indices[i++] = index - 1;
				indices[i++] = index;
				indices[i++] = index;
				indices[i++] = index - 1;
				indices[i++] = index + 1;
				index += 2;
			}
			indices[i++] = index - 2;
			indices[i++] = index - 1;
			indices[i++] = baseIndex;
			indices[i++] = baseIndex;
			indices[i++] = index - 1;
			indices[i] = baseIndex + 1;

			return vertices;
		}

		public static Vector3[] CreateCylinder(CylinderShape shape, out uint[] indices)
		{
			int up = shape.UpAxis;
			float radius = shape.Radius;
			float halfHeight = shape.HalfExtentsWithoutMargin[up] + shape.Margin;

			const int numSteps = 10;
			const float angleStep = (2 * (float)Math.PI) / numSteps;

			const int vertexCount = 2 + 6 * numSteps;
			const int indexCount = (4 * numSteps + 2) * 3;

			var vertices = new Vector3[vertexCount * 2];
			indices = new uint[indexCount];

			int i = 0, v = 0;
			uint index = 0;
			uint baseIndex;
			Vector3 normal;

			// Draw two sides
			for (int side = 1; side != -3; side -= 2)
			{
				normal = RotateYAxisUp(side * Vector3.UnitY, up);

				baseIndex = index;
				vertices[v++] = RotateYAxisUp(0, side * halfHeight, 0, up);
				vertices[v++] = normal;

				vertices[v++] = RotateYAxisUp(0, side * halfHeight, radius, up);
				vertices[v++] = normal;
				index += 2;

				for (int j = 1; j < numSteps; j++)
				{
					float x = radius * (float)Math.Sin(j * angleStep);
					float z = radius * (float)Math.Cos(j * angleStep);

					vertices[v++] = RotateYAxisUp(x, side * halfHeight, z, up);
					vertices[v++] = normal;

					indices[i++] = baseIndex;
					if (side == 1)
					{
						indices[i++] = index - 1;
						indices[i++] = index;
					}
					else
					{
						indices[i++] = index;
						indices[i++] = index - 1;
					}
					index++;
				}
				indices[i++] = baseIndex;
				if (side == 1)
				{
					indices[i++] = index - 1;
					indices[i++] = baseIndex + 1;
				}
				else
				{
					indices[i++] = baseIndex + 1;
					indices[i++] = index - 1;
				}
			}


			normal = RotateYAxisUp(0, 0, radius, up);
			normal.Normalize();

			baseIndex = index;
			vertices[v++] = RotateYAxisUp(0, halfHeight, radius, up);
			vertices[v++] = normal;

			vertices[v++] = RotateYAxisUp(0, -halfHeight, radius, up);
			vertices[v++] = normal;
			index += 2;

			for (int j = 1; j < numSteps + 1; j++)
			{
				float x = radius * (float)Math.Sin(j * angleStep);
				float z = radius * (float)Math.Cos(j * angleStep);

				normal = RotateYAxisUp(x, 0, z, up);
				normal.Normalize();

				vertices[v++] = RotateYAxisUp(x, halfHeight, z, up);
				vertices[v++] = normal;

				vertices[v++] = RotateYAxisUp(x, -halfHeight, z, up);
				vertices[v++] = normal;

				indices[i++] = index - 2;
				indices[i++] = index - 1;
				indices[i++] = index;
				indices[i++] = index;
				indices[i++] = index - 1;
				indices[i++] = index + 1;
				index += 2;
			}
			indices[i++] = index - 2;
			indices[i++] = index - 1;
			indices[i++] = baseIndex;
			indices[i++] = baseIndex;
			indices[i++] = index - 1;
			indices[i] = baseIndex + 1;

			return vertices;
		}

		public static Vector3[] CreateConvexHull(ConvexHullShape shape)
		{
			var hull = new ShapeHull(shape);
			hull.BuildHull(shape.Margin);

			int vertexCount = hull.NumIndices;
			UIntArray indices = hull.Indices;
			Vector3Array points = hull.Vertices;

			var vertices = new Vector3[vertexCount * 2];

			int v = 0;
			for (int i = 0; i < vertexCount; i += 3)
			{
				Vector3 v0 = points[(int)indices[i]];
				Vector3 v1 = points[(int)indices[i + 1]];
				Vector3 v2 = points[(int)indices[i + 2]];

				Vector3 v01 = v0 - v1;
				Vector3 v02 = v0 - v2;
				Vector3 normal = Vector3.Cross(v01, v02);
				normal.Normalize();

				vertices[v++] = v0;
				vertices[v++] = normal;
				vertices[v++] = v1;
				vertices[v++] = normal;
				vertices[v++] = v2;
				vertices[v++] = normal;
			}

			hull.Dispose();
			return vertices;
		}

		public static Vector3[] CreateMultiSphere(MultiSphereShape shape, out uint[] indices)
		{
			var allVertices = new List<Vector3[]>();
			var allIndices = new List<uint[]>();
			int vertexCount = 0;
			int indexCount = 0;

			for (int i = 0; i < shape.SphereCount; i++)
			{
				uint[] sphereIndices;
				Vector3[] sphereVertices = CreateSphere(shape.GetSphereRadius(i), out sphereIndices);

				// Adjust sphere position
				Vector3 position = shape.GetSpherePosition(i);
				for (int j = 0; j < sphereVertices.Length; j += 2)
				{
					sphereVertices[j] += position;
				}

				// Adjust indices
				if (indexCount != 0)
				{
					int indexOffset = vertexCount / 2;
					for (int j = 0; j < sphereIndices.Length; j++)
					{
						sphereIndices[j] += (uint)indexOffset;
					}
				}

				allVertices.Add(sphereVertices);
				allIndices.Add(sphereIndices);
				vertexCount += sphereVertices.Length;
				indexCount += sphereIndices.Length;
			}

			Vector3[] finalVertices = new Vector3[vertexCount];
			int vo = 0;
			foreach (Vector3[] v in allVertices)
			{
				v.CopyTo(finalVertices, vo);
				vo += v.Length;
			}

			indices = new uint[indexCount];
			int io = 0;
			foreach (uint[] ind in allIndices)
			{
				ind.CopyTo(indices, io);
				io += ind.Length;
			}

			return finalVertices;
		}

		private static Vector3[] CreatePolyhedralConvexShape(PolyhedralConvexShape polyhedralConvexShape, out uint[] indices)
		{
			int numVertices = polyhedralConvexShape.NumVertices;
			Vector3[] vertices = new Vector3[numVertices * 3];
			for (int i = 0; i < numVertices; i += 4)
			{
				Vector3 v0, v1, v2, v3;
				polyhedralConvexShape.GetVertex(i, out v0);
				polyhedralConvexShape.GetVertex(i + 1, out v1);
				polyhedralConvexShape.GetVertex(i + 2, out v2);
				polyhedralConvexShape.GetVertex(i + 3, out v3);

				Vector3 v01 = v0 - v1;
				Vector3 v02 = v0 - v2;
				Vector3 normal = Vector3.Cross(v01, v02);

				int i3 = i * 3;
				vertices[i3] = v0;
				vertices[i3 + 1] = normal;
				vertices[i3 + 2] = v1;
				vertices[i3 + 3] = normal;
				vertices[i3 + 4] = v2;
				vertices[i3 + 5] = normal;
				vertices[i3 + 6] = v0;
				vertices[i3 + 7] = normal;
				vertices[i3 + 8] = v2;
				vertices[i3 + 9] = normal;
				vertices[i3 + 10] = v3;
			}
			indices = null;
			return vertices;
		}

		public static Vector3[] CreateSphere(SphereShape shape, out uint[] indices)
		{
			return CreateSphere(shape.Radius, out indices);
		}

		private static Vector3[] CreateSphere(float radius, out uint[] indices)
		{
			int slices = (int)(radius * 10.0f);
			int stacks = (int)(radius * 10.0f);
			slices = MinMax(slices, 6, 16);
			stacks = MinMax(stacks, 6, 16);

			int vertexCount = 2 + slices * (stacks - 1);
			int indexCount = 6 * slices * (stacks - 1);

			var vertices = new Vector3[vertexCount * 2];
			indices = new uint[indexCount];


			// Vertices
			// Top and bottom
			int v = 0;
			const int topVertexIndex = 0;
			const int bottomVertexIndex = 1;

			vertices[v++] = new Vector3(0, -radius, 0);
			vertices[v++] = -Vector3.UnitY;
			vertices[v++] = new Vector3(0, radius, 0);
			vertices[v++] = Vector3.UnitY;

			// Stacks
			float vAngle = -(float)Math.PI / 2;
			float horAngleStep = (float)Math.PI * 2 / slices;
			float vertAngleStep = (float)Math.PI / stacks;
			for (int j = 0; j < stacks - 1; j++)
			{
				vAngle += vertAngleStep;

				for (int k = 0; k < slices; k++)
				{
					float angle = k * horAngleStep;

					var vertex = new Vector3(
						(float)Math.Cos(vAngle) * (float)Math.Sin(angle),
						(float)Math.Sin(vAngle),
						(float)Math.Cos(vAngle) * (float)Math.Cos(angle));
					vertices[v++] = vertex * radius;
					vertices[v++] = Vector3.Normalize(vertex);
				}
			}

			// Indices
			// Top cap
			int i = 0;
			ushort index = 2;
			for (int k = 0; k < slices; k++)
			{
				indices[i++] = index++;
				indices[i++] = topVertexIndex;
				indices[i++] = index;
			}
			indices[i - 1] = 2;

			// Stacks
			int sliceDiff = slices * 3;
			for (int j = 0; j < stacks - 2; j++)
			{
				for (int k = 0; k < slices; k++)
				{
					indices[i] = indices[i - sliceDiff + 2];
					indices[i + 1] = index++;
					indices[i + 2] = indices[i - sliceDiff];
					i += 3;
				}

				for (int k = 0; k < slices; k++)
				{
					indices[i] = indices[i - sliceDiff + 1];
					indices[i + 1] = indices[i - sliceDiff];
					indices[i + 2] = indices[i - sliceDiff + 4];
					i += 3;
				}
				indices[i - 1] = indices[i - sliceDiff];
			}

			// Bottom cap
			index--;
			for (int k = 0; k < slices; k++)
			{
				indices[i++] = index--;
				indices[i++] = bottomVertexIndex;
				indices[i++] = index;
			}
			indices[i - 1] = indices[i - sliceDiff];

			return vertices;
		}

		private static int MinMax(int value, int min, int max)
		{
			return Math.Min(Math.Max(value, min), max);
		}

		private static void PlaneSpace1(Vector3 n, out Vector3 p, out Vector3 q)
		{
			if (Math.Abs(n[2]) > (Math.Sqrt(2) / 2))
			{
				// choose p in y-z plane
				float a = n[1] * n[1] + n[2] * n[2];
				float k = 1.0f / (float)Math.Sqrt(a);
				p = new Vector3(0, -n[2] * k, n[1] * k);
				// set q = n x p
				q = Vector3.Cross(n, p);
			}
			else
			{
				// choose p in x-y plane
				float a = n[0] * n[0] + n[1] * n[1];
				float k = 1.0f / (float)Math.Sqrt(a);
				p = new Vector3(-n[1] * k, n[0] * k, 0);
				// set q = n x p
				q = Vector3.Cross(n, p);
			}
		}

		public static Vector3[] CreateStaticPlane(StaticPlaneShape shape, out uint[] indices)
		{
			Vector3 planeOrigin = shape.PlaneNormal * shape.PlaneConstant;
			Vector3 vec0, vec1;
			PlaneSpace1(shape.PlaneNormal, out vec0, out vec1);
			const float size = 1000;

			indices = new uint[] { 0, 2, 1, 0, 1, 3 };

			return new Vector3[]
			{
				planeOrigin + vec0*size,
				shape.PlaneNormal,
				planeOrigin - vec0*size,
				shape.PlaneNormal,
				planeOrigin + vec1*size,
				shape.PlaneNormal,
				planeOrigin - vec1*size,
				shape.PlaneNormal
			};
		}

		private static Vector3[] CreateTriangleMesh(StridingMeshInterface meshInterface)
		{
			// StridingMeshInterface can only be TriangleIndexVertexArray
			var meshes = (meshInterface as TriangleIndexVertexArray).IndexedMeshArray;
			int numTriangles = 0;
			foreach (var mesh in meshes)
			{
				numTriangles += mesh.NumTriangles;
			}
			int numVertices = numTriangles * 3;
			Vector3[] vertices = new Vector3[numVertices * 2];

			int v = 0;
			for (int part = 0; part < meshInterface.NumSubParts; part++)
			{
				var mesh = meshes[part];

				var indexStream = mesh.GetTriangleStream();
				var vertexStream = mesh.GetVertexStream();
				var indexReader = new BinaryReader(indexStream);
				var vertexReader = new BinaryReader(vertexStream);

				int vertexStride = mesh.VertexStride;
				int triangleStrideDelta = mesh.TriangleIndexStride - 3 * sizeof(int);
				PhyScalarType vertexType = mesh.VertexType;

				while (indexStream.Position < indexStream.Length)
				{
					uint i = indexReader.ReadUInt32();
					vertexStream.Position = vertexStride * i;
					float f1 = ReadFloat(vertexReader, vertexType);
					float f2 = ReadFloat(vertexReader, vertexType);
					float f3 = ReadFloat(vertexReader, vertexType);
					Vector3 v0 = new Vector3(f1, f2, f3);
					i = indexReader.ReadUInt32();
					vertexStream.Position = vertexStride * i;
					f1 = vertexReader.ReadSingle();
					f2 = vertexReader.ReadSingle();
					f3 = vertexReader.ReadSingle();
					Vector3 v1 = new Vector3(f1, f2, f3);
					i = indexReader.ReadUInt32();
					vertexStream.Position = vertexStride * i;
					f1 = ReadFloat(vertexReader, vertexType);
					f2 = ReadFloat(vertexReader, vertexType);
					f3 = ReadFloat(vertexReader, vertexType);
					Vector3 v2 = new Vector3(f1, f2, f3);

					Vector3 v01 = v0 - v1;
					Vector3 v02 = v0 - v2;
					Vector3 normal = Vector3.Cross(v01, v02);
					normal.Normalize();

					var scaling = meshInterface.Scaling;

					vertices[v++] = v0 * scaling;
					vertices[v++] = normal * scaling;
					vertices[v++] = v1 * scaling;
					vertices[v++] = normal * scaling;
					vertices[v++] = v2 * scaling;
					vertices[v++] = normal * scaling;

					indexStream.Position += triangleStrideDelta;
				}

				indexStream.Dispose();
				vertexStream.Dispose();
			}

			return vertices;
		}

		private static float ReadFloat(BinaryReader vertexReader, PhyScalarType vertexType)
		{
			if (vertexType == PhyScalarType.Double)
			{
				return (float)vertexReader.ReadDouble();
			}
			return vertexReader.ReadSingle();
		}

		private static Vector3[] CreateHeightFieldTerrainShape(HeightfieldTerrainShape heightfieldTerrainShape, out uint[] indices)
		{
			// HeightfieldTerrainShape does not expose its data
			indices = null;
			return new Vector3[1];
		}

	}
}
