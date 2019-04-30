using UnityEngine;

namespace TaiwuEditor
{
    class UI
    {
        private static readonly int evilButtonWidth = 120;
        private static readonly int idFieldWidth = 100;
        private static readonly GUIStyle nameFieldFontSize = new GUIStyle() { fontSize = 20 };

        public static void BasePropertiesUi(ref Settings _instance)
        {
            DateFile instance = DateFile.instance;
            if (instance == null || instance.mianActorId == 0)
            {
                GUILayout.Box("未载入存档");
                return;
            }
            
            GUILayout.Label("<color=#87CEEB>修改人物：</color>");
            GUILayout.BeginHorizontal();
            _instance.basePropertyChoose = GUILayout.SelectionGrid(_instance.basePropertyChoose, new string[]
            {
                "<color=#FFA500>太吾本人</color>",
                "<color=#FFA500>最近打开过人物菜单的人物</color>",
                "<color=#FFA500>自定义人物</color>"
            }, 3);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();         
            GUILayout.Box("<color=#87CEEB>人物ID:</color>", GUILayout.Width(idFieldWidth));
            switch (_instance.basePropertyChoose)
            {
                // 修改上次打开人物菜单的人物
                case 1:
                    if (ActorMenu.instance != null && ActorMenu.instance.acotrId > 0)
                    {
                        _instance.actorId = ActorMenu.instance.acotrId;
                        GUILayout.Box($"{_instance.actorId}", GUILayout.Width(idFieldWidth));                        
                    }
                    break;
                case 2:
                    if (_instance.actorId > 0)
                    {
                        _instance.displayedActorId = GUILayout.TextField(_instance.displayedActorId, GUILayout.Width(idFieldWidth));
                        if(GUILayout.Button("确定", GUILayout.Width(idFieldWidth)))
                        {
                            if (Helper.TryParseInt(_instance.displayedActorId, out int parsedValue) && parsedValue >= 0)
                            {
                                _instance.actorId = parsedValue;
                            }
                            else
                            {
                                _instance.displayedActorId = "0";
                            }
                        }
                    }
                    break;
                default:
                    if (instance.mianActorId >= 0)
                    {
                        _instance.actorId = instance.mianActorId;
                        GUILayout.Box($"{_instance.actorId}", GUILayout.Width(idFieldWidth));
                    }
                    break;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Box($"<color=#FFA500>{instance.GetActorName(_instance.actorId)}</color>", nameFieldFontSize);
            GUILayout.EndHorizontal();

            GUILayout.Label("<color=#87CEEB>直接修改对应输入框中数值即可完成修改，请不要输入过于巨大的数值或是输入非自然数</color>");
            GUILayout.Label("<color=#87CEEB>修改完成后数值不发生变化是游戏界面没有刷新的原因，并不是修改不成功。</color>");
            GUILayout.Label("<color=#87CEEB>属性资质修改需重新进入人物菜单才会刷新结果而资源和威望修改需发生对应变化的行为后才会更新</color>");
            GUILayout.Label("<color=#87CEEB>所有资质属性均为基础值，不含特性、装备、早熟晚熟以及年龄加成</color>");            
            GUILayout.BeginHorizontal("Box");
            GUILayout.BeginVertical();
            GUILayout.Label("<color=#87CEEB>基本属性：</color>");
            Helper.FieldHelper(instance, 61, "膂力", _instance.actorId);
            Helper.FieldHelper(instance, 62, "体质", _instance.actorId);
            Helper.FieldHelper(instance, 63, "灵敏", _instance.actorId);
            Helper.FieldHelper(instance, 64, "根骨", _instance.actorId);
            Helper.FieldHelper(instance, 65, "悟性", _instance.actorId);
            Helper.FieldHelper(instance, 66, "定力", _instance.actorId);
            GUILayout.Label("<color=#87CEEB>资源： </color>");
            Helper.FieldHelper(instance, 401, "食材", _instance.actorId);
            Helper.FieldHelper(instance, 402, "木材", _instance.actorId);
            Helper.FieldHelper(instance, 403, "金石", _instance.actorId);
            Helper.FieldHelper(instance, 404, "织物", _instance.actorId);
            Helper.FieldHelper(instance, 405, "草药", _instance.actorId);
            Helper.FieldHelper(instance, 406, "金钱", _instance.actorId);
            Helper.FieldHelper(instance, 407, "威望", _instance.actorId);
            GUILayout.Label("<color=#87CEEB>太吾本人限定： </color>");
            if (_instance.actorId == instance.mianActorId)
            {
                
                Helper.FieldHelper(instance, ref instance.gongFaExperienceP, "历练");                
                Helper.FieldHelper(instance, 706, "无属性内力", _instance.actorId);                
            }
            GUILayout.Label("每10点无属性内力增加1点真气");
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("<color=#87CEEB>技艺资质： </color>");
            Helper.FieldHelper(instance, 501, "音律", _instance.actorId);
            Helper.FieldHelper(instance, 502, "弈棋", _instance.actorId);
            Helper.FieldHelper(instance, 503, "诗书", _instance.actorId);
            Helper.FieldHelper(instance, 504, "绘画", _instance.actorId);
            Helper.FieldHelper(instance, 505, "术数", _instance.actorId);
            Helper.FieldHelper(instance, 506, "品鉴", _instance.actorId);
            Helper.FieldHelper(instance, 507, "锻造", _instance.actorId);
            Helper.FieldHelper(instance, 508, "制木", _instance.actorId);
            Helper.FieldHelper(instance, 509, "医术", _instance.actorId);
            Helper.FieldHelper(instance, 510, "毒术", _instance.actorId);
            Helper.FieldHelper(instance, 511, "织锦", _instance.actorId);
            Helper.FieldHelper(instance, 512, "巧匠", _instance.actorId);
            Helper.FieldHelper(instance, 513, "道法", _instance.actorId);
            Helper.FieldHelper(instance, 514, "佛学", _instance.actorId);
            Helper.FieldHelper(instance, 515, "厨艺", _instance.actorId);
            Helper.FieldHelper(instance, 516, "杂学", _instance.actorId);
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("<color=#87CEEB>功法资质： </color>");
            Helper.FieldHelper(instance, 601, "内功", _instance.actorId);
            Helper.FieldHelper(instance, 602, "身法", _instance.actorId);
            Helper.FieldHelper(instance, 603, "绝技", _instance.actorId);
            Helper.FieldHelper(instance, 604, "拳掌", _instance.actorId);
            Helper.FieldHelper(instance, 605, "指法", _instance.actorId);
            Helper.FieldHelper(instance, 606, "腿法", _instance.actorId);
            Helper.FieldHelper(instance, 607, "暗器", _instance.actorId);
            Helper.FieldHelper(instance, 608, "剑法", _instance.actorId);
            Helper.FieldHelper(instance, 609, "刀法", _instance.actorId);
            Helper.FieldHelper(instance, 610, "长兵", _instance.actorId);
            Helper.FieldHelper(instance, 611, "奇门", _instance.actorId);
            Helper.FieldHelper(instance, 612, "软兵", _instance.actorId);
            Helper.FieldHelper(instance, 613, "御射", _instance.actorId);
            Helper.FieldHelper(instance, 614, "乐器", _instance.actorId);            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            int evilValue = Helper.LifeDateHelper(instance, 501, _instance.actorId);
            GUILayout.BeginVertical("Box");
            GUILayout.Label("<color=#87CEEB>相枢入邪值修改：</color><color=#FFA500>(对“执迷入邪” 和 “执迷化魔” 无效，这两者只与奇书功法学习进度有关)</color>");
            GUILayout.BeginHorizontal();
            GUILayout.Box("当前入邪值: ", GUILayout.Width(evilButtonWidth));
            if (evilValue == -1)
            {
                GUILayout.Box("无", GUILayout.Width(evilButtonWidth));
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Box($"{evilValue}", GUILayout.Width(evilButtonWidth));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("恢复正常", GUILayout.Width(evilButtonWidth)))
                {
                    Helper.SetActorXXChange(instance, _instance.actorId, 0);
                }
                if (GUILayout.Button("相枢入邪", GUILayout.Width(evilButtonWidth)))
                {
                    Helper.SetActorXXChange(instance, _instance.actorId, 100);
                }
                if (_instance.actorId != 0 && _instance.actorId != instance.mianActorId)
                {
                    if (GUILayout.Button("相枢化魔", GUILayout.Width(evilButtonWidth)))
                    {
                        Helper.SetActorXXChange(instance, _instance.actorId, 200);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        public static void SettingsUi(ref Settings __instance)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal("Box");
            __instance.lockTime = GUILayout.Toggle(__instance.lockTime, "锁定一月行动不减", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            __instance.lockFastRead = GUILayout.Toggle(__instance.lockFastRead, "快速读书（对残缺篇章有效）", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (__instance.lockFastRead)
            {
                GUILayout.Label($"每次阅读{__instance.pagesPerFastRead}篇(只对功法类书籍有效，技艺类书籍会全部读完)");
                __instance.pagesPerFastRead = (int)GUILayout.HorizontalScrollbar(__instance.pagesPerFastRead, 1, 11, 1);
            }
            GUILayout.BeginHorizontal("Box");
            __instance.lockMaxOutProficiency = GUILayout.Toggle(__instance.lockMaxOutProficiency, "修习单击全满", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            __instance.lockFastQiyuCompletion = GUILayout.Toggle(__instance.lockFastQiyuCompletion, "奇遇直接到达目的地", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            __instance.lockNeverOverweigh = GUILayout.Toggle(__instance.lockNeverOverweigh, "身上物品永不超重（仓库无效）", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            __instance.lockMaxOutRelation = GUILayout.Toggle(__instance.lockMaxOutRelation, "见面关系全满", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            __instance.lockMaxLifeFace = GUILayout.Toggle(__instance.lockMaxLifeFace, "见面印象最深(换衣服会重置印象)", new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            __instance.lockZeroWariness = GUILayout.Toggle(__instance.lockZeroWariness, "锁定戒心为零", new GUILayoutOption[0]);            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
