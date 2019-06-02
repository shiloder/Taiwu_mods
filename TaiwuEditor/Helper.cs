using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityModManagerNet;

namespace TaiwuEditor
{
    class Helper
    {
        private static readonly Dictionary<int, string> fieldNames = new Dictionary<int, string>()
        {
            { -1, "历练"},
            { 11, "年龄"},
            { 12, "健康"},
            { 13, "基础寿命"},
            { 61, "膂力"},
            { 62, "体质"},
            { 63, "灵敏"},
            { 64, "根骨"},
            { 65, "悟性"},
            { 66, "定力"},
            {401, "食材"},
            {402, "木材"},
            {403, "金石"},
            {404, "织物"},
            {405, "草药"},
            {406, "金钱"},
            {407, "威望"},
            {501, "音律"},
            {502, "弈棋"},
            {503, "诗书"},
            {504, "绘画"},
            {505, "术数"},
            {506, "品鉴"},
            {507, "锻造"},
            {508, "制木"},
            {509, "医术"},
            {510, "毒术"},
            {511, "织锦"},
            {512, "巧匠"},
            {513, "道法"},
            {514, "佛学"},
            {515, "厨艺"},
            {516, "杂学"},
            {601, "内功"},
            {602, "身法"},
            {603, "绝技"},
            {604, "拳掌"},
            {605, "指法"},
            {606, "腿法"},
            {607, "暗器"},
            {608, "剑法"},
            {609, "刀法"},
            {610, "长兵"},
            {611, "奇门"},
            {612, "软兵"},
            {613, "御射"},
            {614, "乐器"},
            {706, "无属性内力"}
        };

        private const string errorString = "无人物数据";

        public float fieldHelperLblWidth = 90f;
        public float fieldHelperTextWidth = 120f;
        public float fieldHelperBtnWidth = 80f;

        private readonly HashSet<int> activeFieldsResid;
        private readonly Dictionary<int, string> fieldValues;
        private int lastActorId = -1;
        private DateFile lastDateFile;
        private ActorMenu lastActorMenu;
        private Dictionary<int, string> lastActorDate;

        private UnityModManager.ModEntry.ModLogger logger;

        public GUIStyle LabelStyle { get; set; }
        public GUIStyle ButtonStyle { get; set; }
        public GUIStyle TextFieldStyle { get; set; }

        //
        public Helper(UnityModManager.ModEntry.ModLogger logger)
        {
            this.logger = logger;
            activeFieldsResid = new HashSet<int>(fieldNames.Count);
            fieldValues = new Dictionary<int, string>(fieldNames.Count);
        }

        /// <summary>
        /// 属性修改框框架
        /// </summary>
        /// <param name="resid">对应属性的编号</param>
        public void FieldHelper(int resid)
        {
            GUILayout.BeginHorizontal("Box");
            if (fieldNames.TryGetValue(resid, out string fieldname))
            {
                if (LabelStyle == null)
                    GUILayout.Label(fieldname, GUILayout.Width(fieldHelperLblWidth));
                else
                    GUILayout.Label(fieldname, LabelStyle, GUILayout.Width(fieldHelperLblWidth));
                activeFieldsResid.Add(resid);
                if (fieldValues.TryGetValue(resid, out string fieldValue))
                {
                    if (TextFieldStyle == null)
                        fieldValues[resid] = GUILayout.TextField(fieldValue, GUILayout.Width(fieldHelperTextWidth));
                    else
                        fieldValues[resid] = GUILayout.TextField(fieldValue, TextFieldStyle, GUILayout.Width(fieldHelperTextWidth));
                    if (ButtonStyle == null)
                    {
                        if (GUILayout.Button("修改", GUILayout.Width(fieldHelperBtnWidth)))
                        {
                            SetFieldValue(resid);
                            UpdateField(resid);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("修改", ButtonStyle, GUILayout.Width(fieldHelperBtnWidth)))
                        {
                            SetFieldValue(resid);
                            UpdateField(resid);
                        }
                    }
                }
                else
                {
                    if (TextFieldStyle == null)
                        GUILayout.TextField(errorString, GUILayout.Width(fieldHelperTextWidth));
                    else
                        GUILayout.TextField(errorString, TextFieldStyle, GUILayout.Width(fieldHelperTextWidth));
                }
            }
            else
            {
                GUILayout.Label("FieldName不存在");
            }
            GUILayout.EndHorizontal();
        }

        public void FieldHelper(int resid, int max)
        {
            GUILayout.BeginHorizontal("Box");
            if (fieldNames.TryGetValue(resid, out string fieldname))
            {
                if (LabelStyle == null)
                    GUILayout.Label(fieldname, GUILayout.Width(fieldHelperLblWidth));
                else
                    GUILayout.Label(fieldname, LabelStyle, GUILayout.Width(fieldHelperLblWidth));
                activeFieldsResid.Add(resid);
                if (fieldValues.TryGetValue(resid, out string fieldValue))
                {
                    GUILayout.BeginHorizontal();
                    if (TextFieldStyle == null)
                        fieldValues[resid] = GUILayout.TextField(fieldValue, GUILayout.Width(fieldHelperTextWidth));
                    else
                        fieldValues[resid] = GUILayout.TextField(fieldValue, TextFieldStyle, GUILayout.Width(fieldHelperTextWidth));
                    if(LabelStyle == null)
                    {
                        GUILayout.Label($"/<color=#FFA500>{max}</color>", GUILayout.ExpandWidth(false));
                    }
                    else
                    {
                        LabelStyle.fontStyle = FontStyle.Bold;
                        GUILayout.Label($"/<color=#FFA500>{max}</color>", LabelStyle, GUILayout.ExpandWidth(false));
                        LabelStyle.fontStyle = FontStyle.Normal;
                    }
                    GUILayout.EndHorizontal();
                    if (ButtonStyle == null)
                    {
                        if (GUILayout.Button("修改", GUILayout.Width(fieldHelperBtnWidth)))
                        {
                            SetFieldValue(resid, 0, max);
                            UpdateField(resid);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("修改", ButtonStyle, GUILayout.Width(fieldHelperBtnWidth)))
                        {
                            SetFieldValue(resid, 0, max);
                            UpdateField(resid);
                        }
                    }
                }
                else
                {
                    if (TextFieldStyle == null)
                        GUILayout.TextField(errorString, GUILayout.Width(fieldHelperTextWidth));
                    else
                        GUILayout.TextField(errorString, TextFieldStyle, GUILayout.Width(fieldHelperTextWidth));
                }
            }
            else
            {
                GUILayout.Label("FieldName不存在");
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 属性修改框框架
        /// </summary>
        /// <param name="resid">对应属性的编号</param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void FieldHelper(int resid, int min, int max)
        {
            GUILayout.BeginHorizontal("Box");
            if (fieldNames.TryGetValue(resid, out string fieldname))
            {
                if (LabelStyle == null)
                    GUILayout.Label($"{fieldname}(<color=#FFA500>{min}~{max}</color>)", GUILayout.Width(fieldHelperLblWidth));
                else
                    GUILayout.Label($"{fieldname}(<color=#FFA500>{min}~{max}</color>)", LabelStyle, GUILayout.Width(fieldHelperLblWidth));
                activeFieldsResid.Add(resid);
                if (fieldValues.TryGetValue(resid, out string fieldValue))
                {
                    if (TextFieldStyle == null)
                        fieldValues[resid] = GUILayout.TextField(fieldValue, GUILayout.Width(fieldHelperTextWidth));
                    else
                        fieldValues[resid] = GUILayout.TextField(fieldValue, TextFieldStyle, GUILayout.Width(fieldHelperTextWidth));
                    if (ButtonStyle == null)
                    {
                        if (GUILayout.Button("修改", GUILayout.Width(fieldHelperBtnWidth)))
                        {
                            SetFieldValue(resid, min, max);
                            UpdateField(resid);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("修改", ButtonStyle, GUILayout.Width(fieldHelperBtnWidth)))
                        {
                            SetFieldValue(resid, min, max);
                            UpdateField(resid);
                        }
                    }
                }
                else
                {
                    if (TextFieldStyle == null)
                        GUILayout.TextField(errorString, GUILayout.Width(fieldHelperTextWidth));
                    else
                        GUILayout.TextField(errorString, TextFieldStyle, GUILayout.Width(fieldHelperTextWidth));
                }
            }
            else
            {
                GUILayout.Label("FieldName不存在");
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 获得当前编辑的角色属性数据
        /// </summary>
        /// <param name="index">对应属性的编号</param>
        /// <param name="text">获取的属性数据，获取失败则为空</param>
        /// <returns>成功获取返回True，否则返回false</returns>
        public bool GetLastActorData(int index, out string text)
        {
            if (lastActorDate != null && lastActorDate.TryGetValue(index, out text))
            {
                return true;
            }
            text = "";
            return false;
        }

        /// <summary>
        /// 将游戏数值同步到所有属性修改框里
        /// </summary>
        /// <param name="dateFileInstance">DateFile实例</param>
        /// <param name="actorId">需要同步数据的角色Id</param>
        public void UpdateAllFields(DateFile dateFileInstance, ActorMenu actorMenuInstance, int actorId)
        {
            lastDateFile = dateFileInstance;
            lastActorMenu = actorMenuInstance;
            if (lastDateFile == null || lastDateFile.actorsDate == null || !lastDateFile.actorsDate.TryGetValue(actorId, out lastActorDate))
            {
                foreach (var resid in activeFieldsResid)
                {
                    fieldValues.Remove(resid);
                }
                lastActorId = -1;
            }
            else if (actorId != lastActorId)
            {
                lastActorId = actorId;
                foreach (var resid in activeFieldsResid)
                {
                    FetchFieldValue(lastDateFile, lastActorMenu, lastActorDate, resid);
                }
            }
        }

        /// <summary>
        /// 重置所有修改框的状态，需要配合UpdateAllFields()才能更新数值
        /// </summary>
        public void ResetAllFields()
        {
            lastActorId = -1;
        }

        /// <summary>
        /// 将游戏数值同步到属性修改框里
        /// </summary>
        /// <param name="resid">对应属性的编号</param>
        private void UpdateField(int resid)
        {
            if (lastDateFile == null || lastActorDate == null)
            {
                fieldValues.Remove(resid);
                return;
            }
            else
            {
                FetchFieldValue(lastDateFile, lastActorMenu, lastActorDate, resid);
            }
        }

        private void FetchFieldValue(DateFile dateFileInstance, ActorMenu actorMenuInstance, Dictionary<int, string> actorDate, int resid)
        {
            switch (resid)
            {
                case -1:
                    fieldValues[resid] = dateFileInstance.gongFaExperienceP.ToString();
                    break;
                case 12:
                    if(actorMenuInstance != null)
                    {
                        fieldValues[resid] = actorMenuInstance.Health(lastActorId).ToString();
                    }
                    else
                    {
                        fieldValues.Remove(resid);
                    }
                    break;
                default:
                    if(!actorDate.TryGetValue(resid, out string text))
                    {
                        if(!dateFileInstance.presetActorDate.TryGetValue(lastActorId, out Dictionary<int,string> presetActorData) || !presetActorData.TryGetValue(resid, out text))
                        {
                            text = "0";
                        }
                    }
                    fieldValues[resid] = text;
                    break;
            }
        }

        /// <summary>
        /// 将修改框里的数据设定为游戏数据
        /// </summary>
        /// <param name="resid">对应属性的编号</param>
        private void SetFieldValue(int resid)
        {
            if (lastDateFile != null && lastActorId != -1 && fieldValues.TryGetValue(resid, out string text))
            {
                if (TryParseInt(text, out int value) && value >= 0)
                {
                    SetValueHelper(resid, text, value);
                }
            }
        }

        /// <summary>
        /// 将修改框里的数据设定为游戏数据
        /// </summary>
        /// <param name="resid">对应属性的编号</param>
        /// <param name="min">数值最小值</param>
        /// <param name="max">数值最大值</param>
        private void SetFieldValue(int resid, int min, int max)
        {
            if (lastDateFile != null && lastActorId != -1 && fieldValues.TryGetValue(resid, out string text))
            {
                if (TryParseInt(text, out int value) && value >= min && value <= max)
                {
                    SetValueHelper(resid, text, value);
                }
            }
        }

        /// <summary>
        /// 为了简化代码，将设定值的部分另写一个方法
        /// </summary>
        /// <param name="resid">对应属性的编号</param>
        /// <param name="text">目标数值的字符串形式</param>
        /// <param name="value">目标数值的整型形式，值必须与text相同</param>
        private void SetValueHelper(int resid, string text, int value)
        {
            switch (resid)
            {
                case -1:
                    lastDateFile.gongFaExperienceP = value;
                    break;                
                default:
                    lastActorDate[resid] = text;
                    break;
            }
        }

        /// <summary>
        /// 治疗解毒
        /// </summary>
        /// <param name="actorId">要处理的角色ID</param>
        /// <param name="func">功能选择，0疗伤，1祛毒，2调理内息</param>
        public static void CureHelper(DateFile instance, int actorId, int func, bool battle = true)
        {
            if (instance == null)
                return;
            switch (func)
            {
                case 0:
                    if (instance.actorInjuryDate.TryGetValue(actorId, out Dictionary<int,int> injuries))
                    {
                        List<int> injuryIds = new List<int>(injuries.Keys);
                        for(int i=0; i< injuryIds.Count; i++)
                        {
                            injuries.Remove(injuryIds[i]);
                        }
                    }
                    if(battle && instance.ActorIsInBattle(actorId) != 0 && instance.battleActorsInjurys.TryGetValue(actorId, out Dictionary<int, int[]> battleInjuries))
                    {
                        List<int> battleInjuriesIds = new List<int>(battleInjuries.Keys);
                        for (int i = 0; i < battleInjuriesIds.Count; i++)
                        {
                            battleInjuries.Remove(battleInjuriesIds[i]);
                        }
                    }
                    break;
                case 1:
                    if (instance.actorsDate.TryGetValue(actorId, out Dictionary<int,string> actorData))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            actorData[i + 51] = "0";
                        }
                    }
                    if (battle && instance.ActorIsInBattle(actorId) != 0 && instance.battleActorsPoisons.TryGetValue(actorId, out int[] poisons))
                    {
                        for(int i=0; i< poisons.Length; i++)
                        {
                            poisons[i] = 0;
                        }
                    }
                    break;
                case 2:
                    if (instance.actorsDate.TryGetValue(actorId, out actorData))
                    {
                        actorData[39] = "0";
                    }
                    if(battle && instance.ActorIsInBattle(actorId) != 0)
                    {
                        instance.battleActorsMianQi[actorId] = 0;
                    }
                    break;
            }
        }

        public static int LifeDateHelper(DateFile instance, int resid, int actorid)
        {
            if (instance.actorLife == null || !instance.actorLife.ContainsKey(actorid))
            {
                return -1;
            }
            actorid = (actorid == 0) ? instance.mianActorId : actorid;
            int result = instance.GetLifeDate(actorid, resid, 0);
            return (result == -1) ? 0 : result;
        }


        /// <summary>
        /// 设置人物相枢入邪值
        /// </summary>
        /// <param name="instance">DateFile实例</param>
        /// <param name="actorid">角色ID</param>
        /// <param name="value">目标入邪值</param>
        // 设置人物相枢入邪值, 根据DateFile.SetActorXXChange重写
        public static void SetActorXXValue(DateFile instance, int actorid, int value)
        {
            if (instance.actorLife == null || !instance.actorLife.ContainsKey(actorid))
            {
                return;
            }
            value = Mathf.Max(value, 0);
            // 清空角色相支持度缓存
            instance.AICache_ActorPartValue.Remove(actorid);
            instance.AICache_PartValue.Remove(instance.ParseInt(instance.GetActorDate(actorid, 19, false)));
            if (instance.HaveLifeDate(actorid, 501))
            {
                instance.actorLife[actorid][501][0] = Mathf.Max(value, 0);
            }
            else
            {
                instance.actorLife[actorid].Add(501, new List<int>
                {
                    Mathf.Max(value, 0)
                });
            }
            if (value >= 200)
            {
                // 是否是敌方阵营（如相枢）
                if (instance.ParseInt(instance.GetActorDate(actorid, 6, false)) == 0)
                {
                    // 化魔之后加入敌方
                    instance.actorsDate[actorid][6] = "1";
                    // 获取该角色的坐标
                    List<int> actorAtPlace = instance.GetActorAtPlace(actorid);
                    if (actorAtPlace != null)
                    {
                        PeopleLifeAI.instance.AISetMassage(84, actorid, actorAtPlace[0], actorAtPlace[1], null, -1, true);
                    }
                }
                // 相枢化魔特性
                instance.ChangeActorFeature(actorid, 9997, 9999);
                instance.ChangeActorFeature(actorid, 9998, 9999);
            }
            else
            {
                // 不为敌
                instance.actorsDate[actorid].Remove(6);
                if (value >= 100)
                {
                    // 相枢入邪特性
                    instance.ChangeActorFeature(actorid, 9997, 9998);
                    instance.ChangeActorFeature(actorid, 9999, 9998);
                }
                else
                {
                    // 正常
                    instance.ChangeActorFeature(actorid, 9998, 9997);
                    instance.ChangeActorFeature(actorid, 9999, 9997);
                }
            }
        }

        /// <summary>
        /// 快速读书
        /// </summary>
        /// <param name="readbookid">书籍ID</param>
        /// <param name="studyskilltyp">技能类型</param>
        /// <param name="counter">每次快速读书篇数</param>
        // 快速读书 Highly Inspired by ReadBook.GetReadBooty()
        // counter: 每次快速读书篇数（只针对需要平衡正逆练的功法书，技艺书还是一次全读完）
        public static void EasyReadV2(int readbookid, int studyskilltyp, int counter)
        {
            int mainActorId = DateFile.instance.MianActorID();
            // 功法id
            int gongFaId = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(readbookid, 32, true));
            // 可能代表“每页是否是残卷”
            int[] bookPage = DateFile.instance.GetBookPage(readbookid);
            for (int j = 0; j < 10; j++) // 每书10篇
            {
                //int experienceGainRatio = 100; // 读书获得历练加成比例
                // 获得的历练
                // int experienceGainPerPage = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 34, true)) * experienceGainRatio / 100; 
                int experienceGainPerPage = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(readbookid, 34, true)) * 100 / 100;
                // 读的是功法
                if (studyskilltyp == 17)
                {
                    // 如果是从来没读过的功法
                    if (!DateFile.instance.gongFaBookPages.ContainsKey(gongFaId))
                    {
                        DateFile.instance.gongFaBookPages.Add(gongFaId, new int[10]);
                    }
                    int studyDegree = DateFile.instance.gongFaBookPages[gongFaId][j];
                    // 若该篇章未曾读过
                    if (studyDegree != 1 && studyDegree > -100)
                    {
                        // 每篇读完应获得的遗惠
                        int scoreGain = DateFile.instance.ParseInt(DateFile.instance.gongFaDate[gongFaId][2]);
                        // 是否为手抄
                        int isShouChao = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(readbookid, 35, true));
                        DateFile.instance.ChangeActorGongFa(mainActorId, gongFaId, 0, 0, isShouChao, true);
                        if (isShouChao != 0)
                        {
                            //增加内息紊乱
                            ActorMenu.instance.ChangeMianQi(mainActorId, 50 * scoreGain, 5);
                        }
                        // 该篇已经阅读完毕
                        DateFile.instance.gongFaBookPages[gongFaId][j] = 1;
                        counter--;
                        // 增加遗惠
                        DateFile.instance.AddActorScore(303, scoreGain * 100);
                        if (DateFile.instance.GetGongFaLevel(mainActorId, gongFaId, 0) >= 100 && DateFile.instance.GetGongFaFLevel(mainActorId, gongFaId) >= 10)
                        {
                            // 增加遗惠
                            DateFile.instance.AddActorScore(304, scoreGain * 100);
                        }
                        if (bookPage[j] == 0)
                        {
                            // 增加遗惠
                            DateFile.instance.AddActorScore(305, scoreGain * 100);
                        }
                    }
                    else
                    {
                        // 已经读过的篇章加1/10基础历练
                        experienceGainPerPage = experienceGainPerPage * 10 / 100;
                    }
                    DateFile.instance.gongFaExperienceP += experienceGainPerPage;
                    //达到快速读书单次上限，停止读书                    
                    if (counter < 1)
                    {
                        break;
                    }
                }
                else   //读的是技艺书
                {
                    // 如果是从来没读过的技艺
                    if (!DateFile.instance.skillBookPages.ContainsKey(gongFaId))
                    {
                        DateFile.instance.skillBookPages.Add(gongFaId, new int[10]);
                    }
                    int studyDegree = DateFile.instance.skillBookPages[gongFaId][j];
                    // 若该篇章未曾读过
                    if (studyDegree != 1 && studyDegree > -100)
                    {
                        // 每篇读完应获得的遗惠
                        int scoreGain = DateFile.instance.ParseInt(DateFile.instance.skillDate[gongFaId][2]);
                        // 若还未习得该项技艺
                        if (!DateFile.instance.actorSkills.ContainsKey(gongFaId))
                        {
                            // 将该技艺添加到太吾身上
                            DateFile.instance.ChangeMianSkill(gongFaId, 0, 0, true);
                        }
                        DateFile.instance.skillBookPages[gongFaId][j] = 1;
                        // 增加遗惠
                        DateFile.instance.AddActorScore(203, scoreGain * 100);
                        if (DateFile.instance.GetSkillLevel(gongFaId) >= 100 && DateFile.instance.GetSkillFLevel(gongFaId) >= 10)
                        {
                            // 增加遗惠
                            DateFile.instance.AddActorScore(204, scoreGain * 100);
                        }
                        if (bookPage[j] == 0)
                        {
                            // 增加遗惠
                            DateFile.instance.AddActorScore(205, scoreGain * 100);
                        }
                    }
                    else
                    {
                        // 已经读过的篇章加1/10基础历练
                        experienceGainPerPage = experienceGainPerPage * 10 / 100;
                    }
                    // 增加历练
                    DateFile.instance.gongFaExperienceP += experienceGainPerPage;
                }
            }
        }

        // 终点事件设置，Inspired by DateFile.EventSetup()
        public static bool EventSetup(int endeventid, int storysystempartid, int storysystemplaceid, int storysystemstoryId)
        {
            if (endeventid != 0)
            {
                int _storyValue = DateFile.instance.worldMapState[storysystempartid][storysystemplaceid][2];
                switch (endeventid)
                {
                    // 十四奇书奇遇终点
                    case 2307:
                        int _getBookActorId = 0;
                        int _getBookActorScore = 0;
                        // 获取同道(包含太吾)
                        List<int> _actorFamily = DateFile.instance.GetFamily(true);
                        // 奇遇地块上的人物ID
                        List<int> _placeActors = DateFile.instance.HaveActor(storysystempartid, storysystemplaceid, true, false, false, false);
                        for (int i = 0; i < _placeActors.Count; i++)
                        {
                            int _actorId = _placeActors[i];
                            if (!_actorFamily.Contains(_actorId) && DateFile.instance.HaveLifeDate(_actorId, 710))
                            {
                                // 选出战力最强的拿到书与主角在终点决战
                                int _score = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(_actorId, 993, false));
                                if (_score > _getBookActorScore)
                                {
                                    _getBookActorScore = _score;
                                    _getBookActorId = _actorId;
                                }
                            }
                        }
                        // 奇书ID
                        int _bookId = new List<int>(DateFile.instance.legendBookId.Keys)[storysystemstoryId - 11001];
                        // 如果奇遇区块有人则要抢, 否则直接得到奇书
                        if (_getBookActorId > 0)
                        {
                            DateFile.instance.SetEvent(new int[5]
                            {
                                0,
                                _getBookActorId,
                                endeventid + DateFile.instance.GetActorGoodness(_getBookActorId),
                                _getBookActorId,
                                _bookId
                            }, true, true);
                        }
                        else
                        {
                            DateFile.instance.SetEvent(new int[4]
                            {
                                0,
                                -1,
                                2312,
                                _bookId
                            }, true, true);
                        }
                        break;
                    // 天材地宝奇遇
                    case 2325:
                        DateFile.instance.SetEvent(new int[5]
                        {
                            0,
                            -1,
                            endeventid,
                            // 最高品级（二品）材料的ID
                            storysystemstoryId,
                            // 得到高品级的概率，计算公式在MessageWindow.EndEvent2325_1()
                            Mathf.Max(100 + DateFile.instance.storyBuffs[-3] - DateFile.instance.storyDebuffs[-3], 0)
                        }, true, true);
                        break;
                    // 私奔奇遇终点
                    // 中庸
                    case 9606:
                    // 仁善
                    case 9607:
                    // 刚正
                    case 9608:
                        DateFile.instance.SetEvent(new int[4]
                        {
                            0,
                            _storyValue,
                            endeventid,
                            _storyValue
                        }, true, true);
                        break;
                    // 叛逆
                    case 9609:
                    // 唯我
                    case 9610:
                        DateFile.instance.SetEvent(new int[4]
                        {
                            0,
                            7010 + DateFile.instance.ParseInt(DateFile.instance.GetActorDate(_storyValue, 19, addValue: false)) * 100,
                            endeventid,
                            _storyValue
                        }, true, true);
                        break;
                    // 其他情况
                    default:
                        DateFile.instance.SetEvent(new int[3]
                        {
                            0,
                            -1,
                            endeventid
                        }, true, true);
                        break;
                }
                return true;
            }
            else
            {
                // 如果奇遇终点不存在，直接移除改奇遇
                DateFile.instance.SetStory(true, storysystempartid, storysystemplaceid, 0, 0);
                return false;
            }
        }

        public static bool TryParseInt(string text, out int integer)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out integer);
        }
    }
}
