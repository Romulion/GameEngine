using System;
using IniParser;
using IniParser.Model;

namespace Toys
{
	public class Settings
	{
		static Settings settingInstance;
		public SettingsSystem System = new SettingsSystem();
		public SettingsGraphics Graphics = new SettingsGraphics();


		Settings()
		{
			LoadSettings();
		}

		public static Settings GetInstance()
		{
			if (settingInstance == null)
				settingInstance = new Settings();

			return settingInstance;
		}

		void LoadSettings()
		{
			
			var parser = new FileIniDataParser();
			IniData data;
			try
			{
				data = parser.ReadFile(System.ConfigPath);
			}
			catch (Exception)
			{
				return;
			}

			ReadGraphicsSettings(data);
		}

		void ReadGraphicsSettings(IniData data)
		{
			KeyDataCollection keys = data["Graphics"];
			if (keys == null)
				return;

			if (keys.ContainsKey("ShadowRes"))
			{
				int val;
				if (Int32.TryParse(keys["ShadowRes"], out val))
					Graphics.ShadowRes = val;
			}

			if (keys.ContainsKey("EnableShadow"))
			{
				int val;
				if (Int32.TryParse(keys["EnableShadow"], out val))
					Graphics.EnableShadow = (val == 1);
			}

		}

	}
}
