using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;



namespace AutoGenerateCode
{
    public class UITreeview2 : TreeView
    {

        public event System.Action OnNullSelete; //当没有选择物体
        public event System.Action<int> OnDoubleClick;
        //当前选择的所有UI控件
        public Dictionary<TreeViewItem, bool> dictTreeItemDatas = new Dictionary<TreeViewItem, bool>();
        public Transform seleteTrans;

        /// <summary>
        /// 是否子父级关联
        /// </summary>
        public bool linkPartentChild = false;

        //初始化
        public UITreeview2(TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
            rowHeight = 18f; //行高
        }

        /// <summary>
        /// 绑定数据
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            if (Selection.activeGameObject == null)
            {
                OnNullSelete?.Invoke();
                UITreeViewItem treeRoot0 = new UITreeViewItem { id = 0, depth = -1, displayName = "Root"};

                 var root0 = new UITreeViewItem { id = 1, depth = 0, displayName = "请选择一个对象" };
                 SetupParentsAndChildrenFromDepths(treeRoot0, new List<TreeViewItem>() { root0 });
                return treeRoot0;
            }
            dictTreeItemDatas.Clear();
            //当前选中的
            seleteTrans = Selection.activeGameObject.transform;
            //生命一个根目录
            var treeRoot = new UITreeViewItem { id = 0, depth = -1, displayName = "Root" };
            List<TreeViewItem> allItems = new List<TreeViewItem>();

            int depth = 0;
            var root = new UITreeViewItem { id = seleteTrans.GetInstanceID(), depth = depth, displayName = seleteTrans.name };
            allItems.Add(root);
            dictTreeItemDatas.Add(root, true);
            //添加所有子物体
            AddTransChildren(seleteTrans, depth, allItems);

            SetupParentsAndChildrenFromDepths(treeRoot, allItems);
            return treeRoot;
        }

        ///// <summary>
        ///// 当选中item时候
        ///// </summary>
        ///// <param name="selectedIds"></param>
        //protected override void SelectionChanged(IList<int> selectedIds)
        //{
        //    //在Hierarchy 中显示选中的物体
        //    Selection.activeInstanceID = selectedIds[0];
        //}

        /// <summary>
        /// 自定义UI
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            if (Selection.activeGameObject == null || Helper.dictIDs.Count <= 0) return;
            Event evt = Event.current;

            //图标和标签之前的间距的值
            extraSpaceBeforeIconAndLabel = 38;

            UITreeViewItem item = args.item as UITreeViewItem;

            //图标和checkbox 区域
            Rect toggleRect = args.rowRect;
            toggleRect.x += GetContentIndent(item);
            toggleRect.width = 16f;

            //绘制图标
            Rect gizmoRect = toggleRect;
            gizmoRect.x += 17;
            gizmoRect.width = 18f;

            //绘制图标
            if (Helper.IDToGameObject(item.id) != null)
            {
                GUI.DrawTexture(gizmoRect, new GUIContent(EditorGUIUtility.ObjectContent(null, Helper.GetType(Helper.IDToGameObject(item.id)))).image);
            }

            if (evt.type == EventType.MouseDown && toggleRect.Contains(evt.mousePosition))
                SelectionClick(args.item, false);

            try
            {
                EditorGUI.BeginChangeCheck();
                bool isCheck = EditorGUI.Toggle(toggleRect, dictTreeItemDatas[item]);
                if (EditorGUI.EndChangeCheck())
                {
                    dictTreeItemDatas[item] = isCheck;
                    if (linkPartentChild)
                        CheckChildren(item, isCheck);
                }
            }
            catch { }

            base.RowGUI(args);
        }

        /// <summary>
        /// 双击
        /// </summary>
        /// <param name="id"></param>
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            OnDoubleClick?.Invoke(id);
            Selection.activeInstanceID = id;
        }

        public void SeleteAll(bool ison)
        {
            //dictTreeItemDatas[rootItem] = ison;
            CheckChildren2(rootItem, ison);

        }

        private void CheckChildren2(TreeViewItem item, bool isCheck)
        {
            if (IsExpanded(item.id))
            {
                List<TreeViewItem> treeChilld = item.children;
                for (int i = 0; i < treeChilld.Count; i++)
                {
                    dictTreeItemDatas[treeChilld[i]] = isCheck;
                    CheckChildren2(treeChilld[i], isCheck);
                }
                dictTreeItemDatas[item] = isCheck;
            }
            else
            {
                dictTreeItemDatas[item] = isCheck;
            }
            //foreach (var itemChild in item.children)
            //{
            //    dictTreeItemDatas[itemChild] = isCheck;
            //    CheckChildren2(itemChild, isCheck);
            //}
            Debug.Log(IsExpanded(item.id) + " " + item.displayName);
        }

        //递归查询
        private void CheckChildren(TreeViewItem item, bool isCheck)
        {
            if (item.hasChildren)
            {
                foreach (var itemChild in item.children)
                {
                    dictTreeItemDatas[itemChild] = isCheck;
                    CheckChildren(itemChild, isCheck);
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
                    //item.icon = EditorGUIUtility.ObjectContent(null, Helper.GetType(itemTrans.gameObject)).image as Texture2D;  //有些显示不出来
                    dictTreeItemDatas.Add(item, true);
                    allItems.Add(item);
                    AddTransChildren(itemTrans, depth, allItems);
                }
            }
        }

        public TreeViewItem GetRootTreeView()
        {
            return rootItem;
        }
        public TreeViewItem GetRootFirstTreeView()
        {
            return rootItem.children[0];
        }
    }

}
}
