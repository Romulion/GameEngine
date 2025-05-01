/// Original code from https://github.com/RazTools/Studio/


#pragma once

#include <fbxsdk.h>
#include "fbxsdk_material_bind.h"

struct FbxSDKMeshContext
{

	FbxMesh* pMesh;
	FbxSDKMaterialBind* lBlendShape;

	FbxSDKMeshContext();
	~FbxSDKMeshContext() = default;

};