using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Lobby : MonoBehaviour {
	public GameObject playersInfosGameObject;

	public List<string> joinKeys = new List <string>() {"kb1_join"}; //TODO
	public List<string> joinKeysPrefixes = new List <string>() {"kb1"}; //TODO

	public List<GameObject> JoinObjects = new List<GameObject>();

	PlayerInfoHolder _holder = null;


	void Start () {
		PlayerInfoHolder oldHolder = FindObjectOfType<PlayerInfoHolder>();
		if ( oldHolder != null ) {
			Destroy(oldHolder.gameObject);
		}

		playersInfosGameObject = new GameObject();
		_holder = playersInfosGameObject.AddComponent <PlayerInfoHolder>();
	}
	
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.Return)) {
			GoToGame();
		}

		int i = 0;
		while (i < joinKeys.Count) {
			if (Input.GetButtonDown (joinKeys [i])) {
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
			}
			i++;
		}
	}

	void GoToGame() {
		SceneManager.LoadScene("MainScene");
	}
}
