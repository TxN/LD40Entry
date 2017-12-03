using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;

public class Player : MonoBehaviour {
    const float ROT_SMOOTH_COEF = 0.8f;
    const float MAX_ACCELERATION = 10f;
    const float MINE_DECC_PERCENT = 0.1f;
	const float MINE_LAUNCH_COOLDOWN = 3f;

	public GameObject BodyModel = null;
	public GameObject InternalsModel = null;

    public GameObject DeathPrefab = null;
    public GameObject MinePrefab = null;

    InputManager _input = null;

    Color _shipColor = Color.white;
    int   _playerIndex = 0;

    int _collectedMines = 0;

	float _lastMineLaunchTime = 0f;

    //=========
    //Movement
    //=========
    float _rotationAngle = 0;
    float _moveForce = 0;
    float _initMass = 1;
    Rigidbody2D _rb = null;

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
        _input = controls;
        _playerIndex = index;
        _shipColor = color;
     
        ColorSetter.UpdateModelColor(BodyModel, _shipColor);

        EventManager.Subscribe<Event_PlayerMineCollect>(this, OnMineCollect);
		EventManager.Subscribe<Event_MaximumMinesCount_Change>(this, OnMineMaxCountChange);
    }

    public void Kill() {
        if (!_isAlive) {
            return;
        }
        EventManager.Fire<Event_PlayerDead>(new Event_PlayerDead() { Player = this, PlayerIndex = _playerIndex });
        var deadObject = Instantiate(DeathPrefab, transform.position, Quaternion.identity);
		deadObject.SetActive(true);
		Destroy(gameObject);
        //Spawn death prefab and etc
    }


    void OnMineCollect(Event_PlayerMineCollect e) {
        if (e.playerIndex == _playerIndex) {
            _collectedMines++;
        }
		UpdateInternals();
    }

	void OnMineMaxCountChange(Event_MaximumMinesCount_Change e) {
		UpdateInternals();
	}


	void ProcessControls() {
        if (!_isAlive) {
            return;
        }
        _rotationAngle = _input.GetDirection();
        _moveForce = _input.GetMoveAcceleration();

        if (_input.GetLaunchTrigger() && _collectedMines > 0 ) {
            LaunchMine(_input.GetLaunchDirection());
        }
    }

	void UpdateInternals() {
		int maxMines = GameState.Instance.MaxMinesBeforeExplosion;
		float scale = 0.1f + 0.9f *( (float)_collectedMines/ (float) maxMines );
		InternalsModel.transform.localScale = new Vector3(scale, scale, scale);

		if ( _collectedMines > maxMines ) {
			Kill();
		} 
	}

    void LaunchMine(Vector2 direction) {
		if ( Time.time - _lastMineLaunchTime < MINE_LAUNCH_COOLDOWN ) {
			return;
		}
		_lastMineLaunchTime = Time.time;

        GameObject mineObj = Instantiate(MinePrefab, transform.position + (Vector3)direction*0.5f,Quaternion.identity);
        Mine mine = mineObj.GetComponent<Mine>();
        mine.Spawn(direction);

		_collectedMines--;
		UpdateInternals();
    }

    void CalcShipMass() {
        _rb.mass = _initMass + _initMass * (GameState.Instance.MaxMinesBeforeExplosion * MINE_DECC_PERCENT);
    }

    void Update() {
        ProcessControls();
        var tgAngle = Quaternion.Euler(0, 0, _rotationAngle);
		_rb.MoveRotation(_rotationAngle);
       // transform.rotation = Quaternion.Lerp(transform.rotation, tgAngle, ROT_SMOOTH_COEF);
    }

    void FixedUpdate() {
        if (Mathf.Abs(_moveForce) > 0.1f) {
            _rb.AddForce(transform.TransformDirection(Vector2.up) * MAX_ACCELERATION*_moveForce, ForceMode2D.Force);
        }
    }

    void OnDestroy() {
        EventManager.Unsubscribe<Event_PlayerMineCollect>(OnMineCollect);
    }

    void OnBecameInvisible() {
        Kill();
    }
}
