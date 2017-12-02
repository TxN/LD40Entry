﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackNode : MonoBehaviour {
	public GameObject pole;
	public TrackNode previous;
	public TrackNode next;
	public Vector2 position;
	public float pole1Shift = -0.5f;
	public float pole2Shift = 0.5f;
	public GameObject pole1;
	public GameObject pole2;
	private Vector2 pos;
	public void Init(TrackNode prev)
	{
		previous = prev;
		position = new Vector2(previous.position.x + 1, previous.position.y + 1);
		pos = new Vector2 (pole1Shift, 0);
		pole1 = Instantiate (pole, pos, Quaternion.identity, transform);
		pos = new Vector2 (pole2Shift, 0);
		pole2 = Instantiate (pole, pos, Quaternion.identity, transform);
		previous.next = this;
	}
	void Update(){
		//pos = new Vector2 (pole1Shift, 0);
		pole1.transform.localPosition.Set (pole1Shift, 0, 0);
		//pos = new Vector2 (pole2Shift, 0);
		pole2.transform.localPosition.Set (pole2Shift, 0, 0);
	}
}