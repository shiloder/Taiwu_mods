using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace TaiwuEditor
{
    class UI : MonoBehaviour
    {
        #region Constant
        // 窗口背景图
        private const string windowBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAIAAAEACAYAAACZCaebAAAAnElEQVRIS63MtQHDQADAwPdEZmaG/fdJCq2g7qqLvu/7hRBCZOF9X0ILz/MQWrjvm1DHdV3MFs7zJLRwHAehhX3fCS1s20ZoYV1XQgvLshDqmOeZ2cI0TYQWxnEktDAMA6GFvu8JLXRdR2ihbVtCHU3TMFuo65rQQlVVhBbKsiS0UBQFoYU8zwktZFlGqCNNU2YLSZIQWojjmFDCH22GtZAncD8TAAAAAElFTkSuQmCC";

        // 功能选项卡
        private static readonly string[] funcTab =
        {
            "<color=#FFA500>基本功能</color>",
            "<color=#FFA500>属性修改</color>"
        };
        // 属性资源修改选项卡
        private static readonly string[] dataTabNames =
        {
            "<color=#87CEEB>基本属性：</color>",
            "<color=#87CEEB>资源： </color>",
            "<color=#87CEEB>技艺资质： </color>",
            "<color=#87CEEB>功法资质： </color>",
            "<color=#87CEEB>太吾本人限定： </color>",
            "<color=#87CEEB>健康、寿命： </color>",
            "<color=#87CEEB>相枢入邪值修改：</color><color=#FFA500>(对“执迷入邪” 和 “执迷化魔” 无效，这两者只与奇书功法学习进度有关)</color>"
        };
        
        #endregion

        #region Style
        // 窗口的样式
        public static GUIStyle windowStyle;
        private static Texture2D windowTexture;
        // 标题名称的样式
        public static GUIStyle titleStyle;
        public static GUIStyle propertiesUIButtonStyle;
        public static GUIStyle labelStyle;
        public static GUIStyle nameFieldStyle;
        public static GUIStyle buttonStyle;
        public static GUIStyle toggleStyle;
        public static GUIStyle boxStyle;
        public static GUIStyle textFieldStyle;
        public static GUIStyle commentStyle;
        #endregion

        #region Instance
        public static UI Instance { get; private set; } = null;
        #endregion

        //logger
        public static UnityModManager.ModEntry.ModLogger logger;
        // 太吾修改器的参数
        private static Settings modSettings;
        private static StoryTyp[] storyTyps;

        #region Private Class Member
        // 是否初始化
        private bool mInit = false;

        private int lastScreenWidth;
        // 主窗口尺寸
        private Rect mWindowRect = new Rect(0f, 0f, 0f, 0f);
        // 主窗口最小宽度
        private float mWindowMinWidth = 960f;
        // 调整入魔值按钮的宽度
        private float evilButtonWidth = 120f;
        // 调整疗伤祛毒理气按钮宽度
        private float healthButtonWidth = 150f;
        // 人物属性修改页面中，人物id字段显示宽度
        private float idFieldWidth = 100f;
        // 字体大小
        private int normalFontSize = 16;
        // 关闭按钮长度
        private float closeBtnWidth = 150f;
        // 屏蔽游戏界面
        private GameObject mCanvas;     
        private Helper helper;

        // UI的参数
        // 功能选择，0是基本功能，1是修改属性
        private int funcChoose = 0;
        // 选择修改哪个人物的属性，0太吾，1上一个打开菜单的人物，2自定义人物
        private int basePropertyChoose = 0;
        // 显示在自定义人物ID输入框的值
        private string displayedActorId = "0";
        // 想要修改属性的NPC ID
        private int actorId = 0;
        // 滚动条的位置
        private Vector2[] scrollPosition = new Vector2[0];
        // 人物属性修改界面选择修改哪一类属性
        private int showTabDetails = -1;
        private string tmpCustomGangMaxPartValue = "0";
        private string tmpCustomBaseMaxPartValue = "0";
        #endregion

        #region Public Class Member
        public bool Opened { get; private set; } = false;
        #endregion

        internal static bool Load(UnityModManager.ModEntry modEntry, Settings instance, StoryTyp[] storyTyps = null)
        {
            if(instance == null)
            {
                Debug.Log("[TaiwuEditor] UI.Load() Settings instance is null");
                return false;
            }
            logger = modEntry.Logger;
            try
            {
                new GameObject(typeof(UI).FullName, typeof(UI));
                modSettings = instance;
                UI.storyTyps = storyTyps;
                windowTexture = new Texture2D(2, 2);
                if (!windowTexture.LoadImage(Convert.FromBase64String(windowBase64)))
                {
                    logger.Log("[TaiwuEditor]Loading Background Texture Failure");
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return false;
        }

        // UI实例创建时执行
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            this.helper = new Helper(logger);
        }

        // 第一次运行实例在Update()前执行一次
        private void Start()
        {
            CalculateWindowPos();
        }

        // 每帧执行一次
        private void Update()
        {
            if (Opened)
            {
                if (lastScreenWidth != Screen.width)
                {
                    AdjustWindow();
                }
                helper.UpdateAllFields(DateFile.instance, ActorMenu.instance, actorId);
            }

            if (Input.GetKeyUp(modSettings.hotKey))
            {
                ToggleWindow(!Opened);
            }
        }

        // 渲染GUI
        private void OnGUI()
        {
            if (!mInit)
            {
                mInit = true;
                PrepareGUI();
            }
            if (Opened)
            {
                var backgroundColor = GUI.backgroundColor;
                var color = GUI.color;
                GUI.backgroundColor = Color.white;
                GUI.color = Color.white;
                mWindowRect = GUILayout.Window(10086, mWindowRect, WindowFunction, "", windowStyle, GUILayout.Height(Screen.height - 200));
                GUI.backgroundColor = backgroundColor;
                GUI.color = color;
            }
        }

        // 该实例被析构的时候
        private void OnDestroy()
        {
            Destroy(mCanvas);
        }

        private void CalculateWindowPos()
        {
            lastScreenWidth = Screen.width;
            float ratio = Math.Max(Screen.width * 0.000625f, 1f);
            mWindowMinWidth = 960f * ratio;
            mWindowRect.x = ((float) Screen.width - mWindowMinWidth) * 0.5f;
            mWindowRect.y = 100f;
            evilButtonWidth = 120f * ratio;
            healthButtonWidth = 150f * ratio;
            idFieldWidth = 100f * ratio;
            closeBtnWidth = 150f * ratio;
            helper.fieldHelperBtnWidth = 75f * ratio;
            helper.fieldHelperLblWidth = 90f * ratio;
            helper.fieldHelperTextWidth = 125f * ratio;
            normalFontSize = (int) (16 * ratio);
        }

        private void AdjustWindow()
        {
            lastScreenWidth = Screen.width;
            float ratio = Math.Max(Screen.width * 0.000625f, 1f);
            mWindowMinWidth = 960f * ratio;
            mWindowRect.x = ((float) Screen.width - mWindowMinWidth) * 0.5f;
            mWindowRect.width = 0;
            mWindowRect.height = 0;
            evilButtonWidth = 120f * ratio;
            healthButtonWidth = 150f * ratio;
            idFieldWidth = 100f * ratio;
            closeBtnWidth = 150f * ratio;
            helper.fieldHelperBtnWidth = 75f * ratio;
            helper.fieldHelperLblWidth = 90f * ratio;
            helper.fieldHelperTextWidth = 120f * ratio;
            normalFontSize = (int) (16 * ratio);
            titleStyle.fontSize = normalFontSize;
            propertiesUIButtonStyle.fontSize = normalFontSize;
            labelStyle.fontSize = normalFontSize;
            nameFieldStyle.fontSize = normalFontSize + 4;
            buttonStyle.fontSize = normalFontSize;
            toggleStyle.fontSize = normalFontSize;
            boxStyle.fontSize = normalFontSize;
            textFieldStyle.fontSize = normalFontSize;
            commentStyle.fontSize = normalFontSize;
        }

        private void PrepareGUI()
        {
            windowStyle = new GUIStyle
            {
                name = "te window",
                padding = RectOffset(5)
            };
            windowStyle.normal.background = windowTexture;
            windowStyle.normal.background.wrapMode = TextureWrapMode.Repeat;

            titleStyle = new GUIStyle
            {
                name = "te h1",
                fontSize = normalFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = RectOffset(0, 5)
            };
            titleStyle.normal.textColor = Color.white;

            propertiesUIButtonStyle = new GUIStyle(GUI.skin.button)
            {
                name = "te h2",
                fontSize = normalFontSize,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                name = "te normallabel",
                fontSize = normalFontSize,
            };

            // 人物姓名样式
            nameFieldStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                stretchHeight = true,
                fontSize = normalFontSize + 4
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                name = "te button",
                fontSize = normalFontSize
            };

            toggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                name = "te toggle",
                fontSize = normalFontSize
            };

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                name = "te box",
                fontSize = normalFontSize
            };

            textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                name = "te textfield",
                fontSize = normalFontSize
            };

            commentStyle = new GUIStyle(GUI.skin.label)
            {
                name = "te normallabel",
                fontSize = normalFontSize - 2,
                fontStyle = FontStyle.Bold
            };

            helper.ButtonStyle = buttonStyle;
            helper.TextFieldStyle = textFieldStyle;
            helper.LabelStyle = labelStyle;
        }

        private void WindowFunction(int windowId)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                GUI.DragWindow(mWindowRect);
            }
            // 设置标题
            GUILayout.Label("太吾修改器 V1.0.10", titleStyle, GUILayout.Width(mWindowMinWidth));
            GUILayout.Space(3f);
            SetFuncMenu();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<color=#8FBAE7>[CTRL + F10]</color> 打开 UnityModManager 修改当前快捷键：<color=#F28234>{modSettings.hotKey.ToString()}</color>" +
                $"          <color=#8FBAE7>[CTRL + 鼠标左键]</color>   拖动窗口", commentStyle);
            GUILayout.Space(5f);
            if (GUILayout.Button("关闭", buttonStyle, GUILayout.Width(closeBtnWidth)))
            {
                ToggleWindow(false);
            }
        }

        // 设置功能选项
        private void SetFuncMenu()
        {
            GUILayout.Label("<color=#87CEEB>功能选择：</color>", labelStyle, GUILayout.MinWidth(mWindowMinWidth));
            var funcChooseTmp = GUILayout.SelectionGrid(funcChoose, funcTab, funcTab.Length, buttonStyle, GUILayout.Width(mWindowMinWidth));
            if (funcChoose != funcChooseTmp && funcChoose == 1)
            {
                helper.ResetAllFields();
            }
            funcChoose = funcChooseTmp;
            GUILayout.Space(5f);
            if (scrollPosition.Length != funcTab.Length)
            {
                scrollPosition = new Vector2[funcTab.Length];
            }
            // 滚动条
            scrollPosition[funcChoose] = GUILayout.BeginScrollView(scrollPosition[funcChoose], GUILayout.MinWidth(mWindowMinWidth));
            switch (funcChoose)
            {
                default:
                    BasicFuncUI();
                    break;
                case 1:
                    BasicPropertiesUI();
                    break;
            }
            GUILayout.EndScrollView();
        }

        // 设置基本功能
        private void BasicFuncUI()
        {
            GUILayout.BeginVertical("Box");
            modSettings.lockTime = GUILayout.Toggle(modSettings.lockTime, "锁定一月行动不减", toggleStyle);
            modSettings.lockFastRead = GUILayout.Toggle(modSettings.lockFastRead, "快速读书（对残缺篇章有效）", toggleStyle);
            if (modSettings.lockFastRead)
            {
                GUILayout.Label($"每次阅读<color=#F28234>{modSettings.pagesPerFastRead}</color>篇(只对功法类书籍有效，技艺类书籍会全部读完)", labelStyle);
                modSettings.pagesPerFastRead = (int) GUILayout.HorizontalScrollbar(modSettings.pagesPerFastRead, 1, 11, 1);
            }
            modSettings.lockMaxOutProficiency = GUILayout.Toggle(modSettings.lockMaxOutProficiency, "修习单击全满", toggleStyle);
            modSettings.lockFastQiyuCompletion = GUILayout.Toggle(modSettings.lockFastQiyuCompletion, "奇遇直接到达目的地", toggleStyle);
            if (modSettings.lockFastQiyuCompletion && storyTyps != null && storyTyps.Length == modSettings.includedStoryTyps.Length)
            {
                GUILayout.BeginHorizontal("Box");
                if(GUILayout.Button("<color=#F28234>全选</color>", buttonStyle))
                {
                    for(int i = 0; i< storyTyps.Length; i++)
                    {
                        modSettings.includedStoryTyps[i] = true;
                    }                    
                }
                for (int i = 0; i < storyTyps.Length; i++)
                {
                    modSettings.includedStoryTyps[i] = GUILayout.Toggle(modSettings.includedStoryTyps[i], storyTyps[i].Name, toggleStyle);
                }
                GUILayout.EndHorizontal();
            }
            modSettings.lockNeverOverweigh = GUILayout.Toggle(modSettings.lockNeverOverweigh, "身上物品永不超重（仓库无效）", toggleStyle);
            modSettings.lockMaxOutRelation = GUILayout.Toggle(modSettings.lockMaxOutRelation, "见面关系全满", toggleStyle);
            modSettings.lockMaxLifeFace = GUILayout.Toggle(modSettings.lockMaxLifeFace, "见面印象最深(换衣服会重置印象)", toggleStyle);
            modSettings.lockZeroWariness = GUILayout.Toggle(modSettings.lockZeroWariness, "锁定戒心为零", toggleStyle);
            modSettings.lockGangMaxPartValue = GUILayout.Toggle(modSettings.lockGangMaxPartValue, "锁定门派支持度", toggleStyle);
            if (modSettings.lockGangMaxPartValue)
            {
                GUILayout.Label("自定义最大门派支持度（范围0-100)\n<color=#F28234>设置为0则根据剑冢世界进度自动设定最大门派支持度(推荐)</color>", labelStyle);
                GUILayout.BeginHorizontal();                
                tmpCustomGangMaxPartValue = GUILayout.TextField(tmpCustomGangMaxPartValue, textFieldStyle, GUILayout.Width(helper.fieldHelperTextWidth));
                if (GUILayout.Button("确定", buttonStyle, GUILayout.Width(helper.fieldHelperBtnWidth)))
                {
                    if(Helper.TryParseInt(tmpCustomGangMaxPartValue, out int value))
                    {
                        modSettings.customGangMaxPartValue = value;
                    }
                    else
                    {
                        tmpCustomGangMaxPartValue = modSettings.customGangMaxPartValue.ToString();
                    }
                }
                GUILayout.EndHorizontal();
            }
            modSettings.lockBaseMaxPartValue = GUILayout.Toggle(modSettings.lockBaseMaxPartValue, "锁定地区恩义", toggleStyle);
            if (modSettings.lockBaseMaxPartValue)
            {
                GUILayout.Label("自定义最大地区恩义（范围0-100)\n<color=#F28234>设置为0则根据剑冢世界进度自动设定最大地区恩义(推荐)</color>", labelStyle);
                GUILayout.BeginHorizontal();
                tmpCustomBaseMaxPartValue = GUILayout.TextField(tmpCustomBaseMaxPartValue, textFieldStyle, GUILayout.Width(helper.fieldHelperTextWidth));
                if (GUILayout.Button("确定", buttonStyle, GUILayout.Width(helper.fieldHelperBtnWidth)))
                {
                    if (Helper.TryParseInt(tmpCustomBaseMaxPartValue, out int value))
                    {
                        modSettings.customBaseMaxPartValue = value;
                    }
                    else
                    {
                        tmpCustomBaseMaxPartValue = modSettings.customBaseMaxPartValue.ToString();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 设置人物属性修改功能
        /// </summary>
        private void BasicPropertiesUI()
        {
            DateFile instance = DateFile.instance;
            if (instance == null || instance.mianActorId == 0)
            {
                GUILayout.Box("未载入存档", boxStyle);
                return;
            }
            GUILayout.Label("<color=#87CEEB>修改人物：</color>", labelStyle);
            // 选择属性修改的主体
            var basePropertyChooseTmp = GUILayout.SelectionGrid(basePropertyChoose, new string[]
            {
                "<color=#FFA500>太吾本人</color>",
                "<color=#FFA500>最近打开过人物菜单的人物</color>",
                "<color=#FFA500>自定义人物</color>"
            }, 3, buttonStyle);
            if (basePropertyChoose != basePropertyChooseTmp)
            {
                helper.ResetAllFields();
                if (basePropertyChooseTmp == 2)
                {
                    displayedActorId = actorId.ToString();
                }
            }
            basePropertyChoose = basePropertyChooseTmp;
            GUILayout.Label("<color=#87CEEB>修改完成后数值不发生变化是游戏界面没有刷新的原因，并不是修改不成功。" +
                "属性资质修改需重新进入人物菜单才会刷新结果而资源和威望修改需发生对应变化的行为后才会更新。" +
                "所有资质属性均为基础值，不含特性、装备、早熟晚熟以及年龄加成</color>", labelStyle);
            // 显示待修改人物的ID
            DisplayId();
            // 显示待修改人物的姓名
            GUILayout.Box($"<color=#FFA500>{instance.GetActorName(actorId)}</color>", nameFieldStyle);
            labelStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("<color=#F28234>点击按钮修改对应类型属性</color>", labelStyle);
            labelStyle.fontStyle = FontStyle.Normal;
            ModData(instance);
        }

        /// <summary>
        /// 显示待修改人物的ID
        /// </summary>
        private void DisplayId()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box("<color=#87CEEB>人物ID:</color>", boxStyle, GUILayout.Width(idFieldWidth));
            switch (basePropertyChoose)
            {
                // 修改上次打开人物菜单的人物
                case 1:
                    if (ActorMenu.instance != null && ActorMenu.instance.acotrId > 0)
                    {
                        actorId = ActorMenu.instance.acotrId;
                        GUILayout.Box($"{actorId}", boxStyle, GUILayout.Width(idFieldWidth));
                    }
                    break;
                case 2:
                    if (actorId > 0)
                    {
                        displayedActorId = GUILayout.TextField(displayedActorId, textFieldStyle, GUILayout.Width(idFieldWidth));
                        if (GUILayout.Button("确定", buttonStyle, GUILayout.Width(idFieldWidth)))
                        {
                            if (Helper.TryParseInt(displayedActorId, out int parsedValue) && parsedValue >= 0)
                            {
                                actorId = parsedValue;
                            }
                            else
                            {
                                displayedActorId = actorId.ToString();
                            }
                        }
                    }
                    break;
                default:
                    if (DateFile.instance.mianActorId >= 0)
                    {
                        actorId = DateFile.instance.mianActorId;
                        GUILayout.Box($"{actorId}", boxStyle, GUILayout.Width(idFieldWidth));
                    }
                    break;
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 修改属性和资源
        /// </summary>
        /// <param name="instance">DateFile类的实例</param>
        private void ModData(DateFile instance)
        {
            GUILayout.BeginVertical("box");
            for (int k = 0; k < dataTabNames.Length; k++)
            {
                if (GUILayout.Button(dataTabNames[k], propertiesUIButtonStyle, GUILayout.ExpandWidth(true)))
                {
                    helper.ResetAllFields();
                    showTabDetails = (showTabDetails == k) ? (-1) : k;
                }

                if (showTabDetails == k)
                {
                    switch (k)
                    {
                        case 0:
                            // 基本属性 resid 61-66
                            DisplayDataFields(61, 67);
                            break;
                        case 1:
                            // 资源 resid 401-407
                            DisplayDataFields(401, 408);
                            break;
                        case 2:
                            // 技艺资质 resid 501-516
                            DisplayDataFields(501, 517);
                            break;
                        case 3:
                            // 功法资质 resid 601-614
                            DisplayDataFields(601, 615);
                            break;
                        case 4:
                            if (actorId == instance.mianActorId)
                            {
                                // 历练 此处resid无实际意义，在update()换算成对应的字段
                                helper.FieldHelper(-1);
                                // 无属性内力 id 44
                                helper.FieldHelper(706);
                                GUILayout.Label("每10点无属性内力增加1点真气", labelStyle);
                            }
                            break;
                        case 5:
                            DisplayHealthAge();
                            break;
                        case 6:
                            DisplayXXField(instance);
                            break;
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private void DisplayDataFields(int residBegin, int residEnd)
        {
            if (residBegin >= residEnd)
            {
                return;
            }
            GUILayout.BeginHorizontal();
            int total = residEnd - residBegin;
            // 除以3的优化(要求count不能超过100)，详见https://www.codeproject.com/KB/cs/FindMulShift.aspx
            int numRow = total * 43 >> 7;
            int rest = total - numRow * 3;
            uint count = 0;
            for (int resid = residBegin; resid < residEnd; resid++)
            {
                count++;
                if (count == 1)
                {
                    GUILayout.BeginVertical();
                }
                helper.FieldHelper(resid);
                if (count == numRow + ((rest > 0) ? 1 : 0))
                {
                    GUILayout.EndVertical();
                    rest--;
                    count = 0;
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示修改健康、寿命选项卡
        /// </summary>
        private void DisplayHealthAge()
        {
            GUILayout.Label("<color=#F28234>注意：\n1.基础寿命为不含人物特性加成的寿命\n2. 人物健康修改为0后，过月就会死亡</color>", labelStyle);
            GUILayout.BeginHorizontal();
            helper.FieldHelper(11);
            helper.FieldHelper(13);
            if (ActorMenu.instance != null)
            {
                helper.FieldHelper(12, ActorMenu.instance.MaxHealth(actorId));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();            
            if (GUILayout.Button("一键疗伤", buttonStyle, GUILayout.Width(healthButtonWidth)))
            {
                Helper.CureHelper(DateFile.instance, actorId, 0);
            }
            if (GUILayout.Button("一键祛毒", buttonStyle, GUILayout.Width(healthButtonWidth)))
            {
                Helper.CureHelper(DateFile.instance, actorId, 1);
            }
            if (GUILayout.Button("一键调理内息", buttonStyle, GUILayout.Width(healthButtonWidth)))
            {
                Helper.CureHelper(DateFile.instance, actorId, 2);
            }
            if (GUILayout.Button("我全部都要", buttonStyle, GUILayout.Width(healthButtonWidth)))
            {
                Helper.CureHelper(DateFile.instance, actorId, 0);
                Helper.CureHelper(DateFile.instance, actorId, 1);
                Helper.CureHelper(DateFile.instance, actorId, 2);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 修改相枢入邪值
        /// </summary>
        /// <param name="instance">DateFile类的实例</param>
        private void DisplayXXField(DateFile instance)
        {
            int evilValue = Helper.LifeDateHelper(instance, 501, actorId);
            GUILayout.BeginHorizontal();
            GUILayout.Box("当前入邪值: ", boxStyle, GUILayout.Width(evilButtonWidth));
            if (evilValue == -1)
            {
                GUILayout.Box("无", boxStyle, GUILayout.Width(evilButtonWidth));
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Box($"{evilValue}", boxStyle, GUILayout.Width(evilButtonWidth));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("恢复正常", buttonStyle, GUILayout.Width(evilButtonWidth)))
                {
                    Helper.SetActorXXValue(instance, actorId, 0);
                }
                if (GUILayout.Button("相枢入邪", buttonStyle, GUILayout.Width(evilButtonWidth)))
                {
                    Helper.SetActorXXValue(instance, actorId, 100);
                }
                if (actorId != 0 && actorId != instance.mianActorId)
                {
                    if (GUILayout.Button("相枢化魔", buttonStyle, GUILayout.Width(evilButtonWidth)))
                    {
                        Helper.SetActorXXValue(instance, actorId, 200);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void ToggleWindow(bool toOpen)
        {
            if (!Main.enabled && toOpen)
            {
                return;
            }
            Opened = toOpen;
            BlockGameUI(toOpen);
            if (!toOpen)
            {
                UpdateTmpValue();
                helper.ResetAllFields();
            }
        }

        private void BlockGameUI(bool value)
        {
            if (value)
            {
                // 屏蔽游戏界面
                mCanvas = new GameObject("TEGameUIBlocker", typeof(Canvas), typeof(GraphicRaycaster));
                mCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                mCanvas.GetComponent<Canvas>().sortingOrder = Int16.MaxValue;
                DontDestroyOnLoad(mCanvas);
                var panel = new GameObject("TEGameUIBlockerPanel", typeof(Image));
                panel.transform.SetParent(mCanvas.transform);
                panel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                panel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            }
            else
            {
                Destroy(mCanvas);
            }
        }

        private void UpdateTmpValue()
        {
            tmpCustomGangMaxPartValue = modSettings.customGangMaxPartValue.ToString();
            tmpCustomBaseMaxPartValue = modSettings.customBaseMaxPartValue.ToString();
        }

        private static RectOffset RectOffset(int value)
        {
            return new RectOffset(value, value, value, value);
        }

        private static RectOffset RectOffset(int x, int y)
        {
            return new RectOffset(x, x, y, y);
        }
    }
}
