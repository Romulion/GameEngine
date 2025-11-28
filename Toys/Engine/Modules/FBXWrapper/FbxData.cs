/// Inspired by original code from https://github.com/RazTools/Studio/
/// 

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace FbxWrapper
{
    public class FbxDataScene
    {
        public FbxDataNode RootNode { get; set; }
        public List<FbxDataMesh> MeshList { get; set; }
        public List<FbxDataMaterial> MaterialList { get; set; }
        public List<FbxDataTexture> TextureList { get; set; }
        public List<FbxDataMorph> MorphList { get; set; }
        public List<FbxDataKeyframedAnimation> AnimationList { get; set; }
        public List<string> BonesOrdered { get; set; }
        public List<Matrix4> BonesMatrices { get; set; }
    }
    public class FbxDataNode
    {
        public string Name { get; set; }
        public Vector3 LocalRotation { get; set; }
        public Vector3 LocalPosition { get; set; }
        public Vector3 LocalScale { get; set; }
        public FbxDataNode Parent { get; set; }

        public List<FbxDataNode> Children { get; private set; }

        public FbxDataNode this[int i] => Children[i];

        public FbxDataNode GetFirstChild
        {
            get
            {
                if (Children.Count > 0)
                    return Children[0];
                else
                    return null;
            }
        }

        public int Count => Children.Count;

        public string Path
        {
            get
            {
                var frame = this;
                var path = frame.Name;
                while (frame.Parent != null)
                {
                    frame = frame.Parent;
                    path = frame.Name + "/" + path;
                }
                return path;
            }
        }

        public FbxDataNode(int childrenCount = 0)
        {
            Children = new List<FbxDataNode>(childrenCount);
        }

        public void AddChild(FbxDataNode obj)
        {
            Children.Add(obj);
            obj.Parent?.Remove(obj);
            obj.Parent = this;            
        }

        public void Remove(FbxDataNode frame)
        {
            Children.Remove(frame);
            frame.Parent = null;
        }

        public FbxDataNode FindFrameByPath(string path)
        {
            var name = path.Substring(path.LastIndexOf('/') + 1);
            foreach (var frame in FindChilds(name))
            {
                if (frame.Path.EndsWith(path, StringComparison.Ordinal))
                {
                    return frame;
                }
            }
            return null;
        }

        public FbxDataNode FindRelativeFrameWithPath(string path)
        {
            var subs = path.Split(new[] { '/' }, 2);
            foreach (var child in Children)
            {
                if (child.Name == subs[0])
                {
                    if (subs.Length == 1)
                    {
                        return child;
                    }
                    else
                    {
                        var result = child.FindRelativeFrameWithPath(subs[1]);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }

        public FbxDataNode FindFrame(string name)
        {
            if (Name == name)
            {
                return this;
            }
            foreach (var child in Children)
            {
                var frame = child.FindFrame(name);
                if (frame != null)
                {
                    return frame;
                }
            }
            return null;
        }

        public FbxDataNode FindChild(string name, bool recursive = true)
        {
            foreach (var child in Children)
            {
                if (recursive)
                {
                    var frame = child.FindFrame(name);
                    if (frame != null)
                    {
                        return frame;
                    }
                }
                else
                {
                    if (child.Name == name)
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        public IEnumerable<FbxDataNode> FindChilds(string name)
        {
            if (Name == name)
            {
                yield return this;
            }
            foreach (var child in Children)
            {
                foreach (var item in child.FindChilds(name))
                {
                    yield return item;
                }
            }
        }
    }

    public class FbxDataMesh
    {
        public string Path { get; set; }
        public List<FbxDataVertex> VertexList { get; set; }
        public List<FbxDataSubMesh> SubmeshList { get; set; }
        public List<FbxDataBone> BoneList { get; set; }
        public bool HasNormal { get; set; }
        public bool[] HasUV { get; set; }
        public int[] UVType { get; set; }
        public bool HasTangent { get; set; }
        public bool HasColor { get; set; }
    }

    public class FbxDataVertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Color4 Color { get; set; }
        public Vector4 Tangent { get; set; }
        public Vector2[] UV { get; set; }
        public float[] Weights { get; set; }
        public int[] BoneIndices { get; set; }
    }

    public class FbxDataSubMesh
    {
        public int[] Indeces { get; set; }
        public string Material { get; set; }

    }

    public class FbxDataBone
    {
        public string Path { get; set; }
        public Matrix4 Matrix { get; set; }
    }

    public class FbxDataMaterial
    {
        public string Name { get; set; }
        public Color4 Diffuse { get; set; }
        public Color4 Ambient { get; set; }
        public Color4 Specular { get; set; }
        public Color4 Emissive { get; set; }
        public Color4 Reflection { get; set; }
        public float Shininess { get; set; }
        public float Transparency { get; set; }
        public List<FbxDataTexture> Textures { get; set; }
    }

    public class FbxDataTexture
    {
        public string Name { get; set; }
        public FbxTextureType Type { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 Scale { get; set; }
    }

    public class FbxDataKeyframedAnimation
    {
        public string Name { get; set; }
        public float SampleRate { get; set; }
        public List<FbxDataAnimationKeyframedTrack> TrackList { get; set; }

        public FbxDataAnimationKeyframedTrack FindTrack(string path)
        {
            var track = TrackList.Find(x => x.Path == path);
            if (track == null)
            {
                track = new FbxDataAnimationKeyframedTrack { Path = path };
                TrackList.Add(track);
            }

            return track;
        }
    }

    public class FbxDataAnimationKeyframedTrack
    {
        public string Path { get; set; }
        public List<FbxDataKeyframe<Vector3>> Scalings = new List<FbxDataKeyframe<Vector3>>();
        public List<FbxDataKeyframe<Vector3>> Rotations = new List<FbxDataKeyframe<Vector3>>();
        public List<FbxDataKeyframe<Vector3>> Translations = new List<FbxDataKeyframe<Vector3>>();
        public FbxDataBlendShape BlendShape;
    }

    public class FbxDataKeyframe<T>
    {
        public float time { get; set; }
        public T value { get; set; }

        public FbxDataKeyframe(float time, T value)
        {
            this.time = time;
            this.value = value;
        }
    }

    public class FbxDataBlendShape
    {
        public string ChannelName;
        public List<FbxDataKeyframe<float>> Keyframes = new List<FbxDataKeyframe<float>>();
    }

    public class FbxDataMorph
    {
        public string Path { get; set; }
        public List<FbxDataMorphChannel> Channels { get; set; }
    }

    public class FbxDataMorphChannel
    {
        public string Name { get; set; }
        public List<FbxDataMorphKeyframe> KeyframeList { get; set; }
    }

    public class FbxDataMorphKeyframe
    {
        public bool hasNormals { get; set; }
        public bool hasTangents { get; set; }
        public float Weight { get; set; }
        public List<FbxDataMorphVertex> VertexList { get; set; }
    }

    public class FbxDataMorphVertex
    {
        public uint Index { get; set; }
        public FbxDataVertex Vertex { get; set; }
    }

    public static class FbxDataHelpers
    {
        public static FbxDataMesh FindMesh(string path, List<FbxDataMesh> importedMeshList)
        {
            foreach (var mesh in importedMeshList)
            {
                if (mesh.Path == path)
                {
                    return mesh;
                }
            }

            return null;
        }

        public static FbxDataMaterial FindMaterial(string name, List<FbxDataMaterial> importedMats)
        {
            foreach (var mat in importedMats)
            {
                if (mat.Name == name)
                {
                    return mat;
                }
            }

            return null;
        }

        public static FbxDataTexture FindTexture(string name, List<FbxDataTexture> importedTextureList)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            foreach (var tex in importedTextureList)
            {
                if (tex.Name == name)
                {
                    return tex;
                }
            }

            return null;
        }
    }
}
