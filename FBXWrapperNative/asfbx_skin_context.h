/// Original code from https://github.com/RazTools/Studio/

#pragma once

#include <fbxsdk.h>

struct AsFbxContext;

struct AsFbxSkinContext
{
	
	FbxSkin* pSkin;
	FbxAMatrix lMeshMatrix;

	AsFbxSkinContext(AsFbxContext* pContext, FbxNode* pFrameNode);
	~AsFbxSkinContext() = default;

};
