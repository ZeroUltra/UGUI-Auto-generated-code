using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class Helper
{
    public static Dictionary<int, GameObject> dictIDs = new Dictionary<int, GameObject>();

    public static Type GetType(GameObject go)
    {
        if (go.GetComponent<Canvas>() != null)
            return typeof(Canvas);

        if (go.GetComponent<Button>() != null)
            return typeof(Button);

        else if (go.GetComponent<InputField>() != null)
            return typeof(InputField);

        else if (go.GetComponent<Toggle>() != null)
            return typeof(Toggle);

        else if (go.GetComponent<Slider>() != null)
            return typeof(Slider);

        else if (go.GetComponent<Dropdown>() != null)
            return typeof(Dropdown);

        else if (go.GetComponent<Scrollbar>() != null)
            return typeof(Scrollbar);

        else if (go.GetComponent<Image>() != null)
            return typeof(Image);

        else if (go.GetComponent<RawImage>() != null)
            return typeof(RawImage);

        else if (go.GetComponent<Text>() != null)
            return typeof(Text);

        //if (go.GetComponent<RectTransform>() != null)
        //    return typeof(RectTransform);

        else return typeof(RectTransform);
    }

    /// <summary>
    /// 将所有UI 控件添加到字典中
    /// </summary>
    /// <param name="canvasTranss"></param>
    public static void FindObjIDToDict(Canvas[] canvasTranss)
    {
        dictIDs.Clear();
        for (int i = 0; i < canvasTranss.Length; i++)
        {
            Transform itemCancas = canvasTranss[i].transform;
            dictIDs.Add(itemCancas.GetInstanceID(), itemCancas.gameObject);
            RecursiveTrans(itemCancas);
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

    public static GameObject IDToGameObject(int id)
    {
        if (dictIDs.Count > 0)
        {
            return dictIDs[id];
        }
        return null;
    }
}
