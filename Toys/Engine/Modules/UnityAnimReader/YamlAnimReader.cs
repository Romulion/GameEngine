using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OpenTK.Mathematics;

namespace Toys.UnityAnimReader
{
    public class YamlAnimReader
    {
        const string rotationContext = "m_RotationCurves";
        const string positionContext = "m_PositionCurves";
        const string scaleContext = "m_PositionCurves";
        const string nameContext = "m_Name";
        public string Name { get; set; }

        public List<AnimPart> AnimationData { get; private set; }
        enum AnimContext 
        {
            None,
            Position,
            Rotation,
            Scale
        }

        public YamlAnimReader(string filePath)
        {
            var animFile = File.ReadLines(filePath);
            ReadAnimData(animFile);
        }

        void ReadAnimData(IEnumerable<string> fileData)
        {
            var resDict = new Dictionary<string, AnimPart>();

            var currentContext = AnimContext.None;
            Vector3 posBuffer = Vector3.Zero;
            string bonePath = "";
            Quaternion rotBuffer = Quaternion.Identity;
            bool isBlockCompleted = false;
            foreach (var line in fileData)
            {
                if (line.Contains(nameContext))
                    Name = ParceLineStr(line);
                else if (line.Contains(rotationContext))
                    currentContext = AnimContext.Rotation;
                else if (line.Contains(positionContext))
                    currentContext = AnimContext.Position;
                else if (line.Contains(scaleContext))
                    currentContext = AnimContext.Scale;


                if (currentContext > 0 && isBlockCompleted)
                {
                    
                    if (bonePath != "")
                    {
                        AnimPart part;
                        if (!resDict.TryGetValue(bonePath, out part))
                        {
                            part = new AnimPart();
                            part.Path = bonePath;
                            resDict.Add(bonePath, part);
                        }
                        switch (currentContext)
                        {
                            case AnimContext.Rotation:
                                part.Rotation = rotBuffer;
                                rotBuffer = Quaternion.Identity;
                                break;
                            case AnimContext.Position:
                                part.Position = posBuffer;
                                posBuffer = Vector3.Zero;
                                break;
                            case AnimContext.Scale:
                                break;
                            default:
                                break;
                        }

                        bonePath = "";
                    }
                    isBlockCompleted = false;
                }
                else if (line.Contains(" value:"))
                {
                    switch (currentContext)
                    {
                        case AnimContext.Rotation:
                            rotBuffer = ParceLineQuat(line);
                            break;
                        case AnimContext.Position:
                            posBuffer = ParceLineVec3(line);
                            break;
                        case AnimContext.Scale:
                            break;
                        default:
                            break;
                    }

                }
                else if (line.Contains(" path:"))
                {
                    bonePath = ParceLineStr(line);
                    isBlockCompleted = true;
                }
            }

            AnimationData = resDict.Values.ToList();
        }

        Vector3 ParceLineVec3(string line)
        {
            Vector3 result = Vector3.Zero;
            var keyvalPair = Regex.Match(line, @"[\-\d.E]+");

            if (keyvalPair.Success)
            {
                result.X = -float.Parse(keyvalPair.Value);
                keyvalPair = keyvalPair.NextMatch();
                result.Y = float.Parse(keyvalPair.Value);
                keyvalPair = keyvalPair.NextMatch();
                result.Z = float.Parse(keyvalPair.Value);
            }

            return result;
        }

        Quaternion ParceLineQuat(string line)
        {
            Quaternion result = Quaternion.Identity;
            var keyvalPair = Regex.Match(line, @"[\-\d.E]+");

            if (keyvalPair.Success)
            {
                result.X = float.Parse(keyvalPair.Value);
                keyvalPair = keyvalPair.NextMatch();
                result.Y = -float.Parse(keyvalPair.Value);
                keyvalPair = keyvalPair.NextMatch();
                result.Z = -float.Parse(keyvalPair.Value);
                keyvalPair = keyvalPair.NextMatch();
                result.W = float.Parse(keyvalPair.Value);
            }

            return result;
        }

        string ParceLineStr(string line)
        {
            string result = "";
            var keyvalPair = Regex.Match(line, @"^\s+\w+:\s+([\w,/]+)");

            if (keyvalPair.Success)
            {
                result = keyvalPair.Groups[1].Value;
            }

            return result;
        }
    }
}
