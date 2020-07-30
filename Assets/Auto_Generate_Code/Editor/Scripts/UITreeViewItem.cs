using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;


namespace AutoGenerateCode
{
    public class UITreeViewItem : TreeViewItem
    {

        public GameObject gameObject { get; set; }

        /// <summary>
        /// 是否添加成员变量
        /// </summary>
        public bool isVariable { get; set; } = true;

        /// <summary>
        /// 是否使用属性器
        /// </summary>
        public bool isProperty { get; set; } = false;

        /// <summary>
        /// 是否添加事件
        /// </summary>
        public bool isUseEvent { get; set; } = false;

        /// <summary>
        /// 当前选择的component索引
        /// </summary>
        public int CurrentSeleteComponentIndex { get; set; } = 0;

        /// <summary>
        /// 当前物体上的Components类型
        /// </summary>
        public string[] CurrentComponent
        {
            get
            {
                List<ComponentSort> coms = new List<ComponentSort>();
                Component[] components = this.gameObject.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    coms.Add(new ComponentSort(components[i].GetType().Name));
                }
                coms.Sort();
                string[] strs = new string[coms.Count];
                for (int i = 0; i < coms.Count; i++)
                {
                    strs[i] = coms[i].componentName;
                }
                return strs;
            }
        }

        public UITreeViewItem(int id, int depth, string displayName, bool isOn) : base(id, depth, displayName)
        {
            this.isVariable = isOn;
        }

        public UITreeViewItem() : base()
        {
        }



    }
}