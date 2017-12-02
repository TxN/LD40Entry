using UnityEngine;
using UnityEditor;

public class TrackSetup : MonoBehaviour {
	[MenuItem("Tools/Setup Track")]
	public static void SetupTrack() {
		var nodes = FindObjectsOfType<TrackNode>();
		foreach (var node in nodes) {
			if (node.next == null) {
				continue;
			}
			var ren = node.pole1.GetComponentInChildren<LineRenderer>();
			Vector3[] points = new Vector3[2];
			points[0] = node.pole1.transform.GetChild(0).position;
			points[1] = node.next.pole1.transform.GetChild(0).position;
			ren.positionCount = 2;
			ren.SetPositions(points);

			ren = node.pole2.GetComponentInChildren<LineRenderer>();
			points[0] = node.pole2.transform.GetChild(0).position;
			points[1] = node.next.pole2.transform.GetChild(0).position;
			ren.positionCount = 2;
			ren.SetPositions(points);
		}
	}
}
