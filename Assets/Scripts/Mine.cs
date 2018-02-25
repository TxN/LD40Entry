using UnityEngine;
using EventSys;

public class Mine : MonoBehaviour {

	public enum MineTypes{ Simple = 0, Speed = 1, Dash = 2 }
    const float LAUNCH_FORCE = 2f;

    public GameObject ExplosionFab = null;

	public MineTypes mineType = MineTypes.Simple;
	protected Color mineTypeColor = Color.grey;

    Rigidbody2D _rb = null;
    Collider2D _col = null;
    bool _mineEnabled = true;
    bool _appeared = false;
	bool _forceFlag = false;
	Vector2 _lastSpeedVector;
	int attackerIndex = -1;

	public static Color MineTypeToColor(MineTypes type) {
		switch (type) {
			case MineTypes.Simple:
				return Color.grey;
			case MineTypes.Speed:
				return Color.blue;
			case MineTypes.Dash:
				return Color.red;
			default:
				return Color.white;
		}
	}


	public void Spawn(Vector2 speedVector, Vector2 initSpeed, int attackerIndex = -1) {
		this.attackerIndex = attackerIndex;

		if (speedVector == Vector2.zero && initSpeed == Vector2.zero) {
			ChangeLayerToRestState();
		} else {
			ChangeLayerToMotionState();
		}

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

	public static System.Type GetTypeOfMineByIntCode(Mine.MineTypes type) {
		switch (type) {
			case MineTypes.Simple:
				return typeof(MineSimple);
			case MineTypes.Speed:
				return typeof(MineSpeed);
			case MineTypes.Dash:
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
		if (player) {
			if (player.CanAcceptMine && player._playerIndex != attackerIndex) {
				EventManager.Fire(new Event_PlayerMineCollect() { playerIndex = player.Index, mineType = mineType, attackerIndex = attackerIndex });
				Collect();
			}
		} else {
			// Collision with other object, non-player
			ChangeLayerToRestState();
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

	void ChangeLayerToMotionState() {
		gameObject.layer = 10;
	}

	void ChangeLayerToRestState() {
		gameObject.layer = 9;
		// reset attacker index
		attackerIndex = -1;
	}
}
