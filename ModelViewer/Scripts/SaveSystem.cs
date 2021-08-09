using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using Toys;

namespace ModelViewer
{
    class SaveSystem
    {
        Logger logger = new Logger("SaveSystem");

        private Save CreateSave()
        {
            Save save = new Save();

            var camera = CoreEngine.GetCamera.Node.GetTransform;
            save.cameraPos = camera.Position;
            save.cameraRot = camera.RotationQuaternion;

            var nodes = CoreEngine.MainScene.FindByName("Michelle.Seifuku");
            if (nodes.Length > 0)
            {
                save.charPos = nodes[0].GetTransform.Position;
                save.charRot = nodes[0].GetTransform.RotationQuaternion;
            }

            return save;
        }

        private void LoadSave(Save save)
        {

            var camera = CoreEngine.GetCamera.Node.GetTransform;
            camera.Position = save.cameraPos;
            camera.RotationQuaternion = save.cameraRot;
            Console.WriteLine(save.cameraPos);
            Console.WriteLine(save.cameraRot);
            var nodes = CoreEngine.MainScene.FindByName("Michelle.Seifuku");
            if (nodes.Length > 0)
            {
                nodes[0].GetTransform.Position = save.charPos;
                nodes[0].GetTransform.RotationQuaternion = save.charRot;

                var phys = nodes[0].GetComponent<PhysicsManager>();
                phys.ReinstalizeBodys();
            }
        }

        public void SaveGame()
        {
            try
            {
                Save save = CreateSave();
                var fileName = Environment.CurrentDirectory + "/gamesave.save";
                var jsonText = JsonSerializer.Serialize(save);
                File.WriteAllText(fileName, jsonText);
                logger.Info("Game Saved", "");
            }
            catch (Exception e)
            {
                logger.Error("Save Failed: " + e.Message, "");
            }
        }

        public void LoadGame()
        {

            try
            {
                var jsonText = File.ReadAllText(Environment.CurrentDirectory + "/gamesave.save");
                var save = JsonSerializer.Deserialize<Save>(jsonText);

                LoadSave(save);
                logger.Info("Game Loaded","");
            }
            catch (Exception e)
            {
                logger.Error("Load Failed: " + e.Message,"");
            }
        }

    }
}
