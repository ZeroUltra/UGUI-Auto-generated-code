using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace AutoGenerateCode
{
    public class ScriptGenerator
    {
        //引用空间
        private const string Str_Using = "using System.Collections;\nusing UnityEngine;\nusing UnityEngine.UI;\n\n";
        //类声明
        private const string Str_Class = "public class #ScriptName#:MonoBehaviour";
        //要替换的类名
        public const string ScriptName = "#ScriptName#";

        private static UITreeView uitreeview;

        //存放要添加事件的UI类型
        private static HashSet<Type> hashUIEventType = new HashSet<Type>() {
            typeof(Button), typeof(Toggle), typeof(InputField), typeof(Dropdown),typeof(Slider),typeof(Scrollbar),typeof(ScrollRect)
        };


        /// <summary>
        /// 获取层级路径
        /// </summary>
        /// <param name="treeItem"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 生成代码
        /// </summary>
        /// <param name="uitreeView"></param>
        /// <returns></returns>
        public static string StartScriptGenerate(UITreeView uitreeView, bool addEvent)
        {
            uitreeview = uitreeView;

            StringBuilder sb = new StringBuilder();
            sb.Append(Str_Using);
            sb.Append(Str_Class);
            sb.Append("\n{\n");

            //添加数据
            List<VariableNameData> listVariableName = new List<VariableNameData>();//变量名
            List<PathData> listVariablePath = new List<PathData>();//变量名路径
            foreach (var item in uitreeView.dictTreeItemDatas)
            {
                if (item.Value)
                {
                    TreeViewItem treeItem = item.Key;
                    if (treeItem.displayName == "Root") continue; //排除Root

                    try
                    {
                        if (Helper.IDToGameObject(treeItem.id) != null)
                        {
                            //类型 Image Button
                            Type type = Helper.GetType(Helper.IDToGameObject(treeItem.id));
                            string disName = treeItem.displayName.Replace(" ", "_");
                            //添加变量
                            listVariableName.Add(new VariableNameData(type, disName));
                            //添加查找路径
                            listVariablePath.Add(new PathData(disName, GetTreeItemFullPath(treeItem).ToString(), type.Name));
                        }
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError(treeItem.id + " " + treeItem.displayName);
                    }
                }
            }

            sb.Append("\t[Header(\"UI\")]\n");
            #region 添加变量
            foreach (var item in listVariableName)
            {
                sb.Append($"\t[SerializeField] {item.type.Name} {item.goName};\r\n");
            }
            #endregion

            #region 添加UI事件
            if (addEvent)
            {
                List<string> listMethodName = new List<string>();
                sb.Append("\r\n");
                sb.Append("\tprivate void Start()\n\t{\n"); //start 方法
                foreach (var item in listVariableName)
                {
                    if (hashUIEventType.Contains(item.type))
                    {
                        sb.Append($"\t\t{item.goName}.");
                        if (item.type == typeof(Button))
                        {
                            string methodName = $"{item.goName}_OnClick";
                            sb.Append($"onClick.AddListener({methodName});\n");
                            listMethodName.Add(methodName + "()");
                        }
                        else if (item.type == typeof(Toggle))
                        {
                            string methodName = $"{item.goName}_OnValueChanged";
                            sb.Append($"onValueChanged.AddListener({methodName});\n");
                            listMethodName.Add(methodName + "(bool isOn)");
                        }
                        else if (item.type == typeof(InputField))
                        {
                            string methodName = $"{item.goName}_OnValueChanged";
                            sb.Append($"onValueChanged.AddListener({methodName});\n");
                            listMethodName.Add(methodName + "(string arg)");
                        }
                        else if (item.type == typeof(Slider))
                        {
                            string methodName = $"{item.goName}_OnValueChanged";
                            sb.Append($"onValueChanged.AddListener({methodName});\n");
                            listMethodName.Add(methodName + "(float value)");
                        }
                        else if (item.type == typeof(Dropdown))
                        {
                            string methodName = $"{item.goName}_OnValueChanged";
                            sb.Append($"onValueChanged.AddListener({methodName});\n");
                            listMethodName.Add(methodName + "(int index)");
                        }
                        else if (item.type == typeof(Scrollbar))
                        {
                            string methodName = $"{item.goName}_OnValueChanged";
                            sb.Append($"onValueChanged.AddListener({methodName});\n");
                            listMethodName.Add(methodName + "(float value)");
                        }
                        else if (item.type == typeof(ScrollRect))
                        {
                            string methodName = $"{item.goName}_OnValueChanged";
                            sb.Append($"onValueChanged.AddListener({methodName});\n");
                            listMethodName.Add(methodName + "(Vector2 detal)");
                        }
                    }
                }
                sb.Append("\t}\n");
                //添加方法
                foreach (var method in listMethodName)
                {
                    sb.Append($"\tprivate void {method}\n");
                    sb.Append("\t{\n\n\t}\n");
                }
            }
            #endregion

            sb.Append("\r\n\r\n\r\n");
            #region 添加Reset方法 查询路径
            sb.Append("\t#region 用于寻找控件,当控件丢失,点击脚本齿轮->Reset菜单可恢复,也可重新编写下面的路径代码\r\n");
            sb.Append("#if UNITY_EDITOR\n\tprivate void Reset()\n");
            sb.Append("\t{\n");
            //添加寻找代码
            for (int i = 0; i < listVariablePath.Count; i++)
            {
                string needFind = string.IsNullOrEmpty(listVariablePath[i].path) ? string.Empty : $"transform.Find(\"{listVariablePath[i].path}\").";
                sb.Append($"\t\t{listVariablePath[i].variablename}={needFind}GetComponent<{listVariablePath[i].typename}>();\r\n");
            }

            sb.Append("\t}\n");
            sb.Append("#endif\n");
            sb.Append("\t#endregion\n");
            #endregion

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 保存代码
        /// </summary>
        /// <param name="codeStr"></param>
        /// <param name="destfile"></param>
        /// <returns></returns>
        public static string SaveScript(string codeStr, string destfile)
        {
            try
            {
                codeStr = codeStr.Replace(ScriptName, Path.GetFileNameWithoutExtension(destfile));
                File.WriteAllText(destfile, codeStr);
                AssetDatabase.Refresh();
                Debug.Log(Helper.Log_UIGenerate + "脚本保存成功");
                return Helper.GetCurrentTime() + "脚本保存成功,位于:" + destfile;
            }
            catch (Exception e)
            {
                Debug.Log(Helper.Log_UIGenerate + "脚本保存失败 " + e.ToString());
                return Helper.GetCurrentTime() + "脚本保存失败:" + e.ToString();
            }
        }

        #region Struct Data

        private struct VariableNameData
        {
            public Type type;
            public string goName;
            public VariableNameData(Type _type, string _goName)
            {
                this.type = _type;
                this.goName = _goName;
            }
        }

        private struct PathData
        {
            public string variablename;
            public string path;
            public string typename;
            public PathData(string _variablename, string _path, string _typename)
            {
                this.variablename = _variablename;
                this.path = _path;
                this.typename = _typename;
            }
        }


        #endregion
    }
}
