using System;
namespace Toys
{
	public interface IModelLoader
	{
		ModelPrefab GetModel { get; }
        Morph[] GetMorphes { get; }
    }
}
