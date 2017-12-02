using EventSys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoHolder : MonoBehaviour {
	public List<PlayerInfo> playersInfos = new List<PlayerInfo>();

	public void AddPlayerInfo(PlayerInfo player) {
		playersInfos.Add(player);
	}

	public void RemovePlayerInfo(PlayerInfo player) {
		playersInfos.Remove(player);
	}

	void Start() {
		DontDestroyOnLoad (this);
	}
}