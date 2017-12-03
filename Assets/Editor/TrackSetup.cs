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

            var pole1tr = node.pole1.transform.GetChild(0);
            var pole2tr = node.pole2.transform.GetChild(0);

            EdgeCollider2D col = node.pole1.transform.GetChild(0).gameObject.GetComponent<EdgeCollider2D>();
            if (!col) {
                col = node.pole1.transform.GetChild(0).gameObject.AddComponent<EdgeCollider2D>();
            }
            col.points = new Vector2[] { pole1tr.localPosition, pole1tr.InverseTransformPoint(node.next.pole1.transform.GetChild(0).position) };
            col = null;

            

			ren = node.pole2.GetComponentInChildren<LineRenderer>();
			points[0] = node.pole2.transform.GetChild(0).position;
			points[1] = node.next.pole2.transform.GetChild(0).position;
			ren.positionCount = 2;
			ren.SetPositions(points);

            col = node.pole2.transform.GetChild(0).gameObject.GetComponent<EdgeCollider2D>();
            if (!col) {
                col = node.pole2.transform.GetChild(0).gameObject.AddComponent<EdgeCollider2D>();
            }

            col.points = new Vector2[] { pole2tr.localPosition, pole2tr.InverseTransformPoint(node.next.pole2.transform.GetChild(0).position) };
            col = null;
		}
	}
}
