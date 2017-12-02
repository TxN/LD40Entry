using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby : MonoBehaviour {
	public GameObject playersInfosGameObject;

	private List<string> joinKeys = new List <string>() {"key1"}; //TODO
	private List<string> joinKeysPrefixes = new List <string>() {"prefix1"}; //TODO

	// Use this for initialization
	void Start () {
		playersInfosGameObject = new GameObject();
		playersInfosGameObject.AddComponent <PlayerInfoHolder>();
	}
	
	// Update is called once per frame
	void Update () {
		PlayerInfoHolder holder = playersInfosGameObject.GetComponent(typeof(PlayerInfoHolder)) as PlayerInfoHolder;

		int i = 0;
		while (i < joinKeys.Count) {
			if (Input.GetKeyDown (joinKeys [i])) {
				holder.AddPlayerInfo (new PlayerInfo (Color.black, joinKeysPrefixes [i])); //TODO: color
				joinKeys.RemoveAt (i);
				joinKeysPrefixes.RemoveAt (i);
				i -= 1;
			}

			i += 1;
		}
	}
}
