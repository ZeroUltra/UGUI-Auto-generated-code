using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace AutoGenerateCode
{
    public class UITreeViewState : TreeViewState
    {
        private string jsonDataPath;
        public UITreeViewState() : base()
        {
            jsonDataPath = Application.persistentDataPath + "/tree.json";
        }
        /// <summary>
        /// 将所有的treeitem数据保存
        /// </summary>
        /// <param name="items"></param>
        public void SaveAllTreeItemData(UITreeview uiTreeview)
        {
            List<TreeViewItem> items = uiTreeview.allItems;
            ListTreeItemDatas listTreeItemDatas = new ListTreeItemDatas();

            for (int i = 0; i < items.Count; i++)
            {
                UITreeViewItem item = items[i] as UITreeViewItem;
                if (item != null)
                {
                    listTreeItemDatas.ItemTreeList.Add(new TreeItemData(item.id, item.depth, item.displayName, item.isVariable, item.isProperty, item.isUseEvent, uiTreeview.IsExpanded(item.id)));
                }
            }

            System.IO.File.WriteAllText(jsonDataPath, listTreeItemDatas.ToJson(), System.Text.Encoding.UTF8);
            //Debug.Log("json 生成结束");
        }

        public bool GetAllTreeItemData(out List<TreeViewItem> items)
        {
            items = new List<TreeViewItem>();
            if (!System.IO.File.Exists(jsonDataPath)) return false;
            string json = System.IO.File.ReadAllText(jsonDataPath, System.Text.Encoding.UTF8);
            ListTreeItemDatas listTree = ListTreeItemDatas.FormJson(json);
            foreach (var item in listTree.ItemTreeList)
            {
                UITreeViewItem uiItem = new UITreeViewItem();
                uiItem.id = item.id;
                uiItem.depth = item.depth;
                uiItem.displayName = item.displayname;
                uiItem.gameObject = Helper.FindGameObjectWithID(item.id);
                uiItem.isVariable = item.isVariable;
                uiItem.isProperty = item.isProperty;
                uiItem.isUseEvent = item.isUseEvent;
                uiItem.isExpand = item.isExpand;
                items.Add(uiItem);
            }
            return items.Count > 0;
        }
        public void DeleteDatas()
        {
            if (System.IO.File.Exists(jsonDataPath)) System.IO.File.Delete(jsonDataPath);
        }
    }

   

    [System.Serializable]
    public class ListTreeItemDatas
    {
        public List<TreeItemData> ItemTreeList;
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
        public ListTreeItemDatas()
        {
            ItemTreeList = new List<TreeItemData>();
        }

        public static ListTreeItemDatas FormJson(string json)
        {
            return JsonUtility.FromJson<ListTreeItemDatas>(json);
        }
    }
    [System.Serializable]
    public class TreeItemData
    {
        public int id;
        public int depth;
        public string displayname;
        public bool isVariable;
        public bool isProperty;
        public bool isUseEvent;
        public bool isExpand;

        public TreeItemData(int _id, int _depth, string _displayname, bool _isVariable, bool _isProperty, bool _isUseEvent, bool _isExpand)
        {
            this.id = _id; this.depth = _depth; this.displayname = _displayname;
            this.isVariable = _isVariable;
            this.isProperty = _isProperty;
            this.isUseEvent = _isUseEvent;
            this.isExpand = _isExpand;
        }
    }
}
