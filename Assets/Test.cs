using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// --(*^__^*) --
/// ____AUTHOR:    #AUTHOR#
/// ____DATE:      #DATE#
/// ____DESC:      #DESC#
/// ____VER:	   #VER#
/// ____UNITYVER:  #UNITYVER#
/// --(＝^ω^＝) --
/// </summary>
public class Test : MonoBehaviour
{

    private void Start()
    {
        Component[] components = GetComponents<Component>();
        foreach (var item in components)
        {
            if (item!=this)
            {
                Debug.Log(item.ToString());
            }
        }
    }



}
