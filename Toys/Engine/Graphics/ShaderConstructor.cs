using System;
using System.Windows.Forms;

namespace Toys
{
	public class ShaderConstructor
	{
		ShaderSettings setting;
		string rawVertex = "";
		string rawFragment = "";

		ShaderConstructor(ShaderSettings stng)
		{
			setting = stng;
		}

		public static Shader CreateShader(ShaderSettings settings)
		{
			var constructor = new ShaderConstructor(settings);
			return constructor.Creator();
		}

		Shader Creator()
		{

			GenerateVertex();
			GenerateFragment();
			Shader shdr = new ShaderMain(rawVertex, rawFragment);

			//binding buffers
			var ubm = UniformBufferManager.GetInstance;
			if (setting.hasSkeleton)
			{
				var ubs = ubm.GetBuffer("skeleton");
				shdr.SetUBO(ubs.bufferIndex, "skeleton");
			}
			var ubsp = ubm.GetBuffer("space");
			shdr.SetUBO(ubsp.bufferIndex, "space");
			var ubl = ubm.GetBuffer("light");
			shdr.SetUBO(ubl.bufferIndex, "light");

			//bind textures
			shdr.ApplyShader();
			if (setting.TextureDiffuse)
				shdr.SetUniform((int)TextureType.diffuse, "material.texture_diffuse");
			if (setting.toonShadow)
			{
				shdr.SetUniform((int)TextureType.toon, "material.texture_toon");
				SceneManager.GetInstance.GetLight.BindShadowMap();
			}
			if (setting.recieveShadow)
				shdr.SetUniform((int)TextureType.shadow, "shadowMap");
			if (setting.TextureSpecular)
				shdr.SetUniform((int)TextureType.specular, "texture_specular");

			return shdr;
		}

		void GenerateVertex()
		{
			rawVertex += "#version 330 core\n";

			rawVertex += "layout (location = 0) in vec3 aPos;\n"
						+"layout (location = 1) in vec3 aNormal;\n"
						+"layout (location = 2) in vec2 aTexcord;\n";

			if (setting.hasSkeleton)
				rawVertex += "layout (location = 3) in ivec4 BoneIDs;\n"
							+"layout (location = 4) in vec4 Weights;\n";

			rawVertex += "\n";

			//setting uniform buffers
			if (setting.hasSkeleton)
				rawVertex += "layout (std140) uniform skeleton\n{\n    mat4 gBones[500];\n};\n";

			rawVertex += "layout (std140) uniform space {\n\tmat4 model;\n\tmat4 pvm;\n\tmat4 NormalMat;\n\tmat4 lightSpacePos;\n};\n";

			//setting out structure
			rawVertex += "out VS_OUT {\n\tvec2 Texcord;\n\tvec3 FragPos;\n\tvec3 Normal;\n\tvec4 lightSpace;\n} vs_out;\n";

			//main body
			rawVertex += "void main()\n{\n";

			string applySkeleton = "";
			if (setting.hasSkeleton)
			{
				rawVertex += "mat4 BoneTransform = gBones[BoneIDs[0]] * Weights[0];\n"
							+"\tBoneTransform += gBones[BoneIDs[1]] * Weights[1];\n"
							+"\tBoneTransform += gBones[BoneIDs[2]] * Weights[2];\n"
							+"\tBoneTransform += gBones[BoneIDs[3]] * Weights[3];\n";
				applySkeleton = " * BoneTransform";
			}

			rawVertex += "gl_Position =  pvm"+ applySkeleton +" * vec4(aPos, 1.0);\n";

			rawVertex += "vs_out.Texcord = aTexcord;\n";
			rawVertex += "vs_out.FragPos = vec3(model"+ applySkeleton +" * vec4(aPos, 1.0));\n";
			rawVertex += "vs_out.Normal = mat3(NormalMat"+ applySkeleton +") * aNormal;\n";
			rawVertex += "vs_out.lightSpace = lightSpacePos * vec4(vs_out.FragPos,1.0);\n";

			rawVertex += "}\n";
		}

		void GenerateFragment()
		{
			rawFragment += "";
			rawFragment += "#version 330 core\n";
			rawFragment += "out vec4 FragColor;\n";

			rawFragment += "in VS_OUT {\n\tvec2 Texcord;\n\tvec3 FragPos;\n\tvec3 Normal;\n\tvec4 lightSpace;\n} fs_in;\n";

			rawFragment += "layout (std140) uniform light {\n\tvec3 LightPos;\n\tvec3 viewPos;\n\tfloat near_plane;\n\tfloat far_plane;\n};\n";

			//setting material specific parameters
			rawFragment += "struct Material{\n";

			//if (setting.TextureDiffuse) //test enabling
				rawFragment += "\tsampler2D texture_diffuse;\n";

			if (setting.TextureSpecular)
				rawFragment += "\tsampler2D texture_specular;\n";

			if (setting.toonShadow)
				rawFragment += "\tsampler2D texture_toon;\n";

			rawFragment += "};\n";

			rawFragment += "uniform Material material;\n";

			if (setting.recieveShadow)
				rawFragment += "uniform sampler2D shadowMap;\n";

			if (setting.recieveShadow)
			{
				rawFragment += "float LinearizeDepth(float depth)\n"
					+ "{\n"
					+ "\tfloat z = depth * 2.0 - 1.0;\n"
					+ "\treturn (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));\n"
					+ "}\n";

				rawFragment += "float ShadowCalculation(vec4 fragPosLightSpace)\n"
					+ "{\n"
					+ "\tfloat bias = 0.005;\n"
					+ "\tvec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;\n"
					+ "\tprojCoords = projCoords * 0.5 + 0.5;\n"
					+ "\tfloat closestDepth = texture(shadowMap, projCoords.xy).r;\n"
					//+"\t//float closestDepth = LinearizeDepth(texture(shadowMap, projCoords.xy).r); \n"
					+ "float currentDepth = projCoords.z;\n"
					+ "\tfloat shadow = currentDepth - bias > closestDepth  ? 0.98 : 0.02;\n"
					/*+"\t/* disabled shadow smoothing\n\tfloat shadow = 0.0;\n\tvec2 texelSize = 1.0 / textureSize(shadowMap, 0);\n\tfor(int x = -1; x <= 1; ++x)\n\t{\n\t\tfor(int y = -1; y <= 1; ++y)\n\t\t{\n\t\t\tfloat pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; \n\t\t\tshadow += currentDepth - bias > pcfDepth ? 0.98 : 0.02;        \n\t\t}    \n\t}\n\tshadow /= 9.0;\n\t
					 */
					+ "\treturn shadow;\n}";
			}

			//main function
			rawFragment += "void main()\n{\n";

			if (setting.TextureDiffuse)
			{
				rawFragment += "vec4 texcolor = texture(material.texture_diffuse,fs_in.Texcord);\n";
				if (setting.discardInvisible)
					rawFragment += "if (texcolor.a < 0.05)\n\t\tdiscard;\n";
			}

			//shadowing section
			if (setting.recieveShadow)
				rawFragment += "float shadow = ShadowCalculation(fs_in.lightSpace);\n";

			if (setting.affectedByLight)
			{
				rawFragment += "vec3 normal = normalize(fs_in.Normal);\n";
				rawFragment += "vec3 lightDir = normalize(LightPos - fs_in.FragPos);\n";
				rawFragment += "float diffuse = 1 - max(dot(lightDir,normal) , 0.02);\n";

				if (setting.toonShadow)
				{
					if (setting.recieveShadow)
						rawFragment += "vec4 shadowcolor  = texture(material.texture_toon,vec2(max(diffuse,shadow)));\n";
					else
						rawFragment += "vec4 shadowcolor  = texture(material.texture_toon,vec2(diffuse));\n";
				}
				else
				{
					if (setting.recieveShadow)
						rawFragment += "vec4 shadowcolor  = vec4(vec3(max(diffuse,shadow)),1.0);\n";
					else
						rawFragment += "vec4 shadowcolor  = vec4(vec3(diffuse),1.0);\n";
				}
			}
			else
			{
				if (setting.toonShadow)
				{
					if (setting.recieveShadow)
						rawFragment += "vec4 shadowcolor  = texture(material.texture_toon,vec2(shadow)));\n";
					else
						rawFragment += "vec4 shadowcolor  = texture(material.texture_toon,vec2(1.0));\n";
				}
				else
				{
					if (setting.recieveShadow)
						rawFragment += "vec4 shadowcolor  = vec4(vec3(1 - shadow),1.0);\n";
					else
						rawFragment += "vec4 shadowcolor  = vec4(1.0);\n";
				}

			}



				

			string output = "";
			if (setting.TextureDiffuse)
				output = "texcolor * shadowcolor";
			else if (!setting.TextureDiffuse)
				output = "shadowcolor";

				rawFragment += "FragColor = " + output + ";\n";

			rawFragment += "}\n";

			//MessageBox.Show(rawVertex);
			//Console.WriteLine(rawFragment);

		}

	}
}
