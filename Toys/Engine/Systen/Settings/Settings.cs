using System;
using IniParser;
using IniParser.Model;

namespace Toys
{
	public class Settings
	{
		static Settings settingInstance;
		string settingsPathDefault = "";
		public SettingsSystem System = new SettingsSystem();
		public SettingsGraphics Graphics = new SettingsGraphics();


		Settings()
		{
		}

		void LoadSettings()
		{
			//var parser = new IniDataParser();
			//System.ConfigPath
		}



	}
}
