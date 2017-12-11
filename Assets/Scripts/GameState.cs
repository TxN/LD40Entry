using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EventSys;
using System.Linq;

public class GameState : MonoBehaviour {

    public static GameState Instance = null;

    public GameObject PlayerPrefab = null;
	public GameObject MinePrefab = null;
    public GameObject PauseMenu = null;
    public Text WinBanner = null;
    public TrackNode FirstTrackNode = null;

    public List<Transform> StartPoints = new List<Transform>();

    public List<Player> Players = new List<Player>();

    [HideInInspector]
    public TrackNode CurrentNode = null;

	[HideInInspector]
	public Player FirstPlayer = null;

	public IngameUI UIHolder = null;

	float _startTime = 0f;
	List<TrackNode> _trackNodes = new List<TrackNode>();
	Player _leader = null;
	int _maximumMines = 5;

	int _pauseSelection = 0;

	int _cachedLapCount = 1;
	int _lastLapNum = 1;

	bool _raceStarted = false;
	bool _pauseFlag = false;

	//-------------------------------
	//-------Public properties-------
	//-------------------------------

	public bool RaceStarted {
		get {
			return _raceStarted;
		}
	}

	public float TimeFromStart {
        get {
			if (!RaceStarted) {
				return 0;
			}
            return Time.time - _startTime;
        }
    }

    public int MaxMinesBeforeExplosion {
        get {
            return _maximumMines;
        }
        set {
            _maximumMines = value;
            EventManager.Fire(new Event_MaximumMinesCount_Change() {count = value });
        }
    }

    public bool PauseEnabled {
        get {
            return _pauseFlag;
        }
        set {
            _pauseFlag = value;
            Time.timeScale = _pauseFlag ? 0 : 1;
        }
    }

	public int LapCount {
		get {
			if (_leader == null) {
				return _lastLapNum;
			}
			var newLapNum = 1 + (_leader.waypointSum / _trackNodes.Count);
			if (newLapNum != _lastLapNum) {
				EventManager.Fire(new Event_LapPassed() { lap = _lastLapNum });
				_lastLapNum = newLapNum;
			}
			return _lastLapNum;
		}
	}

	//-------------------------------
	//------Unity mono callbacks-----
	//-------------------------------

	void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(this);
            return;
        }
    }

    void Start() {
		if ( UIHolder == null ) {
			UIHolder = FindObjectOfType<IngameUI>();
		}
		//Надо по-хорошему перестать юзать инвоки, инвоки зло, мало того что медленные, еще и читаемость кода убивают не хуже goto
		Invoke("StartRace", 3f);
		UIHolder.RaceCountdown.StartSequence();

        GetTrackNodes();
        CurrentNode = FirstTrackNode;
        var holder = FindObjectOfType<PlayerInfoHolder>();
        SpawnPlayers(holder.playersInfos);
		SpawnMines ();
		
        EventManager.Subscribe<Event_Paused>(this, OnPauseToggle);
        EventManager.Subscribe<Event_PlayerDead>(this, OnPlayerDead);
		EventManager.Subscribe<Event_ChangeSelectedPauseMenuItem>(this, OnMenuItemChanged);
		EventManager.Subscribe<Event_SelectPauseMenuItem> (this, OnMenuItemSelection);
    }

	void Update() {
		_cachedLapCount = LapCount;
		_leader = GetFirstPlayer();
		if (_leader != null) {
			CamControl.Instance.player = GetFirstPlayer().transform;
		}
	}

	void OnDestroy() {
		EventManager.Unsubscribe<Event_Paused>(OnPauseToggle);
		EventManager.Unsubscribe<Event_PlayerDead>(OnPlayerDead);
		EventManager.Unsubscribe<Event_ChangeSelectedPauseMenuItem>(OnMenuItemChanged);
		EventManager.Unsubscribe<Event_SelectPauseMenuItem>(OnMenuItemSelection);
		if (Instance == this) {
			Instance = null;
		}
	}

	//-------------------------------
	//--------Public methods---------
	//-------------------------------

	public void EndGame() {
		UnityEngine.SceneManagement.SceneManager.LoadScene("JoinScreen");
	}

	//-------------------------------
	//--------Private methods--------
	//-------------------------------

	void SpawnPlayers(List<PlayerInfo> players) {
        for (int i = 0; i < players.Count; i++) {
            GameObject playerGo = Instantiate(PlayerPrefab, StartPoints[i].position, Quaternion.identity, null);
			var controls = playerGo.AddComponent<InputManager>();
			controls.Init(players[i].playerNumber);
			var player = playerGo.GetComponent<Player>();
            player.Init(i, controls, players[i].color);
			Players.Add(player);
        }
    }

	void SpawnMines() {
		int trackNodesTotal = FindObjectsOfType<TrackNode> ().Length;
		int minesTotal = Players.Count * MaxMinesBeforeExplosion + Players.Count*3;
		//TODO: what will be if minesTotal > trackNodesTotal

		int maxTrackNodesBetweenMines = trackNodesTotal / minesTotal;
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
            mine.Spawn(new Vector2(0, 0), new Vector2(0, 0));
		}
	}

    Player CreatePlayer(int index, InputManager controls, Color color) {
        var go = Instantiate(PlayerPrefab, null, StartPoints[index]);
        var player = go.GetComponent<Player>();
        player.Init(index, controls, color);

        return player;
    }

	void StartRace() {
		_raceStarted = true;
		_startTime = Time.time;
		EventManager.Fire(new Event_ControlsLockState_Change() { ControlsEnabled = true });
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
		List<Player> orderedPlayers = Players.OrderByDescending (item => item.waypointSum).ToList();
		if (FirstPlayer == null) {
			FirstPlayer = orderedPlayers[0];
		} else {
			for (int i = 0; i < orderedPlayers.Count; i += 1) {
				if (orderedPlayers [i].waypointSum > FirstPlayer.waypointSum) {
					FirstPlayer = orderedPlayers[i];
					break;
				}

				if (orderedPlayers [i].waypointSum == FirstPlayer.waypointSum && orderedPlayers [i].Index != FirstPlayer.Index) {
					int nearedNotPassedNodeIndex = FirstPlayer.lastPassedWaypoint + 1;
					if (nearedNotPassedNodeIndex > _trackNodes.Count - 1) {
						nearedNotPassedNodeIndex = 1;
					}

					TrackNode nearestNotPassedNode = _trackNodes [nearedNotPassedNodeIndex];
					Vector2 nodePos = new Vector2 (nearestNotPassedNode.transform.position.x, nearestNotPassedNode.transform.position.y);
					Vector2 firstPlayerPos = new Vector2 (FirstPlayer.transform.position.x, FirstPlayer.transform.position.y);
					Vector2 candidatePlayerPos = new Vector2 (orderedPlayers [i].transform.position.x, orderedPlayers [i].transform.position.y);
					float magnitude1 = (nodePos - firstPlayerPos).magnitude;
					float magnitude2 = (nodePos - candidatePlayerPos).magnitude;

					if (magnitude1 > magnitude2) {
						FirstPlayer = orderedPlayers [i];
					}
				}

				if (orderedPlayers [i].waypointSum < FirstPlayer.waypointSum) {
					// No need to continue
					break;
				}
			}
		}
		return FirstPlayer;
	}

	void UpdateMenuColorSelection(string item) {
		List<string> items = new List<string>() { "Continue", "Restart", "Quit" };
		items.Remove(item);
		// Highlight item
		Color selectedColor = new Color(222, 0, 222);
		PauseMenu.transform.Find(item).GetComponent<Image>().color = selectedColor;
		PauseMenu.transform.Find(item).Find("Text").GetComponent<Text>().color = selectedColor;
		// Restore default color to others
		for (int i = 0; i < items.Count; i += 1) {
			PauseMenu.transform.Find(items[i]).GetComponent<Image>().color = Color.white;
			PauseMenu.transform.Find(items[i]).Find("Text").GetComponent<Text>().color = Color.white;
		}
	}

	//-------------------------------
	//---------Event handlers--------
	//-------------------------------

	void OnMenuItemChanged(Event_ChangeSelectedPauseMenuItem e) {
		int activeItem = _pauseSelection + e.offset;
		if (activeItem < 0) {
			activeItem = 0;
		} else if (activeItem > 2) {
			activeItem = 2;
		}
		_pauseSelection = activeItem;

		List<string> items = new List<string> () { "Continue", "Restart", "Quit" };
		UpdateMenuColorSelection (items[activeItem]);
	}

	void OnMenuItemSelection(Event_SelectPauseMenuItem e) {
		if (PauseEnabled) {
			switch (_pauseSelection) {
			case 0:
				OnPauseToggle ();
				break;
			case 1:
				OnPauseToggle ();
				EndGame ();
				break;
			case 2:
				OnPauseToggle ();
				Application.Quit ();
				break;
			}
		}
	}
	
	void OnPauseToggle(Event_Paused e = new Event_Paused()) {
        PauseEnabled = !PauseEnabled;
        PauseMenu.SetActive(PauseEnabled);
    }

    void OnPlayerDead(Event_PlayerDead e) {
        Players.Remove(e.Player);
        CamControl.Instance.GetComponentInChildren<CameraShake>().ShakeCamera(1, 0.7f);

        if (Players.Count <= 1) {
            if (Players.Count == 1 && WinBanner) {
                WinBanner.text = string.Format("PLAYER {0} WINS!", Players[0].Index + 1 );
                WinBanner.color = Players[0].PlayerColor;
                WinBanner.gameObject.SetActive(true);
            }
            Invoke("EndGame",1.5f);
        }
    }
}
