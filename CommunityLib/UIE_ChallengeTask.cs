using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CommunityLib
{
    public class UIE_ChallengeTask : UIE_ChallengePerception
    {
        public virtual void setTo(World world, SortableTaskBlock_Advanced srt, UA ua)
        {
            //Console.WriteLine("CommunityLib: Line 1");
            this.srt = srt;
            //Console.WriteLine("CommunityLib: Got block");

            if (srt.challenge != null || srt.unitToAttack != null || srt.unitToDisrupt != null || srt.unitToGuard != null)
            {
                setTo(world, srt);

                if (srt.challenge is Ritual && srt.location != null)
                {
                    tLoc.text = srt.location.getName();
                }
                return;
            }

            title.text = "ERROR: Task Not Found";
            tLoc.text = "ERROR: Location Not Found";
            //Console.WriteLine("CommunityLib: Set default text.");
            iconBack.sprite = srt.taskData.aiTask.backgroundSprite;
            icon.sprite = srt.taskData.aiTask.foregroundSprite;
            //Console.WriteLine("CommunityLib: Set Sprites.");
            double utility = Math.Round(srt.utility, 1);
            tUtility.text = "Motivation: " + utility.ToString();
            //Console.WriteLine("CommunityLib: Set utility.");
            backColour.color = srt.taskData.aiTask.colour;
            //Console.WriteLine("CommunityLib: Set colour.");

            //Console.WriteLine("CommunityLib: Set defaults.");

            if (srt.taskData != null)
            {
                //Console.WriteLine("CommunityLib: Block is not default");
                Assets.Code.Task task = srt.taskData.aiTask.instantiateTask(ua, srt.taskData.targetCategory, srt.taskData);
                if (task != null)
                {
                    //Console.WriteLine("CommunityLib: Got Task Instance");
                    title.text = srt.taskData.aiTask.title;
                }
                
                if (srt.location != null)
                {
                    //Console.WriteLine("CommunityLib: Got location name.");
                    tLoc.text = srt.location.getName();
                }
                else if (srt.socialGroup != null)
                {
                    //Console.WriteLine("CommunityLib: Got social group name");
                    tLoc.text = srt.socialGroup.getName();
                }
                else if (srt.unit != null)
                {
                    //Console.WriteLine("CommunityLib: Got unit");
                    tLoc.text = srt.unit.location.getName();
                    iconBack.sprite = srt.unit.getPortraitBackground();
                    icon.sprite = srt.unit.getPortraitForeground();
                }
                else
                {
                    //Console.WriteLine("CommunityLib: No target category");
                    tLoc.text = "";
                }
            }
        }

        new public void clickGOTO()
        {
            if (srt is SortableTaskBlock_Advanced advBlock)
            {
                if (advBlock.location != null)
                {
                    GraphicalMap.panTo(advBlock.location.hex);
                    return;
                }

                if (advBlock.socialGroup != null)
                {
                    GraphicalMap.panTo(advBlock.socialGroup.getCapitalHex());
                    return;
                }

                if (advBlock.unit != null)
                {
                    GraphicalMap.panTo(advBlock.unit.location.hex);
                    return;
                }

                if (srt.challenge != null || srt.unitToAttack != null || srt.unitToDisrupt != null || srt.unitToGuard != null)
                {
                    base.clickGOTO();
                    return;
                }

                GraphicalMap.panTo(GraphicalMap.selectedUnit.location.hex);
                return;
            }
            else
            {
                base.clickGOTO();
            }
        }
    }
}
