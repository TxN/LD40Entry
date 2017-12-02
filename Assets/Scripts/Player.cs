using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;

public class Player : MonoBehaviour {
    const float ROT_SMOOTH_COEF = 0.8f;
    const float MAX_ACCELERATION = 10f;
    const float MINE_DECC_PERCENT = 0.1f;

    public GameObject DeathPrefab = null;
    public GameObject MinePrefab = null;

    InputManager _input = null;

    Color _shipColor = Color.white;
    int   _playerIndex = 0;

    int _collectedMines = 0;

    //====
    //Movement
    //====
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
        UpdateModelColor();

        EventManager.Subscribe<Event_PlayerMineCollect>(this, OnMineCollect);
    }

    public void Kill() {
        if (!_isAlive) {
            return;
        }
        //Spawn death prefab and etc
    }

    void UpdateModelColor() {
        
    }

    void OnMineCollect(Event_PlayerMineCollect e) {
        if (e.playerIndex == _playerIndex) {
            _collectedMines++;
        } 
    }

    void ProcessControls() {
        if (!_isAlive) {
            return;
        }
        _rotationAngle = _input.GetDirection();
        _moveForce = _input.GetMoveAcceleration();

        if (_input.GetLaunchTrigger() && _collectedMines > 0 ) {
            LaunchMine();
        }
    }

    void LaunchMine() {

    }

    void CalcShipMass() {
        _rb.mass = _initMass + _initMass * (GameState.Instance.MaxMinesBeforeExplosion * MINE_DECC_PERCENT);
    }

    void Update() {
        ProcessControls();
        var tgAngle = Quaternion.Euler(0, 0, _rotationAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, tgAngle, ROT_SMOOTH_COEF);
    }

    void FixedUpdate() {
        if (Mathf.Abs(_moveForce) > 0.1f) {
            _rb.AddForce(transform.TransformDirection(Vector2.up) * MAX_ACCELERATION, ForceMode2D.Force);
        }
    }

    void OnDestroy() {
        EventManager.Unsubscribe<Event_PlayerMineCollect>(OnMineCollect);
    }

    void OnBecameInvisible() {
        Kill();
    }
}
