#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 3) in ivec4 BoneIDs;
layout (location = 4) in vec4 Weights;

layout (std140) uniform skeleton
{
    mat4 gBones[300];
};

uniform mat4 pvm;

void main()
{
	mat4 BoneTransform = gBones[BoneIDs[0]] * Weights[0];
    BoneTransform += gBones[BoneIDs[1]] * Weights[1];
    BoneTransform += gBones[BoneIDs[2]] * Weights[2];
    BoneTransform += gBones[BoneIDs[3]] * Weights[3];
	
    gl_Position =  pvm * BoneTransform * vec4(aPos, 1.0);
}