using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor.TreeViewExamples
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

        [MenuItem("Tools/UI Window")]
        static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<UITreeViewWindow>();
            window.titleContent = new GUIContent("UI Window");
            window.position = new Rect(400, 250, 900, 680);
            window.Show();
        }

        void OnEnable()
        {
            // Check if we already had a serialized view state (state 
            // that survived assembly reloading)
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            uiTreeView = new UITreeView(m_TreeViewState);
            
            uiTreeView.ExpandAll();
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += uiTreeView.SetFocusAndEnsureSelectedItem;
        }
        //private void OnDisable()
        //{
        //    foreach (var item in m_TreeView.dictTreeItemDatas)
        //    {
        //        Debug.Log($"{item.Key.displayName}->{item.Value}");
        //    }
        //}

        void OnGUI()
        {
            DrawSearchbar();

            GUILayout.BeginHorizontal();
            GUILayout.Space(200);
            if (GUILayout.Button("选择此对象", GUILayout.Width(70), GUILayout.Height(30)))
            {
              
                uiTreeView.Reload();
            }
            GUILayout.Space(100);
            if (GUILayout.Button("生成代码", GUILayout.Width(70), GUILayout.Height(30)))
            {

            }
            GUILayout.EndHorizontal();
            //--------------------
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Space(50);
            if (GUILayout.Button("按钮事件", GUILayout.Width(70), GUILayout.Height(30)))
            {
                uiTreeView.Reload();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("生成代码", GUILayout.Width(70), GUILayout.Height(30)))
            {

            }
            GUILayout.EndHorizontal();

            TreeAndScriptUI();
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
        void TreeAndScriptUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("UI Tree View", EditorStyles.whiteLargeLabel);

            using (new GUILayout.HorizontalScope())
            {
                Rect rect = new Rect(10, 130, 350, 520);
                GUILayout.Box("", GUILayout.Width(rect.width), GUILayout.Height(rect.height));
                uiTreeView.OnGUI(rect);
                //------------
                GUILayout.Space(15);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,  GUILayout.Height(rect.height));
                GUILayout.Label("Scripts");
                EditorGUILayout.EndVertical();
            }
        }
    }
}
