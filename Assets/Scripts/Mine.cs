using UnityEngine;
using EventSys;

public class Mine : MonoBehaviour {

	public const int MINE_TYPE_SIMPLE = 0;
	public const int MINE_TYPE_SPEED = 1;
	public const int MINE_TYPE_DASH = 2;

    const float LAUNCH_FORCE = 2f;

    public GameObject ExplosionFab = null;

	public int mineType = MINE_TYPE_SIMPLE;
	protected Color mineTypeColor = Color.grey;

    Rigidbody2D _rb = null;
    Collider2D _col = null;
    bool _mineEnabled = true;
    bool _appeared = false;
	bool _forceFlag = false;
	Vector2 _lastSpeedVector;

    public void Spawn(Vector2 speedVector, Vector2 initSpeed) {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _rb.velocity = initSpeed + speedVector * LAUNCH_FORCE;
        _rb.AddForce(speedVector * LAUNCH_FORCE, ForceMode2D.Impulse);
        _col = GetComponent<Collider2D>();
        _col.enabled = false;
        Invoke("EnableCollision", 0.05f);

		_forceFlag = true;
		_lastSpeedVector = speedVector * 0.5f;
		Invoke("DisableForce", 0.15f);

		ColorSetter.UpdateModelColor(gameObject, mineTypeColor);
    }

	public static System.Type GetTypeOfMineByIntCode(int type) {
		switch (type) {
			case MINE_TYPE_SIMPLE:
				return typeof(MineSimple);
			case MINE_TYPE_SPEED:
				return typeof(MineSpeed);
			case MINE_TYPE_DASH:
				return typeof(MineDash);
			default:
				return typeof(MineSimple);
		}
	}

	void Update() { 
		if (_forceFlag) {
			_rb.AddForce(_lastSpeedVector, ForceMode2D.Force);
		}
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
			EventManager.Fire(new Event_PlayerMineCollect() { playerIndex = player.Index, mineType = mineType });
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

	void DisableForce() {
		_forceFlag = false;
	}

	void Explode() {
		// TODO - return commented line ?
        // Instantiate(ExplosionFab, transform.position, Quaternion.identity);
		Destroy(gameObject);
    }

    void Collect() {
		Destroy(gameObject);
    }

    void EnableMine() {
        _mineEnabled = true;
    }

    void EnableCollision() {
        _col.enabled = true;
    }
}
