using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace TaiwuEditor
{
    class Helper
    {
        private static readonly string errorString = "无人物数据";
        private static readonly int fieldHelperLblWidth = 90;
        private static readonly int fieldHelperTextWidth = 120;

        // 属性修改
        public static void FieldHelper(DateFile _instance, int resid, string fieldname, int actorid)
        {
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            GUILayout.Label(fieldname, GUILayout.Width(fieldHelperLblWidth));
            if (_instance.actorsDate == null || !_instance.actorsDate.ContainsKey(actorid))
            {
                GUILayout.TextField(errorString, GUILayout.Width(fieldHelperTextWidth));
            }
            else
            {
                if (actorid == 0)
                {
                    actorid = _instance.mianActorId;
                }
                _instance.actorsDate[actorid][resid] = GUILayout.TextField(_instance.GetActorDate(actorid, resid, false), GUILayout.Width(fieldHelperTextWidth));
            }
            GUILayout.EndHorizontal();
        }

        // 属性修改
        public static void FieldHelper(DateFile _instance, ref int field, string fieldname)
        {
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            GUILayout.Label(fieldname, GUILayout.Width(fieldHelperLblWidth));
            if (_instance.actorsDate == null)
            {
                GUILayout.TextField(errorString, GUILayout.Width(fieldHelperTextWidth));
            }
            else
            {
                field = _instance.ParseInt(GUILayout.TextField(field.ToString(), GUILayout.Width(fieldHelperTextWidth)));
            }
            GUILayout.EndHorizontal();
        }

        public static int LifeDateHelper(DateFile _instance, int resid, int actorid)
        {
            if (_instance.actorLife == null || !_instance.actorLife.ContainsKey(actorid))
            {
                return -1;
            }
            if (actorid == 0)
            {
                actorid = _instance.mianActorId;
            }
            int result = _instance.GetLifeDate(actorid, resid, 0);
            if(result == -1)
            {
                return 0;
            }
            else
            {
                return result;
            }
        }

        public static string actorDateHelper(DateFile _instance, int resid, int actorid, out bool isExist)
        {
            if (_instance.actorsDate == null || !_instance.actorsDate.ContainsKey(actorid))
            {
                isExist = false;
                return errorString;
            }
            else
            {
                if (actorid == 0)
                {
                    actorid = _instance.mianActorId;
                }
                isExist = true;
                return _instance.GetActorDate(actorid, resid, false);
            }            
        }

        // 设置人物相枢入邪值
        public static void SetActorXXChange(DateFile _instance, int actorid, int value)
        {
            if (_instance.actorLife == null || !_instance.actorLife.ContainsKey(actorid))
            {
                return;
            }
            value = Mathf.Max(value, 0);
            _instance.AICache_ActorPartValue.Remove(actorid);
            _instance.AICache_PartValue.Remove(_instance.ParseInt(_instance.GetActorDate(actorid, 19, false)));
            if (_instance.HaveLifeDate(actorid, 501))
            {
                _instance.actorLife[actorid][501][0] = value;
            }
            else
            {
                _instance.actorLife[actorid].Add(501, new List<int>
                {
                    value
                });
            }
            if (value >= 200)
            {
                if (_instance.ParseInt(_instance.GetActorDate(actorid, 6, false)) == 0)
                {
                    _instance.actorsDate[actorid][6] = "1";
                    List<int> actorAtPlace = _instance.GetActorAtPlace(actorid);
                    if (actorAtPlace!= null)
                    {
                        PeopleLifeAI.instance.AISetMassage(84, actorid, actorAtPlace[0], actorAtPlace[1], null, -1, true);
                    }
                }
                _instance.ChangeActorFeature(actorid, 9997, 9999);
                _instance.ChangeActorFeature(actorid, 9998, 9999);
            }
            else
            {
                _instance.actorsDate[actorid].Remove(6);
                if(value >= 100)
                {
                    _instance.ChangeActorFeature(actorid, 9997, 9998);
                    _instance.ChangeActorFeature(actorid, 9999, 9998);
                }
                else
                {
                    _instance.ChangeActorFeature(actorid, 9998, 9997);
                    _instance.ChangeActorFeature(actorid, 9999, 9997);
                }
            }
        } 

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
                                int _score = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(_actorId, 993, addValue: false));
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
