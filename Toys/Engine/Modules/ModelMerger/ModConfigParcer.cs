using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using OpenTK.Windowing.Common.Input;

namespace Toys.Mod3DMigotoReconstructor
{
    public class ModConfigParcer
    {
        const char blockStart = '[';
        Dictionary<string, List<string>> _fileBlocks = new Dictionary<string, List<string>>();
        public ModelDataStruct _model { get; private set; }

        Dictionary<string, string> bufferReferences;

        public ModConfigParcer(string iniFile) 
        {
            _model = new ModelDataStruct();

            if (Path.GetExtension(iniFile) != ".ini")
                throw new Exception("wrong file");

            var _file = File.ReadAllLines(iniFile);
            ParceBlock(_file);

            FindBuffers();
            FindMaterials();

            //DebugParcedData();
        }

        void FindBuffers()
        {
            string start = "TextureOverride";

            foreach (var block in _fileBlocks.Keys)
            {
                if (block.StartsWith(start))
                {
                    var blockData = BlockParce(_fileBlocks[block]);
                    if (block.EndsWith("Position"))
                    {
                        //Jump to reference block in exist
                        if (blockData.ContainsKey("vb0"))
                            blockData = BlockParce(_fileBlocks[blockData["vb0"]]);

                        if (blockData.ContainsKey("stride"))
                            if (blockData["stride"] != "40")
                                throw new Exception("wrong position data format");

                        if (blockData.ContainsKey("filename"))
                            _model.PositionPath = blockData["filename"];
                    }
                    else if (block.EndsWith("Blend"))
                    {
                        //Jump to reference block in exist
                        if (blockData.ContainsKey("vb1"))
                            blockData = BlockParce(_fileBlocks[blockData["vb1"]]);

                        if (blockData.ContainsKey("stride"))
                            if (blockData["stride"] != "32")
                                throw new Exception("wrong blend data format");

                        if (blockData.ContainsKey("filename"))
                            _model.BlendPath = blockData["filename"];
                    }
                    else if (block.EndsWith("Texcoord"))
                    {
                        //Jump to reference block in exist
                        if (blockData.ContainsKey("vb1"))
                            blockData = BlockParce(_fileBlocks[blockData["vb1"]]);

                        if (blockData.ContainsKey("stride"))
                        {
                            if (blockData["stride"] != "20" && blockData["stride"] != "12")
                                throw new Exception("wrong textcord data format");
                            _model.TexCordStride = int.Parse(blockData["stride"]);
                        }
                        if (blockData.ContainsKey("filename"))
                            _model.TexcoordPath = blockData["filename"];
                    }
                }
            }
        }

        void FindMaterials()
        {
            string start = "TextureOverride";

            foreach (var block in _fileBlocks.Keys)
            {
                if (block.StartsWith(start))
                {
                    var blockData = BlockParce(_fileBlocks[block]);

                    if (block.EndsWith("FaceHead"))
                    {
                        var material = new MaterialData();
                        material.Name = "Face";
                        //if (blockData.ContainsKey("match_first_index"))
                        //    material.Offset = int.Parse(blockData["match_first_index"]);

                        if (blockData.ContainsKey("ib"))
                            material.IndexBufferFile = GetIBFile(blockData["ib"]);

                        if (blockData.ContainsKey("ps-t0"))
                            material.TextureDiffuse = GetTextureFile(blockData["ps-t0"]);
                        //if (blockData.ContainsKey("ps-t1"))
                        //    material.TextureDiffuse = GetTextureFile(blockData["ps-t1"]);
                        //if (blockData.ContainsKey("ps-t2"))
                        //    material.TextureLight = GetTextureFile(blockData["ps-t2"]);

                        material.MeshSubParts = TryParceMeshSubParts(_fileBlocks[block]);
                        _model.materials.Add(material);
                    }
                    else if (block.EndsWith("Head"))
                    {
                        var material = new MaterialData();
                        material.Name = "Head";

                        if (blockData.ContainsKey("ib"))
                            material.IndexBufferFile = GetIBFile(blockData["ib"]);

                        if (blockData.ContainsKey("ps-t0"))
                            material.TextureNormal = GetTextureFile(blockData["ps-t0"]);
                        if (blockData.ContainsKey("ps-t1"))
                            material.TextureDiffuse = GetTextureFile(blockData["ps-t1"]);
                        if (blockData.ContainsKey("ps-t2"))
                            material.TextureLight = GetTextureFile(blockData["ps-t2"]);

                        material.MeshSubParts = TryParceMeshSubParts(_fileBlocks[block]);
                        _model.materials.Add(material);
                    }
                    else if (block.EndsWith("Body"))
                    {
                        var material = new MaterialData();
                        material.Name = "Body";

                        if (blockData.ContainsKey("ib"))
                            material.IndexBufferFile = GetIBFile(blockData["ib"]);

                        if (blockData.ContainsKey("ps-t0"))
                            material.TextureNormal = GetTextureFile(blockData["ps-t0"]);
                        if (blockData.ContainsKey("ps-t1"))
                            material.TextureDiffuse = GetTextureFile(blockData["ps-t1"]);
                        if (blockData.ContainsKey("ps-t2"))
                            material.TextureLight = GetTextureFile(blockData["ps-t2"]);

                        material.MeshSubParts = TryParceMeshSubParts(_fileBlocks[block]);
                        _model.materials.Add(material);
                    }

                }
            }
        }

        string GetIBFile(string blockName)
        {
            var block =  BlockParce(_fileBlocks[blockName]);
            if (block["format"] != "DXGI_FORMAT_R32_UINT")
                throw new Exception("unknown IB format" + block["format"]);
            return block["filename"];
        }

        string GetTextureFile(string blockName)
        {
            var block = BlockParce(_fileBlocks[blockName]);
            return block["filename"];
        }

        List<MeshSubPart> TryParceMeshSubParts(List<string> block)
        {
            var result = new List<MeshSubPart>();
            foreach (var line in block)
            {
                var keyvalPair = Regex.Match(line, @"[\S]+\s+=\s(\d+),\s(\d+),");
                if (keyvalPair.Success)
                {
                    var offset = int.Parse(keyvalPair.Groups[2].Value);
                    var count = int.Parse(keyvalPair.Groups[1].Value);
                    //skip empty parts
                    if (count == 0)
                        continue;

                    result.Add(new MeshSubPart() {Count = count, Offset = offset });
                }
            }
            return result;
        }
        Dictionary<string,string>  BlockParce(List<string> blockData)
        {
            var result = new Dictionary<string, string>();
            foreach (var line in blockData)
            {
                //look for "key = value" pattern
                var keyvalPair = Regex.Match(line, @"^([\S]+)\s+=\s([\S]+)");
                if (keyvalPair.Success)
                {
                    //skip key dublicates
                    if (!result.ContainsKey(keyvalPair.Groups[1].Value))
                        result.Add(keyvalPair.Groups[1].Value, keyvalPair.Groups[2].Value);
                } 
            }
            return result;
        }


        void ParceBlock(string[] file)
        {
            string currnetContext = "";
            foreach (var line in file)
            {
                if (line.StartsWith(blockStart))
                {
                    //remove brackets
                    currnetContext = line.Substring(1, line.Length - 2);
                    _fileBlocks.Add(currnetContext, new List<string>());
                }
                else if (currnetContext != "" && line != "")
                {
                    _fileBlocks[currnetContext].Add(line);
                }
            }
        }

        void DebugParcedData()
        {
            Logger.Info(_model.PositionPath);
            Logger.Info(_model.TexcoordPath);
            Logger.Info(_model.BlendPath);
            Logger.Info(_model.TexCordStride);
            foreach (var mat in _model.materials)
            {
                Logger.Info(mat.Name);
                Logger.Info(mat.Offset);
                
                Logger.Info(mat.TextureNormal);
                Logger.Info(mat.TextureDiffuse);
                Logger.Info(mat.TextureLight);
                
                Logger.Info("Mesh Parts:");
                foreach (var part in mat.MeshSubParts)
                    Logger.Info($"count {part.Count} offset {part.Offset}");
                
            }
        }
    }
}
