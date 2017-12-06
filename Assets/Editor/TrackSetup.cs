using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TrackSetup : MonoBehaviour {
	[MenuItem("Tools/Setup Track")]
	public static void SetupTrack() {
        var TrackHolder = FindObjectOfType<RacetrackHolder>();
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
	
	/*	MeshFilter filter = TrackHolder.GetComponent<MeshFilter>();
		if ( !filter ) {
			filter = TrackHolder.gameObject.AddComponent<MeshFilter>();
			if ( !TrackHolder.gameObject.GetComponent<MeshRenderer>()) {
				TrackHolder.gameObject.AddComponent<MeshRenderer>();
			}
		}

		filter.sharedMesh = DoMesh(TrackHolder);
		*/
	}

	static Mesh DoMesh(RacetrackHolder TrackHolder) {
		
		Mesh trackFloor = new Mesh();
		trackFloor.name = TrackHolder.TrackName;
		List<Vector3> trackVerts = new List<Vector3>(512);
		List<Vector3> trackNormals = new List<Vector3>(512);
		List<Vector2> trackUVs = new List<Vector2>(512);
		List<int> trackIndices = new List<int>(512);
		int indiceIndex = 0;

		TrackNode node = TrackHolder.FirstNode;
		do {
			trackVerts.Add(node.pole1.transform.position);
			trackVerts.Add(node.previous.pole1.transform.position);
			trackVerts.Add(node.pole2.transform.position);
			trackUVs.Add(new Vector2(0, 1));		
			trackUVs.Add(new Vector2(0, 0));
			trackUVs.Add(new Vector2(1, 1));
			var normal = Vector3.Cross((trackVerts[indiceIndex + 1] - trackVerts[indiceIndex]), (trackVerts[indiceIndex + 2] - trackVerts[indiceIndex])).normalized;
			trackNormals.Add(normal); 
			trackNormals.Add(normal);
			trackNormals.Add(normal);

			trackVerts.Add(node.previous.pole2.transform.position);
			trackVerts.Add(node.pole2.transform.position);
			trackVerts.Add(node.previous.pole1.transform.position);
			

			trackUVs.Add(new Vector2(1, 0));
			trackUVs.Add(new Vector2(1, 1));
			trackUVs.Add(new Vector2(0, 0));
			
			normal = Vector3.Cross((trackVerts[indiceIndex + 4] - trackVerts[indiceIndex + 3]), (trackVerts[indiceIndex + 5] - trackVerts[indiceIndex + 3])).normalized;
			trackNormals.Add(normal);
			trackNormals.Add(normal);
			trackNormals.Add(normal);

			trackIndices.Add(indiceIndex);
			trackIndices.Add(indiceIndex + 1);
			trackIndices.Add(indiceIndex + 2);
			trackIndices.Add(indiceIndex + 3);
			trackIndices.Add(indiceIndex + 4);
			trackIndices.Add(indiceIndex + 5);

			indiceIndex += 6;

			node = node.next;
		} while (node != TrackHolder.FirstNode);

		trackFloor.SetVertices(trackVerts);
		trackFloor.SetNormals(trackNormals);
		trackFloor.SetUVs(0, trackUVs);
		trackFloor.SetIndices(trackIndices.ToArray(), MeshTopology.Triangles, 0);
		trackFloor.SetTriangles(trackIndices.ToArray(), 0);
		trackFloor.RecalculateBounds();
		trackFloor.RecalculateNormals();
		trackFloor.RecalculateTangents();
		trackFloor.UploadMeshData(false);
		return trackFloor;
	}
}
