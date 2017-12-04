﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;

public class Mine : MonoBehaviour {

    const float LAUNCH_FORCE = 2f;

    public GameObject ExplosionFab = null;

    Rigidbody2D _rb = null;
    Collider2D _col = null;
    bool _mineEnabled = true;
    bool _appeared = false;
	bool _forceFlag = false;
	Vector2 _lastSpeedVector;

    public void Spawn(Vector2 speedVector, Vector2 initSpeed) {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _rb.velocity = initSpeed + speedVector * LAUNCH_FORCE;
       // _rb.AddForce(speedVector * LAUNCH_FORCE, ForceMode2D.Impulse);
        _col = GetComponent<Collider2D>();
        _col.enabled = false;
        Invoke("EnableCollision", 0.05f);

		_forceFlag = true;
		_lastSpeedVector = new Vector2(speedVector.x, speedVector.y);
		Invoke("DisableForce", 0.15f);

        //Invoke("EnableMine", 0.05f);
    }

	void Update()
	{ 
		if (_forceFlag)
		{
			_rb.AddForce(_lastSpeedVector, ForceMode2D.Impulse);
		}
	}

	void DisableForce()
	{
		_forceFlag = false;
	}

	void Explode() {
        Instantiate(ExplosionFab, transform.position, Quaternion.identity);
		Destroy(this.gameObject);
    }

    void Collect() {
		Destroy(this.gameObject);
    }

    void EnableMine() {
        _mineEnabled = true;
    }

    void EnableCollision() {
        _col.enabled = true;
    }

    void OnCollisionEnter2D(Collision2D coll) {
        if (!_mineEnabled) {
            return;
        }

        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = 0;
        
        var player = coll.gameObject.GetComponent<Player>();
        if (!player) {
            player = coll.otherCollider.gameObject.GetComponent<Player>();
        }
        if (player && player.CanAcceptMine) {
            EventManager.Fire<Event_PlayerMineCollect>(new Event_PlayerMineCollect() { playerIndex = player.Index });
            Collect();
        }
    }

    void OnBecameVisible() {
        _appeared = true;
    }

    void OnBecameInvisible() {
		if (_appeared) {
			Explode();
		}
       
    }
}
