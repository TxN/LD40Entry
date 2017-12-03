using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;
using System.Linq;

public class GameState : MonoBehaviour {

    public static GameState Instance = null;

    public GameObject PlayerPrefab = null;
	public GameObject MinePrefab = null;
    public GameObject PauseMenu = null;
    public TrackNode FirstTrackNode = null;

    public List<Transform> StartPoints = new List<Transform>();

    public List<Player> Players = new List<Player>();
	public Player FirstPlayer = null;

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
		SpawnMines ();
        EventManager.Subscribe<Event_Paused>(this, OnPauseToggle);
        EventManager.Subscribe<Event_PlayerDead>(this, OnPlayerDead);
    }

    void SpawnPlayers(List<PlayerInfo> players) {
        for (int i = 0; i < players.Count; i++) {
            GameObject playerGo = Instantiate(PlayerPrefab, StartPoints[i].position, Quaternion.identity, null);
			var controls = playerGo.AddComponent<InputManager>();
			controls.Init(players[i].prefix);
			var player = playerGo.GetComponent<Player>();
            player.Init(i, controls, players[i].color);
			Players.Add(player);
        }
    }

	void SpawnMines() {
		int trackNodesTotal = FindObjectsOfType<TrackNode> ().Length;
		int minesTotal = Players.Count * MaxMinesBeforeExplosion + Players.Count;
		//TODO: what will be if minesTotal > trackNodesTotal

		int maxTrackNodesBetweenMines = trackNodesTotal / minesTotal * Players.Count;
		int minTrackNodesBetweenMines = maxTrackNodesBetweenMines / 2;

		int lastTrackNodeIndexWithMine = 1; // fist spawned mine
		int minePositionOffset = 0;

		for (int i = 0; i < minesTotal; i += 1) {
			do {
				minePositionOffset = Random.Range (minTrackNodesBetweenMines, maxTrackNodesBetweenMines + 1);
			} while (minePositionOffset == 0);

			int position = lastTrackNodeIndexWithMine + minePositionOffset;
			TrackNode trackNode = _trackNodes [position - 1];
			lastTrackNodeIndexWithMine = position;

			GameObject mineGo = Instantiate(MinePrefab, trackNode.transform.position, Quaternion.identity, null);
			Mine mine = mineGo.GetComponent<Mine> ();
			mine.Spawn (new Vector2 (0, 0));
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

	Player GetFirstPlayer() {
        if (Players.Count == 0) {
            return null;
        }
		List<Player> firstPlayers = Players.OrderByDescending (item => item.waypointSum).ToList();
		if (FirstPlayer == null && !firstPlayers.Contains(FirstPlayer)) {
			FirstPlayer = firstPlayers.First ();
		}

		return FirstPlayer;
	}

    void Update() {
        Player leader = GetFirstPlayer();
        if (leader != null) {
            CamControl.Instance.player = GetFirstPlayer().transform;
        }
    }

    void OnPauseToggle(Event_Paused e) {
        PauseEnabled = !PauseEnabled;
    }

    void OnDestroy() {
        EventManager.Unsubscribe<Event_Paused>(OnPauseToggle);
        EventManager.Unsubscribe<Event_PlayerDead>(OnPlayerDead);
    }

    void OnPlayerDead(Event_PlayerDead e) {
        Players.Remove(e.Player);
        CamControl.Instance.GetComponentInChildren<CameraShake>().ShakeCamera(1, 0.7f);

        if (Players.Count <= 1) {
            Invoke("EndGame",1.5f);
        }
    }

    void EndGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("JoinScreen");
    }
}
