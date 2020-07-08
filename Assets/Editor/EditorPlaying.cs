using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// --(*^__^*) --
/// ____AUTHOR:    #AUTHOR#
/// ____DATE:      #DATE#
/// ____DESC:      #DESC#
/// ____VER:	   #VER#
/// ____UNITYVER:  #UNITYVER#
/// --(＝^ω^＝) --
/// </summary>
public class EditorPlaying : MonoBehaviour {


    [MenuItem("Examples/Execute menu items")]
    static void EditorPl()
    {
       // EditorApplication.ExecuteMenuItem("GameObject/3D Object/Cube");
        EditorApplication.ExecuteMenuItem("GameObject/UI/Text");

    }

}
