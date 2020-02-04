using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using BulletSharp;
//using BulletSharp.Math;
//using OpenTK.Input;
using OpenTK;
using Toys;

namespace ModelViewer
{
    class TestScript : ScriptingComponent
    {
        BoneController bc;
        Material mat;
        public static Texture2D texture;
        PhysicsEngine physics;
        bool active;
        int i = 0;
        Mesh mesh;
        MeshDrawer md;
        void Awake()
        {
            CreateNavMesh();

            physics = CoreEngine.pEngine;
            var canvas = (Canvas)Node.AddComponent<Canvas>();
            
            var ui0 = new UIElement();
            ui0.GetTransform.anchorMax = new Vector2(0f, 1f);
            ui0.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui0.GetTransform.offsetMax = new Vector2(400, 0);
            ui0.GetTransform.offsetMin = new Vector2(0, -400);
            canvas.Root = ui0;
            canvas.AddObject(ui0);
            var ui7 = new UIElement();
            ui7.GetTransform.anchorMax = new Vector2(0f, 1f);
            ui7.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui7.GetTransform.offsetMax = new Vector2(170, -40);
            ui7.GetTransform.offsetMin = new Vector2(20, -180);
            ui7.SetParent(ui0);
            var mask = (UIMaskComponent)ui7.AddComponent<UIMaskComponent>();
            //scrollbox test
            var ui = new UIElement();
            ui.GetTransform.anchorMax = new Vector2(0f, 1f);
            ui.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui.GetTransform.offsetMax = new Vector2(130, 0);
            ui.GetTransform.offsetMin = new Vector2(0, -200);
            ui.SetParent(ui7);
            var scrollBox = (ScrollBoxComponent)ui.AddComponent<ScrollBoxComponent>();
            scrollBox.Mask = mask;
            scrollBox.ScrollDirection = ScrollMode.Vertical;
            scrollBox.color.W = 0.3f;
            var ui1 = new UIElement();
            ui1.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui1.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui1.GetTransform.offsetMax = new Vector2(0, 0);
            ui1.GetTransform.offsetMin = new Vector2(0, -25);
            var image1 = (ButtonComponent)ui1.AddComponent<ButtonComponent>();
            
            var butLabel1 = (TextBox)ui1.AddComponent<TextBox>();
            butLabel1.textCanvas.colour = new Vector3(1, 0, 0);
            butLabel1.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            butLabel1.textCanvas.alignVertical = TextAlignVertical.Center;
            
            butLabel1.SetText("wind ON");
            butLabel1.textCanvas.Scale = 0.5f;

            image1.OnClick = () => { active = true; };
            var ui2 = new UIElement();
            ui2.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui2.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui2.GetTransform.offsetMax = new Vector2(0, -30);
            ui2.GetTransform.offsetMin = new Vector2(0, -55);
            var butLabel2 = (TextBox)ui2.AddComponent<TextBox>();
            
            butLabel2.SetText("wind OFF");
            butLabel2.textCanvas.Scale = 0.5f;

            butLabel2.textCanvas.colour = new Vector3(1, 1, 0);
            butLabel2.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            butLabel2.textCanvas.alignVertical = TextAlignVertical.Center;
            var image2 = (ButtonComponent)ui2.AddComponent<ButtonComponent>();

            image2.OnClick = () => { active = false; physics.SetGravity(new Vector3(0, -10, 0)); };
            //var ISS = (ImageStreamerScript)node.AddComponent<ImageStreamerScript>();
            //ISS.SetDSS(script);
            ui1.SetParent(ui);
            ui2.SetParent(ui);

            var ui3 = new UIElement();
            ui3.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui3.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui3.GetTransform.offsetMax = new Vector2(0, -60);
            ui3.GetTransform.offsetMin = new Vector2(0, -80);
            var slider1 = (SliderCompoent)ui3.AddComponent<SliderCompoent>();
            ui3.SetParent(ui);

            var ui4 = new UIElement();
            ui4.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui4.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui4.GetTransform.offsetMax = new Vector2(0, -85);
            ui4.GetTransform.offsetMin = new Vector2(0, -110);
            ui4.SetParent(ui);
            var checkbox = (CheckboxComponent)ui4.AddComponent<CheckboxComponent>();
            //checkbox.OnChange = () => Console.WriteLine(444);
            var butLabel4 = (TextBox)ui4.AddComponent<TextBox>();
            butLabel4.textCanvas.colour = new Vector3(1, 0, 0);
            butLabel4.textCanvas.alignHorizontal = TextAlignHorizontal.Left;
            butLabel4.textCanvas.alignVertical = TextAlignVertical.Center;
            butLabel4.textCanvas.colour = Vector3.Zero;
            butLabel4.textCanvas.Scale = 0.7f;
            butLabel4.SetText("checkbox");

            var ui5 = new UIElement();
            ui5.GetTransform.anchorMax = new Vector2(1f, 1f);
            ui5.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui5.GetTransform.offsetMax = new Vector2(0, -115);
            ui5.GetTransform.offsetMin = new Vector2(0, -140);
            ui5.SetParent(ui);
            var input = (TextInputComponent)ui5.AddComponent<TextInputComponent>();
            //debug textures
            /*
            var canvas1 = (Canvas)Node.AddComponent<Canvas>();
            var ui10 = new UIElement();
            ui10.GetTransform.anchorMax = new Vector2(0f, 1f);
            ui10.GetTransform.anchorMin = new Vector2(0f, 1f);
            ui10.GetTransform.offsetMax = new Vector2(400, 100);
            ui10.GetTransform.offsetMin = new Vector2(100, 400);
            canvas1.Root = ui10;

            var img =  ui10.AddComponent<RawImage>();
            img.Material.SetTexture(Node.scene.GetLight.shadowMap, TextureType.Diffuse);
            */
            /*
            var msd = (Animator)Node.GetComponent<Animator>();
            if (msd != null)
                bc = msd.BoneController;
            */



            //slider1.OnValueChanged = () => { active = false; physics.SetGravity(new Vector3(0, -10, 0)); };
        }

        void Start()
        {
            /*
            if (texture != null)
            {
                mat.SetTexture(texture, TextureType.Diffuse);
            }
            */
            //Console.WriteLine(bc.GetBone(13).LocalMatrix);
            //Console.WriteLine(bc.GetBone(13).World2BoneInitial);
            //Console.WriteLine(bc.GetBone(13).TransformMatrix);
            //Console.WriteLine(bc.GetBone(3).BoneMatrix);
            // bc.GetBone(0).SetTransform(new Quaternion(0, 0, 0), new Vector3(0, 0.5f, 0));

            //bc.GetBone(8).SetTransform(new Quaternion(0, 1.5f, 0), new Vector3(0, 0, 0));
            //bc.GetBone(21).SetTransform(new Quaternion(0, 1.5f, 0), new Vector3(0, 0, 0));
            //bc.GetBone(22).SetTransform(new Quaternion(0, 1.5f, 0), new Vector3(0, 0, 0));
            //bc.UpdateSkeleton();
            //Console.WriteLine(bc.GetBone(34).LocalMatrix);
            // Console.WriteLine(bc.GetBone(34).Bone2WorldInitial);
            // Console.WriteLine(bc.GetBone(34).TransformMatrix);
            /*
            CollisionShape shape = new SphereShape(0.1f);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0f, new DefaultMotionState(Matrix.Translation(new Vector3(0, 1, 0))), shape, Vector3.Zero);
            Body = new RigidBody(rbInfo);
            Body.ActivationState = ActivationState.DisableDeactivation;
            Body.Friction = 1.5f;
            Body.SetDamping(0.99f, 0.99f);
            Body.Restitution = 0.1f;
            Body.SetCustomDebugColor(new Vector3(0, 1, 0));
            World.AddRigidBody(Body);

            CollisionShape shape1 = new SphereShape(0.1f);
            Vector3 inertia1 = shape1.CalculateLocalInertia(0.05f);
            RigidBodyConstructionInfo rbInfo1 = new RigidBodyConstructionInfo(.05f, new DefaultMotionState(Matrix.Translation(new Vector3(0f, 1.5f, 0))), shape1, inertia1);
            RigidBody Body1 = new RigidBody(rbInfo1);
            Body1.ActivationState = ActivationState.DisableDeactivation;
            Body1.Friction = 1.5f;
            Body1.SetDamping(0.99f, 0.99f);
            Body1.Restitution = 0.1f;
            Body1.SetCustomDebugColor(new Vector3(1, 0, 0));
            World.AddRigidBody(Body1);


            CollisionShape shape2 = new SphereShape(0.1f);
            Vector3 inertia2 = shape2.CalculateLocalInertia(0.05f);
            RigidBodyConstructionInfo rbInfo2 = new RigidBodyConstructionInfo(.05f, new DefaultMotionState(Matrix.Translation(new Vector3(0.6f, 1.5f, 0))), shape2, inertia2);
            RigidBody Body2 = new RigidBody(rbInfo2);
            Body2.ActivationState = ActivationState.DisableDeactivation;
            Body2.Friction = 1.5f;
            Body2.SetDamping(0.99f, 0.99f);
            Body2.Restitution = 0.1f;
            Body2.SetCustomDebugColor(new Vector3(1, 0, 0));
            World.AddRigidBody(Body2);

            Matrix Conn1 = Matrix.Translation(new Vector3(0f, .25f, 0));
            Matrix Conn11 = Matrix.Translation(new Vector3(0f, -.25f, 0));

            Generic6DofSpringConstraint jointSpring6 = new Generic6DofSpringConstraint(Body, Body1, Conn1, Conn11,true);
            jointSpring6.AngularLowerLimit = new Vector3(0, 0, dec2rad(0));
            jointSpring6.AngularUpperLimit = new Vector3(0, 0, dec2rad(0));
            jointSpring6.LinearLowerLimit = Vector3.Zero;
            jointSpring6.LinearUpperLimit = Vector3.Zero;
            jointSpring6.EnableSpring(0, false);
            jointSpring6.EnableSpring(1, true);
            jointSpring6.EnableSpring(2, false);
            //jointSpring6.SetStiffness(1, 39.478f);
            //jointSpring6.SetDamping(1, 39.478f);
            jointSpring6.EnableSpring(3, false);
            jointSpring6.EnableSpring(4, false);
            //jointSpring6.SetParam(ConstraintParam.Erp, .9f);
            //jointSpring6.SetParam(ConstraintParam.StopCfm, 0f);
           // jointSpring6.SetParam(ConstraintParam.Cfm, 0f);
            jointSpring6.EnableSpring(5, false);
            jointSpring6.SetEquilibriumPoint();
            World.AddConstraint(jointSpring6, true);
            


            Matrix Conn2 = Matrix.Translation(new Vector3(-0.5f, 0, 0));
            Matrix Conn22 = Matrix.Translation(new Vector3(0.3f, 0, 0));
            Generic6DofSpringConstraint jointSpring61 = new Generic6DofSpringConstraint(Body1, Body2, Conn2, Conn22,true);
            jointSpring61.AngularLowerLimit = Vector3.Zero;
            jointSpring61.AngularUpperLimit = Vector3.Zero;
            jointSpring61.LinearLowerLimit = Vector3.Zero;
            jointSpring61.LinearUpperLimit = Vector3.Zero;
            //jointSpring61.SetParam(ConstraintParam.StopCfm, 0f);
            //jointSpring61.SetParam(ConstraintParam.Cfm, 0f);
            jointSpring61.SetEquilibriumPoint();
            World.AddConstraint(jointSpring61, true);
            */
        }

        void Update()
        {
            if (active)
            {
                i++;
                Vector3 force = new Vector3(0,-10,0);
                force.Z = (float)(6 + 4 * Math.Sin(i*2) +  5 * Math.Cos(i  * 0.4f + 24) + 3 * Math.Sin(i * 1.5f + 10) + 4 * Math.Cos(i * 0.1f + 76) + 3 * Math.Sin(i * 2.9f + 154));
                physics.SetGravity(force);
            }
            // update++;
            // bc.GetBone(3).SetTransform(new Quaternion(0, 0, (float)(dec2rad(45) * Math.Cos(update * 3 * Math.PI / 180))), new Vector3(0, 0, 0));
            //update += .UpdateTime * 1000;
            //Matrix world = Body.WorldTransform;
            //world.M42 = (float)(1 + 0.5 * Math.Cos(update * 3 * Math.PI / 180));
            //Body.MotionState.SetWorldTransform(ref world);
            //Body.WorldTransform = world;
        }

        float dec2rad(int grad)
        {
           return ((float)grad / 180 * (float)Math.PI);
        }

        void CreateNavMesh()
        {
            var data = new float[]
            {
                325.285675f,-0.341648f,-114.897858f,
325.285675f,-0.341650f,-98.587662f,
-48.270622f,-0.341696f,-114.897858f,
-48.270622f,-0.341697f,-98.587669f,
-48.270622f,-0.341701f,-37.839989f,
45.360939f,-0.341655f,-37.838974f,
-48.270622f,-0.341705f,32.774048f,
45.360939f,-0.341658f,32.774033f,
325.285675f,-0.341648f,-114.897858f,
325.285675f,-0.341648f,-119.348709f,
-48.270622f,-0.341695f,-119.348709f,
-48.270622f,-0.341701f,-37.839989f,
-48.270622f,-0.341701f,-42.519363f,
325.285675f,-0.341655f,-37.838974f,
325.285675f,-0.341654f,-42.519363f,
325.285675f,-0.341650f,-93.714401f,
-48.270622f,-0.341697f,-93.714401f,
325.285675f,-0.341650f,-97.281364f,
-48.270622f,-0.341697f,-97.281364f,
-48.270622f,-0.341701f,-42.519363f,
-48.270622f,-0.341697f,-93.714401f,
325.285675f,-0.341654f,-42.519363f,
325.285675f,-0.341650f,-93.714401f,
204.673660f,-0.341672f,-154.600311f,
263.570099f,-0.341668f,-154.600311f,
321.981628f,-0.341663f,-156.565887f,
284.045929f,-0.341665f,-154.600327f,
321.981445f,-0.341665f,-119.348709f,
148.846588f,-0.341679f,-119.348709f,
123.410881f,-0.341683f,-98.587669f,
347.164398f,-0.341665f,-98.587662f,
123.410881f,-0.341683f,-97.281357f,
347.164398f,-0.341665f,-97.281357f,
325.285675f,-0.331640f,-91.268257f,
-48.270622f,-0.331688f,-91.268265f,
325.285675f,-0.331641f,-92.118004f,
-48.270622f,-0.331687f,-92.118011f,
325.285675f,-0.331643f,-44.278732f,
-48.270622f,-0.331691f,-44.278725f,
325.285675f,-0.331643f,-45.128479f,
-48.270622f,-0.331690f,-45.128479f,
325.285675f,-0.331640f,-93.714401f,
-48.270622f,-0.331687f,-93.714401f,
325.285675f,-0.331640f,-97.281357f,
-48.270622f,-0.331687f,-97.281364f,
-48.270622f,-0.331685f,-114.897865f,
327.608459f,-0.331638f,-119.348709f,
-48.270622f,-0.331685f,-119.348709f,
-48.270622f,-0.331692f,-37.839981f,
-48.270622f,-0.331690f,-42.519356f,
325.285675f,-0.331644f,-37.838982f,
325.285675f,-0.331643f,-42.519356f,
204.673660f,-0.331662f,-154.600327f,
263.570099f,-0.331657f,-154.600327f,
148.846588f,-0.331669f,-119.348709f,
321.981445f,-0.331655f,-119.348709f,
284.045929f,-0.331656f,-154.600311f
            };

            int[] indexes = { 3, 2, 1, 2, 3, 4, 7, 6, 5, 6, 7, 8, 14, 13, 12, 13, 14, 15, 18, 17, 16, 17, 18, 19, 22, 21, 20, 21, 22, 23, 26, 27, 28, 25, 28, 27, 24, 28, 25, 28, 24, 29, 32, 31, 30, 31, 32, 33, 36, 35, 34, 35, 36, 37, 40, 39, 38, 39, 40, 41, 44, 43, 42, 43, 44, 45, 46, 47, 48, 51, 50, 49, 50, 51, 52, 56, 53, 55, 53, 56, 54, 54, 56, 57 };

            var vertex = new Vertex3D[data.Length / 3];
            for (int i = 0; i < vertex.Length; i++)
            {
                var point = new Vector3(data[i * 3], data[i * 3 + 1], data[i * 3 + 2]) / 60;
                point.Y -= 1;
                vertex[i] = new Vertex3D(point);
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                indexes[i] -= 1;
            }

            mesh = new Mesh(vertex, indexes);
            var Materials = new Material[indexes.Length / 3];
            var shdrst = new ShaderSettings();
            shdrst.Ambient = true;
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i] = new MaterialPMX(shdrst, new RenderDirectives());
                Materials[i].Offset = i * 3;
                Materials[i].Count = 3;
            }
            md = new MeshDrawer(mesh,Materials);
            Node.AddComponent(md);


            var navMesh = new NavigationMesh(vertex, indexes);
            var navList = navMesh.navigationCells.ToList();
            var cell1 = navMesh.GetCellFromPosition(new Vector3(0));
            var cell2 = navMesh.GetCellFromPosition(new Vector3(0.5f,0,-.6f));
            /*
            Materials[0].UniManager.Set("specular_color", specularColour);
            Materials[0].UniManager.Set("ambient_color", ambientColour);
            Materials[0].UniManager.Set("specular_power", specularPower);
            Materials[0].UniManager.Set("diffuse_color", difColor);
            */

            Materials[27].RenderDirrectives.IsRendered = false;
            Materials[25].RenderDirrectives.IsRendered = false;
            Materials[26].RenderDirrectives.IsRendered = false;



            /*
            //Console.WriteLine(navMesh.navigationCells[13].Center);
            foreach (var bord in navMesh.navigationCells[13].linkedCells)
                if (bord != null)
                    Console.WriteLine(bord.Index);
            */
            
            /*
            int indx = 0;
            foreach (var nav in navList)
            {
                int num = 0;
                foreach (var ls in nav.linkedCells)
                    if (ls != null)
                        num++;
                if (num == 2 && nav.Index > 27) {
                    indx = nav.Index;
                    break;
                }
            }
            
            Console.WriteLine(indx);     
            */

            int start = 11, end = 13;
            var searcher = new AStarSearch(navMesh);
            var result = searcher.CalculatePath(navMesh.navigationCells[start].Center, navMesh.navigationCells[end].Center);
            if (result != null)
            {
                Console.WriteLine(result.Length);
                foreach (var waipoint in result)
                {
                    Materials[waipoint.Index].UniManager.Set("ambient_color", Vector3.UnitY);
                    Console.WriteLine(waipoint.Index);
                }
            }
            Materials[start].UniManager.Set("ambient_color", Vector3.UnitX);
            Materials[end].UniManager.Set("ambient_color", Vector3.UnitZ);
        }
    }
}
