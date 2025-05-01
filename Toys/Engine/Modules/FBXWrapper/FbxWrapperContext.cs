/// Original code from https://github.com/RazTools/Studio/
/// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using FFmpeg.AutoGen;
using OpenTK.Mathematics;

namespace FbxWrapper.FbxInterop
{
    internal sealed partial class FbxWrapperContext : IDisposable
    {

        private IntPtr _pContext;
        private readonly Dictionary<FbxDataNode, IntPtr> _nodeList;
        private readonly List<KeyValuePair<string, IntPtr>> _createdMaterials;
        private readonly Dictionary<string, IntPtr> _createdTextures;
        private readonly Fbx.ExportOptions _exportOptions;

        public FbxWrapperContext(Fbx.ExportOptions exportOptions)
        {
            Fbx.QuaternionToEuler(Quaternion.Identity); // workaround to init dll
            _pContext = AsFbxCreateContext();
            _createdMaterials = new List<KeyValuePair<string, IntPtr>>();
            _createdTextures = new Dictionary<string, IntPtr>();
            _nodeList = new Dictionary<FbxDataNode, IntPtr>();
            _exportOptions = exportOptions;
        }

        ~FbxWrapperContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed { get; private set; }

        private void Dispose(bool disposing)
        {
            IsDisposed = true;

            _nodeList.Clear();
            _createdMaterials.Clear();
            _createdTextures.Clear();

            AsFbxDisposeContext(ref _pContext);
        }

        private void EnsureNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FbxWrapperContext));
            }
        }


        public bool ImportFromFile(string fileName)
        {
            return AsFbxImportScene(_pContext, fileName);
        }


        public bool ExportToFile(string fileName)
        {
            EnsureNotDisposed();
            FbxSDKSceneSetScale(_pContext, _exportOptions.scaleFactor);
            return AsFbxExportScene(_pContext, fileName,  5, false);
        }

        public string GetLastError()
        {
            return FbxSDKGetError(_pContext);
        }

        public FbxNode GetRootNode()
        {
            var pNode = FbxSDKGetRootNode(_pContext);
            return Fbx.GetNativeWrapper<FbxNode>(pNode);
        }

        internal void Initialize(bool is60Fps)
        {
            EnsureNotDisposed();

            var b = AsFbxInitializeContext(_pContext);

            if (!b)
            {
                var fullMessage = $"Failed to initialize FbxExporter: {FbxSDKGetError(_pContext)}";
                throw new ApplicationException(fullMessage);
            }
        }

        internal void SetFramePaths(HashSet<string> framePaths)
        {
            EnsureNotDisposed();

            if (framePaths == null || framePaths.Count == 0)
            {
                return;
            }

            var framePathList = new List<string>(framePaths);
            var framePathArray = framePathList.ToArray();

            AsFbxSetFramePaths(_pContext, framePathArray);
        }
        
        internal void ExportFrame(List<FbxDataMesh> meshList, List<FbxDataNode> meshNodes, FbxDataNode rootNodeData)
        {
            var rootNode = AsFbxGetSceneRootNode(_pContext);

            Debug.Assert(rootNode != IntPtr.Zero);

            var nodeStack = new Stack<IntPtr>();
            var frameStack = new Stack<FbxDataNode>();

            nodeStack.Push(rootNode);
            frameStack.Push(rootNodeData);

            while (nodeStack.Count > 0)
            {
                var parentNode = nodeStack.Pop();
                var frame = frameStack.Pop();

                var childNode = AsFbxExportSingleFrame(_pContext, parentNode, frame.Path, frame.Name, frame.LocalPosition, frame.LocalRotation, frame.LocalScale);

                if (meshList != null && FbxDataHelpers.FindMesh(frame.Path, meshList) != null)
                {
                    meshNodes.Add(frame);
                }

                _nodeList.Add(frame, childNode);

                for (var i = frame.Count - 1; i >= 0; i -= 1)
                {
                    nodeStack.Push(childNode);
                    frameStack.Push(frame[i]);
                }
            }
        }
        
        
        internal void SetJointsNode(FbxDataNode rootFrame, HashSet<string> bonePaths, bool castToBone)
        {
            var frameStack = new Stack<FbxDataNode>();

            frameStack.Push(rootFrame);

            while (frameStack.Count > 0)
            {
                var frame = frameStack.Pop();

                if (_nodeList.TryGetValue(frame, out var node))
                {
                    Debug.Assert(node != IntPtr.Zero);

                    if (castToBone)
                    {
                        AsFbxSetJointsNode_CastToBone(_pContext, node, _exportOptions.boneSize);
                    }
                    else
                    {
                        Debug.Assert(bonePaths != null);

                        if (bonePaths.Contains(frame.Path))
                        {
                            AsFbxSetJointsNode_BoneInPath(_pContext, node, _exportOptions.boneSize);
                        }
                        else
                        {
                            AsFbxSetJointsNode_Generic(_pContext, node);
                        }
                    }
                }

                for (var i = frame.Count - 1; i >= 0; i -= 1)
                {
                    frameStack.Push(frame[i]);
                }
            }
        }
        
        internal void PrepareMaterials(int materialCount, int textureCount)
        {
            AsFbxPrepareMaterials(_pContext, materialCount, textureCount);
        }
        
        internal void ExportMeshFromFrame(FbxDataNode rootFrame, FbxDataNode meshFrame, List<FbxDataMesh> meshList, List<FbxDataMaterial> materialList, List<FbxDataTexture> textureList)
        {
            var meshNode = _nodeList[meshFrame];
            var mesh = FbxDataHelpers.FindMesh(meshFrame.Path, meshList);

            ExportMesh(rootFrame, materialList, textureList, meshNode, mesh);
        }
        
        
        
        private IntPtr ExportTexture(FbxDataTexture texture)
        {
            if (texture == null)
            {
                return IntPtr.Zero;
            }

            if (_createdTextures.ContainsKey(texture.Name))
            {
                return _createdTextures[texture.Name];
            }

            var pTex = AsFbxCreateTexture(_pContext, texture.Name);

            _createdTextures.Add(texture.Name, pTex);

            return pTex;
        }
        
        
        public void ExportMesh(FbxDataNode rootFrame, List<FbxDataMaterial> materialList, List<FbxDataTexture> textureList, IntPtr frameNode, FbxDataMesh fdMesh)
        {
            
            var boneList = fdMesh.BoneList;
            var totalBoneCount = 0;
            var hasBones = false;
            if (boneList?.Count > 0)
            {
                totalBoneCount = boneList.Count;
                hasBones = true;
            }
            
            var pClusterArray = IntPtr.Zero;

            try
            {
                if (hasBones)
                {
                    pClusterArray = AsFbxMeshCreateClusterArray(totalBoneCount);

                    foreach (var bone in boneList)
                    {
                        if (bone.Path != null)
                        {
                            var frame = rootFrame.FindFrameByPath(bone.Path);
                            var boneNode = _nodeList[frame];

                            var cluster = AsFbxMeshCreateCluster(_pContext, boneNode);

                            AsFbxMeshAddCluster(pClusterArray, cluster);
                        }
                        else
                        {
                            AsFbxMeshAddCluster(pClusterArray, IntPtr.Zero);
                        }
                    }
                }

                var mesh = AsFbxMeshCreateMesh(_pContext, frameNode);

                AsFbxMeshInitControlPoints(mesh, fdMesh.VertexList.Count);

                if (fdMesh.HasNormal)
                {
                    AsFbxMeshCreateElementNormal(mesh);
                }

                for (int i = 0; i < fdMesh.HasUV.Length; i++)
                {
                    if (!fdMesh.HasUV[i]) { continue; }

                    var type = fdMesh.UVType[i];
                    AsFbxMeshCreateUV(mesh, i, type);
                }

                if (fdMesh.HasTangent)
                {
                    AsFbxMeshCreateElementTangent(mesh);
                }

                if (fdMesh.HasColor)
                {
                    AsFbxMeshCreateElementVertexColor(mesh);
                }


                AsFbxMeshCreateElementMaterial(mesh);
                foreach (var meshObj in fdMesh.SubmeshList)
                {
                    var materialIndex = 0;
                    /*
                    var materialIndex = 0;
                    var mat = materialList.Find(m => m.Name == meshObj.Material);

                    if (mat != null)
                    {
                        materialIndex = AsFbxAddMaterialToFrame(fNode.GetPointer, mat.GetPointer);
                    }
                    */

                    var mat = FbxDataHelpers.FindMaterial(meshObj.Material, materialList);
                    
                    if (mat != null)
                    {
                        var foundMat = _createdMaterials.FindIndex(kv => kv.Key == mat.Name);
                        IntPtr pMat;

                        if (foundMat >= 0)
                        {
                            pMat = _createdMaterials[foundMat].Value;
                        }
                        else
                        {
                            var diffuse = mat.Diffuse;
                            var ambient = mat.Ambient;
                            var emissive = mat.Emissive;
                            var specular = mat.Specular;
                            var reflection = mat.Reflection;

                            pMat = AsFbxCreateMaterial(_pContext, mat.Name, in diffuse, in ambient, in emissive, in specular, in reflection, mat.Shininess, mat.Transparency);

                            _createdMaterials.Add(new KeyValuePair<string, IntPtr>(mat.Name, pMat));
                        }

                        materialIndex = AsFbxAddMaterialToFrame(frameNode, pMat);

                        var hasTexture = false;

                        foreach (var texture in mat.Textures)
                        {
                            var tex = FbxDataHelpers.FindTexture(texture.Name, textureList);
                            var pTexture = ExportTexture(tex);

                            if (pTexture != IntPtr.Zero)
                            {
                                switch ((int)texture.Type)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                        {
                                            AsFbxLinkTexture((int)texture.Type, pTexture, pMat, texture.Offset.X, texture.Offset.Y, texture.Scale.X, texture.Scale.Y);
                                            hasTexture = true;
                                            break;
                                        }
                                    default:
                                        break;
                                }
                            }
                        }

                        if (hasTexture)
                        {
                            AsFbxSetFrameShadingModeToTextureShading(frameNode);
                        }
                    }

                    for (int face = 0; face < meshObj.Indeces.Length; face += 3)
                    {
                        
                        var index0 = meshObj.Indeces[face + 0];
                        var index1 = meshObj.Indeces[face + 1];
                        var index2 = meshObj.Indeces[face + 2];

                        AsFbxMeshAddPolygon(mesh, materialIndex, index0, index1, index2);
                    }
                }

                var vertexList = fdMesh.VertexList;

                var vertexCount = vertexList.Count;

                for (var j = 0; j < vertexCount; j += 1)
                {
                    var importedVertex = vertexList[j];

                    var vertex = importedVertex.Position;
                    AsFbxMeshSetControlPoint(mesh, j, vertex.X, vertex.Y, vertex.Z);

                    if (fdMesh.HasNormal)
                    {
                        var normal = importedVertex.Normal;
                        AsFbxMeshElementNormalAdd(mesh, 0, normal.X, normal.Y, normal.Z);
                    }

                    var uvSize = fdMesh.HasUV.Length;
                    for (var uvIndex = 0; uvIndex < uvSize; uvIndex += 1)
                    {
                        if (fdMesh.HasUV[uvIndex])
                        {
                            var uv = importedVertex.UV[uvIndex];
                            AsFbxMeshElementUVAdd(mesh, uvIndex, uv[0], uv[1]);
                        }
                    }

                    if (fdMesh.HasTangent)
                    {
                        var tangent = importedVertex.Tangent;
                        AsFbxMeshElementTangentAdd(mesh, 0, tangent.X, tangent.Y, tangent.Z, tangent.W);
                    }

                    if (fdMesh.HasColor)
                    {
                        var color = importedVertex.Color;
                        AsFbxMeshElementVertexColorAdd(mesh, 0, color.R, color.G, color.B, color.A);
                    }
                    
                    if (hasBones && importedVertex.BoneIndices != null)
                    {
                        
                        var boneIndices = importedVertex.BoneIndices;
                        var boneWeights = importedVertex.Weights;

                        for (var k = 0; k < 4; k += 1)
                        {
                            if (boneIndices[k] < totalBoneCount && boneWeights[k] > 0)
                            {
                                AsFbxMeshSetBoneWeight(pClusterArray, boneIndices[k], j, boneWeights[k]);
                            }
                        }
                    }
                }


                if (hasBones)
                {
                    IntPtr pSkinContext = IntPtr.Zero;

                    try
                    {
                        pSkinContext = AsFbxMeshCreateSkinContext(_pContext, frameNode);

                        unsafe
                        {
                            var boneMatrix = stackalloc float[16];

                            for (var j = 0; j < totalBoneCount; j += 1)
                            {
                                if (!FbxClusterArray_HasItemAt(pClusterArray, j))
                                {
                                    continue;
                                }

                                var m = boneList[j].Matrix;
                                CopyMatrix4x4(in m, boneMatrix);

                                AsFbxMeshSkinAddCluster(pSkinContext, pClusterArray, j, boneMatrix);
                            }
                        }

                        AsFbxMeshAddDeformer(pSkinContext, mesh);
                    }
                    finally
                    {
                        AsFbxMeshDisposeSkinContext(ref pSkinContext);
                    }
                }
            }
            finally
            {
                AsFbxMeshDisposeClusterArray(ref pClusterArray);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void CopyMatrix4x4(in Matrix4 matrix, float* buffer)
        {
            for (var m = 0; m < 4; m += 1)
            {
                for (var n = 0; n < 4; n += 1)
                {
                    var index = IndexFrom4x4(m, n);
                    buffer[index] = matrix[m, n];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IndexFrom4x4(int m, int n)
        {
            return 4 * m + n;
        }

        
        internal void ExportAnimations(FbxDataNode rootFrame, List<FbxDataKeyframedAnimation> animationList)
        {
            if (animationList == null || animationList.Count == 0)
            {
                return;
            }

            var pAnimContext = IntPtr.Zero;

            try
            {
                pAnimContext = AsFbxAnimCreateContext(_exportOptions.eulerFilter);

                for (int i = 0; i < animationList.Count; i++)
                {
                    var importedAnimation = animationList[i];
                    string takeName;

                    if (importedAnimation.Name != null)
                    {
                        takeName = importedAnimation.Name;
                    }
                    else
                    {
                        takeName = $"Take{i.ToString()}";
                    }

                    AsFbxAnimPrepareStackAndLayer(_pContext, pAnimContext, takeName);

                    ExportKeyframedAnimation(rootFrame, importedAnimation, pAnimContext);
                }
            }
            finally
            {
                AsFbxAnimDisposeContext(ref pAnimContext);
            }
        }

        private void ExportKeyframedAnimation(FbxDataNode rootFrame, FbxDataKeyframedAnimation parser, IntPtr pAnimContext)
        {
            foreach (var track in parser.TrackList)
            {
                if (track.Path == null)
                {
                    continue;
                }

                var frame = rootFrame.FindFrameByPath(track.Path);

                if (frame == null)
                {
                    continue;
                }

                var pNode = _nodeList[frame];

                AsFbxAnimLoadCurves(pNode, pAnimContext);

                AsFbxAnimBeginKeyModify(pAnimContext);

                foreach (var scaling in track.Scalings)
                {
                    var value = scaling.value;
                    AsFbxAnimAddScalingKey(pAnimContext, scaling.time, value.X, value.Y, value.Z);
                }

                foreach (var rotation in track.Rotations)
                {
                    var value = rotation.value;
                    AsFbxAnimAddRotationKey(pAnimContext, rotation.time, value.X, value.Y, value.Z);
                }

                foreach (var translation in track.Translations)
                {
                    var value = translation.value;
                    AsFbxAnimAddTranslationKey(pAnimContext, translation.time, value.X, value.Y, value.Z);
                }

                AsFbxAnimEndKeyModify(pAnimContext);

                AsFbxAnimApplyEulerFilter(pAnimContext, _exportOptions.filterPrecision);

                var blendShape = track.BlendShape;

                if (blendShape != null)
                {
                    var channelCount = AsFbxAnimGetCurrentBlendShapeChannelCount(pAnimContext, pNode);

                    if (channelCount > 0)
                    {
                        for (var channelIndex = 0; channelIndex < channelCount; channelIndex += 1)
                        {
                            if (!AsFbxAnimIsBlendShapeChannelMatch(pAnimContext, channelIndex, blendShape.ChannelName))
                            {
                                continue;
                            }

                            AsFbxAnimBeginBlendShapeAnimCurve(pAnimContext, channelIndex);

                            foreach (var keyframe in blendShape.Keyframes)
                            {
                                AsFbxAnimAddBlendShapeKeyframe(pAnimContext, keyframe.time, keyframe.value);
                            }

                            AsFbxAnimEndBlendShapeAnimCurve(pAnimContext);
                        }
                    }
                }
            }
        }

        internal void ExportMorphs(FbxDataNode rootFrame, List<FbxDataMorph> morphList)
        {
            if (morphList == null || morphList.Count == 0)
            {
                return;
            }

            foreach (var morph in morphList)
            {
                var frame = rootFrame.FindFrameByPath(morph.Path);

                if (frame == null)
                {
                    continue;
                }

                var pNode = _nodeList[frame];

                var pMorphContext = IntPtr.Zero;

                try
                {
                    pMorphContext = AsFbxMorphCreateContext();

                    AsFbxMorphInitializeContext(_pContext, pMorphContext, pNode);

                    foreach (var channel in morph.Channels)
                    {
                        AsFbxMorphAddBlendShapeChannel(_pContext, pMorphContext, channel.Name);

                        for (var i = 0; i < channel.KeyframeList.Count; i++)
                        {
                            var keyframe = channel.KeyframeList[i];

                            AsFbxMorphAddBlendShapeChannelShape(_pContext, pMorphContext, keyframe.Weight, i == 0 ? channel.Name : $"{channel.Name}_{i + 1}");

                            AsFbxMorphCopyBlendShapeControlPoints(pMorphContext);

                            foreach (var vertex in keyframe.VertexList)
                            {
                                var v = vertex.Vertex.Position;
                                AsFbxMorphSetBlendShapeVertex(pMorphContext, vertex.Index, v.X, v.Y, v.Z);
                            }

                            if (keyframe.hasNormals)
                            {
                                AsFbxMorphCopyBlendShapeControlPointsNormal(pMorphContext);

                                foreach (var vertex in keyframe.VertexList)
                                {
                                    var v = vertex.Vertex.Normal;
                                    AsFbxMorphSetBlendShapeVertexNormal(pMorphContext, vertex.Index, v.X, v.Y, v.Z);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    AsFbxMorphDisposeContext(ref pMorphContext);
                }
            }
        }

    }
}
