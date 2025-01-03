﻿using System.IO;
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

            var palyer = CoreEngine.GetCamera.Node.Parent.GetTransform;
            save.playerPos = palyer.Position;
            save.playerRot = palyer.RotationQuaternion;
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
            var player = CoreEngine.GetCamera.Node.Parent.GetTransform;
            var ccp = player.Node.GetComponent<CharacterControllPlayer>();
            /*
            var dir = new OpenTK.Mathematics.Vector4(0, 0, -1, 1);
            var newdir = (dir * camera.GlobalTransform).Xyz - camera.Position;
            Console.WriteLine(newdir);
            */
            ccp.LoadPos(save.playerPos);
            player.Position = save.playerPos;
            player.RotationQuaternion = save.playerRot;
            player.Node.UpdateTransform();

            var cps = CoreEngine.GetCamera.Node.GetComponent<CameraPOVScript>();
            cps.RecalculateAngles();
            var nodes = CoreEngine.MainScene.FindByName("Michelle.Seifuku");
            if (nodes.Length > 0)
            {
                nodes[0].GetTransform.Position = save.charPos;
                nodes[0].GetTransform.RotationQuaternion = save.charRot;

                var phys = nodes[0].GetComponent<PhysicsManager>();

                CoreEngine.ActiveCore.AddTask = () => phys.ReinstalizeBodys();
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
