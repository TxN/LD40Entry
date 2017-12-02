using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;

public class GameState : MonoBehaviour {

    public static GameState Instance = null;

    public GameObject PlayerPrefab = null;
    public GameObject PauseMenu = null;
    public TrackNode FirstTrackNode = null;

    public List<Transform> StartPoints = new List<Transform>();

    public List<Player> Players = new List<Player>();

    float _startTime = 0f;

    List<TrackNode> _trackNodes = new List<TrackNode>();

    [HideInInspector]
    public TrackNode CurrentNode = null;

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

    bool _pauseFlag = false;

    public bool PauseEnabled {
        get {
            return _pauseFlag;
        }
        set {
            _pauseFlag = value;
            Time.timeScale = _pauseFlag ? 0 : 1;
        }
    }

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(this);
            return;
        }
    }

    void Start() {
        GetTrackNodes();
        CurrentNode = FirstTrackNode;
        var holder = FindObjectOfType<PlayerInfoHolder>();
        SpawnPlayers(holder.playersInfos);
        EventManager.Subscribe<Event_Paused>(this, OnPauseToggle);
    }

    void SpawnPlayers(List<PlayerInfo> players) {
        for (int i = 0; i < players.Count; i++) {
            GameObject playerGo = Instantiate(PlayerPrefab, StartPoints[i].position, Quaternion.identity, null);
			var controls = playerGo.AddComponent<InputManager>();
			controls.Init(players[i].prefix);
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

    void GetTrackNodes() {
        _trackNodes.Clear();
        var _curTrackNode = FirstTrackNode;
        while (_curTrackNode.next != FirstTrackNode) {
            _trackNodes.Add(_curTrackNode);
            _curTrackNode = _curTrackNode.next;
        }
    }

    void OnPauseToggle(Event_Paused e) {
        PauseEnabled = !PauseEnabled;
    }

    void OnDestroy() {
        EventManager.Unsubscribe<Event_Paused>(OnPauseToggle);
    }
}
