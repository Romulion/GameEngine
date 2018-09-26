#version 430

struct VertexRigged
{
 	vec3 aPos;
	vec3 aNormal;
	vec2 aTexcord;
	ivec4 BoneIDs;
	vec4 Weights;
};

struct Vertex
{
 	vec3 aPos;
	vec3 aNormal;
	vec2 aTexcord;
};

layout(std430, binding = 0) buffer Input
{
	VertexRigged modelRigged [];
};

layout(std430, binding = 1) buffer Output
{
	Vertex model [];
};

layout (std140) uniform skeleton
{
    mat4 gBones[500];
};

void main() {

	uint vIndex = gl_GlobalInvocationID.x;

	VertexRigged vert = modelRigged[vIndex];

	mat4 BoneTransform = gBones[vert.BoneIDs[0]] * vert.Weights[0];
    BoneTransform += gBones[vert.BoneIDs[1]] * vert.Weights[1];
    BoneTransform += gBones[vert.BoneIDs[2]] * vert.Weights[2];
    BoneTransform += gBones[vert.BoneIDs[3]] * vert.Weights[3];

    model[vIndex].aPos = (BoneTransform * vec4(vert.aPos, 1.0)).xyz;
    model[vIndex].aNormal = (BoneTransform * vec4(vert.aNormal, 1.0)).xyz;
    model[vIndex].aTexcord = vert.aTexcord;
}