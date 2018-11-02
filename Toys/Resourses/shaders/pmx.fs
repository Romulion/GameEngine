#version 330 core

out vec4 FragColor;

in VS_OUT {
	vec2 Texcord;
	vec3 FragPos;
	vec3 Normal;
	vec4 lightSpace;
} fs_in;

layout (std140) uniform light {
	vec3 LightPos;
	vec3 viewPos;
	float near_plane;
	float far_plane;
};

/*
uniform vec3 LightPos;
uniform vec3 viewPos;
uniform float near_plane;
uniform float far_plane;
*/
uniform sampler2D shadowMap;

struct Material{
    sampler2D texture_diffuse;
    sampler2D texture_specular;
	sampler2D texture_height;
	sampler2D texture_toon;
	vec3 Color;
	vec4 DiffuseColor;
    float shininess;
};

uniform Material material;

float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0;
    return (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));
}

//calculating shadow
float ShadowCalculation(vec4 fragPosLightSpace)
{
	float bias = 0.005;
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    float closestDepth = texture(shadowMap, projCoords.xy).r; 
	//float closestDepth = LinearizeDepth(texture(shadowMap, projCoords.xy).r); 
    float currentDepth = projCoords.z;
    float shadow = currentDepth - bias > closestDepth  ? 0.98 : 0.02;
	/* disabled shadow smoothing
	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
			float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
			shadow += currentDepth - bias > pcfDepth ? 0.98 : 0.02;        
		}    
	}
	shadow /= 9.0;
	*/
    return shadow;
}

void main()
{
	vec4 texcolor = texture(material.texture_diffuse,fs_in.Texcord);
	
	//discard invisible texels
	if (texcolor.a < 0.05)
		discard;
	
	//calculating shadows
	vec3 normal = normalize(fs_in.Normal);
	vec3 lightDir = normalize(LightPos - fs_in.FragPos);	
	//float diffuse =   0.5 - dot(lightDir,normal) * 0.5;
	float diffuse = 1 - max(dot(lightDir,normal) , 0.02);
	
	/*
	vec4 shadowcolor;
	if (diffuse < 0.98){
		float shadow = ShadowCalculation(fs_in.lightSpace);
		shadowcolor  = texture(material.texture_toon,vec2(max(diffuse,shadow)));
	}
	else 
		shadowcolor  = texture(material.texture_toon,vec2(diffuse));
	*/
	
	float shadow = ShadowCalculation(fs_in.lightSpace);
	vec4 shadowcolor  = texture(material.texture_toon,vec2(max(diffuse,shadow)));
	FragColor =  texcolor  * shadowcolor;
	//FragColor = vec4(vec3(diffuse),1.0);
}