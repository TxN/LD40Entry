using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;

public class Mine : MonoBehaviour {

    const float LAUNCH_FORCE = 10f;

    public GameObject ExplosionFab = null;

    Rigidbody2D _rb = null;
    Collider2D _col = null;
    bool _mineEnabled = false;
    bool _appeared = false;

    public void Spawn(Vector2 speedVector) {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _rb.AddForce(speedVector * LAUNCH_FORCE, ForceMode2D.Impulse);
        _col = GetComponent<Collider2D>();
        _col.enabled = false;
        Invoke("EnableCollision", 0.1f);
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

    void EnableCollision() {
        _col.enabled = true;
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

    void OnBecameVisible() {
        _appeared = true;
    }

    void OnBecameInvisible() {
        Explode();
    }
}
