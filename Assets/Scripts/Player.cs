using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;

public class Player : MonoBehaviour {

    public GameObject DeathPrefab = null;

    InputManager _input = null;

    Color _shipColor = Color.white;
    int   _playerIndex = 0;

    int _collectedMines = 0;

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

        } 
        _collectedMines++;
        
    }

    void Update() {

    }

    void FixedUpdate() {

    }

    void OnDestroy() {
        EventManager.Unsubscribe<Event_PlayerMineCollect>(OnMineCollect);
    }
}
