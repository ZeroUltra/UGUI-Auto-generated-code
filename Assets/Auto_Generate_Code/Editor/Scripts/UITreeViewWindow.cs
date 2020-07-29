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
        [SerializeField] TreeViewState m_TreeViewState;

        public static Dictionary<int, GameObject> dictIDs = new Dictionary<int, GameObject>();


        UITreeView uiTreeView;
        SearchField m_SearchField;
       

        private Vector2 v2_GuiBtnSize = new Vector2(70, 20);

        private bool seleteAll = true;
        private bool tempSeleteAll = true;
        private bool addEvents = true;
        private bool tempAddEvents = true;
        private const string SELETEALL = "SELETEALL";
        private const string ADDEVENTS = "ADDEVENTS";

        private string codeStr = "Scrips";
        private string savePath;

        //提示信息
        private string tipStates;
        private string tipSelete;
        private MessageType msgSeleteType = MessageType.Info;

        [MenuItem("Tools/UI Window")]
        static void ShowWindow()
        {
            var window = GetWindow<UITreeViewWindow>();
            window.titleContent = new GUIContent("UI Window");
            window.position = new Rect(400, 250, 900, 720);
            window.Show();
            Helper.AddCanvasGoToDict(FindObjectsOfType<Canvas>());
        }
     
        void OnEnable()
        {
            seleteAll = EditorPrefs.GetBool(SELETEALL, true);
            addEvents = EditorPrefs.GetBool(ADDEVENTS, true);

            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();
            uiTreeView = new UITreeView(m_TreeViewState);
            //uiTreeView.OnDoubleClick += UiTreeView_OnDoubleClick;
            uiTreeView.ExpandAll();
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += uiTreeView.SetFocusAndEnsureSelectedItem;
        }

        //当双击
        //private void UiTreeView_OnDoubleClick(int treeID)
        //{
        //    uiTreeView.Reload();
        //}

        private void OnFocus()
        {
            Helper.AddCanvasGoToDict(FindObjectsOfType<Canvas>());
        }

        #region OnGUI
        void OnGUI()
        {
            #region 选择提示
            if (Selection.activeGameObject == null)
            {
                tipSelete = "没有选择UI物体";
                if (msgSeleteType != MessageType.Error)
                    msgSeleteType = MessageType.Error;
            }
            else
            {
                tipSelete = "当前选择了:" + Selection.activeGameObject.name;
                if (msgSeleteType != MessageType.Info)
                    msgSeleteType = MessageType.Info;
            }
            #endregion

            //搜索框
            DrawSearchbar();
            //UI Tree view
            DrawTreeAndScriptUI();

            GUILayout.Space(8);
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("选择此对象", GUILayout.Width(v2_GuiBtnSize.x), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    uiTreeView.Reload();
                }
                GUILayout.Space(5);
                if (GUILayout.Button("全折叠", GUILayout.Width(50), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    uiTreeView.CollapseAll();
                    uiTreeView.SetExpanded(uiTreeView.GetRootFirstTreeView().id, true);
                }
                GUILayout.Space(5);
                if (GUILayout.Button("全展开", GUILayout.Width(50), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    uiTreeView.ExpandAll();
                }
                GUILayout.Space(5);
                seleteAll = GUILayout.Toggle(seleteAll, "全选/全不选", GUILayout.Width(80), GUILayout.Height(v2_GuiBtnSize.y));
                if (tempSeleteAll != seleteAll)
                {
                    EditorPrefs.SetBool(SELETEALL, seleteAll);
                    tempSeleteAll = seleteAll;

                    uiTreeView.SeleteAll(seleteAll);
                }
                GUILayout.Space(5);
                uiTreeView.linkPartentChild = GUILayout.Toggle(uiTreeView.linkPartentChild, "是否子父关联", GUILayout.Width(100), GUILayout.Height(v2_GuiBtnSize.y));

                //-----------------代码预览---------------------------------
                GUILayout.Space(30);
                if (GUILayout.Button("生成代码", GUILayout.Width(v2_GuiBtnSize.x), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    savePath = string.Empty;
                    codeStr = ScriptGenerator.StartScriptGenerate(uiTreeView, addEvents);
                }
                GUILayout.Space(10);
                addEvents = GUILayout.Toggle(addEvents, "添加UI事件", GUILayout.Width(80), GUILayout.Height(v2_GuiBtnSize.y));
                if (addEvents != tempAddEvents)
                {
                    EditorPrefs.SetBool(ADDEVENTS, addEvents);
                }
                #region 绑定到UI GameObject
                if (GUILayout.Button("绑定到GameObject", GUILayout.Width(120), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    if (string.IsNullOrEmpty(savePath))
                    {
                        EditorUtility.DisplayDialog("提示", "没有保存代码文件", "Ok");
                        return;
                    }
                    System.Type scriptType = Helper.GetAssembly().GetType(Path.GetFileNameWithoutExtension(savePath));
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
                #endregion
            }
            #region 保存提示
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(3);
                using (new GUILayout.VerticalScope(GUILayout.Width(380), GUILayout.Height(50)))
                {
                    EditorGUILayout.HelpBox("双击对象可重新选择,且可以在面板中查看属性,当脚本加载会重新绘制 UI Tree View", MessageType.None);
                    EditorGUILayout.HelpBox("选择提示:" + tipSelete, msgSeleteType);
                }
            }
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                GUI.color = Color.cyan;
                if (GUILayout.Button("保存", GUILayout.Width(300), GUILayout.Height(40)))
                {
                    savePath = EditorUtility.SaveFilePanelInProject("选择保存文件夹", Selection.activeGameObject.name, "cs", "Please enter a file name to save the texture to");
                    if (!string.IsNullOrEmpty(savePath))
                    {
                        tipStates = ScriptGenerator.SaveScript(codeStr, savePath);
                        ShowNotification(new GUIContent("脚本生成成功:" + savePath));
                        Helper.AwaitToDo(800, RemoveNotification);
                    }
                    else
                    {
                        Debug.Log(Helper.Log_UIGenerate + "脚本生成失败");
                    }
                }
                GUI.color = Color.white;
                GUILayout.Space(40);
                EditorGUILayout.HelpBox("提示:" + tipStates, MessageType.Info);
            }
            #endregion
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
                GUILayout.Label("UI Tree View", EditorStyles.whiteLargeLabel, GUILayout.Width(410));
                GUILayout.Label("Code Preview", EditorStyles.whiteLargeLabel);
            }
            using (new GUILayout.HorizontalScope())
            {
                Rect rect = new Rect(10, 50, 400, 520);
                GUILayout.Box("", GUILayout.Width(rect.width), GUILayout.Height(rect.height));
                uiTreeView.OnGUI(new Rect(rect.x, rect.y, rect.width - 10, rect.height - 5));
                //------------
                GUILayout.Space(15);
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
