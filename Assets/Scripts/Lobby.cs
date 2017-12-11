using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using InputMng = TeamUtility.IO.InputManager;

public class Lobby : MonoBehaviour {
	public GameObject playersInfosGameObject;

	public List<string> joinKeys = new List <string>() {};
	public List<string> joinKeysPrefixes = new List <string>() {};
    const string READY_KEY = "_pause";

	public List<GameObject> JoinObjects = new List<GameObject>();

	PlayerInfoHolder _holder = null;

	public int playersConnected = 0;

	void Start () {
		PlayerInfoHolder oldHolder = FindObjectOfType<PlayerInfoHolder>();
		if ( oldHolder != null ) {
			Destroy(oldHolder.gameObject);
		}

        playersInfosGameObject = new GameObject("[PlayerInputHolder]");
		_holder = playersInfosGameObject.AddComponent <PlayerInfoHolder>();

		InitInputConfigurations();
	}

    bool _lockFlag = false;
		
	void Update () {
        if (_lockFlag) {
            return;
        }
		if (Input.GetKeyDown(KeyCode.Return) && JoinObjects.Count > 0) {
			GoToGame();
		}

		int i = 0;
		while (i < playersConnected) {
			TeamUtility.IO.PlayerID playerId = (TeamUtility.IO.PlayerID)System.Enum.GetValues(typeof(TeamUtility.IO.PlayerID)).GetValue(i);
			if (InputMng.GetButtonDown("Button A", playerId)) {
				PlayerInfo info = _holder.playersInfos.Find(infs => infs.playerNumber == i);
				if ( info != null ) {
					_holder.RemovePlayerInfo(info);
					GameObject hideGO = JoinObjects.Find(objs => objs.name == joinKeysPrefixes[i]);
					if (hideGO) {
						hideGO.SetActive(false);
					}
				} else {
					Color col = UnityEngine.Random.ColorHSV(0, 1, 1, 1, 1, 1);
					_holder.AddPlayerInfo(new PlayerInfo(col, i));
					GameObject showGO = JoinObjects.Find(objs => objs.activeSelf == false);
					if ( showGO ) {
						showGO.SetActive(true);
						showGO.name = joinKeysPrefixes[i];
						ColorSetter.UpdateModelColor(showGO, col);
					}
				}
                PlayMenuClick();
			}
			i++;
		}
        foreach (var player in _holder.playersInfos) {
			TeamUtility.IO.PlayerID playerId = (TeamUtility.IO.PlayerID)System.Enum.GetValues(typeof(TeamUtility.IO.PlayerID))
				.GetValue(player.playerNumber);
			if (InputMng.GetButtonDown("Start", playerId)) {
               player.ready = !player.ready;
               GameObject readyGOParent = JoinObjects.Find(objs => objs.name == joinKeysPrefixes[player.playerNumber]); 
               readyGOParent.transform.Find("ReadyFlag").gameObject.SetActive(player.ready);
               PlayMenuClick();
           }
        }

        bool _allReady = true;
        if (_holder.playersInfos.Count > 1) {
            foreach (var player in _holder.playersInfos) {
                if (!player.ready) {
                    _allReady = false;
                }
            }
        } else {
            _allReady = false;
        }

        if (_allReady) {
            _lockFlag = true;
            Invoke("GoToGame",0.5f);
        }
	}

	void GoToGame() {
		SceneManager.LoadScene("track1");
	}

    AudioSource _audioSrc = null;
    AudioSource AudioSrc{
        get {
            if (_audioSrc == null) {
                _audioSrc = GetComponent<AudioSource>();
            }
            return _audioSrc;
        }
}
    void PlayMenuClick() {
        AudioSrc.Play();
    }

	void InitInputConfigurations() {
		int playerNumber = InputMng.GetJoystickNames().Length;
		if (playerNumber > 4) playerNumber = 4;
		
		string confNamePrefix = "Win";
		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
			confNamePrefix = "OS_X";
		}

		for (int i = 0; i < playerNumber; i++) {
			TeamUtility.IO.PlayerID playerId = (TeamUtility.IO.PlayerID)System.Enum.GetValues(typeof(TeamUtility.IO.PlayerID)).GetValue(i);
			InputMng.SetInputConfiguration(confNamePrefix + "_gamepad_" + (i + 1), playerId);
		}

		if (playerNumber < 4) {
			TeamUtility.IO.PlayerID playerId = (TeamUtility.IO.PlayerID)System.Enum.GetValues(typeof(TeamUtility.IO.PlayerID)).GetValue(playerNumber);
			InputMng.SetInputConfiguration("keyboard", playerId);
			playersConnected = playerNumber + 1;
		} else {
			playersConnected = 4;
		}
	}
}
