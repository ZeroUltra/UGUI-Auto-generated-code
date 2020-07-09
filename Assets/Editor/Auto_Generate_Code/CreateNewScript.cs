using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AutoGenerateCode
{
    public class CreateNewScript
    {

        private const string Str_Using = "using System.Collections;\nusing UnityEngine;\nusing UnityEngine.UI;\n\n";

        private static string Str_Class = "public class #scriptName#:MonoBehaviour";

        public const string scriptName = "#scriptName#";

        private static UITreeView uitreeview;

        public static string GenerateScript( UITreeView uitreeView)
        {
            uitreeview = uitreeView;
          //  Str_Class = Str_Class.Replace("scriptName", _scriptName);

            StringBuilder sb = new StringBuilder();
            sb.Append(Str_Using);
            sb.Append(Str_Class);
            sb.Append("\n{\n");

            List<TreeData> listData = new List<TreeData>();
            foreach (var item in uitreeView.dictTreeItemDatas)
            {
                if (item.Value)
                {

                    TreeViewItem treeItem = item.Key;
                    if (treeItem.displayName == "Root") continue; //排除Root
                    //添加变量 
                    try
                    {
                        if (Helper.IDToGameObject(treeItem.id) != null)
                        {
                            string typeName = Helper.GetType(Helper.IDToGameObject(treeItem.id)).Name;
                            sb.Append($"\t[SerializeField] {typeName} {treeItem.displayName};\r\n");
                            //获取路径 把第一个删除掉 +1是"/"
                            string path = GetTreeItemFullPath(treeItem).ToString();
                            listData.Add(new TreeData(treeItem.displayName, path, typeName));
                        }
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError(treeItem.id + " " + treeItem.displayName);
                    }
                }
            }
            sb.Append("\r\n\r\n\r\n");

            sb.Append("//用于寻找控件,当控件丢失,点击脚本齿轮->Reset菜单可恢复,也可重新编写下面的路径代码\r\n");
            //添加reset
            sb.Append("#if UNITY_EDITOR\n\tprivate void Reset()\n");
            sb.Append("\t{\n");
            //添加寻找代码
            for (int i = 0; i < listData.Count; i++)
            {
                string needFind = string.IsNullOrEmpty(listData[i].path) ? string.Empty : $"transform.Find(\"{listData[i].path}\").";
                sb.Append($"\t\t{listData[i].variablename}={needFind}GetComponent<{listData[i].typename}>();\r\n");
            }


            sb.Append("\t}\n");
            sb.Append("#endif\n");

            sb.Append("}");
            return sb.ToString();
        }

        private struct TreeData
        {
            public string variablename;
            public string path;
            public string typename;
            public TreeData(string _variablename, string _path, string _typename)
            {
                this.variablename = _variablename;
                this.path = _path;
                this.typename = _typename;
            }
        }
        private static StringBuilder GetTreeItemFullPath(TreeViewItem treeItem)
        {
            StringBuilder sb = new StringBuilder();
            List<string> listpath = new List<string>();
            while (treeItem.parent != null && treeItem.parent.id != uitreeview.GetRootTreeView().id)
            {
                listpath.Add(treeItem.displayName);
                treeItem = treeItem.parent;
            }
            for (int i = listpath.Count - 1; i >= 0; i--)
            {
                if (i > 0)
                    sb.Append(listpath[i] + "/");
                else sb.Append(listpath[i]);
            }
            return sb;
        }

    }
}
