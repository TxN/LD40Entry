using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TrackScale :EditorWindow {

    float scale_factor = 1;

	[MenuItem("Tools/Scale Track")]
	public static void ScaleTrack() {
        EditorWindow.GetWindow(typeof(TrackScale));
	}

    void OnGUI()
    { 
        GUILayout.Label ("Scale to      ///mozet raspidorasit', also pole shifts must always be one positive and one negative", EditorStyles.boldLabel);
        scale_factor = EditorGUILayout.Slider ("Scale factor", scale_factor, 0, 2);
        if (GUILayout.Button("Scale Track"))
        {
            TrackNode[] nodes = FindObjectsOfType<TrackNode>();
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].pole1Shift *= scale_factor;
                nodes[i].pole2Shift *= scale_factor;
                nodes[i].MovePoles();
            }
            
            TrackSetup.SetupTrack();
        } 
    } 
}
