using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UI;
using System.Linq;

namespace UnityEditor.TreeViewExamples
{
    class UITreeView : TreeView
    {
        public event System.Action OnNullSelete; //当没有选择物体
        //当前选择的所有UI控件
        public Dictionary<TreeViewItem, bool> dictTreeItemDatas = new Dictionary<TreeViewItem, bool>();
        public Transform seleteTrans;


        //初始化
        public UITreeView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
            //  showingVerticalScrollBar = true;
            rowHeight = 18f; //行高
        }

        protected override TreeViewItem BuildRoot()
        {
            if (Selection.activeGameObject == null)
            {
                OnNullSelete?.Invoke();
                var treeRoot0 = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
                var root0 = new TreeViewItem { id = 1, depth = 0, displayName = "请选择一个对象" };
                SetupParentsAndChildrenFromDepths(treeRoot0, new List<TreeViewItem>() { root0 });
                return treeRoot0;
            }

            dictTreeItemDatas.Clear();
            //当前选中的
            seleteTrans = Selection.activeGameObject.transform;
            //生命一个根目录
            var treeRoot = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            List<TreeViewItem> allItems = new List<TreeViewItem>();

            int depth = 0;
            var root = new TreeViewItem { id = seleteTrans.GetInstanceID(), depth = depth, displayName = seleteTrans.name };
            allItems.Add(root);
            dictTreeItemDatas.Add(root, true);
            //添加所有子物体
            AddTransChildren(seleteTrans, depth, allItems);

            SetupParentsAndChildrenFromDepths(treeRoot, allItems);
            return treeRoot;
        }
        /// <summary>
        /// 当选中item时候
        /// </summary>
        /// <param name="selectedIds"></param>
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            //在Hierarchy 中显示选中的物体
            Selection.activeInstanceID = selectedIds[0];
        }
        /// <summary>
        /// 自定义UI
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            //if (Selection.activeGameObject == null) return;
            //Event evt = Event.current;

            ////图标和标签之前的间距的值
            extraSpaceBeforeIconAndLabel = 20;

            TreeViewItem item = args.item;

            //图标和checkbox 区域
            Rect toggleRect = args.rowRect;
            toggleRect.x += GetContentIndent(item);
            toggleRect.width = 16f;

            //Rect gizmoRect = toggleRect;
            //gizmoRect.x += 17;
            //gizmoRect.width = 18f;
            //Debug.Log(item.displayName);
            //GUIContent content = new GUIContent(EditorGUIUtility.ObjectContent(null, Helper.GetType(Helper.IDToGameObject(item.id))).image);
            //GUI.DrawTexture(gizmoRect, content.image);
            //if (evt.type == EventType.MouseDown && toggleRect.Contains(evt.mousePosition))
            //    SelectionClick(args.item, false);

            EditorGUI.BeginChangeCheck();
            bool isCheck = EditorGUI.Toggle(toggleRect, dictTreeItemDatas[item]);
            if (EditorGUI.EndChangeCheck())
            {
                dictTreeItemDatas[item] = isCheck;
                CheckChildren(item, isCheck);
            }
            base.RowGUI(args);
        }


        //递归查询
        private void CheckChildren(TreeViewItem item, bool isCheck)
        {
            if (item.hasChildren)
            {
                foreach (var itemChren in item.children)
                {
                    dictTreeItemDatas[itemChren] = isCheck;
                    CheckChildren(itemChren, isCheck);
                }
            }
        }
        /// <summary>
        /// 将子对象添加到TreeView
        /// </summary>
        /// <param name="rootTrans"></param>
        /// <param name="depth"></param>
        /// <param name="allItems"></param>
        private void AddTransChildren(Transform rootTrans, int depth, List<TreeViewItem> allItems)
        {
            if (rootTrans.childCount > 0)
            {
                depth++;
                foreach (Transform itemTrans in rootTrans)
                {

                    var item = new TreeViewItem { id = itemTrans.GetInstanceID(), depth = depth, displayName = itemTrans.name };
                    item.icon = EditorGUIUtility.ObjectContent(null, Helper.GetType(itemTrans.gameObject)).image as Texture2D;
                    dictTreeItemDatas.Add(item, true);
                    allItems.Add(item);
                    AddTransChildren(itemTrans, depth, allItems);
                }
            }
        }
    }
}

