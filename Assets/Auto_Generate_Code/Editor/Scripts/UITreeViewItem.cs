using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;


namespace AutoGenerateCode
{
    public class UITreeViewItem : TreeViewItem
    {

        public bool isOn { get; set; } = true;

        public UITreeViewItem(int id, int depth, string displayName,bool isOn) : base(id, depth, displayName)
        {
            this.isOn = isOn;
        }

        public UITreeViewItem() : base()
        {
        }
    }
}