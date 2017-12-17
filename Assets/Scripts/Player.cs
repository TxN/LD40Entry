using UnityEngine;
using EventSys;

public class Player : MonoBehaviour {
    const float ROT_SMOOTH_COEF = 0.8f;
    const float MAX_ACCELERATION = 5f;
    const float MINE_DECC_PERCENT = 0.2f;
	const float MINE_LAUNCH_COOLDOWN = 0.5f;
	const float DASH_COOLDOWN = 2f;
	const float MINE_LAUNCH_MIN_DISTANCE = 1.3f;
	const int WAYPOINT_VALUE = 1;


	public GameObject BodyModel = null;
	public GameObject InternalsModel = null;

    public GameObject DeathPrefab = null;
    public GameObject MinePrefab = null;

	public int waypointSum = 0;
	public int lastPassedWaypoint = 0;

    InputManager _input = null;

    public AudioSource PickupSource = null;

    Color _shipColor = Color.white;
    public Color PlayerColor {
        get {
            return _shipColor;
        }
    }
    int   _playerIndex = 0;

    int _collectedMines = 0;

	float _lastMineLaunchTime = 0f;
	float _lastDashUseTime = 0f;

    //=========
    //Movement
    //=========
    float _rotationAngle = 0;
    float _moveForce = 0;
    float _initMass = 1;
    Rigidbody2D _rb = null;
    float _initDrag = 0;

    bool _becameInvisibleFlag = false;

	bool _controlsEnabled = false;

    //DYNAMIC DRAG
	public bool dynamicDrag = true;
    public float dynamicDragMaxValue = 5f;
    public float dynamicDragSpeedMinValue = 0.5f;
    public float dynamicDragAngleK = 0.0001f;

    public bool Alive {
        get {
            return _isAlive;
        }
    }

    public bool CanAcceptMine {
        get {
			return _isAlive;
        }
    }

    public int Index {
        get {
            return _playerIndex;
        }
    }

    bool _isAlive = true;


    public void Init(int index, InputManager controls, Color color) {
        _rb = GetComponent<Rigidbody2D>();
        _initMass = _rb.mass;
        _initDrag = _rb.drag;
        _input = controls;
        _playerIndex = index;
        _shipColor = color;
     
        ColorSetter.UpdateModelColor(BodyModel, _shipColor);

        EventManager.Subscribe<Event_PlayerMineCollect>(this, OnMineCollect);
		EventManager.Subscribe<Event_MaximumMinesCount_Change>(this, OnMineMaxCountChange);
		EventManager.Subscribe<Event_ControlsLockState_Change>(this, OnControlsLockStateChange);
    }

	[ContextMenu("DIE!")]
    public void Kill() {
        if (!_isAlive) {
            return;
        }
        EventManager.Fire<Event_PlayerDead>(new Event_PlayerDead() { Player = this, PlayerIndex = _playerIndex });
        var deadObject = Instantiate(DeathPrefab, transform.position, Quaternion.identity);
		deadObject.SetActive(true);
		var parts = deadObject.GetComponent<ParticleSystem>().main;
		parts.startColor = _shipColor;
		Destroy(gameObject);
    }


    void OnMineCollect(Event_PlayerMineCollect e) {
        if (e.playerIndex == _playerIndex) {
            _collectedMines++;
        }
        PickupSource.Play();
		UpdateInternals();
    }

	void OnMineMaxCountChange(Event_MaximumMinesCount_Change e) {
		UpdateInternals();
	}

	void OnControlsLockStateChange(Event_ControlsLockState_Change e) {
		_controlsEnabled = e.ControlsEnabled;
	}


	void ProcessControls() {
        if (!_isAlive || !_controlsEnabled) {
            return;
        }
        _rotationAngle = _input.GetDirection();
        _moveForce = _input.GetMoveAcceleration();

        if (_input.GetLaunchTrigger() && _collectedMines > 0 ) {
            LaunchMine(-_input.GetLaunchDirection());
        }

		if (_input.GetDashTrigger() && Time.time - _lastDashUseTime >= DASH_COOLDOWN) {
			_rb.AddForce(transform.TransformDirection(Vector2.up) * MAX_ACCELERATION * 2f, ForceMode2D.Impulse);
			_lastDashUseTime = Time.time;
        }
    }

	void UpdateInternals() {
		int maxMines = GameState.Instance.MaxMinesBeforeExplosion;
		float scale = 0.1f + 0.9f *( (float)_collectedMines / (float) maxMines );
		InternalsModel.transform.localScale = new Vector3(scale, scale, scale);
        CalcShipMass();
		if ( _collectedMines > maxMines ) {
			Kill();
		} 
	}

    void LaunchMine(Vector2 direction) {
		if ( Time.time - _lastMineLaunchTime < MINE_LAUNCH_COOLDOWN ) {
			return;
		}

		RaycastHit2D[] hits = Physics2D.RaycastAll (transform.position, (Vector2)direction, MINE_LAUNCH_MIN_DISTANCE);
		for (int i = 0; i < hits.Length; i += 1) {
			if (hits [i].collider.GetType () == typeof(EdgeCollider2D)) {
				return;
			}
		}

		if (direction.magnitude == 0) {
			return;
		}

		_lastMineLaunchTime = Time.time;

        GameObject mineObj = Instantiate(MinePrefab, transform.position + (Vector3)direction*0.75f,Quaternion.identity);
        Mine mine = mineObj.GetComponent<Mine>();
        mine.Spawn(direction,_rb.velocity);

		_collectedMines--;
		UpdateInternals();
        GetComponent<AudioSource>().Play();
    }

    void CalcShipMass() {
        _rb.mass = _initMass + _initMass * ((float)_collectedMines/GameState.Instance.MaxMinesBeforeExplosion * MINE_DECC_PERCENT);
        _rb.drag = _initDrag + _initDrag * ((float)_collectedMines / GameState.Instance.MaxMinesBeforeExplosion * MINE_DECC_PERCENT);
    }


    void Update() {
        ProcessControls();
        var tgAngle = Quaternion.Euler(0, 0, _rotationAngle);
		_rb.MoveRotation(_rotationAngle);

		if (dynamicDrag)
		{
			DynamicDragUpdate();
		}

        if (_becameInvisibleFlag) {
            Kill();
            Debug.Log("Death " + _playerIndex);
        }
    }

    void FixedUpdate() {
        if (Mathf.Abs(_moveForce) > 0.1f) {
            _rb.AddForce(transform.TransformDirection(Vector2.up) * MAX_ACCELERATION*_moveForce, ForceMode2D.Force);
        }
    }

    void OnDestroy() {
        EventManager.Unsubscribe<Event_PlayerMineCollect>(OnMineCollect);
        EventManager.Unsubscribe<Event_MaximumMinesCount_Change>(OnMineMaxCountChange);
		EventManager.Unsubscribe<Event_ControlsLockState_Change>(OnControlsLockStateChange);
    }

    void OnBecameInvisible() {
        if (this == null) {
            return;
        }
        _becameInvisibleFlag = true;
    }

	void OnTriggerEnter2D(Collider2D collider) {
		TrackNode trackNode = collider.GetComponent<TrackNode>();
		if (trackNode) {
			int trackNodeIndex = trackNode.GetIndex ();
			// If Player moved to the next waypoint of passed lap-start position
			if (lastPassedWaypoint < trackNodeIndex || (lastPassedWaypoint > trackNodeIndex && trackNodeIndex == 1)) {
				waypointSum += WAYPOINT_VALUE;
				lastPassedWaypoint = trackNodeIndex;
			}
		}
	}
	void DynamicDragUpdate()
	{ 
		Vector2 sp = _rb.velocity;
        if (sp.magnitude < dynamicDragSpeedMinValue) { return; }
		float speed_angle = Mathf.Atan2(sp.y, -sp.x) * Mathf.Rad2Deg;
        float rot_ang_val = _rotationAngle;
        if (rot_ang_val >= -90 && rot_ang_val <= 90) { rot_ang_val = 90 - rot_ang_val; }
        else if (rot_ang_val >= 90 && rot_ang_val <= 180) { rot_ang_val = 90 - rot_ang_val; }
        else  { rot_ang_val = -270 - rot_ang_val; } 
		float diff = Mathf.Abs(speed_angle - rot_ang_val);
        if (diff > 180) diff = 360 - diff; 
		//_rb.drag = Mathf.Clamp(_initDrag + (diff * 1) * sp.magnitude * 0.001f, _initDrag, dynamicDragMaxValue);
		_rb.drag = Mathf.Clamp(_initDrag + (diff * diff * dynamicDragAngleK), _initDrag, dynamicDragMaxValue);
	}
}
