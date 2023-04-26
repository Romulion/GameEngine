using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;
using GLib;

namespace ModelViewer
{
    public enum ActivityState
    {
        Inactive,
        Prepearing,
        Plaing,
        Finishing,
    }
    class NPCActivity
    {
        public List<NPCBasicAction> Actions { get; private set; }
        public readonly string Name;
        protected bool ready = false;
        //NPCBasicAction currentAction = null;
        //int actionId = 0;
        public ActivityState state { get; private set; }
        public NPCActivity(string name, List<NPCBasicAction> actions)
        {
            Name = name;
            Actions = actions;
        }

        /*
        public void Start(NPCController controller)
        {
            state = ActivityState.Prepearing;
            actionId = 0;
            currentAction = Actions[actionId];
            currentAction.Start(controller);
        }

        public void Exit(NPCController controller)
        {
            state = ActivityState.Finishing;
            if (currentAction == null)
                currentAction = Actions[actionId];
            currentAction.Stop(controller);
        }
        */
        //Plays on activity performs
        public void Update(NPCController controller)
        {
            /*
            if (state == ActivityState.Prepearing)
            {
                //play next action
                if (currentAction.IsCompleted(controller) && ++actionId < Actions.Count)
                {
                    currentAction = Actions[actionId];
                    currentAction.Start(controller);
                }
                //stop if completed
                else if (actionId >= Actions.Count)
                {
                    state = ActivityState.Plaing;
                    actionId--;
                    currentAction = null;
                }
            }
            //reverse
            else if (state == ActivityState.Finishing)
            {
                //play next action
                if (!currentAction.IsCompleted(controller) && --actionId >= 0)
                {
                    currentAction = Actions[actionId];
                    currentAction.Stop(controller);
                }
                //stop if completed
                else if (actionId < 0)
                {
                    state = ActivityState.Inactive;
                    actionId = 0;
                    currentAction = null;
                }
            }
            */
        }
    }
}
