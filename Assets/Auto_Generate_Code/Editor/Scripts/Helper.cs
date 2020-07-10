using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AutoGenerateCode
{
    /// <summary>
    /// 帮助类
    /// </summary>
    public class Helper
    {
        public const string Log_UIGenerate = "<color=#00ff00>[脚本代码提示]:</color>";

        /// <summary>
        /// 存放所有Canvas下的物体和ID
        /// </summary>
        public static Dictionary<int, GameObject> dictIDs = new Dictionary<int, GameObject>();

        /// <summary>
        /// 获取物体的类型
        /// </summary>
        /// <param name="uigo">必须是ui go</param>
        /// <returns></returns>
        public static Type GetType(GameObject uiGo)
        {
            if (uiGo == null) return null;

            if (uiGo.GetComponent<Canvas>() != null)
                return typeof(Canvas);

            if (uiGo.GetComponent<Button>() != null)
                return typeof(Button);

            else if (uiGo.GetComponent<InputField>() != null)
                return typeof(InputField);

            else if (uiGo.GetComponent<Toggle>() != null)
                return typeof(Toggle);

            else if (uiGo.GetComponent<Slider>() != null)
                return typeof(Slider);

            else if (uiGo.GetComponent<Dropdown>() != null)
                return typeof(Dropdown);

            else if (uiGo.GetComponent<Scrollbar>() != null)
                return typeof(Scrollbar);

            else if (uiGo.GetComponent<Image>() != null)
                return typeof(Image);

            else if (uiGo.GetComponent<RawImage>() != null)
                return typeof(RawImage);

            else if (uiGo.GetComponent<Text>() != null)
                return typeof(Text);


            //if (go.GetComponent<RectTransform>() != null)
            //    return typeof(RectTransform);

            else return typeof(RectTransform);
        }

        /// <summary>
        /// 将所有UI 控件添加到字典中
        /// </summary>
        /// <param name="canvasTranss"></param>
        public static void AddCanvasGoToDict(Canvas[] canvasTranss)
        {
            dictIDs.Clear();
            for (int i = 0; i < canvasTranss.Length; i++)
            {
                Transform itemUIgo = canvasTranss[i].transform;
                dictIDs.Add(itemUIgo.GetInstanceID(), itemUIgo.gameObject);
                RecursiveTrans(itemUIgo);
            }
        }
        private static void RecursiveTrans(Transform trans)
        {
            if (trans.childCount > 0)
            {
                for (int i = 0; i < trans.childCount; i++)
                {
                    Transform item = trans.GetChild(i);
                    try
                    {
                        dictIDs.Add(item.GetInstanceID(), item.gameObject);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(item.name);
                    }
                    RecursiveTrans(item);
                }
            }
        }

        /// <summary>
        /// 根据id 查找物体
        /// </summary>
        /// <param name="id">transform ID</param>
        /// <returns></returns>
        public static GameObject IDToGameObject(int id)
        {
            if (dictIDs.Count > 0)
            {
                try
                {
                    return dictIDs[id];
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取程序集 (用于加载脚本)
        /// </summary>
        /// <returns>Assembly-CSharp</returns>
        public static Assembly GetAssembly()
        {
            Assembly[] AssbyCustmList = System.AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < AssbyCustmList.Length; i++)
            {
                string assbyName = AssbyCustmList[i].GetName().Name;
                if (assbyName == "Assembly-CSharp")
                {
                    return AssbyCustmList[i];
                }
            }
            return null;
        }

        public static string GetCurrentTime()
        {
            return DateTime.Now.ToString();
        }

        public static async void AwaitToDo(int millis,System.Action callback)
        {
            await Task.Delay(millis);
            callback?.Invoke();

        }
    }
}