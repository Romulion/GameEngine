#version 330 core
layout (location = 0) in vec4 vertex;
out vec2 TexCoords;

uniform mat4 projection;
uniform vec3 position_scale;

void main()
{
	gl_Position = (projection * vec4(vertex.xy * position_scale.z + position_scale.xy, 0.0, 1.0));
    TexCoords = vertex.zw;
}  