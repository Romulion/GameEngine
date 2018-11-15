using System;
using OpenTK;
using System.Collections;
using System.Collections.Generic;

namespace Toys
{
	public class Bone
	{
		public readonly string Name;
		public readonly string NameEng;

		//common transform data
		public readonly Vector3 Position;
		public readonly int ParentIndex;
		public int Index;
		public int[] childs;
		public Matrix4 localSpace;

		//unused values
		public int Layer;

		//flags
		public bool tail;
		public bool Rotatable;
		public bool Translatable;
		public bool IsVisible;
		public bool Enabled;
		public bool IK;
		public bool InheritRotation;
		public bool InheritTranslation;
		public bool FixedAxis;
		public bool LocalCoordinate;
		public bool PhysicsAdeform;
		public bool ExternalPdeform;

		//
		public int ParentInheritIndex;
		public float ParentInfluence;

		public Bone(string name, string engName, Vector3 position, int parent, byte[] flags)
		{
			Name = name;
			NameEng = engName;
			Position = position;
			ParentIndex = parent;
			SetFlags(flags);
		}

		public Bone(string name, Matrix4 pos, int parent)
		{
			localSpace = pos;
			Name = name;
			ParentIndex = parent;
		}

		void SetFlags(byte[] bytes)
		{
			BitArray flags = new BitArray(bytes);

			tail = flags[0];
			Rotatable = flags[1];
			Translatable = flags[2];
			IsVisible = flags[3];
			Enabled = flags[4];
			IK = flags[5];
			InheritRotation = flags[8];
			InheritTranslation = flags[9];
			FixedAxis = flags[10];
			LocalCoordinate = flags[11];
			PhysicsAdeform = flags[12];
			ExternalPdeform = flags[13];
			
		}

		//configuring child parent relations
		public static void MakeChilds(Bone[] bones)
		{
			GetChilds(bones,0);
		}

		//exploring child trees
		static int[] GetChilds(Bone[] bones,  int Id)
		{
			Vector3 pos = bones[Id].Position;
			List<int> childs = new List<int>();
			bones[Id].Index = Id;
			for ( int i = 0; i<bones.Length ; i++ )
			{
				if (bones[i].ParentIndex == Id)
				{
					//bones[i].localSpace =CreateLocalSpace(bones[i].Position, pos);
					childs.Add(i);
					childs.AddRange(GetChilds(bones, i));
				}
			}
			bones[Id].childs = childs.ToArray();
			return bones[Id].childs;
		}

		//creating local space
		static Matrix4 CreateLocalSpace(Vector3 pos, Vector3 parent)
		{
			Vector3 X = (parent - pos).Normalized();
			Vector3 Z = Vector3.Cross(X, new Vector3(0f,1f,0f));
			Vector3 Y = Vector3.Cross(Z, X);
			Matrix3 mat = new Matrix3(X,Y,Z);
			return new Matrix4(mat);
		}

		/*
		void setInverseAnimateTransform(Matrix4 parentBindTransform)
		{
			Matrix4 bindTransform = Matrix4.Mult(parentBindTransform, localBindTransform);
			inverseBindTransform = Matrix4.Invert(bindTransform);
			foreach (Bone child in childs)
				child.setInverseAnimateTransform(bindTransform);
		}
*/
	}
}
