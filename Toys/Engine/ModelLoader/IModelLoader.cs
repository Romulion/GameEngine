using System;
namespace Toys
{
	public interface IModelLoader
	{
		SceneNode GetModel { get; }
		SceneNode GetRiggedModel { get; }
        Morph[] GetMorphes { get; }
    }
}
