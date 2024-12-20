using System;

namespace Toys
{
	public class ShaderConstructor
	{
		ShaderSettings setting;
		string rawVertex = "";
		string rawFragment = "";

		internal ShaderConstructor(ShaderSettings stng)
		{
			setting = stng;
		}

        ShaderConstructor()
        {
        }

        public static Shader CreateShader(string vs, string fs)
        {
            var constructor = new ShaderConstructor();
            constructor.rawVertex = vs;
            constructor.rawFragment = fs;
            return constructor.Creator();
        }

        public static Shader CreateShader(ShaderSettings settings)
		{
			var constructor = new ShaderConstructor(settings);
			constructor.rawVertex = constructor.GenerateVertex();
			constructor.rawFragment = constructor.GenerateFragment();
			return constructor.Creator();
		}

		Shader Creator()
		{
			Shader shdr = null;
			try
			{
				shdr = new ShaderMain(rawVertex, rawFragment);
			}
			catch (Exception e)
			{
				Logger.Error(e.Message, e.Source);
				return null;
            }

			//binding buffers
			var ubm = UniformBufferManager.GetInstance;
			var ubs = ubm.GetBuffer("skeleton");
			shdr.SetUBO(ubs.BufferIndex, "skeleton");
			var ubsp = ubm.GetBuffer("space");
			shdr.SetUBO(ubsp.BufferIndex, "space");
			var ubl = ubm.GetBuffer("light");
			shdr.SetUBO(ubl.BufferIndex, "light");

            var ubsy = ubm.GetBuffer("system");
            shdr.SetUBO(ubsy.BufferIndex, "system");

            //bind textures
            shdr.ApplyShader();

            shdr.SetUniform((int)TextureType.ShadowMap, "shadowMap");
            shdr.SetUniform((int)TextureType.Diffuse, "material.texture_diffuse");
			shdr.SetUniform((int)TextureType.Toon, "material.texture_toon");
			shdr.SetUniform((int)TextureType.Specular, "material.texture_specular");
			shdr.SetUniform((int)TextureType.Sphere, "material.texture_spere");
			shdr.SetUniform((int)TextureType.Extra, "material.texture_extra");

			return shdr;
		}

		public string GenerateVertex()
		{

			string rawVertex = "";
			rawVertex += "#version 330 core\n";

			rawVertex += "layout (location = 0) in vec3 aPos;\n"
						+"layout (location = 1) in vec3 aNormal;\n"
						+"layout (location = 2) in vec2 aTexcord;\n"
                        +"layout (location = 3) in ivec4 BoneIDs;\n"
						+"layout (location = 4) in vec4 Weights;\n";

			rawVertex += "\n";

			//setting uniform buffers
			rawVertex += "layout (std140) uniform skeleton\n{\n    mat4 gBones[500];\n};\n";
			rawVertex += "layout (std140) uniform space {\n\tmat4 model;\n\tmat4 pvm;\n\tmat4 NormalMat;\n\tmat4 lightSpacePos;\n\tmat4 pv;\n\tmat4 vm;\n};\n";

			//setting out structure
			rawVertex += "out VS_OUT {\n\tvec2 Texcord;\n\tvec3 FragPos;\n\tvec3 Normal;\n\tvec4 lightSpace;\n\tvec3 NormalLocal;\n} vs_out;\n";

			//main body
			rawVertex += "void main()\n{\n";

			string applySkeleton = "";
#if VertexSkin
            if (setting.hasSkeleton)
			{
				rawVertex += "mat4 BoneTransform = gBones[BoneIDs[0]] * Weights[0];\n"
							+"\tBoneTransform += gBones[BoneIDs[1]] * Weights[1];\n"
							+"\tBoneTransform += gBones[BoneIDs[2]] * Weights[2];\n"
							+"\tBoneTransform += gBones[BoneIDs[3]] * Weights[3];\n";
				applySkeleton = " * BoneTransform";
			}
#endif
            rawVertex += "gl_Position =  pvm"+ applySkeleton +" * vec4(aPos, 1.0);\n";

			rawVertex += "vs_out.Texcord = aTexcord;\n";
			rawVertex += "vs_out.FragPos = vec3(model"+ applySkeleton +" * vec4(aPos, 1.0));\n";
			rawVertex += "vs_out.Normal = mat3(NormalMat"+ applySkeleton +") * aNormal;\n";
			rawVertex += "vs_out.lightSpace = lightSpacePos * vec4(vs_out.FragPos,1.0);\n";

			if (setting.EnvType > 0)
				rawVertex += "vs_out.NormalLocal = mat3(vm"+ applySkeleton +") * aNormal;\n";

			rawVertex += "}\n";

			return rawVertex;
		}

        public string GenerateFragment()
		{
			string rawFragment = "";
			rawFragment += "";
			rawFragment += "#version 330 core\n";
			rawFragment += "out vec4 FragColor;\n";

			rawFragment += "in VS_OUT {\n\tvec2 Texcord;\n\tvec3 FragPos;\n\tvec3 Normal;\n\tvec4 lightSpace;\n\tvec3 NormalLocal;\n} fs_in;\n";

			rawFragment += "layout (std140) uniform light {\n\tvec3 LightPos;\n\tvec3 viewPos;\n\tfloat near_plane;\n\tfloat far_plane;\n};\n";

			//setting material specific parameters
			rawFragment += "struct Material{\n"
                        +"\tsampler2D texture_diffuse;\n"
                        +"\tsampler2D texture_specular;\n"
                        +"\tsampler2D texture_toon;\n"
                        +"\tsampler2D texture_spere;\n"
						+ "\tsampler2D texture_extra;\n"
						+ "\tvec4 diffuse_color;\n"
                        + "\tvec3 specular_color;\n"
                        + "\tfloat specular_power;\n"
                        + "\tvec3 ambient_color;\n"
                        + "};\n";

			rawFragment += "uniform Material material;\n";
            rawFragment += "uniform sampler2DShadow shadowMap;\n";

			if (setting.RecieveShadow)
			{
				/*
				rawFragment += "float LinearizeDepth(float depth)\n"
					+ "{\n"
					+ "\tfloat z = depth * 2.0 - 1.0;\n"
					+ "\treturn (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));\n"
					+ "}\n";
				*/
				rawFragment += "float ShadowCalculation(vec3 normal, vec3 lightDir)\n"
                    + "{\n"
					//+ "\tfloat bias = 0.006;\n"
                    + "\tfloat bias = max(0.01 * (1.0 - dot(normal, lightDir)), 0.005);"
                    + "\tvec3 projCoords = fs_in.lightSpace.xyz / fs_in.lightSpace.w;\n"
                    + "\tprojCoords = projCoords * 0.5 + 0.5;\n"
					+ "\tprojCoords.z -= bias;"
					+ "\tfloat shadow = 1 - texture(shadowMap, projCoords);\n"
					/*
					+ "\tfloat closestDepth = texture(shadowMap, projCoords.xy).r;\n"
					+ "float currentDepth = projCoords.z;\n"
					+ "\tfloat shadow = currentDepth - bias > closestDepth  ? 0.98 : 0.02;\n"
					//+"\t/* disabled shadow smoothing\n\tfloat shadow = 0.0;\n\tvec2 texelSize = 1.0 / textureSize(shadowMap, 0);\n\tfor(int x = -1; x <= 1; ++x)\n\t{\n\t\tfor(int y = -1; y <= 1; ++y)\n\t\t{\n\t\t\tfloat pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; \n\t\t\tshadow += currentDepth - bias > pcfDepth ? 0.98 : 0.02;        \n\t\t}    \n\t}\n\tshadow /= 9.0;\n\t
					 */
					+ "\treturn shadow;\n}\n";
			}

			//main function
			rawFragment += "void main()\n{\n";
            
            rawFragment += "const vec3 amb = vec3(0.07);\n";

            if (setting.TextureDiffuse)
			{
				rawFragment += "vec4 texcolor = texture(material.texture_diffuse,fs_in.Texcord);\n";
				if (setting.DiscardInvisible)
					rawFragment += "if (texcolor.a < 0.05)\n\t\tdiscard;\n";
			}

            rawFragment += "vec3 normal = normalize(fs_in.Normal);\n";
            rawFragment += "vec3 lightDir = normalize(LightPos - fs_in.FragPos);\n";

            
            if (setting.SpecularColor)
            {
                rawFragment += "vec3 viewDir = normalize(viewPos - fs_in.FragPos);\n";
                rawFragment += "vec3 reflectDir = reflect(-lightDir, normal);\n";
                rawFragment += "float spec = pow(max(dot(viewDir, reflectDir), 0.00001), material.specular_power);\n";
            }


            //shadowing section
            if (setting.RecieveShadow)
				rawFragment += "float shadow = ShadowCalculation(normal,lightDir);\n";


			if (setting.AffectedByLight)
			{
                rawFragment += "float diffuse = 1 - max(dot(lightDir,normal) , 0.02);\n";
                if (setting.ToonShadow)
				{
					if (setting.RecieveShadow)
						rawFragment += "vec3 shadowcolor  = texture(material.texture_toon,vec2(max(diffuse,shadow))).xyz * 0.6 + vec3(0.4);\n";
					else
						rawFragment += "vec3 shadowcolor  = texture(material.texture_toon,vec2(diffuse)).xyz * 0.6 + vec3(0.4);\n";
				}
				else
				{
					if (setting.RecieveShadow)
						rawFragment += "vec3 shadowcolor  = vec3(max(diffuse,shadow) * 0.2 + 0.8);\n";
					else
						rawFragment += "vec3 shadowcolor  = vec3(diffuse * 0.2 + 0.8);\n";
				}
			}
			else
			{

				if (setting.RecieveShadow)
                {
                    if (setting.ToonShadow)
                    {
                        rawFragment += "vec3 shadowcolor  = texture(material.texture_toon,vec2(shadow))).xyz;\n";
                    }
						
					else
                        rawFragment += "vec3 shadowcolor  = vec3(1 - shadow* 0.2);\n";
				}
				else
				{
					rawFragment += "vec3 shadowcolor  = vec3(1.0);\n";
				}

			}

			if (setting.EnvType > 0)
			{
				rawFragment += "vec4 envLight = texture(material.texture_spere,vec2(0.5) -normalize(fs_in.NormalLocal).xy * 0.5);\n";
				if (setting.EnvType == EnvironmentMode.Additive || setting.EnvType == EnvironmentMode.Subtract)
					rawFragment += "envLight.w = 0;\n";
				else
					rawFragment += "envLight.w = 1;\n";
			}


            string output = "vec4(clamp(amb";
            if (setting.DifuseColor)
                output += "+ material.diffuse_color.xyz * 0.5";
            if (setting.Ambient)
                output += "+ material.ambient_color";
            if (setting.SpecularColor)
                output += "+ abs(material.specular_color * spec)";

			if (setting.DifuseColor)
				output += ",0.0,1.0),material.diffuse_color.w)";
			else
				output += ",0.0,1.0),1)";

            output += " * (";
            if (setting.TextureDiffuse)
				output += "texcolor";
				

            string mul = "";

            if (setting.EnvType == EnvironmentMode.Additive)
                output += " + envLight";
            else if (setting.EnvType == EnvironmentMode.Subtract)
                output += " - envLight";            
            else if (setting.EnvType == EnvironmentMode.Multiply)
                mul += " * envLight";
            output += ") * texture(material.texture_extra, fs_in.Texcord) * vec4(shadowcolor,1)";

            rawFragment += "FragColor = (" + output + ") " + mul  + ";\n";         
            rawFragment += "}\n";
			//MessageBox.Show(rawFragment);
			//Console.ReadKey();
			//Console.WriteLine(rawFragment);

			return rawFragment;
        }

	}
}
