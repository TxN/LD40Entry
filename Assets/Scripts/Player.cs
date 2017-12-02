using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    bool _isAlive = true;

    public void Init(int index, InputManager controls, Color color) {
        _input = controls;
        _playerIndex = index;
        _shipColor = color;
        UpdateModelColor();
    }

    public void Kill() {
        if (!_isAlive) {
            return;
        }
        //Spawn death prefab and etc
    }

    void UpdateModelColor() {
        
    }

    void OnMineCollect() {

    }

    void OnDestroy() {

    }
}
