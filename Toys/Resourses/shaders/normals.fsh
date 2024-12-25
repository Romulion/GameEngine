#version 330 core
out vec4 FragColor;
in VS_OUT {
        vec2 Texcord;
        vec3 FragPos;
        vec3 Normal;
        vec4 lightSpace;
        vec3 NormalLocal;
} fs_in;
layout (std140) uniform light {
        vec3 LightPos;
        vec3 viewPos;
        float near_plane;
        float far_plane;
};

void main()
{
    FragColor = vec4(fs_in.Normal,1);
}