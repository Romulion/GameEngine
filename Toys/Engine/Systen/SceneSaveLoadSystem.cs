using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Toys
{
    public class SceneSaveLoadSystem
    {
        private static string extension = ".scene";

        public static void Save2File(string path)
        {

            var root = CoreEngine.MainScene.GetRootNodes();
            foreach (var node in root)
            {
                var componets = node.GetComponents();
                foreach (var component in componets)
                {
                    var attrs = component.Type.GetCustomAttributes();
                    foreach (var attr in attrs)
                        if (attr.GetType() == typeof(SerializableAttribute))
                            Console.WriteLine(component.Type);

                }
            }
        }

        public void LoadFromFile(string file)
        {
            
        }
    }
}
