using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackNode : MonoBehaviour {
	public TrackNode previous;
	public TrackNode next;

	public Vector2 position;
	public float pole1Shift;
	public float pole2Shift;

	public TrackNode(TrackNode prev)
	{
		previous = prev;
		position = new Vector2 (previous.position.x + 1, previous.position.y + 1);
		pole1Shift = 0.5f;
		pole2Shift = 0.5f;
	}

	public void BuildObject()
	{
		GameObject obj = new GameObject();
		obj.AddComponent<TrackNode> ();
		obj.name = "Track Node";
	}
}