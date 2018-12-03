using System;
namespace Toys
{
	public interface IModelLoader
	{
		Model GetModel { get; }
		Model GetRiggedModel { get; }
        Morph[] GetMorphes { get; }
    }
}
