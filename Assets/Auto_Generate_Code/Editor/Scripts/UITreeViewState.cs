using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace AutoGenerateCode
{
    public class UITreeViewState : TreeViewState
    {
        public UITreeViewState() : base()
        {

        }

        public void SetTreeViewItemList(List<TreeViewItem> items)
        {
            ListTreeItemDatas listTreeItemDatas = new ListTreeItemDatas();

            for (int i = 0; i < items.Count; i++)
            {
                UITreeViewItem item= items[i] as UITreeViewItem;
                if (item != null)
                {
                    listTreeItemDatas.ItemTreeList.Add(new TreeItemData(item.id, item.depth, item.displayName,item.isVariable,item.isProperty,item.isUseEvent));
                }
            }
            System.IO.File.WriteAllText(Application.dataPath + "/tree.json", listTreeItemDatas.ToJson(), System.Text.Encoding.UTF8);
            Debug.Log("json 生成结束");
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

        public TreeItemData(int _id, int _depth, string _displayname,bool _isVariable,bool _isProperty,bool _isUseEvent)
        {
            this.id = _id;this.depth = _depth;this.displayname = _displayname;
            this.isVariable = _isVariable;
            this.isProperty = _isProperty;
            this.isUseEvent = _isUseEvent;
        }
    }
}
