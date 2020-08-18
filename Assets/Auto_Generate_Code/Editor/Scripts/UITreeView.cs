using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;



namespace AutoGenerateCode
{
    public class UITreeview : TreeView
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

        /// <summary>
        /// 所有的tree item
        /// </summary>
        public List<TreeViewItem> allItems;

        //当前选择的所有UI控件
        public Transform seleteTrans;

        /// <summary>
        /// 是否子父级关联
        /// </summary>
        public bool linkPartentChild = false;

        private UITreeViewState uitreeViewState;
        private bool isFirstInit = false;
        private TreeViewItem currRenameTreeItem;



        //初始化
        public UITreeview(TreeViewState treeViewState) : base(treeViewState)
        {
            uitreeViewState = treeViewState as UITreeViewState;
            Reload();
            rowHeight = 20f; //行高
            if (isFirstInit) ExpandAll();
            else
            {
                SetExpanded(0, true);
                foreach (var item in allItems)
                {
                    UITreeViewItem uitreeitem = (item as UITreeViewItem);
                    SetExpanded(uitreeitem.id, uitreeitem.isExpand);
                }
            }
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

            //判断原来有么有数据
            List<TreeViewItem> tempitems;
            if (uitreeViewState.GetAllTreeItemData(out tempitems))
            {
                //Debug.Log("原来有数据 从json中加载数据");
                allItems.AddRange(tempitems);
                isFirstInit = false;
            }
            else
            {
                isFirstInit = true;
                int depth = 0;
                var root = new UITreeViewItem { id = seleteTrans.GetInstanceID(), depth = depth, displayName = seleteTrans.name, gameObject = seleteTrans.gameObject };
                allItems.Add(root);
                //添加所有子物体
                BuildChildItemRecursive(seleteTrans, depth, allItems);
            }

            SetupParentsAndChildrenFromDepths(treeRoot, allItems);

            return treeRoot;
        }

        /// <summary>
        /// 自定义UI
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            if (Selection.activeGameObject != null)
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
                    {
                        CheckChildRecursive(item, isTogVariable,isVariable:true);
                    }
                }
                #endregion

                #region 属性框
                Rect togPropetty = togLable;
                togPropetty.x += 35;
                togPropetty.width = 16f;

                Rect togLablePropetty = togPropetty;
                togLablePropetty.x += 16;
                togLablePropetty.width = 28;
                GUI.color = Color.cyan;
                GUI.Label(togLablePropetty, "属性", EditorStyles.label);
                GUI.color = Color.white;
                EditorGUI.BeginChangeCheck();
                bool isTogPropetty = EditorGUI.Toggle(togPropetty, item.isProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    item.isProperty = isTogPropetty;
                    if (linkPartentChild)
                    {
                        CheckChildRecursive(item, isTogPropetty, isProperty: true);
                    }
                }
                #endregion

                #region 事件框
                Rect togEvent = togLablePropetty;
                togEvent.x += 35;
                togEvent.width = 16f;

                Rect togLableTogEvent = togEvent;
                togLableTogEvent.x += 16;
                togLableTogEvent.width = 28;
                GUI.color = Color.yellow;
                GUI.Label(togLableTogEvent, "事件", EditorStyles.whiteLabel);
                GUI.color = Color.white;

                EditorGUI.BeginChangeCheck();
                bool isTogEvent = EditorGUI.Toggle(togEvent, item.isUseEvent);
                if (EditorGUI.EndChangeCheck())
                {
                    item.isUseEvent = isTogEvent;
                    if (linkPartentChild)
                    {
                        CheckChildRecursive(item, isTogEvent, isEvent: true);
                    }
                }
                #endregion

                #region 枚举框
                Rect enumRect = togLableTogEvent;
                enumRect.x += 42;
                enumRect.width = 100;
                //ScaleMode scaleMode=ScaleMode.ScaleAndCrop;
                item.CurrentSeleteComponentIndex = EditorGUI.Popup(enumRect, item.CurrentSeleteComponentIndex, item.CurrentGoComponents);
                #endregion

                //if (evt.type == EventType.MouseDown && togVariable.Contains(evt.mousePosition))
                //    SelectionClick(item, false);

                args.rowRect = new Rect(new Vector2(args.rowRect.x + 252, args.rowRect.y), args.rowRect.size);

                //图标和标签之前的间距的值
                extraSpaceBeforeIconAndLabel = 36;
                //绘制图标
                Rect gizmoRect = args.rowRect;
                gizmoRect.x += GetContentIndent(item) + 16;
                gizmoRect.width = 16f;
                gizmoRect.height = 16f;
                //绘制图标
                GUI.DrawTexture(gizmoRect, new GUIContent(EditorGUIUtility.ObjectContent(null, Helper.GetTypeWithName(item.CurrentSeleteComponentName))).image);

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

      
        //重命名
        protected override bool CanRename(TreeViewItem item)
        {
            //Debug.Log(item.displayName);
            currRenameTreeItem = item;
            BeginRename(item);
            return base.CanRename(item);
        }
        protected override void RenameEnded(RenameEndedArgs args)
        {
            //base.RenameEnded(args);
            args.acceptedRename = true;
            string newname = args.newName;
            GameObject go = Helper.FindGameObjectWithID(args.itemID);
            go.name = newname;
            currRenameTreeItem.displayName = newname;

        }


        /// <summary>
        /// 设置子物体状态
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isCheck"></param>
        private void CheckChildRecursive(UITreeViewItem item, bool isOn, bool isVariable = false, bool isProperty = false, bool isEvent = false)
        {
            if (isVariable) item.isVariable = isOn;
            else if (isProperty) item.isProperty = isOn;
            else if (isEvent) item.isUseEvent = isOn;

            if (item.hasChildren)
            {
                foreach (var itemChild in item.children)
                {
                    UITreeViewItem uitreeitem = (itemChild as UITreeViewItem);
                    if (isVariable) uitreeitem.isVariable = isOn;
                    else if (isProperty) uitreeitem.isProperty = isOn;
                    else if (isEvent) uitreeitem.isUseEvent = isOn;
                    CheckChildRecursive(uitreeitem, isOn, isVariable, isProperty, isEvent);
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
        /// 设置变量全选状态
        /// </summary>
        /// <param name="isall"></param>
        public void SetVariableState(bool isall)
        {
            CheckChildRecursive(RootFirstTreeView as UITreeViewItem, isall, isVariable: true);
        }
        /// <summary>
        /// 设置属性旋转状态
        /// </summary>
        /// <param name="isall"></param>
        public void SetPropertyState(bool isall)
        {
            CheckChildRecursive(RootFirstTreeView as UITreeViewItem, isall, isProperty: true);
        }
        /// <summary>
        /// 设置事件选择状态
        /// </summary>
        /// <param name="isall"></param>
        public void SetEventState(bool isall)
        {
            CheckChildRecursive(RootFirstTreeView as UITreeViewItem, isall, isEvent: true);
        }
    }

}
