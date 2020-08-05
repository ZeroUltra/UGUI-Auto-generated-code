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

        UITreeview uiTreeView;
        SearchField m_SearchField;


        private bool seleteAll = true;
        private const string SELETEALL = "SELETEALL";

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
            window.position = new Rect(400, 250, 1200, 720);
            window.Show();

        }

        void OnEnable()
        {
            seleteAll = EditorPrefs.GetBool(SELETEALL, true);
            isExpandAll = EditorPrefs.GetBool(ISEXPANDALL, false);


            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            uiTreeView = new UITreeview(m_TreeViewState);

            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += uiTreeView.SetFocusAndEnsureSelectedItem;
        }

        //当双击
        //private void UiTreeView_OnDoubleClick(int treeID)
        //{
        //    uiTreeView.Reload();
        //}

        #region OnGUI
        void OnGUI()
        {

            //搜索框
            DrawSearchbar();
            //UI Tree view
            DrawTreeAndScriptUI();

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
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(3);
                using (new GUILayout.VerticalScope(GUILayout.Width(treeviewWidth), GUILayout.Height(10)))
                {
                    // EditorGUILayout.HelpBox("双击对象可重新选择,且可以在面板中查看属性,当脚本加载会重新绘制 UI Tree View", MessageType.None);
                    EditorGUILayout.HelpBox("(双击Tree view中的对象可切换选择)   选择提示:" + tipSelete, msgSeleteType);
                }
            }
            #endregion
            //GUILayout.Space();
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("加载选择对象", GUILayout.Width(v2_GuiBtnSize.x), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    uiTreeView.Reload();
                }

                GUILayout.Space(5);

                string foldStr = isExpandAll ? "全折叠" : "全展开";
                Color foldColor = isExpandAll ? Color.cyan : Color.yellow;
                GUI.color = foldColor;
                if (GUILayout.Button(foldStr, GUILayout.Width(50), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    if (isExpandAll)
                        uiTreeView.CollapseAll();
                    else
                        uiTreeView.ExpandAll();
                    isExpandAll = !isExpandAll;
                    EditorPrefs.SetBool(ISEXPANDALL, isExpandAll);
                }
                GUI.color = Color.white;


                GUILayout.Space(5);
                using (var check=new EditorGUI.ChangeCheckScope())
                {
                    seleteAll = GUILayout.Toggle(seleteAll, "全选/全不选", GUILayout.Width(80), GUILayout.Height(v2_GuiBtnSize.y));
                    if (check.changed)
                    {
                        EditorPrefs.SetBool(SELETEALL, seleteAll);
                        uiTreeView.SeleteAll(seleteAll);
                    }
                }
                GUILayout.Space(5);
                uiTreeView.linkPartentChild = GUILayout.Toggle(uiTreeView.linkPartentChild, "是否子父关联", GUILayout.Width(100), GUILayout.Height(v2_GuiBtnSize.y));

                //-----------------代码预览---------------------------------
                GUILayout.Space(30);
                if (GUILayout.Button("生成代码", GUILayout.Width(v2_GuiBtnSize.x), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    codeSavePath = string.Empty;
                    codeStr = ScriptGenerator.StartScriptGenerate(uiTreeView);
                }

                #region 绑定到UI GameObject
                GUILayout.Space(10);
                if (GUILayout.Button("绑定到GameObject", GUILayout.Width(120), GUILayout.Height(v2_GuiBtnSize.y)))
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
                #endregion
            }
            #region 保存提示
            GUILayout.Space(10);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                GUI.color = Color.cyan;
                if (GUILayout.Button("保存", GUILayout.Width(300), GUILayout.Height(40)))
                {
                    codeSavePath = EditorUtility.SaveFilePanelInProject("选择保存文件夹", Selection.activeGameObject.name, "cs", "Please enter a file name to save the texture to");
                    if (!string.IsNullOrEmpty(codeSavePath))
                    {
                        tipStates = ScriptGenerator.SaveScript(codeStr, codeSavePath);
                        ShowNotification(new GUIContent("脚本生成成功:" + codeSavePath));
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
