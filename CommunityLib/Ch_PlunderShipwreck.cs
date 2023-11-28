using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class Ch_PlunderShipwreck : Challenge
    {
        public Sub_Shipwreck wreck;

        public Ch_PlunderShipwreck(Sub_Shipwreck wreck)
            : base(wreck.settlement.location)
        {
            this.wreck = wreck;
        }

        public override string getName()
        {
            return "Plunder Shipwreck";
        }

        public override string getDesc()
        {
            return "Plunder the wreck in search of coins and other precious items. Performing this action degrades the wreck, reducing its integrity by " + ((int)wreck.integrityLossPlunder).ToString() + ".";
        }

        public override string getCastFlavour()
        {
            return "Lost to stormy seas, a failure of navigation, or sunk during the siege of a city. There are many reasons for a ship to find itself at the bottom of the ocean it once proudly rode across, or beached against a granite-grey cliff face. There are equally many treasures it may have been carrying when it was.";
        }

        public override double getProfile()
        {
            double result = map.param.ch_exploreruins_aiProfile;

            if (wreck.isReinforced())
            {
                result += 10;
            }

            return result;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = ua.map.param.ch_exploreRuinsBaseMotivation;
            msgs?.Add(new ReasonMsg("Base", utility));

            double val = ((ua.maxHp - ua.hp) / ua.maxHp) * ua.map.param.ch_exploreRuinsBaseMotivation * 2;
            val *= -1;

            msgs?.Add(new ReasonMsg("HP is not max", val));
            utility += val;

            if (wreck.allure > 0.0)
            {
                val = wreck.allure;
                msgs?.Add(new ReasonMsg("Allure of Wreck", val));
                utility += val;
            }

            return utility;
        }

        public override double getMenace()
        {
            return 0.0;
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("CLib.Icon_Shipwreck.png");
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override bool validFor(UM ua)
        {
            return false;
        }

        public override bool valid()
        {
            return wreck.integrity > 0.0;
        }

        public override double getComplexity()
        {
            return map.param.ch_exploreRuinsTime;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 1.0;
            msgs?.Add(new ReasonMsg("Base", result));

            if (unit.isCommandable() && unit.location.getShadow() > unit.map.param.ch_exploreruins_parameterValue1)
            {
                result++;
                msgs?.Add(new ReasonMsg("Agent in Shadow", 1.0));
            }

            return result;
        }

        public override int getDanger()
        {
            int danger = 10 - (int)Math.Floor(wreck.integrity * 10);

            if (danger < 0)
            {
                return 0;
            }

            return danger;
        }

        public override void complete(UA u)
        {
            EventManager.ActiveEvent activeEvent = null;
            EventContext ctx = EventContext.withUnit(map, u);
            List<EventManager.ActiveEvent> events = new List<EventManager.ActiveEvent>();
            double weight = 0.0;

            foreach (EventManager.ActiveEvent aEvent in EventManager.events.Values)
            {
                if (aEvent.type == EventData.Type.INERT && aEvent.data.id.Contains("plunderWreck_"))
                {
                    if (aEvent.willTrigger(ctx))
                    {
                        events.Add(aEvent);
                        weight += aEvent.data.probability;
                    }
                }
            }

            weight *= Eleven.random.NextDouble();

            foreach (EventManager.ActiveEvent aEvent in events)
            {
                weight -= aEvent.data.probability;
                if (weight <= 0.0)
                {
                    activeEvent = aEvent;
                    break;
                }
            }

            if (activeEvent == null)
            {
                map.world.prefabStore.popMsg("UNABLE TO FIND A VALID EXPLORATION EVENT AT " + base.location.getName(true), true, true);
            }
            else
            {
                EventData data = activeEvent.data;
                if (u.isCommandable())
                {
                    map.world.prefabStore.popEvent(data, ctx, null, false);
                }
                else
                {
                    aiOutcome(u);
                    if (!wreck.isReinforced())
                    {
                        wreck.integrity -= wreck.integrityLossPlunder;
                    }
                    msgString = u.getName() + " has plunder the " + wreck.getName() + ", damaging the wreck in the process. The shipwreck's integrity is now at " + ((int)wreck.integrity * 100).ToString() + "%.";
                    if (wreck.integrity <= 0.0)
                    {
                        wreck.removeWreck();
                        msgString = u.getName() + " has plunder the " + wreck.getName() + ", destroying the last traces of the shipwreck in the process.";
                    }
                }
            }
        }

        public void aiOutcome(UA ua)
        {
            int roll = Eleven.random.Next(4);

            if (roll == 0)
            {
                int damageTaken = map.param.ch_exploreRuinsAIOutcomeDMG;
                ua.defence = ua.getMaxDefence();
                foreach (Minion minion in ua.minions)
                {
                    if (minion != null)
                    {
                        minion.defence = minion.getMaxDefence();
                    }
                }

                int i = 0;
                while (i < damageTaken)
                {
                    i++;

                    int coinToss = Eleven.random.Next(2);
                    if (coinToss == 0)
                    {
                        List<int> minionIndexes = new List<int>();
                        for (int j = 0; j < ua.minions.Length; j++)
                        {
                            if (ua.minions[j] != null)
                            {
                                minionIndexes.Add(j);
                            }
                        }

                        if (minionIndexes.Count > 0)
                        {
                            int targetIndex = minionIndexes[Eleven.random.Next(minionIndexes.Count)];
                            if (ua.minions[targetIndex].defence > 0)
                            {
                                ua.minions[targetIndex].defence--;
                            }
                            else
                            {
                                ua.minions[targetIndex].hp--;
                                if (ua.minions[targetIndex].hp <= 0)
                                {
                                    ua.minions[targetIndex].die("Killed by the danger of challenge: " + getName());
                                    ua.minions[targetIndex] = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ua != map.awarenessManager.getChosenOne() || ua.hp > 1)
                        {
                            if (ua.defence > 0)
                            {
                                ua.defence--;
                            }
                            else
                            {
                                ua.hp--;
                                if (ua.hp <= 0)
                                {
                                    map.addUnifiedMessage(ua, null, "Death Plundering Shipwrecks", ua.getName() + " has been killed while plundering shipwrecks at " + location.getName(true), UnifiedMessage.messageType.DEATH_EXPLORING_RUINS, false);
                                    ua.die(map, "Killed by dangers during challenge: " + getName(), null);
                                }
                            }
                        }
                    }
                }
            }
            else if (roll == 1)
            {
                ua.person.addGold(Eleven.random.Next(map.param.ch_exploreRuinsAIOutcomeGoldMax));
            }
            else
            {
                bool gotAbyssalItem = false;

                if (Eleven.random.Next(2) == 0 && ModCore.core.data.tryGetModIntegrationData("DeepOnesPlus", out ModIntegrationData intDataDOPlus) && intDataDOPlus.assembly != null && intDataDOPlus.typeDict.TryGetValue("Kernel", out Type kernelType) && kernelType != null)
                {
                    ModKernel kernel = map.mods.FirstOrDefault(k => k.GetType() == kernelType);

                    if (kernel != null && intDataDOPlus.methodInfoDict.TryGetValue("getAbyssalItem", out MethodInfo MI_getAbyssalItem) && MI_getAbyssalItem != null)
                    {
                        msgString += ", and an artifact from the abyssal depths.";
                        ua.person.gainItem((Item)MI_getAbyssalItem.Invoke(kernel, new object[] { ua.map, ua }));
                        gotAbyssalItem = true;
                    }
                }

                if (!gotAbyssalItem)
                {
                    roll = Eleven.random.Next(10);
                    bool flag13 = roll < map.param.ch_exploreruins_parameterValue2;
                    if (flag13)
                    {
                        ua.person.gainItem(Item.getItemFromPool1(map, -1), false);
                    }
                    else
                    {
                        bool flag14 = roll < map.param.ch_exploreruins_parameterValue3;
                        if (flag14)
                        {
                            ua.person.gainItem(Item.getItemFromPool2(map, -1), false);
                        }
                        else
                        {
                            ua.person.gainItem(Item.getItemFromPool3(map, -1), false);
                        }
                    }
                }
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.GOLD
            };
        }
    }
}
