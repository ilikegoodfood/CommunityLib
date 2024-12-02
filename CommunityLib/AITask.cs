using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class AITask
    {
        public enum TaskTags
        {
            None,
            Forbidden
        }

        public enum TargetCategory
        {
            None,
            Location,
            SocialGroup,
            Unit
        }

        public Type taskType;

        public string title;

        public TargetCategory targetCategory;

        public List<TaskTags> tags;

        public Color colour;

        public Sprite foregroundSprite;

        public Sprite backgroundSprite;

        public List<Func<UA, TargetCategory, AgentAI.TaskData, bool>> delegates_Valid;

        public List<Func<UA, TargetCategory, AgentAI.TaskData, List<ReasonMsg>, double>> delegates_Utility;

        public Func<UA, TargetCategory, AgentAI.TaskData, Assets.Code.Task> delegate_Instantiate;

        public AITask(Type taskType, string title, Map map, Func<UA, TargetCategory, AgentAI.TaskData, Assets.Code.Task> delegate_Instantiate, TargetCategory targetCategory = TargetCategory.None, List<TaskTags> tags = null, Color? colour = null, Sprite foregroundSprite = null, Sprite backgroundSprite = null)
        {
            if (taskType == null || !taskType.IsSubclassOf(typeof(Assets.Code.Task)))
            {
                throw new ArgumentException("CommunityLib: taskType is not subclass of Task");
            }

            this.taskType = taskType;
            this.title = title;
            //Console.WriteLine("CommunityLib: Assigned taskType");
            this.delegate_Instantiate = delegate_Instantiate;
            //Console.WriteLine("CommunityLib: Assigned delegate_instantiate");
            this.targetCategory = targetCategory;
            //Console.WriteLine("CommunityLib: Asigned targetCategory");

            if (tags == null)
            {
                //Console.WriteLine("CommunityLib: tags is null");
                tags = new List<TaskTags>();
            }
            this.tags = tags;
            //Console.WriteLine("CommunityLib: Assigned tags");

            if (!(colour is Color c))
            {
                //Console.WriteLine("CommunityLib: Colour is null.");
                this.colour = new Color(0.5f, 0.5f, 0.5f);
                //Console.WriteLine("CommunityLib: Assigned colour to default");
            }
            else
            {
                //Console.WriteLine("CommunityLib: Assigned colour");
                this.colour = c;
            }

            if (foregroundSprite == null)
            {
                //Console.WriteLine("CommunityLib: foregroundSprite is null");
                foregroundSprite = map.world.textureStore.clear;
                //Console.WriteLine("CommunityLib: got default");
            }
            this.foregroundSprite = foregroundSprite;
            //Console.WriteLine("CommunityLib: Assigned foregroundSprite");

            if (backgroundSprite == null)
            {
                //Console.WriteLine("CommunityLib: backGroundSprite is null");
                backgroundSprite = map.world.textureStore.clear;
                //Console.WriteLine("CommunityLib: got default");
            }
            this.backgroundSprite = backgroundSprite;
            //Console.WriteLine("CommunityLib: Assigned backgroundSprite");

            delegates_Valid = new List<Func<UA, TargetCategory, AgentAI.TaskData, bool>>();
            //Console.WriteLine("CommunityLib: Created delegates_Valid");
            delegates_Utility = new List<Func<UA, TargetCategory, AgentAI.TaskData, List<ReasonMsg>, double>>();
            //Console.WriteLine("CommunityLib: Created delegates_Utility");
        }

        public bool checkTaskIsValid(AgentAI.TaskData taskData, UA ua, AgentAI.ControlParameters controlParams)
        {
            if (tags.Contains(TaskTags.Forbidden))
            {
                return false;
            }

            if (delegates_Valid != null)
            {
                foreach (Func<UA, TargetCategory, AgentAI.TaskData, bool> delegate_Valid in delegates_Valid)
                {
                    if (!delegate_Valid(ua, taskData.targetCategory, taskData))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public double checkTaskUtility(AgentAI.TaskData taskData, UA ua, AgentAI.ControlParameters controlParams, List<ReasonMsg> reasonMsgs = null)
        {
            double utility = 0.0;

            switch (taskData.targetCategory)
            {
                case TargetCategory.Location:
                    if (taskData.targetLocation != null && taskData.targetLocation != ua.location)
                    {
                        Location[] pathTo;
                        pathTo = ua.location.map.getPathTo(ua.location, taskData.targetLocation, ua);
                        if (pathTo == null || pathTo.Length < 2)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Cannot find path to target location", -10000.0));
                            utility -= 10000.0;
                        }
                    }
                    break;
                case TargetCategory.SocialGroup:
                    if (taskData.targetSocialGroup != null && ua.location.soc != taskData.targetSocialGroup)
                    {
                        Location[] pathTo = ua.location.map.getPathTo(ua.location, taskData.targetSocialGroup, ua);
                        if (pathTo == null || pathTo.Length < 2)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Cannot find path to target social group", -10000.0));
                            utility -= 10000.0;
                        }
                    }
                    break;
                case TargetCategory.Unit:
                    if (taskData.targetUnit != null && ! ua.location.units.Contains(taskData.targetUnit))
                    {
                        Location[] pathTo = ua.location.map.getPathTo(ua.location, taskData.targetUnit.location, ua);
                        if (pathTo == null || pathTo.Length < 2)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Cannot find path to target location", -10000.0));
                            utility -= 10000.0;
                        }
                    }
                    break;
                default:
                    break;
            }

            if (delegates_Utility != null)
            {
                foreach (Func<UA, TargetCategory, AgentAI.TaskData, List<ReasonMsg>, double> delegate_Utility in delegates_Utility)
                {
                    utility += delegate_Utility(ua, taskData.targetCategory, taskData, reasonMsgs);
                }
            }

            return utility;
        }

        public Assets.Code.Task instantiateTask(UA ua, TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            return delegate_Instantiate(ua, targetCategory, taskData);
        }

        public static Assets.Code.Task delegate_Instantiate_NOARGS(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            return (Assets.Code.Task)Activator.CreateInstance(taskData.aiTask.taskType, new object[0]);
        }
    }
}
