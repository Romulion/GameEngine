/// Original code from https://github.com/RazTools/Studio/

#pragma once

#include <fbxsdk.h>

struct FbxSDKMaterialBind
{
	int* Indexes;
	int material_index;

	FbxSDKMaterialBind();
	~FbxSDKMaterialBind() = default;

};