using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;



namespace AutoGenerateCode
{
    public class UITreeview2 : TreeView
    {

        #region 事件
        public event System.Action OnNullSelete; //当没有选择物体
        public event System.Action<int> OnDoubleClick;
        #endregion


        #region 属性
        public TreeViewItem RootTreeView
        {
            get { return rootItem; }
        }

        public TreeViewItem RootFirstTreeView
        {
            get
            {
                if (rootItem != null)
                    return rootItem.children[0];
                else return null;
            }
        }
        #endregion

        public List<TreeViewItem> allItems;

        //当前选择的所有UI控件
        public Transform seleteTrans;

        /// <summary>
        /// 是否子父级关联
        /// </summary>
        public bool linkPartentChild = false;

        // public Dictionary<TreeViewItem, bool> dictTreeItemDatas = new Dictionary<TreeViewItem, bool>();

        //初始化
        public UITreeview2(TreeViewState treeViewState) : base(treeViewState)
        {
            Reload();
            rowHeight = 20f; //行高
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
                TreeViewItem treeRoot0 = new TreeViewItem(0, -1, "Root");
                SetupParentsAndChildrenFromDepths(treeRoot0, new List<TreeViewItem>() { new TreeViewItem(100, 0, "请选择一个对象") });
                return treeRoot0;
            }

            //当前选中的
            seleteTrans = Selection.activeGameObject.transform;
            //生命一个根目录
            var treeRoot = new UITreeViewItem { id = 0, depth = -1, displayName = "Root" };
            allItems = new List<TreeViewItem>();

            int depth = 0;
            var root = new UITreeViewItem { id = seleteTrans.GetInstanceID(), depth = depth, displayName = seleteTrans.name, gameObject = seleteTrans.gameObject };
            allItems.Add(root);

            //添加所有子物体
            BuildChildItemRecursive(seleteTrans, depth, allItems);

            SetupParentsAndChildrenFromDepths(treeRoot, allItems);
            return treeRoot;

        }


        /// <summary>
        /// 自定义UI
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            if (Selection.activeGameObject != null && Helper.dictIDs.Count > 0)
            {
                Event evt = Event.current;

                UITreeViewItem item = args.item as UITreeViewItem;
                if (item == null) return;

                #region 变量框
                Rect togVariable = args.rowRect;
                togVariable.x += GetContentIndent(item);
                togVariable.width = 16f;

                Rect togLable = togVariable;
                togLable.x += 16;
                togLable.width = 28;
                GUI.Label(togLable, "变量", EditorStyles.whiteLabel);

                EditorGUI.BeginChangeCheck();
                bool isTogVariable = EditorGUI.Toggle(togVariable, item.isVariable);
                if (EditorGUI.EndChangeCheck())
                {
                    item.isVariable = isTogVariable;
                    if (linkPartentChild)
                        CheckChildRecursive(item, isTogVariable);
                }
                #endregion

                #region 属性框
                Rect togPropetty = togLable;
                togPropetty.x += 32;
                togPropetty.width = 16f;

                Rect togLablePropetty = togPropetty;
                togLablePropetty.x += 16;
                togLablePropetty.width = 28;
                GUI.Label(togLablePropetty, "属性", EditorStyles.whiteLabel);

                EditorGUI.BeginChangeCheck();
                bool isTogPropetty = EditorGUI.Toggle(togPropetty, item.isProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    item.isProperty = isTogPropetty;
                }
                #endregion


                #region 事件框
                Rect togEvent = togLablePropetty;
                togEvent.x += 32;
                togEvent.width = 16f;

                Rect togLableTogEvent = togEvent;
                togLableTogEvent.x += 16;
                togLableTogEvent.width = 28;
                GUI.Label(togLableTogEvent, "事件", EditorStyles.whiteLabel);

                EditorGUI.BeginChangeCheck();
                bool isTogEvent = EditorGUI.Toggle(togEvent, item.isUseEvent);
                if (EditorGUI.EndChangeCheck())
                {
                    item.isUseEvent = isTogEvent;
                }
                #endregion

                #region 枚举框
                Rect enumRect = togLableTogEvent;
                enumRect.x += 40;
                enumRect.width = 100;
                //ScaleMode scaleMode=ScaleMode.ScaleAndCrop;
                item.CurrentSeleteComponentIndex = EditorGUI.Popup(enumRect, item.CurrentSeleteComponentIndex, item.CurrentComponent);
                #endregion

                //if (evt.type == EventType.MouseDown && togVariable.Contains(evt.mousePosition))
                //    SelectionClick(item, false);

                args.rowRect = new Rect(new Vector2(args.rowRect.x + 250, args.rowRect.y), args.rowRect.size);

                //图标和标签之前的间距的值
                extraSpaceBeforeIconAndLabel = 36;
                //绘制图标
                Rect gizmoRect = args.rowRect;
                gizmoRect.x += GetContentIndent(item) + 16;
                gizmoRect.width = 16f;
                gizmoRect.height = 16f;

                //绘制图标
                if (Helper.IDToGameObject(item.id) != null)
                {
                    GUI.DrawTexture(gizmoRect, new GUIContent(EditorGUIUtility.ObjectContent(null, Helper.GetType(Helper.IDToGameObject(item.id)))).image);
                }
                base.RowGUI(args);
            }
            else
            {
                base.RowGUI(args);
            }
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


        /// <summary>
        /// 设置子物体状态
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isCheck"></param>
        private void CheckChildRecursive(UITreeViewItem item, bool isOn)
        {
            item.isVariable = isOn;
            if (item.hasChildren)
            {
                foreach (var itemChild in item.children)
                {
                    UITreeViewItem uitreeitem = (itemChild as UITreeViewItem);
                    uitreeitem.isVariable = isOn;
                    CheckChildRecursive(uitreeitem, isOn);
                }
            }
        }

        /// <summary>
        /// 将子对象添加到TreeView
        /// </summary>
        /// <param name="rootTrans"></param>
        /// <param name="depth"></param>
        /// <param name="allItems"></param>
        private void BuildChildItemRecursive(Transform rootTrans, int depth, List<TreeViewItem> allItems)
        {
            if (rootTrans.childCount > 0)
            {
                depth++;
                foreach (Transform itemTrans in rootTrans)
                {
                    var item = new UITreeViewItem { id = itemTrans.GetInstanceID(), depth = depth, displayName = itemTrans.name, gameObject = itemTrans.gameObject };
                    //item.icon = EditorGUIUtility.ObjectContent(null, Helper.GetType(itemTrans.gameObject)).image as Texture2D;  //有些显示不出来
                    allItems.Add(item);
                    BuildChildItemRecursive(itemTrans, depth, allItems);
                }
            }
        }


        /// <summary>
        /// 设置全选状态
        /// </summary>
        /// <param name="ison"></param>
        public void SeleteAll(bool ison)
        {
            CheckChildRecursive(RootFirstTreeView as UITreeViewItem, ison);
        }
    }

}
