using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;

public class GameState : MonoBehaviour {

    public static GameState Instance = null;

    public GameObject PlayerPrefab = null;

    public List<Transform> StartPoints = new List<Transform>();

    public List<Player> Players = new List<Player>();

    float _startTime = 0f;

    public float TimeFromStart {
        get {
            return Time.time - _startTime;
        }
    }

    public int MaxMinesBeforeExplosion {
        get {
            return _maximumMines;
        }
        set {
            _maximumMines = value;
            EventManager.Fire<Event_MaximumMinesCount_Change>(new Event_MaximumMinesCount_Change() {count = value });
        }
    }

    int _maximumMines = 10;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(this);
            return;
        }
    }

    void Start() {
        var holder = FindObjectOfType<PlayerInfoHolder>();
        SpawnPlayers(holder.playersInfos);
    }

    void SpawnPlayers(List<PlayerInfo> players) {
        for (int i = 0; i < players.Count; i++) {
            var controls = new InputManager();
            controls.Init(players[i].prefix);
            GameObject playerGo = Instantiate(PlayerPrefab, StartPoints[i].position, Quaternion.identity, null);
            var player = playerGo.GetComponent<Player>();
            player.Init(i, controls, players[i].color);
        }
    }

    Player CreatePlayer(int index, InputManager controls, Color color) {
        var go = Instantiate(PlayerPrefab, null, StartPoints[index]);
        var player = go.GetComponent<Player>();
        player.Init(index, controls, color);

        return player;
    }

   

    

}
