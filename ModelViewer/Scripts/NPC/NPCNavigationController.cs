﻿//#define DebugPath
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;

namespace ModelViewer
{
    /// <summary>
    /// Low level acess to character move data
    /// </summary>
    class NPCNavigationController
    {
        public LocationState CurrentLocation;
        NavigationMesh navMesh;
        Material[] Materials;
        Task<Vector3[]> pathTask;
        Vector3[] path;
        NavigationAgent navAgent;
        public bool IsWalking { get; private set; }
        int waypoint = 0;
        public float speed = 0.03f;
        Vector3 direction;
        float prevDist;
        Animator animator;
        public AnimationController AnimController { get; private set; }
        float keepDistance = 1.0f;
        public bool IsRotating { get; private set; }
        float roteteSpeed = Toys.MathHelper.ConvertGrad2Radians(5);
        bool IsStartImmedeatly = false;
        bool isKeepDistance = false;
        public bool IsTaskCompleted { get; private set; }
        Vector3 targetDirection = Vector3.Zero;
        Action OnComplete = null;
        //Debug
        List<SceneNode> pathPoints = new List<SceneNode>();
        SceneNode Node;

        public NPCNavigationController(SceneNode node)
        {
            Node = node;
            //CreateNavMesh();
            navMesh = NavigationMesh.GetInstance;
            navAgent = new NavigationAgent(navMesh);
            navAgent.AgentSize = .3f;
            IsTaskCompleted = true;
            var trans = Node.GetTransform.GlobalTransform;
            direction = new Vector3(trans.M13, trans.M23, trans.M33);
            try
            {
                CreateAnimationController();
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }


        public void SetDestination(Vector3 dest)
        {
            Clenapath();
            AnimController.SetFloat("speed", 0f);
            path = null;
            pathTask = navAgent.SearchPathAsync(Node.GetTransform.Position, dest);
        }

        public void Go()
        {
            if (path != null && path.Length > 0 && !IsWalking)
            {
                IsWalking = true;
                AnimController.SetFloat("speed", 1f);
            }
            else
                AnimController.SetFloat("speed", 0f);

        }

        public void GoImmedeatly(Vector3 dest, Vector3 faceDirection, Action onComplete = null, bool keepDistance = false)
        {
            Clenapath();
            IsStartImmedeatly = true;
            path = null;
            isKeepDistance = keepDistance;
            //check if at position
            if ((Node.GetTransform.Position - dest).LengthSquared > 0.0001f) //1cm
            {
                pathTask = navAgent.SearchPathAsync(Node.GetTransform.Position, dest);
            }
            targetDirection = faceDirection;
            OnComplete = onComplete;
            IsTaskCompleted = false;

#if DebugPath
            var point = CreatePoint(dest);
            pathPoints.Add(point);
            CoreEngine.MainScene.AddNode2Root(point);
#endif
        }

        public void Stop()
        {
            IsWalking = false;
            IsRotating = false;
            path = null;
            IsStartImmedeatly = false;
            OnComplete = null;
            IsTaskCompleted = true;
            AnimController.SetFloat("speed", 0f);
        }

        internal void Update()
        {
            if (AnimController.CurrentAnimation.Name != "Idle" && !IsWalking)
                return;
            if (IsRotating)
                RotateToDirection(direction);
            else if (pathTask != null && pathTask.IsCompleted)
            {
                path = pathTask.Result;
#if DebugPath
                foreach (var dest in path)
                {
                    var point = CreatePoint(dest);
                    pathPoints.Add(point);
                    CoreEngine.MainScene.AddNode2Root(point);
                }
#endif

                
                if (path.Length > 0)
                {
                    //UpdatePathColor();
                    waypoint = 0;
                    direction = path[0] - Node.GetTransform.Position;
                    prevDist = direction.Length;
                    direction.Normalize();
                    IsRotating = true;

                    //stop before target
                    Vector3 dest = Vector3.Zero;
                    if (path.Length > 1)
                        dest = path[path.Length - 1] - path[path.Length - 2];
                    else
                        dest = direction;
                    dest.Normalize();
                    if (isKeepDistance)
                        path[^1] -= dest * keepDistance;

                    if (IsStartImmedeatly)
                    {
                        Go();
                        IsStartImmedeatly = false;
                    }

                }
                else
                {
                    Logger.Warning("path creation failed");
                }
                pathTask = null;
            }
            else if (path != null && IsWalking)
            {
                var coord = Node.GetTransform.Position;
                path[waypoint].Y = 0;
                var distance = (coord - path[waypoint]).Length;
                if (distance < 0.01f || prevDist < distance)
                {
                    //New waipoint
                    if (waypoint < path.Length - 1)
                    {
                        waypoint++;
                        Vector3 dir = path[waypoint] - Node.GetTransform.Position;
                        prevDist = dir.Length;
                        dir.Normalize();
                        direction = dir;
                        IsRotating = true;
                    }
                    //Path complete
                    else
                    {
                        AnimController.SetFloat("speed", 0f);
                        path = null;
                        IsWalking = false;

                        if (targetDirection != Vector3.Zero)
                        {
                            direction = targetDirection;
                            targetDirection = Vector3.Zero;
                            IsRotating = true;
                        }
                        else
                        {
                            OnComplete?.Invoke();
                            IsTaskCompleted = true;
                        }
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
            //rotate bo desired direction
            else if (path == null && pathTask == null && targetDirection != Vector3.Zero)
            {
                direction = targetDirection;
                IsRotating = true;
                targetDirection = Vector3.Zero;
            }
            //finish task
            else if (!IsTaskCompleted && path == null && pathTask == null)
            {
                OnComplete?.Invoke();
                Stop();
            }

        }
        
        public void RotateCharacter(Vector3 dir)
        {
            direction = dir;
            IsRotating = true;
        }

        void RotateToDirection(Vector3 dir)
        {
            dir.Y = 0;
            Vector3 look = new Vector3(Node.GetTransform.GlobalTransform.M31, Node.GetTransform.GlobalTransform.M32, Node.GetTransform.GlobalTransform.M33);
            look.Normalize();
            Vector3 axis = Vector3.Cross(look, dir);
            float dot = Vector3.Dot(dir, look);
            float angle = MathF.Atan2(axis.Length, dot);
            if (MathF.Abs(angle) < roteteSpeed)
            {
                IsRotating = false;
                /*
                if (path == null && Vector3.Dot(direction, targetDirection) > 0.99f)
                {
                    targetDirection = Vector3.Zero;
                    IsTaskCompleted = true;
                    OnComplete?.Invoke();
                }
                */
            }
            else
                angle = (angle < 0) ? -roteteSpeed : roteteSpeed;
            var rotation = Quaternion.FromAxisAngle(axis, angle);
            Node.GetTransform.RotationQuaternion *= rotation;
        }


        void UpdatePathColor()
        {
            if (navAgent.pathMesh == null)
                return;
            foreach (var mat in Materials)
                mat.UniformManager.Set("ambient_color", Vector3.Zero);


            foreach (var waipoint in navAgent.pathMesh)
            {
                
                Materials[waipoint.Index].UniformManager.Set("ambient_color", Vector3.UnitY);
            }
        }

        /*
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

        */


        void CreateAnimationController()
        {
            animator = Node.GetComponent<Animator>();

            

            var idle = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\standIdle.vmd"));
            var walk = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\walk.vmd"));
            var sitDown = new AnimationNode(ResourcesManager.LoadAsset <Animation>(@"Assets\Animations\sitEnter.vmd"));
            var sitIdle = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\sit.vmd"));
            var standUp = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\sitExit.vmd"));

            idle.Name = "Idle";
            walk.Name = "Walk";
            sitDown.Name = "Sitdown";
            sitIdle.Name = "Sit";
            standUp.Name = "Standup";

            sitDown.Repeat = false;
            standUp.Repeat = false;
            sitDown.NextAnimation = sitIdle;
            standUp.NextAnimation = idle;

            AnimController = new AnimationController(idle);
            var idleWalkTransit = new AnimationTransition((anim) => anim.GetFloat("speed") > 0, walk);
            var walkIdleTransit = new AnimationTransition((anim) => anim.GetFloat("speed") == 0, idle);
            var idleSitTransit = new AnimationTransition((anim) => anim.GetBool("sit"), sitDown);
            sitIdle.Transitions.Add(new AnimationTransition((anim) => !anim.GetBool("sit"), standUp));
            idle.Transitions.Add(idleWalkTransit);
            idle.Transitions.Add(idleSitTransit);
            walk.Transitions.Add(walkIdleTransit);

            AnimController.AddAnimation(idle);
            AnimController.AddAnimation(walk);
            AnimController.AddAnimation(sitDown);
            AnimController.AddAnimation(sitIdle);
            AnimController.AddAnimation(standUp);

            animator.Controller = AnimController;

            /*
            var standPity = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\stand-pity.vmd"));
            var pity = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\pity.vmd"));
            var pityStand = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\pity-stand.vmd"));
            standPity.NextAnimation = pity;
            pityStand.NextAnimation = idle;
            standPity.Repeat = false;
            pityStand.Repeat = false;
            idle.Transitions.Add(new AnimationTransition((anim) => anim.GetBool("pity"), standPity));
            pity.Transitions.Add(new AnimationTransition((anim) => !anim.GetBool("pity"), pityStand));
            */

            AddAnim(@"stand-pity.vmd", @"pity.vmd", @"pity-stand.vmd", "pity",AnimController);

            AddAnim(@"stand2crouch.vmd", @"crouch.vmd", @"crouch2stand.vmd", "crouch", AnimController);

            AddAnim(@"handSideStandEnter.vmd", "handSideStand.vmd", "handSideStandExit.vmd", "handSideStand", AnimController);

            AddAnim(@"standHandcrossEnter.vmd", "standHandcross.vmd", "standHandcrossExit.vmd", "standHandcross", AnimController);

            AddAnim(@"standLeanEnter.vmd", "standLean.vmd", "standLeanExit.vmd", "standLean", AnimController);

            AddAnim(@"sitLeanEnter.vmd", "sitLean.vmd", "sitLeanExit.vmd", "sitLean", AnimController, sitIdle);

            AddAnim(@"standSkirtCoverEnter.vmd", "standSkirtCover.vmd", "standSkirtCoverExit.vmd", "standSkirtCover", AnimController);

            AddAnim(@"standShockEnter.vmd", "standShock.vmd", "standShockExit.vmd", "standShock", AnimController);
            
            void AddAnim(string transit1, string main, string transit2, string trigger, AnimationController AController, AnimationNode parent = null)
            {
                if (parent == null)
                    parent = idle;
                var transTo = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\" + transit1));
                var mainA = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\" + main));
                var transBack = new AnimationNode(ResourcesManager.LoadAsset<Animation>(@"Assets\Animations\" + transit2));
                transTo.NextAnimation = mainA;
                transBack.NextAnimation = parent;
                transTo.Repeat = false;
                transBack.Repeat = false;
                parent.Transitions.Add(new AnimationTransition((anim) => anim.GetBool(trigger), transTo));
                mainA.Transitions.Add(new AnimationTransition((anim) => !anim.GetBool(trigger), transBack));
                AnimController.AddAnimation(transTo);
                AnimController.AddAnimation(mainA);
                AnimController.AddAnimation(transBack);

                transTo.Name = transit1;
                mainA.Name = main;
                transBack.Name = transit2;
            }
        }


        #region Debug Path
        void Clenapath()
        {
            foreach (var poi in pathPoints)
            {
                CoreEngine.MainScene.RemoveNode(poi);
                ResourcesManager.DeleteResource(poi);
            }
        }

        SceneNode CreatePoint(Vector3 loc)
        {
            var canvas = new SceneNode();
            canvas.GetTransform.Position = loc;
            var textboxCanvas = canvas.AddComponent<Canvas>();
            var root = new UIElement();
            textboxCanvas.Add2Root(root);
            root.AddComponent<RawImage>();

            var text = (TextBox)root.AddComponent<TextBox>();
            var rect = root.GetTransform;
            rect.anchorMax = new Vector2(0, 0);
            rect.anchorMin = new Vector2(0, 0);
            rect.offsetMin = new Vector2(-50, -50);
            rect.offsetMax = new Vector2(50, 50);
            text.Text = "T";
            text.textCanvas.colour = Vector3.Zero;
            text.textCanvas.alignVertical = TextAlignVertical.Center;
            text.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            textboxCanvas.Mode = Canvas.RenderMode.WorldSpace;
            textboxCanvas.Canvas2WorldScale = 0.0025f;

            return canvas;
        }
        #endregion
        /*
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
         */

    }
}
