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
        public static bool enabled;
        private static UnityModManager.ModEntry.ModLogger logger;
        private static Timer timer;
        private static MethodInfo MassageWindow_DoEvent;
        private static Settings settings;
        private static readonly StoryTyp[] storytyps =
        {
            new StoryTyp(new HashSet<int>{101,102,103,104,105,106,107,108,109,110,111,112}, "外道巢穴"),
            new StoryTyp(new HashSet<int>{1,10001,10005,10006},"促织高鸣"),
            new StoryTyp(new HashSet<int>{2,3,4,5},"静谧竹庐/深谷出口/英雄猴杰/古墓仙人"),
            new StoryTyp(new HashSet<int>{6,7,8},"大片血迹"),
            new StoryTyp(new HashSet<int>{11001,11002,11003,11004,11005,11006,11007,11008,11009,11010,11011,11012,11013,11014},"奇书"),
            new StoryTyp(new HashSet<int>{3007,3014,3107,3114,3207,3214,3307,3314,3407,3414,3421,3428,4004,4008,4012,4016,4020,
                4024,4028,4032,4036,4040,4044,4048,4052,4056,4060,4064,4068,4072,4076,4080,4084,4088,4092,4096,4207,4214,4221,
                4228,4235,4242},"天材地宝"),
            //new StoryTyp(new HashSet<int>{5001,5002,5003,5004,5005},"门派争端"),
            new StoryTyp(new HashSet<int>{20001,20002,20003,20004,20005,20006,20007,20008,20009},"剑冢")
        };
        private static bool uiIsShow = false;
        private static bool bindingKey = false;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create(modEntry.Info.Id);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            logger = modEntry.Logger;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            if (!uiIsShow)
            {
                UI.Load(modEntry, settings, storytyps);
                uiIsShow = true;
            }

            // 用于在“奇遇直达终点”功能
            MassageWindow_DoEvent = typeof(MassageWindow).GetMethod("DoEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            if (MassageWindow_DoEvent == null)
            {
                logger.Log("获取MassageWindow.DoEvent失败");
            }
            // 初始化直接到终点的奇遇的ID清单
            if (settings.includedStoryTyps == null || settings.includedStoryTyps.Length != storytyps.Length)
            {
                settings.includedStoryTyps = new bool[storytyps.Length];
            }

            // 用于锁定每月行动点数（每秒重置一次行动点数）
            timer = new Timer(1000);
            timer.Elapsed += Main.Timer_Elapsed;
            timer.Start();

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        // 主界面
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Event e = Event.current;
            if (e.isKey && Input.anyKeyDown)
            {
                if (bindingKey)
                {
                    if ((e.keyCode >= KeyCode.A && e.keyCode <= KeyCode.Z)
                        || (e.keyCode >= KeyCode.F1 && e.keyCode <= KeyCode.F12)
                        || (e.keyCode >= KeyCode.Alpha0 && e.keyCode <= KeyCode.Alpha9)
                        )
                    {
                        Main.settings.hotKey = e.keyCode;
                    }
                    bindingKey = false;
                }
            }
            if (!enabled)
            {
                GUILayout.Box("<color=#E4504D>MOD未激活，请检查右侧Status是否为绿灯，如未激活请点击On/Off列的按钮激活</color>");
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("设置快捷键：", GUILayout.Width(130));
            if (GUILayout.Button((bindingKey ? "请按键" : settings.hotKey.ToString()),
                GUILayout.Width(80)))
            {
                bindingKey = !bindingKey;
            }
            GUILayout.Label("（支持0-9,A-Z,F1-F12）");
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 锁定时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (enabled && DateFile.instance != null && Main.settings.lockTime)
            {
                DateFile.instance.dayTime = DateFile.instance.GetMaxDayTime();
            }
        }

        /// <summary>
        /// 最大好感和最大印象
        /// </summary>
        [HarmonyPatch(typeof(MassageWindow), "SetMassageWindow")]
        private static class SAW_Hook
        {
            private static bool Prefix(int[] baseEventDate)
            {
                // 最大好感
                if (enabled && Main.settings.lockMaxOutRelation)
                {
                    MassageWindow.instance.mianEventDate = (int[]) baseEventDate.Clone();
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
                if (enabled && Main.settings.lockMaxLifeFace)
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

        /// <summary>
        /// 锁定戒心为0
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "GetActorWariness")]
        private static class ZW_Hook
        {
            private static void Postfix(ref int __result)
            {
                if (enabled && Main.settings.lockZeroWariness)
                {
                    __result = 0;
                }
            }
        }

        /// <summary>
        /// 快速修习
        /// </summary>
        [HarmonyPatch(typeof(HomeSystem), "StudySkillUp")]
        private static class SSU_Hook
        {
            private static bool Prefix(int ___studySkillId, int ___studySkillTyp, ref HomeSystem __instance)
            {
                if (!enabled || !Main.settings.lockMaxOutProficiency || ___studySkillId <= 0 || ___studySkillTyp <= 0 || ___studySkillTyp > 17)
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

        /// <summary>
        /// 奇遇直接到终点
        /// </summary>
        [HarmonyPatch(typeof(StorySystem), "OpenStory")]
        private static class OS_Hook
        {
            private static bool Prefix(ref StorySystem __instance)
            {
                if (!enabled || !Main.settings.lockFastQiyuCompletion)
                {
                    return true;
                }
                else
                {
                    int storyId = __instance.storySystemStoryId;
#if DEBUG
                    Main.logger.Log($"[TaiwuEditor]OpenStory: StoryId: {storyId}");
#endif                    
                    if (storyId > 0)
                    {
                        bool storyIdExist = false;
                        for(int i=0; i < settings.includedStoryTyps.Length; i++)
                        {
                            if (settings.includedStoryTyps[i])
                            {
                                if (storytyps[i].IsContainStoryId(storyId))
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

        /// <summary>
        /// 背包最大载重
        /// </summary>
        [HarmonyPatch(typeof(ActorMenu), "GetMaxItemSize")]
        private static class GMIS_Hook
        {
            private static void Postfix(ref int key, ref int __result)
            {
                if (enabled && Main.settings.lockNeverOverweigh && DateFile.instance.mianActorId == key)
                {
                    __result = 999999999;
                }
            }
        }

        /// <summary>
        /// 快速读书
        /// </summary>
        [HarmonyPatch(typeof(HomeSystem), "StartReadBook")]
        private static class SRB_Hook
        {
            private static bool Prefix(int ___readBookId, int ___studySkillTyp, HomeSystem __instance)
            {
#if DEBUG
                Main.logger.Log($"[TaiwuEditor]快速读书: id: {___readBookId}，SkillTyp: {___studySkillTyp}");
#endif
                if (!enabled || !Main.settings.lockFastRead || ___studySkillTyp <= 0 || ___studySkillTyp > 17 || ___readBookId <= 0)
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

        /// <summary>
        /// 锁定门派支持度
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "GetGangPartValue")]
        private static class GangPartValue_Hook
        {
            private static bool Prefix(int gangId, ref int __result)
            {
                if (!Main.enabled || !settings.lockGangMaxPartValue)
                {
                    return true;
                }
                // 太吾村没有支持度
                __result = (gangId == 16) ? 0 : (settings.customGangMaxPartValue == 0) ? DateFile.instance.GetMaxWorldValue() : settings.customGangMaxPartValue * 10;
                return false;
            }
        }

        /// <summary>
        /// 锁定地区恩义
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "GetBasePartValue")]
        private static class GetBasePartValue_Hook
        {
            // 返回锁定的值
            private static bool Prefix(ref int __result)
            {
                if (!Main.enabled || !settings.lockBaseMaxPartValue)
                {
                    return true;
                }
                __result = (settings.customBaseMaxPartValue == 0) ? DateFile.instance.GetMaxWorldValue() : settings.customBaseMaxPartValue * 10;
                return false;
            }
        }

        /// <summary>
        /// 锁定地区恩义
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "SetGangValue")]
        private static class SetGangValue_Hook
        {
            // 阻止地区恩义减少
            private static bool Prefix(ref int value)
            {
                if (Main.enabled && settings.lockBaseMaxPartValue && value < 0)
                {
                    value = 0;
                }
                return true;
            }
        }
    }

    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }

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
        public bool[] includedStoryTyps;
        public bool includedAllStoryTyps = false;
        // 背包无限
        public bool lockNeverOverweigh = false;
        // 见面最大化好感
        public bool lockMaxOutRelation = false;
        // 见面最大化印象
        public bool lockMaxLifeFace = false;
        // 锁定戒心为0
        public bool lockZeroWariness = false;
        // 锁定门派支持度
        public bool lockGangMaxPartValue = false;
        public int customGangMaxPartValue = 0;
        // 锁定地区恩义
        public bool lockBaseMaxPartValue = false;
        public int customBaseMaxPartValue = 0;
        // 快捷键
        public KeyCode hotKey = KeyCode.F6;
    }

    public class StoryTyp
    {
        private HashSet<int> storyIds;
        public string Name { get; private set; }
        public StoryTyp(HashSet<int> storyIds, string name)
        {
            this.storyIds = storyIds;
            Name = name;
        }

        public bool IsContainStoryId(int storyId) => storyIds.Contains(storyId);
    }
}
