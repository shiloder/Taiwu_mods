using Harmony12;
using System;
using System.Collections.Generic;

namespace TaiwuEditor
{
    static partial class Main
    {
        /// <summary>
        /// 最大好感和最大印象
        /// </summary>
        [HarmonyPatch(typeof(MassageWindow), "SetMassageWindow")]
        private static class MassageWindow_SetMassageWindow_Hook
        {
            private static bool Prefix(int[] baseEventDate)
            {
                // 最大好感
                if (enabled && settings.basicUISettings[5])
                {
                    MassageWindow.instance.mianEventDate = (int[]) baseEventDate.Clone();
                    // 主事件ID
                    int mainEventId = baseEventDate[2];
                    // 事件类型
                    int eventType = int.Parse(DateFile.instance.eventDate[mainEventId][2]);
                    //int num2 = DateFile.instance.MianActorID();
                    //int num3 = (num != 0) ? ((num != -1) ? num : num2) : baseEventDate[1];
                    if (eventType == 0) // 事件类型是与NPC互动
                    {
                        // baseEventDate[1]是互动的NPC的ID
                        int npcId = baseEventDate[1];
                        if (DateFile.instance.actorsDate.ContainsKey(npcId))
                        {
                            //try catch 目前用于跳过个别时候载入游戏时触发过月后npc互动会报错的问题
                            try
                            {
                                // 改变好感
                                DateFile.instance.ChangeFavor(npcId, 60000, true, false);
                            }
                            catch (Exception e)
                            {
                                logger.Log("[TaiwuEditor]");
                                logger.Log("好感修改失败");
                                logger.Log(e.Message);
                                logger.Log(e.StackTrace);
                            }
                        }
                    }
                }
                // 最大印象
                if (enabled && settings.basicUISettings[6])
                {
                    int mainEventId = baseEventDate[2];
                    int eventType = int.Parse(DateFile.instance.eventDate[mainEventId][2]);
                    int mainActorId = DateFile.instance.MianActorID();
                    //int num6 = (eventType != 0) ? ((eventType != -1) ? eventType : num5) : baseEventDate[1];
                    if (eventType == 0)
                    {
                        int npcId = baseEventDate[1];
                        if (DateFile.instance.actorsDate.ContainsKey(npcId))
                        {
                            //try catch 目前用于跳过个别特殊npc本身无印象数据会报错的问题
                            try
                            {
                                // 改变印象
                                // 时装ID
                                int fashionDress = int.Parse(DateFile.instance.GetActorDate(mainActorId, 305));
                                if (fashionDress > 0)
                                {
                                    // 时装身份ID
                                    int faceId = int.Parse(DateFile.instance.GetItemDate(fashionDress, 15));
                                    if (faceId > 0)
                                    {
                                        DateFile.instance.actorLife[npcId].Remove(1001);
                                        // 添加新印象,100%
                                        DateFile.instance.actorLife[npcId].Add(1001, new List<int>
                                        {
                                            faceId,
                                            100
                                        });
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Log("[TaiwuEditor]");
                                logger.Log("印象修改失败");
                                logger.Log(e.Message);
                                logger.Log(e.StackTrace);
                            }
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 锁定戒心为0
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "GetActorWariness")]
        private static class DateFile_GetActorWariness_Hook
        {
            private static void Postfix(ref int __result)
            {
                if (enabled && settings.basicUISettings[7])
                {
                    __result = 0;
                }
            }
        }

        /// <summary>
        /// 快速修习
        /// </summary>
        [HarmonyPatch(typeof(HomeSystem), "StudySkillUp")]
        private static class HomeSystem_StudySkillUp_Hook
        {
            private static bool Prefix(int ___studySkillId, int ___studySkillTyp, ref HomeSystem __instance)
            {
                if (!enabled || !settings.basicUISettings[2] || ___studySkillId <= 0 || ___studySkillTyp <= 0 || ___studySkillTyp > 17)
                {
                    return true;
                }
                else
                {
                    int mainActorId = DateFile.instance.MianActorID();
                    if (___studySkillTyp == 17)
                    {
                        if (DateFile.instance.GetGongFaLevel(mainActorId, ___studySkillId, 0) >= 100)
                        {
                            return false;
                        }
                        // 基础遗惠
                        int scoreGain = int.Parse(DateFile.instance.gongFaDate[___studySkillId][2]);
                        // 清零因为实战而获得的突破成功率加成
                        DateFile.instance.addGongFaStudyValue = 0;
                        //DateFile.instance.actorGongFas[mainActorId][___studySkillId][0] = 100;
                        DateFile.instance.ChangeActorGongFa(mainActorId, ___studySkillId, 100, 0, 0, false);
                        // 突破成功一次增加的遗惠
                        DateFile.instance.AddActorScore(302, scoreGain * 100);
                        if (DateFile.instance.GetGongFaLevel(mainActorId, ___studySkillId, 0) >= 100 && DateFile.instance.GetGongFaFLevel(mainActorId, ___studySkillId) >= 10)
                        {
                            // 修习到100%时增加的遗惠
                            DateFile.instance.AddActorScore(304, scoreGain * 100);
                        }
                    }
                    else
                    {
                        if (DateFile.instance.GetSkillLevel(___studySkillId) >= 100)
                        {
                            return false;
                        }
                        int scoreGain = int.Parse(DateFile.instance.skillDate[___studySkillId][2]);
                        // 清零因为较艺而获得的突破成功率加成
                        DateFile.instance.addSkillStudyValue = 0;
                        DateFile.instance.ChangeMianSkill(___studySkillId, 100, 0, false);
                        //DateFile.instance.actorSkills[___studySkillId][0] = 100;
                        // 突破成功一次增加的遗惠
                        DateFile.instance.AddActorScore(202, scoreGain * 100);
                        if (DateFile.instance.GetSkillLevel(___studySkillId) >= 100 && DateFile.instance.GetSkillFLevel(___studySkillId) >= 10)
                        {
                            // 修习到100%时增加的遗惠
                            DateFile.instance.AddActorScore(204, scoreGain * 100);
                        }
                    }
                    __instance.UpdateStudySkillWindow();
                    __instance.UpdateLevelUPSkillWindow();
                    __instance.UpdateReadBookWindow();
                    return false;
                }
            }
        }

        /// <summary>
        /// 奇遇直接到终点
        /// </summary>
        [HarmonyPatch(typeof(StorySystem), "OpenStory")]
        private static class StorySystem_OpenStory_Hook
        {
            private static bool Prefix(ref StorySystem __instance)
            {
                if (!enabled || !settings.basicUISettings[3])
                {
                    return true;
                }
                else
                {
                    int storyId = __instance.storySystemStoryId;
#if DEBUG
                    logger.Log($"[TaiwuEditor]OpenStory: StoryId: {storyId}");
#endif                    
                    if (storyId > 0)
                    {
                        bool storyIdExist = false;
                        for (int i = 0; i < settings.includedStoryTyps.Length; i++)
                        {
                            if (settings.includedStoryTyps[i])
                            {
                                if (settings.GetStoryTyp(i).IsContainStoryId(storyId))
                                {
                                    storyIdExist = true;
                                    break;
                                }
                            }
                        }
                        if (!storyIdExist)
                        {
                            return true;
                        }
                        // 关闭奇遇窗口
                        __instance.ClossToStoryMenu();
                        // 终点的事件ID
                        int storyEndEventId = int.Parse(DateFile.instance.baseStoryDate[storyId][302]);
#if DEBUG
                        logger.Log($"[TaiwuEditor]OpenStory: storyEndEventId: {storyEndEventId}");
#endif
                        if (Helper.EventSetup(storyEndEventId, __instance.storySystemPartId, __instance.storySystemPlaceId, __instance.storySystemStoryId))
                        {
                            logger.Log("MassageWindow.DoEvent called");
                            try
                            {
                                // 调用MessageWindow.DoEvent(0)执行终点Event
                                MassageWindow.instance.Invoke("DoEvent", new object[] { 0 });
                            }
                            catch (Exception e)
                            {
                                logger.Log($"[TaiwuEditor]奇遇直达终点失效 storyEndEventId: {storyEndEventId}");
                                logger.Log(e.Message);
                                logger.Log(e.StackTrace);
                                // 如果出现问题则return true调用游戏本来的奇遇处理方法
                                return true;
                            }
                        }
                        else
                        {
                            logger.Log($"[TaiwuEditor]OpenStory has been removed due to storyEndEventId is 0");
                            __instance.StoryEnd();
                        }
                        return false;
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// 背包最大载重
        /// </summary>
        [HarmonyPatch(typeof(ActorMenu), "GetMaxItemSize")]
        private static class ActorMenu_GetMaxItemSize_Hook
        {
            private static void Postfix(ref int key, ref int __result)
            {
                if (enabled && settings.basicUISettings[4] && DateFile.instance.mianActorId == key)
                {
                    __result = 999999999;
                }
            }
        }

        /// <summary>
        /// 快速读书
        /// </summary>
        [HarmonyPatch(typeof(HomeSystem), "StartReadBook")]
        private static class HomeSystem_StartReadBook_Hook
        {
            private static bool Prefix(int ___readBookId, int ___studySkillTyp, HomeSystem __instance)
            {
#if DEBUG
                logger.Log($"[TaiwuEditor]快速读书: id: {___readBookId}，SkillTyp: {___studySkillTyp}");
#endif
                if (!enabled || !settings.basicUISettings[1] || ___studySkillTyp < 1 || ___studySkillTyp > 17 || ___readBookId < 1)
                {
                    return true;
                }
                else
                {
                    Helper.EasyReadV2(___readBookId, ___studySkillTyp, settings.pagesPerFastRead);
                    __instance.UpdateReadBookWindow();
                    return false;
                }
            }
        }

        /// <summary>
        /// 锁定门派支持度
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "GetGangPartValue")]
        private static class DateFile_GangPartValue_Hook
        {
            private static bool Prefix(int gangId, ref int __result)
            {
                if (!enabled || !settings.basicUISettings[8])
                {
                    return true;
                }
                // 太吾村没有支持度
                __result = (gangId == 16) ? 0 : (settings.customLockValue[0] == 0) ? DateFile.instance.GetMaxWorldValue() : settings.customLockValue[0] * 10;
                return false;
            }
        }

        /// <summary>
        /// 锁定地区恩义
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "GetBasePartValue")]
        private static class DateFile_GetBasePartValue_Hook
        {
            // 返回锁定的值
            private static bool Prefix(ref int __result)
            {
                if (!enabled || !settings.basicUISettings[9])
                {
                    return true;
                }
                __result = (settings.customLockValue[1] == 0) ? DateFile.instance.GetMaxWorldValue() : settings.customLockValue[1] * 10;
                return false;
            }
        }

        /// <summary>
        /// 锁定地区恩义
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "SetGangValue")]
        private static class DateFile_SetGangValue_Hook
        {
            // 阻止地区恩义减少
            private static bool Prefix(ref int value)
            {
                if (enabled && settings.basicUISettings[9] && value < 0)
                {
                    value = 0;
                }
                return true;
            }
        }
    }
}
