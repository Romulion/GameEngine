using System;
namespace Toys
{
	public interface IModelLoader
	{
		ModelPrefab GetModel { get; }
		ModelPrefab GetRiggedModel { get; }
        Morph[] GetMorphes { get; }
    }
}
