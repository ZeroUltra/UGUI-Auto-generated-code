using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
public class TestEditor : EditorWindow
{
    private static readonly GUILayoutOption[] BUTTON_OPTIONS =
    {
        GUILayout.MinWidth( 28 ),
        GUILayout.MaxWidth( 48 ),
        GUILayout.Height( 24 ),
    };
    [MenuItem("Window/Test")]
    private static void Init()
    {
        var win = GetWindow<TestEditor>("Test");

        var pos = win.position;
        pos.width = 640;
        pos.height = 300;
        win.position = pos;
       
       
    }
    private void OnGUI()
    {
        Image img = Selection.activeGameObject.GetComponent<Image>();
     
        GUIContent content =new GUIContent(EditorGUIUtility.ObjectContent(null, typeof(Image)).image, nameof(Image));
        //GUI.color = Color.cyan;
        //GUILayout.Button("button");
        //content.text = "2222";
        //GUILayout.Label("hello");
        //GUI.color = Color.white;
       
        GUILayout.Box(content);
       // GUILayout(content, BUTTON_OPTIONS);
       
    }
}
