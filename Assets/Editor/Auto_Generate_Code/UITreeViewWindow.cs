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

        // The TreeView is not serializable it should be reconstructed from the tree data.
        UITreeView uiTreeView;
        SearchField m_SearchField;
        public static Dictionary<int, GameObject> dictIDs = new Dictionary<int, GameObject>();

        private Vector2 v2_GuiBtnSize = new Vector2(80, 22);
        private bool seleteAll = true;
        private bool addEvents = true;
        private string codeStr = "Scrips";
        private string savePath;
        private string tip;

        [MenuItem("Tools/UI Window")]
        static void ShowWindow()
        {
            var window = GetWindow<UITreeViewWindow>();
            window.titleContent = new GUIContent("UI Window");
            window.position = new Rect(400, 250, 900, 680);
            window.Show();
            Helper.FindObjIDToDict(FindObjectsOfType<Canvas>());
        }

        void OnEnable()
        {

            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();
            uiTreeView = new UITreeView(m_TreeViewState);
            uiTreeView.OnDoubleClick += UiTreeView_OnDoubleClick;
            uiTreeView.ExpandAll();
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += uiTreeView.SetFocusAndEnsureSelectedItem;
        }

        //当双击
        private void UiTreeView_OnDoubleClick(TreeViewItem obj)
        {
            uiTreeView.Reload();
        }


        private void OnFocus()
        {
            Helper.FindObjIDToDict(FindObjectsOfType<Canvas>());
            uiTreeView.Reload();
        }

        #region OnGUI
        void OnGUI()
        {
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
                GUILayout.Space(10);

                //seleteAll = GUILayout.Toggle(seleteAll, "全选/全不选", GUILayout.Width(v2_GuiBtnSize.x), GUILayout.Height(v2_GuiBtnSize.y));
                //uiTreeView.SeleteAll(seleteAll);

                GUILayout.Space(10);
                uiTreeView.linkPartentChild = GUILayout.Toggle(uiTreeView.linkPartentChild, "是否子父关联", GUILayout.Width(100), GUILayout.Height(v2_GuiBtnSize.y));


                //-----------------代码预览---------------------------------
                GUILayout.Space(210);
                if (GUILayout.Button("生成代码", GUILayout.Width(v2_GuiBtnSize.x), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    //Debug.Log(uiTreeView.GetRootTreeView().children[0].displayName);
                    savePath = string.Empty;
                    codeStr = CreateNewScript.GenerateScript(uiTreeView);
                }
                GUILayout.Space(10);
                if (addEvents = GUILayout.Toggle(addEvents, "添加UI事件", GUILayout.Width(v2_GuiBtnSize.x), GUILayout.Height(v2_GuiBtnSize.y)))
                {

                }
                if (GUILayout.Button("绑定到GameObject", GUILayout.Width(120), GUILayout.Height(v2_GuiBtnSize.y)))
                {
                    if (string.IsNullOrEmpty(savePath))
                    {
                        EditorUtility.DisplayDialog("提示", "没有保存代码文件", "Ok");
                        return;
                    }
                    System.Type scriptType = Helper.GetAssembly().GetType(Path.GetFileNameWithoutExtension(savePath));
                    var target = Selection.activeGameObject.GetComponent(scriptType);
                    if (target == null)
                    {
                        Selection.activeGameObject.AddComponent(scriptType);
                        Debug.Log(Helper.Log_UIGenerate + "脚本绑定成功");
                    }
                }
            }
            GUILayout.Space(30);
            using (new GUILayout.HorizontalScope())
            {
               // GUILayout.FlexibleSpace();
                GUI.color = Color.cyan;
                if (GUILayout.Button("保存", GUILayout.Width(300)))
                {
                    savePath = EditorUtility.SaveFilePanelInProject("选择保存文件夹", Selection.activeGameObject.name, "cs", "Please enter a file name to save the texture to");
                    if (!string.IsNullOrEmpty(savePath))
                    {
                        //替换类名
                        codeStr = codeStr.Replace(CreateNewScript.scriptName, Path.GetFileNameWithoutExtension(savePath));
                        File.WriteAllText(savePath, codeStr);
                        AssetDatabase.Refresh();
                        tip = System.DateTime.Now.ToString()+" 位于:"+savePath;
                        Debug.Log(Helper.Log_UIGenerate + "脚本生成成功");
                    }
                    else
                    {
                        Debug.Log(Helper.Log_UIGenerate + "脚本生成失败");
                    }
                }
                GUI.color = Color.white;
                GUILayout.Space(50);
                EditorGUILayout.HelpBox("代码保存提示,保存时间:"+tip, MessageType.Info);
                // GUILayout.FlexibleSpace();
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
                uiTreeView.OnGUI(new Rect(rect.x, rect.y, rect.width - 10, rect.height));
                //------------
                GUILayout.Space(15);
                using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(rect.height)))
                {
                    GUILayout.Label(codeStr);
                }
            }
        }
        #endregion
    }
}
