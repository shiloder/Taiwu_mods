#define DEBUG
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;

namespace TaiwuEditor
{
    public static class Main
    {
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(modEntry.Info.Id);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Main.settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            Main.logger = modEntry.Logger;
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);

            // 用于在“奇遇直达终点”功能
            Main.MassageWindow_DoEvent = typeof(MassageWindow).GetMethod("DoEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            if (Main.MassageWindow_DoEvent == null)
            {
                Main.logger.Log("获取MassageWindow.DoEvent失败");
            }
            // 奇遇直接到终点功能中不生效的奇遇
            Main.ExcludedStory();

            // 用于锁定每月行动点数（每秒重置一次行动点数）
            Main.timer = new Timer(1000);
            Main.timer.Elapsed += Main.Timer_Elapsed;
            Main.timer.Start();

            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        // 主界面
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("<color=#87CEEB>功能选择：</color>");
            GUILayout.BeginHorizontal("box", new GUILayoutOption[0]);
            Main.settings.funcChoose = GUILayout.SelectionGrid(Main.settings.funcChoose, new string[] 
            {
                "<color=#FFA500>基本功能</color>",
                "<color=#FFA500>属性修改</color>"
            }, 2);            
            GUILayout.EndHorizontal();
            switch (Main.settings.funcChoose)
            {
                case 1:
                    UI.BasePropertiesUi(ref Main.settings);
                    break;
                default:
                    UI.SettingsUi(ref Main.settings);
                    break;
            }
            
        }

        private static UnityModManager.ModEntry.ModLogger logger;

        private static Timer timer;

        private static MethodInfo MassageWindow_DoEvent;

        private static Settings settings;

        private static List<int> excludedStoryIds;


        // 奇遇直接到终点功能中不生效的奇遇
        private static void ExcludedStory()
        {
            Main.excludedStoryIds = new List<int>();
            // 9(七元符图)，10002-10004(商贾云集)，10007-10009(沿街卖艺)，10010-10012(酒宴)，10013-10015(茶会) 不直接到终点
            Main.excludedStoryIds.Add(9);
            for (int i = 10002; i < 10016; i++)
            {
                Main.excludedStoryIds.Add(i);
                if (i == 10004)
                {
                    // 不添加10005(促织高鸣), 10006 (促织高鸣)
                    i = 10006;
                }
            }
        }

        // 锁定时间
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateFile.instance != null && Main.settings.lockTime)
            {
                DateFile.instance.dayTime = DateFile.instance.GetMaxDayTime();
            }
        }

        // 最大好感和最大印象
        [HarmonyPatch(typeof(MassageWindow), "SetMassageWindow")]
        private static class SAW_Hook
        {
            private static bool Prefix(int[] baseEventDate)
            {
                // 最大好感
                if (Main.settings.lockMaxOutRelation)
                {
                    MassageWindow.instance.mianEventDate = (int[])baseEventDate.Clone();
                    // 主事件ID
                    int mainEventId = baseEventDate[2];
                    // 事件类型
                    int eventType = DateFile.instance.ParseInt(DateFile.instance.eventDate[mainEventId][2]);
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
                                Main.logger.Log("[TaiwuEditor]");
                                Main.logger.Log("好感修改失败");
                                Main.logger.Log(e.Message);
                                Main.logger.Log(e.StackTrace);
                            }
                        }
                    }
                }
                // 最大印象
                if (Main.settings.lockMaxLifeFace)
                {
                    int mainEventId = baseEventDate[2];
                    int eventType = DateFile.instance.ParseInt(DateFile.instance.eventDate[mainEventId][2]);
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
                                int fashionDress = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(mainActorId, 305));
                                if (fashionDress > 0)
                                {
                                    // 时装身份ID
                                    int faceId = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(fashionDress, 15));
                                    if (faceId > 0)
                                    {
                                        DateFile.instance.actorLife[npcId].Remove(1001);
                                        // 添加新印象,100%
                                        DateFile.instance.actorLife[npcId].Add(1001, new List<int>
                                        {
                                            faceId,
                                            100
                                        });
                                        // 提示信息
                                        TipsWindow.instance.SetTips(5011, new string[3]
                                        {
                                            DateFile.instance.GetActorName(npcId),
                                            DateFile.instance.GetActorName(),
                                            DateFile.instance.identityDate[faceId][0]
                                        }, 180);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Main.logger.Log("[TaiwuEditor]");
                                Main.logger.Log("印象修改失败");
                                Main.logger.Log(e.Message);
                                Main.logger.Log(e.StackTrace);
                            }
                        }
                    }
                }
                return true;
            }
        }

        // 锁定戒心为0
        [HarmonyPatch(typeof(DateFile), "GetActorWariness")]
        private static class ZW_Hook
        {
            private static void Postfix(ref int __result)
            {
                if (Main.settings.lockZeroWariness)
                {
                    __result = 0;
                }
            }
        }

        // 快速修习
        [HarmonyPatch(typeof(HomeSystem), "StudySkillUp")]
        private static class SSU_Hook
        {
            private static bool Prefix(int ___studySkillId, int ___studySkillTyp, ref HomeSystem __instance)
            {
                if (!Main.settings.lockMaxOutProficiency || ___studySkillId <= 0 || ___studySkillTyp <= 0 || ___studySkillTyp > 17)
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
                        int scoreGain = DateFile.instance.ParseInt(DateFile.instance.gongFaDate[___studySkillId][2]);
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
                        int scoreGain = DateFile.instance.ParseInt(DateFile.instance.skillDate[___studySkillId][2]);
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

        // 奇遇直接到终点
        [HarmonyPatch(typeof(StorySystem), "OpenStory")]
        private static class OS_Hook
        {
            private static bool Prefix(ref StorySystem __instance)
            {
                if (!Main.settings.lockFastQiyuCompletion)
                {
                    return true;
                }
                else
                {
                    int storyId = __instance.storySystemStoryId;
#if DEBUG
                    Main.logger.Log($"[TaiwuEditor]OpenStory: StoryId: {storyId}");
#endif                    
                    if (storyId > 0 && !Main.excludedStoryIds.Contains(storyId))
                    {
                        // 关闭奇遇窗口
                        __instance.ClossToStoryMenu();
                        // 终点的事件ID
                        int storyEndEventId = DateFile.instance.ParseInt(DateFile.instance.baseStoryDate[storyId][302]);
#if DEBUG
                        Main.logger.Log($"[TaiwuEditor]OpenStory: storyEndEventId: {storyEndEventId}");
#endif
                        if (Helper.EventSetup(storyEndEventId, __instance.storySystemPartId, __instance.storySystemPlaceId, __instance.storySystemStoryId))
                        {
                            Main.logger.Log("MassageWindow.DoEvent called");
                            try
                            {
                                // 调用MessageWindow.DoEvent(0)执行终点Event
                                Main.MassageWindow_DoEvent.Invoke(MassageWindow.instance, new object[]
                                {
                                    0
                                });
                            }
                            catch (Exception e)
                            {
                                Main.logger.Log($"[TaiwuEditor]奇遇直达终点失效 storyEndEventId: {storyEndEventId}");
                                Main.logger.Log(e.Message);
                                Main.logger.Log(e.StackTrace);
                                // 如果出现问题则return true调用游戏本来的奇遇处理方法
                                return true;
                            }
                        }
                        else
                        {
                            Main.logger.Log($"[TaiwuEditor]OpenStory has been removed due to storyEndEventId is 0");
                            __instance.StoryEnd();
                        }
                        return false;
                    }
                    return true;
                }
            }
        }

        // 背包最大载重
        [HarmonyPatch(typeof(ActorMenu), "GetMaxItemSize")]
        private static class GMIS_Hook
        {
            private static void Postfix(ref int key, ref int __result)
            {
                bool flag = !Main.settings.lockNeverOverweigh || DateFile.instance.mianActorId != key;
                if (!flag)
                {
                    __result = 999999999;
                }
            }
        }

        // 快速读书
        [HarmonyPatch(typeof(HomeSystem), "StartReadBook")]
        private static class SRB_Hook
        {
            private static bool Prefix(int ___readBookId, int ___studySkillTyp, HomeSystem __instance)
            {
#if DEBUG
                Main.logger.Log($"[TaiwuEditor]快速读书: id: {___readBookId}，SkillTyp: {___studySkillTyp}");
#endif
                if (!Main.settings.lockFastRead || ___studySkillTyp <= 0 || ___studySkillTyp > 17 || ___readBookId <= 0)
                {
                    return true;
                }
                else
                {
                    Helper.EasyReadV2(___readBookId, ___studySkillTyp, Main.settings.pagesPerFastRead);
                    __instance.UpdateReadBookWindow();
                    return false;
                }
            }
        }
    }

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        // 功能选择，0是基本功能，1是修改属性
        public int funcChoose = 0;

        // 选择修改哪个人物的属性，0太吾，1上一个打开菜单的人物，2自定义人物
        public int basePropertyChoose = 0;

        // 显示在自定义人物ID输入框的值
        public string displayedActorId = "0";

        // 想要修改属性的NPC ID
        public int actorId = 0;

        // 锁定时间
        public bool lockTime = false;

        // 快速读书
        public bool lockFastRead = false;

        // 快速读书每次篇数
        public int pagesPerFastRead = 10;

        // 快速修习
        public bool lockMaxOutProficiency = false;

        // 奇遇直接到终点
        public bool lockFastQiyuCompletion = false;

        // 背包无限
        public bool lockNeverOverweigh = false;

        // 见面最大化好感
        public bool lockMaxOutRelation = false;

        // 见面最大化印象
        public bool lockMaxLifeFace = false;

        // 锁定戒心为0
        public bool lockZeroWariness = false;
    }

}
