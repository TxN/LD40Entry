using UnityEngine;
using EventSys;
using System;
using System.Collections.Generic;

public class Player : MonoBehaviour {
    const float ROT_SMOOTH_COEF = 0.8f;
    const float MAX_ACCELERATION = 5f;
    const float MINE_DECC_PERCENT = 0.2f;
	const float MINE_LAUNCH_COOLDOWN = 0.5f;
	const float DASH_COOLDOWN = 2f;
	const float MINE_LAUNCH_MIN_DISTANCE = 1.3f;
	const int WAYPOINT_VALUE = 1;
    const int INITIAL_NUMBER_OF_DASHES = 6;

	public List<GameObject> MineSlots = new List<GameObject>();

	public GameObject BodyModel = null;
	public GameObject InternalsModel = null;
	public GameObject DashIndicator = null;

    public GameObject SpeedIndicator = null;
    public GameObject DeathPrefab = null;
    public GameObject MinePrefab = null;

	public int waypointSum = 0;
	public int lastPassedWaypoint = 0;

    InputManager _input = null;

    public AudioSource PickupSource = null;

	public AnimationCurve ThrustCurve = null;

    Color _shipColor = Color.white;
    public Color PlayerColor {
        get {
            return _shipColor;
        }
    }
    int   _playerIndex = 0;

    //TODO should not be public
    public CollectedMines _collectedMines = new CollectedMines();

	float _lastMineLaunchTime = 0f;
	float _lastDashUseTime = 0f;

    float _lashDashIncreasmentStartingTime = 0f;

    int _dashNumberAvailable = INITIAL_NUMBER_OF_DASHES;

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
    public float dynamicDragAngleK = 4f;

    public class CollectedMines {
        public List<Mine.MineTypes> Mines = new List<Mine.MineTypes>();

        public void Add(Mine.MineTypes mineType) {
            Mines.Add(mineType);
        }

        public void Remove(Mine.MineTypes mineType) {
            Mines.RemoveAt( Mines.FindIndex( x => x == mineType ) );
        }

        public void Clear() {
            Mines.Clear();
        }

        public int Count() {
            return Mines.Count;
        }

        public bool Exists(Mine.MineTypes mineType) {
            return Mines.Exists(x => x == mineType);
        }

        /* Return number of mines that increase speed */
        public int GetSpeedIncreaseRate() {
			List<Mine.MineTypes> speedMines = Mines.FindAll(x => x == Mine.MineTypes.Speed);
            if (speedMines.Count == 0 || speedMines.Count == Mines.Count) {
                return speedMines.Count;
            }

            return 0;
        }

        /* Returns number of mines that decrease speed */
        public int GetSpeedDecreaseRate() {
			if (Mines.FindAll(x => x == Mine.MineTypes.Simple).Count > 0 // || // if simple mines exist
               // (GetSpeedIncreaseRate() > 0 && GetDashIncreaseRate() > 0) // or if different mine-types mixed
            ) {
                return Mines.Count;
            }

            return 0;
        }

        /* Returns number of mines that increase dash */
        public int GetDashIncreaseRate() {
			List<Mine.MineTypes> dashMines = Mines.FindAll(x => x == Mine.MineTypes.Dash);
            if (dashMines.Count == 0 || dashMines.Count == Mines.Count) {
                return dashMines.Count;
            }

            return 0;
        }

        public Mine.MineTypes GetTypeOfMostCommonMines() {
            int speedMines = GetSpeedIncreaseRate();
            int dashMines = GetDashIncreaseRate();
            int simpleMines = GetSpeedDecreaseRate();

            int max = Math.Max(speedMines, Math.Max(dashMines, simpleMines));

            if (max == speedMines) {
                return Mine.MineTypes.Speed;
            } else if (max == dashMines) {
                return Mine.MineTypes.Dash;
            } else {
                // NO common mines TODO
                return Mine.MineTypes.Simple;
            }
        }
    }

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

        SpeedIndicator =  Instantiate(SpeedIndicator, new Vector3(0,0,0), Quaternion.identity, null);
        SpeedIndicator.transform.parent = gameObject.transform;
        SpeedIndicator.transform.localPosition = new Vector3(0,-0.3f,0);

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
            if (e.mineType == Mine.MineTypes.Simple) {
                // calculate mine to swap; TODO: it must be in another place (GameState or another mediator)
                if (e.attackerIndex > -1) {
                    Player attacker = GameState.Instance.Players[e.attackerIndex];//(Player)FindObjectsOfType(typeof(Player))[e.attackerIndex];
                    Mine.MineTypes commonMineTypeToSteal = attacker._collectedMines.GetTypeOfMostCommonMines();

                    //Incorrectness here, always speed swap used
                    if (commonMineTypeToSteal == Mine.MineTypes.Simple) {
                        if (_collectedMines.Exists(Mine.MineTypes.Speed)) {
                            Debug.Log("Swap speed!");
                            // _collectedMines.Remove(Mine.MineTypes.Speed);
                            // EventManager.Fire(new Event_PlayerMineCollect() {
                            //     playerIndex = attacker.Index, mineType = Mine.MineTypes.Speed, attackerIndex = -1 
                            // });
                            _SwapMine(Mine.MineTypes.Speed, attacker);
                        } else if (_collectedMines.Exists(Mine.MineTypes.Dash)) {
                            _SwapMine(Mine.MineTypes.Dash, attacker);
                            Debug.Log("Swap dash!");
                        }
                    } else if (commonMineTypeToSteal == Mine.MineTypes.Speed) {
                        if (_collectedMines.Exists(Mine.MineTypes.Speed)) {
                            _SwapMine(Mine.MineTypes.Speed, attacker);
                            Debug.Log("Swap speed!");
                        } else if (_collectedMines.Exists(Mine.MineTypes.Dash)) {
                            _SwapMine(Mine.MineTypes.Dash, attacker);
                            Debug.Log("Swap dash!");
                        }
                    } else {
                        if (_collectedMines.Exists(Mine.MineTypes.Dash)) {
                            _SwapMine(Mine.MineTypes.Dash, attacker);
                            Debug.Log("Swap dash!");
                        } else if (_collectedMines.Exists(Mine.MineTypes.Speed)) {
                            _SwapMine(Mine.MineTypes.Speed, attacker);
                            Debug.Log("Swap speed!");
                        }
                    }
                }
                // Send mine to attacker
                // Add simple mine
            }
            _collectedMines.Add(e.mineType);
        }
        PickupSource.Play();
		UpdateInternals();
    }

    void _SwapMine(Mine.MineTypes type, Player attacker) {
        _collectedMines.Remove(type);
        EventManager.Fire(new Event_PlayerMineCollect() {
            playerIndex = attacker.Index, mineType = type, attackerIndex = -1 
        });
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
        int speedIncreaseRate = _collectedMines.GetSpeedIncreaseRate();
        if (_moveForce > 0) {
            _moveForce += (float)Math.Pow(speedIncreaseRate, 2) * 0.03f;
        }

		foreach (Mine.MineTypes mineType in Enum.GetValues(typeof(Mine.MineTypes))) {
            if (_input.GetLaunchTrigger((int) mineType) && _collectedMines.Exists(mineType) ) {
                Vector2 dirVect = _input.GetDirectionVector();
                LaunchMine(new Vector2(-dirVect.x, dirVect.y), mineType);
            }
        }

		if ( _input.GetDashTrigger() && CanDash ) {
			_rb.AddForce(transform.TransformDirection(Vector2.up) * MAX_ACCELERATION, ForceMode2D.Impulse);
			_lastDashUseTime = Time.time;
            _dashNumberAvailable -= 1;
        }
    }

	public bool CanDash {
		get {
			return Time.time - _lastDashUseTime >= DASH_COOLDOWN && _dashNumberAvailable > 0;
		}
	}

	void UpdateInternals() {
		//_collectedMines
		int maxMines = GameState.Instance.MaxMinesBeforeExplosion;
		//float scale = 0.1f + 0.9f *( (float)_collectedMines.Count() / (float) maxMines );
		//InternalsModel.transform.localScale = new Vector3(scale, scale, scale);
			
		foreach (var slot in MineSlots) {
			slot.SetActive(false);
		}

		int slotIndex = 0;
		foreach (var mine in _collectedMines.Mines) {
			MineSlots[slotIndex].SetActive(true);

            Color mineColor = Mine.MineTypeToColor(mine);
            if (_lashDashIncreasmentStartingTime > 0f) {
                // It is indicator of full-dash collected
                mineColor = Color.magenta;
            }
			ColorSetter.UpdateModelColor(MineSlots[slotIndex], mineColor);
			slotIndex++;
			slotIndex = Mathf.Clamp(slotIndex, 0, MineSlots.Count - 1);
		}
        CalcShipMass();
		if ( _collectedMines.Count() > maxMines ) {
			Kill();
		}
        ActualizeLayer();
	}

    void LaunchMine(Vector2 direction, Mine.MineTypes mineType) {
        if (!_collectedMines.Exists(mineType)) {
            return;
        }

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
        System.Type typeOfMine = Mine.GetTypeOfMineByIntCode(mineType);
        mineObj.AddComponent(typeOfMine);

        Mine mine = mineObj.GetComponent(typeOfMine) as Mine;
        mine.Spawn(direction,_rb.velocity, _playerIndex);

		_collectedMines.Remove(mineType);
		UpdateInternals();
        GetComponent<AudioSource>().Play();
    }

    void CalcShipMass() {
        // TODO: Should we decrease speed too ?!
        int numberOfMinesThatDecreaseSpeed = _collectedMines.GetSpeedDecreaseRate();
        _rb.mass = _initMass + _initMass * ((float)numberOfMinesThatDecreaseSpeed / GameState.Instance.MaxMinesBeforeExplosion * MINE_DECC_PERCENT);
        _rb.drag = _initDrag + _initDrag * ((float)numberOfMinesThatDecreaseSpeed / GameState.Instance.MaxMinesBeforeExplosion * MINE_DECC_PERCENT);
    }


    void Update() {
        ProcessControls();
        var tgAngle = Quaternion.Euler(0, 0, _rotationAngle);
		_rb.MoveRotation(_rotationAngle);

		if (dynamicDrag)
		{
			//DynamicDragUpdate();
		}

        if (_becameInvisibleFlag) {
            Kill();
            Debug.Log("Death " + _playerIndex);
        }

		DashIndicator.SetActive(CanDash);

        if (_collectedMines.Count() == GameState.Instance.MaxMinesBeforeExplosion
                && _collectedMines.GetSpeedIncreaseRate() == GameState.Instance.MaxMinesBeforeExplosion
        ) {
            SpeedIndicator.SetActive(true);
        } else {
            SpeedIndicator.SetActive(false);
        }
        
        if (_collectedMines.Count() == GameState.Instance.MaxMinesBeforeExplosion
            && _collectedMines.GetDashIncreaseRate() == GameState.Instance.MaxMinesBeforeExplosion
            && _lashDashIncreasmentStartingTime <= 0f
        ) {

            _lashDashIncreasmentStartingTime = Time.time;
            UpdateInternals();
        }

        if (_lashDashIncreasmentStartingTime > 0f && Time.time - _lashDashIncreasmentStartingTime >= DASH_COOLDOWN * 1.5f) {
            _dashNumberAvailable += 1;
            _collectedMines.Clear();
            _lashDashIncreasmentStartingTime = 0f;
            UpdateInternals();
        }
    }

    void FixedUpdate() {
        if (Mathf.Abs(_moveForce) > 0.1f) {
            UpdateTurnForce();
			float collinearCoef = Mathf.Abs(Vector3.Dot(_rb.velocity.normalized, transform.TransformDirection(Vector2.up)));
			_rb.AddForce(transform.TransformDirection(Vector2.up) * MAX_ACCELERATION*_moveForce * ThrustCurve.Evaluate(_rb.velocity.magnitude* collinearCoef), ForceMode2D.Force);
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
            // TODO: here is small bug - if player will go back on start and then pass start - condition will be met. Seems like not-real scenario.
			if (lastPassedWaypoint < trackNodeIndex || (lastPassedWaypoint > trackNodeIndex && trackNodeIndex == 1)) {
                // if start passed
                if (lastPassedWaypoint > trackNodeIndex && trackNodeIndex == 1) {
                    if (_dashNumberAvailable < INITIAL_NUMBER_OF_DASHES) {
                        _dashNumberAvailable = INITIAL_NUMBER_OF_DASHES;
                    }
                }

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
    Vector2 GetForceDirection()
    {
		Vector2 sp = _rb.velocity;
        if (sp.magnitude < dynamicDragSpeedMinValue) { return Vector2.up; }
		float speed_angle = Mathf.Atan2(sp.y, -sp.x) * Mathf.Rad2Deg;
        if (speed_angle < 0) speed_angle = 360 + speed_angle;
        float rot_ang_val = _rotationAngle;
        if (rot_ang_val >= -90 && rot_ang_val <= 90) { rot_ang_val = 90 - rot_ang_val; }
        else if (rot_ang_val >= 90 && rot_ang_val <= 180) { rot_ang_val = 90 - rot_ang_val; }
        else  { rot_ang_val = -270 - rot_ang_val; }
        if (rot_ang_val < 0) rot_ang_val = 360 + rot_ang_val;
		float diff = rot_ang_val - speed_angle;
        float beta = diff;
        Mathf.Clamp(beta, -90f, 90f);
        Debug.Log(beta);
        
        //_rb.velocity = new Vector2(new_x, new_y); 
        Vector2 localUp = transform.TransformDirection(Vector2.up);

        float new_x = -(Mathf.Sin(beta * Mathf.Deg2Rad));
        float new_y = Mathf.Cos(beta * Mathf.Deg2Rad);
        //adjustedForceDirection = new Vector2(new_x, new_y);
        //Debug.Log(adjustedForceDirection);
        Debug.Log("new_x:" + new_x + "    new_y:" + new_y); 
        //return new Vector2(new_x, new_y);
        return Vector2.up; 
    }
    void UpdateTurnForce() {
        Vector2 sp = _rb.velocity;
        if (sp.magnitude < dynamicDragSpeedMinValue) { return; }
        float speed_angle = Mathf.Atan2(sp.y, -sp.x) * Mathf.Rad2Deg;
        if (speed_angle < 0) speed_angle = 360 + speed_angle;
        float rot_ang_val = _rotationAngle;
        if (rot_ang_val >= -90 && rot_ang_val <= 90) { rot_ang_val = 90 - rot_ang_val; } else if (rot_ang_val >= 90 && rot_ang_val <= 180) { rot_ang_val = 90 - rot_ang_val; } else { rot_ang_val = -270 - rot_ang_val; }
        if (rot_ang_val < 0) rot_ang_val = 360 + rot_ang_val;
        float diff = rot_ang_val - speed_angle;
        float beta = diff;
        //Mathf.Clamp(beta, -90f, 90f);
        //Debug.Log(beta);
        beta = Mathf.Sin(beta * Mathf.Deg2Rad);

        float amplitude = dynamicDragAngleK * beta * 100000;
        //Debug.Log(amplitude);
        Vector2 correction_force = new Vector2(amplitude, 0);
        
        _rb.AddForce(transform.TransformDirection(correction_force), ForceMode2D.Force);
        //_rb.AddForce(transform.TransformDirection(Vector2.up) * MAX_ACCELERATION*_moveForce, ForceMode2D.Force);

        //_rb.velocity = new Vector2(new_x, new_y); 
        //Vector2 localUp = transform.TransformDirection(Vector2.up);

        //float new_x = -(Mathf.Sin(beta * Mathf.Deg2Rad));
        //float new_y = Mathf.Cos(beta * Mathf.Deg2Rad);
        //adjustedForceDirection = new Vector2(new_x, new_y);
        //Debug.Log(adjustedForceDirection);
        //Debug.Log("new_x:" + new_x + "    new_y:" + new_y);
        //return new Vector2(new_x, new_y);
 
    }

    void ChangeLayerToMineAcceptableState() {
		gameObject.layer = 0;
	}

    void ChangeLayerToRestMineSkippingState() {
        gameObject.layer = 8;
    }

    void ActualizeLayer() {
        if (_collectedMines.Count() < GameState.Instance.MaxMinesBeforeExplosion) {
            ChangeLayerToMineAcceptableState();
        } else {
            ChangeLayerToRestMineSkippingState();
        }
    }
}
