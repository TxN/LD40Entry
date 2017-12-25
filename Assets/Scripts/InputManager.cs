using EventSys;
using UnityEngine;

using InputMng = TeamUtility.IO.InputManager;

public class InputManager : MonoBehaviour {
	const float ANGLE_PRECISION = 0.1f;
	const int PAUSE_MENU_FRAME_LOCK = 10;
	private int _playerNumber;

	public TeamUtility.IO.PlayerID playerId {
		get {
			return (TeamUtility.IO.PlayerID)System.Enum.GetValues(typeof(TeamUtility.IO.PlayerID)).GetValue(_playerNumber);
		}
	}

	private float _directionAngle = 0f;

	private int _pauseMenuActionLockedFrameNumber = 0;

	public void Init(int playerNumber) {
		_playerNumber = playerNumber;
	}

	public float GetDirection (){
		Vector2 vec = GetDirectionVector();
		if (IsZeroAngle (vec)) {
			return _directionAngle;
		}
		
		_directionAngle = Mathf.Atan2 (vec.x, vec.y) * Mathf.Rad2Deg;
		return _directionAngle;
	}

	public Vector2 GetDirectionVector () {
		return new Vector2(
			InputMng.GetAxis("Left Stick Horizontal", playerId),
			InputMng.GetAxis("Left Stick Vertical", playerId)
		);
	}

	public Vector2 GetLaunchDirection(){
		Vector2 vec = new Vector2(
			InputMng.GetAxis("Right Stick Horizontal", playerId),
			InputMng.GetAxis("Right Stick Vertical", playerId)
		);
        return vec.normalized;
	}

	public float GetMoveAcceleration (){
		if (InputMng.GetButton ("Right Trigger", playerId) || InputMng.GetAxis("Right Trigger", playerId) > 0.5f) {
			return 1f;
		}

		if (InputMng.GetButton ("Left Trigger", playerId)|| InputMng.GetAxis("Left Trigger", playerId) > 0.5f) {
			return -1f;
		}

		return 0f;
	}

	public bool GetLaunchTrigger(int typeOfLaunchedMine) {
		switch (typeOfLaunchedMine) {
			case Mine.MINE_TYPE_SIMPLE:
				return InputMng.GetButtonDown("Button A", playerId);
			case Mine.MINE_TYPE_DASH:
				return InputMng.GetButtonDown("Button B", playerId);
			case Mine.MINE_TYPE_SPEED:
				return InputMng.GetButtonDown("Button X", playerId);
			default:
				throw new System.Exception("Unknown type of launched mine: " + typeOfLaunchedMine);
		}
	}

    public bool GetDashTrigger() {
		return InputMng.GetButtonDown("Right Bumper", playerId);
    }

	void Update() {
		if (InputMng.GetButtonDown ("Back", playerId)) {
			EventManager.Fire (new Event_Paused());
		}

		GameState state = FindObjectOfType<GameState> ();
		if (state && state.PauseEnabled) {
			if (_pauseMenuActionLockedFrameNumber == 0) {
				var offst = GetPauseMenuDirection ();
				EventManager.Fire (new Event_ChangeSelectedPauseMenuItem () { offset = offst });
				_pauseMenuActionLockedFrameNumber = PAUSE_MENU_FRAME_LOCK;
			} else {
				_pauseMenuActionLockedFrameNumber -= 1;
			}
				
			if (InputMng.GetButtonDown("Button A", playerId)) {
				EventManager.Fire (new Event_SelectPauseMenuItem ());
			}
		}
	}

	int GetPauseMenuDirection() {
		int verticalAxisVal = (int)InputMng.GetAxis("Left Stick Vertical", playerId);
		return verticalAxisVal * -1;
	}

	protected bool IsZeroAngle(Vector2 vec) {
		return vec.magnitude - ANGLE_PRECISION <= 0;
	}
}
