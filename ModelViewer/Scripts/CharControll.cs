using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK;

namespace ModelViewer
{
    class CharControll : ScriptingComponent
    {
        public NavigationMesh navMesh;
        public Material[] Materials;
        Task<Vector3[]> pathTask;
        Vector3[] path;
        NavigationAgent navAgent;
        bool isWalking;
        int waipoint = 0;
        public float speed = 0.03f;
        Vector3 direction;
        float prevDist;
        Animation walk;
        Animation idle;
        Animator animator;


        void Start()
        {
            //CreateNavMesh();
            navAgent = new NavigationAgent(navMesh);
            navAgent.AgentSize = 1f;
            
            try
            {
                walk = AnimationLoader.Load("walk.lmd");
                idle = AnimationLoader.Load("idle.lmd");
                animator = Node.GetComponent<Animator>() as Animator;
                animator.AnimationData = idle;
                animator.Play();
            }
            catch (Exception) { }
        }


        public void SetDestination(Vector3 dest)
        {
            WalkingState(false);
            path = null;
            pathTask = navAgent.SearchPathAsync(Node.GetTransform.Position,dest);
            Console.WriteLine("path calc started");
        }

        public void Go()
        {
            if (path != null && path.Length > 0 && !isWalking)
                WalkingState(true);
            else
                WalkingState(false);
        }

        void Update()
        {
            if (pathTask != null && pathTask.IsCompleted)
            {

                path = pathTask.Result;


                if (path.Length > 0)
                {
                    UpdatePathColor();
                    waipoint = 0;
                    direction = path[0] - Node.GetTransform.Position;
                    prevDist = direction.Length;
                    direction.Normalize();
                    RotateToDirection(direction);
                    Console.WriteLine("path calc completed");
                }
                else
                {
                   Console.WriteLine("path creation failed");
                }
                pathTask = null;
            }

            if (path != null && isWalking)
            {
                var coord = Node.GetTransform.Position;
                var distance = (coord - path[waipoint]).Length;
                if (distance < 0.06f || prevDist < distance)
                {
                    if (waipoint < path.Length - 1)
                    {
                        waipoint++;
                        Vector3 dir = path[waipoint] - Node.GetTransform.Position;
                        prevDist = dir.Length;
                        dir.Normalize();
                        direction = dir;
                        RotateToDirection(dir);
                    }
                    else
                    {
                        WalkingState(false);
                        path = null;
                    }
                    

                }
                else
                {
                    direction.Y = 0;
                    var move = (distance < speed) ? distance : speed;
                    Node.GetTransform.Position += direction * move;
                    prevDist = distance;
                }

                
                
            }

        }

        void RotateToDirection(Vector3 dir)
        {
            dir.Y = 0;
            //according to Rodrigues rotation formula
            Vector3 look = new Vector3(Node.GetTransform.GlobalTransform.M31, Node.GetTransform.GlobalTransform.M32, Node.GetTransform.GlobalTransform.M33);
            look.Normalize();
            
            Vector3 axis = Vector3.Cross(look, dir);
            //Matrix4 crossMAt = new Matrix4(new Matrix3(0, -axis.Z, axis.Y, axis.Z, 0, -axis.X, -axis.Y, axis.X, 0));
            // var rotation = Matrix4.Identity + crossMAt + crossMAt * crossMAt * (1 / (1 + Vector3.Dot(look.Xyz, dir)));
            // Node.GetTransform.RotationQuaternion *= rotation.ExtractRotation();
            var rotation = Quaternion.FromAxisAngle(axis, (float)Math.Acos(Vector3.Dot(dir, look)));
            Node.GetTransform.RotationQuaternion *= rotation;
            Console.WriteLine(look);
            Console.WriteLine(dir);
            Console.WriteLine(Node.GetTransform.RotationQuaternion);
        }


        void UpdatePathColor()
        {
            if (navAgent.pathMesh == null)
                return;
            foreach (var mat in Materials)
                mat.UniManager.Set("ambient_color", Vector3.Zero);

            foreach (var waipoint in navAgent.pathMesh)
            {
                Materials[waipoint.Index].UniManager.Set("ambient_color", Vector3.UnitY);
            }
        }

        void WalkingState(bool state)
        {
            if (isWalking && !state)
            {
                isWalking = false;
                if (animator)
                {
                    animator.AnimationData = idle;
                    animator.Play();
                }
            }
            else if (!isWalking && state)
            {
                isWalking = true;
                if (animator)
                {
                    animator.AnimationData = walk;
                    animator.Play();
                }
            }

        }

        void CreateNavMesh()
        {
            var data = new float[]
            {
                -0.510708f,0.000000f,0.560116f,
0.475209f,0.000000f,0.560116f,
-0.510708f,0.000000f,0.223784f,
0.475209f,0.000000f,0.223784f,
-0.510708f,0.000000f,0.560116f,
0.475209f,0.000000f,0.560116f,
-0.498859f,0.000000f,1.604937f,
0.487058f,0.000000f,1.604937f,
-0.503560f,0.000000f,2.655539f,
0.482357f,0.000000f,2.655539f,
-0.487180f,-0.147305f,3.284405f,
0.498737f,-0.147305f,3.284405f,
-1.871698f,0.041424f,2.636110f,
-1.855318f,-0.105881f,3.264977f,
-2.698119f,0.000962f,2.633322f,
-2.681739f,-0.146342f,3.262189f,
-1.869888f,0.000000f,2.081351f,
-2.696310f,0.000000f,2.078564f,
-1.867582f,0.000000f,1.374484f,
-2.694004f,0.000000f,1.371696f,
-1.863584f,0.000000f,0.200000f,
-2.690005f,0.000000f,0.145856f,
-1.834403f,0.000000f,-0.430380f,
-2.660824f,0.000000f,-0.430000f,
0.496814f,-0.085523f,-1.081090f,
-0.502801f,0.000000f,-0.437349f,
-0.489103f,-0.085523f,-1.081090f,
0.483116f,0.000000f,-0.437349f,
0.511144f,-0.174994f,-1.754542f,
-0.474773f,-0.174994f,-1.754542f,
-1.334424f,-0.129101f,-1.093288f,
-1.320094f,-0.218572f,-1.766740f
            };

            int[] indexes = { 2, 3, 1, 1, 6, 2, 6, 7, 8, 7, 10, 8, 10, 11, 12, 9, 14, 11, 13, 16, 14, 15, 17, 18, 18, 19, 20, 20, 21, 22, 21, 24, 22, 23, 3, 26, 4, 26, 3, 28, 27, 26, 25, 30, 27, 30, 31, 27, 2, 4, 3, 1, 5, 6, 6, 5, 7, 7, 9, 10, 10, 9, 11, 9, 13, 14, 13, 15, 16, 15, 13, 17, 18, 17, 19, 20, 19, 21, 21, 23, 24, 23, 21, 3, 4, 28, 26, 28, 25, 27, 25, 29, 30, 30, 32, 31 };

            var vertex = new Vertex3D[data.Length / 3];
            for (int i = 0; i < vertex.Length; i++)
            {
                var point = new Vector3(data[i * 3], data[i * 3 + 1], data[i * 3 + 2]) * 4;
                point.Y = 0;
                vertex[i] = new Vertex3D(point);
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                indexes[i] -= 1;
            }

            Mesh mesh = new Mesh(vertex, indexes);
            Materials = new Material[indexes.Length / 3];
            var shdrst = new ShaderSettings();
            shdrst.Ambient = true;
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i] = new MaterialPMX(shdrst, new RenderDirectives());
                Materials[i].Offset = i * 3;
                Materials[i].Count = 3;
            }
            MeshDrawer md = new MeshDrawer(mesh, Materials);
            Node.AddComponent(md);
            navMesh = new NavigationMesh(vertex, indexes);
        }
    }
}
