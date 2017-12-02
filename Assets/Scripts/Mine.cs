using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;

public class Mine : MonoBehaviour {

    public GameObject ExplosionFab = null;

    Rigidbody2D _rb = null;
    bool _mineEnabled = false;

    void Spawn(Vector2 speedVector) {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _rb.AddForce(speedVector, ForceMode2D.Impulse);
        Invoke("EnableMine", 0.2f);
    }

    void Explode() {
        Instantiate(ExplosionFab, transform.position, Quaternion.identity);
        Destroy(this);
    }

    void Collect() {
        Destroy(this);
    }

    void EnableMine() {
        _mineEnabled = true;
    }

    void OnCollisionEnter2D(Collision2D coll) {
        if (!_mineEnabled) {
            return;
        }

        var player = coll.gameObject.GetComponent<Player>();
        if (player && player.CanAcceptMine) {
            EventManager.Fire<Event_PlayerMineCollect>(new Event_PlayerMineCollect() { playerIndex = player.Index });
            Collect();
        }
    }
}
