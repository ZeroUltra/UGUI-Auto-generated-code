using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System.IO;

namespace AutoGenerateCode
{



    class UITreeViewWindow : EditorWindow
    {
        // We are using SerializeField here to make sure view state is written to the window 
        // layout file. This means that the state survives restarting Unity as long as the window
        // is not closed. If omitting the attribute then the state just survives assembly reloading 
        // (i.e. it still gets serialized/deserialized)
        [SerializeField] UITreeViewState m_TreeViewState;

        UITreeview uiTreeView;


        SearchField m_SearchField;

        //变量
        private bool variableAll = true;
        private const string VARIABLEALL = "VARIABLEALL";
        //属性
        private bool propertyAll = true;
        private const string PROPERTYALL = "PROPERTYALL";
        //事件
        private bool eventAll = true;
        private const string EVENTALL = "EVENTALL";

        private const string ISEXPANDALL = "ISEXPANDALL";
        private bool isExpandAll = false;


        private string codeStr = "Scrips";
        private string codeSavePath;

        //提示信息
        private string tipStates;
        private string tipSelete;
        private MessageType msgSeleteType = MessageType.Info;

        #region 布局参数
        private float treeviewWidth = 500f;
        private Vector2 v2_GuiBtnSize = new Vector2(80, 20);
        #endregion

        [MenuItem("Tools/UI Window")]
        static void ShowWindow()
        {
            var window = GetWindow<UITreeViewWindow>();
            window.titleContent = new GUIContent("UI Window");
            window.position = new Rect(400, 250, 1100, 720);
            window.Show();
        }

        void OnEnable()
        {
            variableAll = EditorPrefs.GetBool(VARIABLEALL, true);
            propertyAll = EditorPrefs.GetBool(PROPERTYALL, true);
            eventAll = EditorPrefs.GetBool(EVENTALL, true);

            isExpandAll = EditorPrefs.GetBool(ISEXPANDALL, false);

            if (m_TreeViewState == null)
                m_TreeViewState = new UITreeViewState();

            //每次打开都会tree形态重新绘制 痛点 要把数据保存
            uiTreeView = new UITreeview(m_TreeViewState);

            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += uiTreeView.SetFocusAndEnsureSelectedItem;
        }

        #region OnGUI
        void OnGUI()
        {
            //搜索框
            DrawSearchbar();
            //UI Tree view
            DrawTreeAndScriptUI();

           //line 1
            using (new GUILayout.HorizontalScope())
            {
                if (Selection.activeGameObject == null)
                {
                    tipSelete = "没有选择UI物体";
                    if (msgSeleteType != MessageType.Error)
                        msgSeleteType = MessageType.Error;
                }
                else
                {
                    tipSelete = "当前选择了--> " + Selection.activeGameObject.name;
                    if (msgSeleteType != MessageType.Info)
                        msgSeleteType = MessageType.Info;
                }
                GUILayout.Space(3);
                using (new GUILayout.VerticalScope(GUILayout.Width(treeviewWidth), GUILayout.Height(10)))
                {
                    EditorGUILayout.HelpBox("(双击Tree view中的对象可切换选择)   选择提示:" + tipSelete, msgSeleteType);
                }
                GUILayout.Space(20);
                using (new GUILayout.VerticalScope(GUILayout.Width(575), GUILayout.Height(10)))
                {
                    EditorGUILayout.HelpBox("代码提示:" + tipStates, MessageType.Info);
                }
            }

            //line 2
            GUILayout.Space(-3);
            using (new GUILayout.HorizontalScope())
            {
                #region 变量 事件...
                using (new GUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Width(500)))
                {
                    GUILayout.Space(8);
                    EditorGUILayout.LabelField("变量:", EditorStyles.whiteLabel, GUILayout.Width(30));
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        variableAll = GUILayout.Toggle(variableAll, "全选/全不选", GUILayout.Width(80), GUILayout.Height(v2_GuiBtnSize.y));
                        if (check.changed)
                        {
                            EditorPrefs.SetBool(VARIABLEALL, variableAll);
                            uiTreeView.SetVariableState(variableAll);
                        }
                    }

                    GUILayout.Space(8);
                    GUI.color = Color.cyan;
                    EditorGUILayout.LabelField("属性:", EditorStyles.whiteLabel, GUILayout.Width(30));
                    GUI.color = Color.white;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        propertyAll = GUILayout.Toggle(propertyAll, "全选/全不选", GUILayout.Width(80), GUILayout.Height(v2_GuiBtnSize.y));
                        if (check.changed)
                        {
                            EditorPrefs.SetBool(PROPERTYALL, propertyAll);
                            uiTreeView.SetPropertyState(propertyAll);
                        }
                    }

                    GUILayout.Space(8);
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField("事件:", EditorStyles.whiteLabel, GUILayout.Width(30));
                    GUI.color = Color.white;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        eventAll = GUILayout.Toggle(eventAll, "全选/全不选", GUILayout.Width(80), GUILayout.Height(v2_GuiBtnSize.y));
                        if (check.changed)
                        {
                            EditorPrefs.SetBool(EVENTALL, eventAll);
                            uiTreeView.SetEventState(eventAll);
                        }
                    }

                    GUILayout.Space(8);
                    uiTreeView.linkPartentChild = GUILayout.Toggle(uiTreeView.linkPartentChild, "是否子父关联", GUILayout.Width(100), GUILayout.Height(30));
                } 
                #endregion

                #region 代码预览按钮
                using (new GUILayout.HorizontalScope(GUILayout.Width(500)))
                {

                    GUILayout.Space(25);
                    if (GUILayout.Button("生成代码", GUILayout.Width(170), GUILayout.Height(35)))
                    {
                        codeSavePath = string.Empty;
                        codeStr = ScriptGenerator.StartScriptGenerate(uiTreeView);
                    }

                    GUILayout.Space(15);
                    if (GUILayout.Button("保存", GUILayout.Width(170), GUILayout.Height(35)))
                    {
                        if (codeStr.Length < 10)
                        {
                            EditorUtility.DisplayDialog("提示", "没有生成代码,请先生成再保存", "OK");
                            return;
                        }
                        codeSavePath = EditorUtility.SaveFilePanelInProject("选择保存文件夹", Selection.activeGameObject.name, "cs", "Please enter a file name to save ");
                        if (!string.IsNullOrEmpty(codeSavePath))
                        {
                            tipStates = ScriptGenerator.SaveScript(codeStr, codeSavePath);
                            ShowNotification(new GUIContent("脚本生成成功:" + codeSavePath));
                            Helper.AwaitToDo(700, RemoveNotification);
                        }
                        else
                        {
                            Debug.Log(Helper.Log_UIGenerate + "脚本生成取消");
                        }

                    }

                    GUILayout.Space(15);
                    if (GUILayout.Button("绑定到GameObject", GUILayout.Width(170), GUILayout.Height(35)))
                    {
                        if (string.IsNullOrEmpty(codeSavePath))
                        {
                            EditorUtility.DisplayDialog("提示", "没有保存代码文件", "Ok");
                            return;
                        }
                        System.Type scriptType = Helper.GetAssembly().GetType(Path.GetFileNameWithoutExtension(codeSavePath));
                        if (scriptType == null)
                        {
                            EditorUtility.DisplayDialog("提示", "没有找到代码文件", "Ok");
                            return;
                        }
                        var target = Selection.activeGameObject.GetComponent(scriptType);
                        if (target == null)
                        {
                            Selection.activeGameObject.AddComponent(scriptType);
                            Debug.Log(Helper.Log_UIGenerate + "脚本绑定成功");
                        }
                    }

                }
                #endregion
            }

            //line 3
            GUILayout.Space(-8);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(10);
                if (GUILayout.Button("加载选择对象", GUILayout.Width(100), GUILayout.Height(28)))
                {
                    uiTreeView.Reload();
                }

                GUILayout.Space(10);
                string foldStr = isExpandAll ? "全折叠" : "全展开";
                Color foldColor = isExpandAll ? Color.cyan : Color.yellow;
                GUI.color = foldColor;
                if (GUILayout.Button(foldStr, GUILayout.Width(80), GUILayout.Height(28)))
                {
                    if (isExpandAll)
                        uiTreeView.CollapseAll();
                    else
                        uiTreeView.ExpandAll();
                    isExpandAll = !isExpandAll;
                    EditorPrefs.SetBool(ISEXPANDALL, isExpandAll);
                }
                GUI.color = Color.white;


            }
        }


        /// <summary>
        /// 绘制搜索框
        /// </summary>
        void DrawSearchbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            // GUILayout.Space(300);
            GUILayout.FlexibleSpace();
            uiTreeView.searchString = m_SearchField.OnToolbarGUI(uiTreeView.searchString);
            GUILayout.EndHorizontal();
        }

        Vector2 lastScroll = Vector2.zero;
        /// <summary>
        /// 树状图和代码UI
        /// </summary>
        void DrawTreeAndScriptUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("UI Tree View", EditorStyles.whiteLargeLabel, GUILayout.Width(treeviewWidth + 15));
                GUILayout.Label("Code Preview", EditorStyles.whiteLargeLabel);
            }
            //横
            using (new GUILayout.HorizontalScope())
            {
                //---------tree view-----
                Rect rect = new Rect(10, 50, treeviewWidth, 520);
                GUILayout.Box("", GUILayout.Width(rect.width), GUILayout.Height(rect.height));
                uiTreeView.OnGUI(new Rect(rect.x, rect.y, rect.width - 10, rect.height - 5));

                //------code preivew------
                GUILayout.Space(15);
                //滑动条
                using (var scrollview = new GUILayout.ScrollViewScope(lastScroll, EditorStyles.helpBox, GUILayout.Height(rect.height)))
                {
                    lastScroll = scrollview.scrollPosition;
                    GUILayout.Label(codeStr);
                }
            }
        }

        #endregion
    }
}
