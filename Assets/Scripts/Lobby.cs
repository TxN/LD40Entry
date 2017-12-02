using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Lobby : MonoBehaviour {
	public GameObject playersInfosGameObject;

	private List<string> joinKeys = new List <string>() {"kb1_join"}; //TODO
	private List<string> joinKeysPrefixes = new List <string>() {"kb1"}; //TODO

	public List<GameObject> JoinObjects = new List<GameObject>();

	
	void Start () {
		playersInfosGameObject = new GameObject();
		playersInfosGameObject.AddComponent <PlayerInfoHolder>();
	}
	
	
	void Update () {
		PlayerInfoHolder holder = playersInfosGameObject.GetComponent(typeof(PlayerInfoHolder)) as PlayerInfoHolder;

		int i = 0;
		while (i < joinKeys.Count) {
			if (Input.GetButtonDown (joinKeys [i])) {
				PlayerInfo info = holder.playersInfos.Find(infs => infs.prefix == joinKeysPrefixes[i]);
				if ( info != null ) {
					holder.RemovePlayerInfo(info);
				} else {
					holder.AddPlayerInfo(new PlayerInfo(Random.ColorHSV(0, 1, 1, 1, 1, 1), joinKeysPrefixes[i]));
				}
			}
			i++;
		}
	}
}
