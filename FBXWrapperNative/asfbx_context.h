/// Original code from https://github.com/RazTools/Studio/


#pragma once

#include <cstdint>
#include <string>
#include <unordered_set>

struct AsFbxContext
{

	fbxsdk::FbxManager* pSdkManager;
	fbxsdk::FbxScene* pScene;
	fbxsdk::FbxArray<fbxsdk::FbxFileTexture*>* pTextures;
	fbxsdk::FbxArray<fbxsdk::FbxSurfacePhong*>* pMaterials;
	fbxsdk::FbxExporter* pExporter;
	fbxsdk::FbxImporter* pImporter;
	fbxsdk::FbxPose* pBindPose;
	const char* pErrMsg;

	std::unordered_set<std::string> framePaths;

	AsFbxContext();
	~AsFbxContext();
};
