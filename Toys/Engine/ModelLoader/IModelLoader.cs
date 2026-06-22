using System;
namespace Toys
{
	public interface IModelLoader
	{
		ModelPrefab GetModel { get; }
        SceneNode[] GetScene { get; }
        Morph[] GetMorphes { get; }
    }
}
