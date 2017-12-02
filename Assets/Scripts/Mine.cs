using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSys;

public class Mine : MonoBehaviour {

    public GameObject ExplosionFab = null;

    Rigidbody2D _rb = null;

    void Spawn(Vector2 speedVector) {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _rb.AddForce(speedVector, ForceMode2D.Impulse);
    }

    void Explode() {
        Instantiate(ExplosionFab, transform.position, Quaternion.identity);
        Destroy(this);
    }

    void Collect() {
        Destroy(this);
    }

    void OnCollisionEnter2D(Collision2D coll) {
        var player = coll.gameObject.GetComponent<Player>();

        if (player && player.CanAcceptMine) {
            EventManager.Fire<Event_PlayerMineCollect>(new Event_PlayerMineCollect() { playerIndex = player.Index });
            Collect();
        }
    }
}
