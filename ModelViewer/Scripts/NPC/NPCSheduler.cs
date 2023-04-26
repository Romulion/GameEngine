using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace ModelViewer
{
    /// <summary>
    /// Character hight level tasking
    /// </summary>
    class NPCSheduler
    {
        readonly SceneTime sceneTime;
        readonly NPCController npcController;
        readonly NPCNavigationSystem navigationSystem;        
        NPCActivity currentTask;
        ActivityState taskState;
        NPCBasicAction currentAction;
        int actionId = 0;


        public List<NPCActivity> Activities = new List<NPCActivity>();
        bool moving = false;
        public NPCSheduler(NPCController controller)
        {
            npcController = controller;
            sceneTime = CoreEngine.Shared.ScriptHolder.GetComponent<SceneTime>();
            navigationSystem = CoreEngine.Shared.ScriptHolder.GetComponent<NPCNavigationSystem>();
        }

        internal void Update()
        {
            if (taskState == ActivityState.Prepearing)
            {
                //play next action
                if (currentAction.IsCompleted(npcController) && ++actionId < currentTask.Actions.Count)
                {
                    currentAction = currentTask.Actions[actionId];
                    currentAction.Start(npcController);
                }
                //stop if completed
                else if (actionId >= currentTask.Actions.Count)
                {
                    taskState = ActivityState.Plaing;
                    actionId--;
                    currentAction = null;
                }
            }
            //reverse
            else if (taskState == ActivityState.Finishing)
            {
                //play next action
                if (!currentAction.IsCompleted(npcController) && --actionId >= 0)
                {
                    currentAction = currentTask.Actions[actionId];
                    currentAction.Stop(npcController);
                }
                //stop if completed
                else if (actionId < 0)
                {
                    taskState = ActivityState.Inactive;
                    actionId = 0;
                    currentAction = null;
                    currentTask = null;
                }
            }
            else if (taskState == ActivityState.Plaing)
                currentTask.Update(npcController);

        }

        public void StartActivity(NPCActivity activity)
        {
            currentTask = activity;
            taskState = ActivityState.Prepearing;
            actionId = 0;
            currentAction = currentTask.Actions[actionId];
            currentAction.Start(npcController);
        }


        public void StopActivity()
        {
            if (currentTask != null)
            {
                //currentTask.Exit(npcController);
                taskState = ActivityState.Finishing;
                if (currentAction == null)
                    currentAction = currentTask.Actions[actionId];
                currentAction.Stop(npcController);
            }
            
        }

        public NPCActivity GetCurrentActivity()
        {
            return currentTask;
        }

        /// <summary>
        /// General method for character movment
        /// </summary>
        /// <param name="poi">Location data (position, direction)</param>
        public void Go2Point(POIData poi)
        {
            //stop current movment
            npcController.navigationController.CurrentLocation?.FreeOccupant(0);
            npcController.navigationController.Stop();

            moving = true;
            npcController.navigationController.GoImmedeatly(poi.Position, poi.Direction, () => moving = false);
        }

        public void Go2Location(LocationState location)
        {
            if (location != null && location.GetOcupant(0) == null)
            {
                npcController.navigationController.CurrentLocation?.FreeOccupant(0);
                var poi = location.GetSlotPOI(0);

                npcController.navigationController.Stop();
                moving = true;
                npcController.navigationController.GoImmedeatly(poi.Position, poi.Direction, () => { moving = false; });
            }
        }

        public void OcupyLocation(LocationState location)
        {
            if (location != null && location.GetOcupant(0) == null)
            {
                npcController.navigationController.CurrentLocation?.FreeOccupant(0);
                var poi = location.GetSlotPOI(0);

                npcController.navigationController.Stop();
                moving = true;
                npcController.navigationController.GoImmedeatly(poi.Position, poi.Direction, () => {
                    location.SetOccupant(npcController,0);  
                    moving = false; });
            }
        }
    }
}
