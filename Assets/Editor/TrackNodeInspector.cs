using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackNode))]
public class TrackNodeInspector : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		TrackNode myScript = (TrackNode)target;
		if(GUILayout.Button("Add Track Node"))
		{
			myScript.BuildObject();
		}
	}
}