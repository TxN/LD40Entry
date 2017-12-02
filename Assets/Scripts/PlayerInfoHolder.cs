using EventSys;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoHolder : MonoBehaviour {
	public List<PlayerInfo> playersInfos;

	public void AddPlayerInfo(PlayerInfo player) {
		playersInfos.Add(player);
	}

	void Start() {
		DontDestroyOnLoad (this);
	}
}