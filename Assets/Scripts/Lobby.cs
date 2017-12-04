using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour {
	public GameObject playersInfosGameObject;

	public List<string> joinKeys = new List <string>() {};
	public List<string> joinKeysPrefixes = new List <string>() {};
    const string READY_KEY = "_pause";

	public List<GameObject> JoinObjects = new List<GameObject>();

	PlayerInfoHolder _holder = null;

	void Start () {
		PlayerInfoHolder oldHolder = FindObjectOfType<PlayerInfoHolder>();
		if ( oldHolder != null ) {
			Destroy(oldHolder.gameObject);
		}

        playersInfosGameObject = new GameObject("[PlayerInputHolder]");
		_holder = playersInfosGameObject.AddComponent <PlayerInfoHolder>();

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
		while (i < joinKeys.Count) {
			if (Input.GetButtonDown (InputManager.GetKey(joinKeys [i]))) {
				PlayerInfo info = _holder.playersInfos.Find(infs => infs.prefix == joinKeysPrefixes[i]);
				if ( info != null ) {
					_holder.RemovePlayerInfo(info);
					GameObject hideGO = JoinObjects.Find(objs => objs.name == joinKeysPrefixes[i]);
					if (hideGO) {
						hideGO.SetActive(false);
					}
				} else {
					Color col = Random.ColorHSV(0, 1, 1, 1, 1, 1);
					_holder.AddPlayerInfo(new PlayerInfo(col, joinKeysPrefixes[i]));
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
			if (Input.GetButtonDown(InputManager.GetKey(player.prefix + READY_KEY))) {
                player.ready = !player.ready;
                GameObject readyGOParent = JoinObjects.Find(objs => objs.name == player.prefix);
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
}
