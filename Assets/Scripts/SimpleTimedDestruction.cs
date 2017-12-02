using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTimedDestruction : MonoBehaviour {
    public float Delay = 0f;

    private float _delay = 0f;

    void Start() {
        if (_delay == 0f) {
            Init(Delay);
        }
    }

    public void Init(float time) {
        _delay = time;
        if (_delay > 0) {
            Invoke("Destruct", _delay);
        }
    }

    void Destruct() {
        Destroy(gameObject);
    }
}
