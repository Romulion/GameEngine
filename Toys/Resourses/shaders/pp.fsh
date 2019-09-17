#version 330 core

out vec4 FragColor;

in vec2 Texcord;
struct Material{
    sampler2D texture_diffuse;
};

uniform Material material;

void main()
{

const float offset = 1.0/300.0;
    vec2 offsets[9] = vec2[](
        vec2(-offset,  offset), // top-left
        vec2( 0.0f,    offset), // top-center
        vec2( offset,  offset), // top-right
        vec2(-offset,  0.0f),   // center-left
        vec2( 0.0f,    0.0f),   // center-center
        vec2( offset,  0.0f),   // center-right
        vec2(-offset, -offset), // bottom-left
        vec2( 0.0f,   -offset), // bottom-center
        vec2( offset, -offset)  // bottom-right    
    );

    float kernel[9] = float[](
        1, 2, 1,
        2,  4, 2,
        1, 2, 1
    );
    
    vec3 sampleTex[9];
    for(int i = 0; i < 9; i++)
    {
        sampleTex[i] = vec3(texture(material.texture_diffuse, Texcord.st + offsets[i]));
    }
    vec3 col = vec3(0.0);
    for(int i = 0; i < 9; i++)
        col += sampleTex[i] * kernel[i]/16;
    
    FragColor = vec4(1,0,0,1.0);
	
	//FragColor = texture(material.texture_diffuse, Texcord);
} 