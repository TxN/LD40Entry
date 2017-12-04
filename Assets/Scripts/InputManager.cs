using EventSys;
using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {
	const float ANGLE_PRECISION = 0.1f;

	private string _directionAxisX;
	private string _directionAxisY;
	private string _launchAxisX;
	private string _launchAxisY;
	private string _moveTrigger;
	private string _backMoveTrigger;
	private string _launchTrigger;
	private string _pauseTrigger;
    private string _dashTrigger;

	private float _directionAngle = 0f;
	private float _launchAngle = 0f;

	public void Init(string prefix) {
		_directionAxisX = prefix + "_direction_x";
		_directionAxisY = prefix + "_direction_y";
		_launchAxisX = prefix + "_launch_x";
		_launchAxisY = prefix + "_launch_y";
		_moveTrigger = prefix + "_move";
		_backMoveTrigger = prefix + "_move_bk";
		_launchTrigger = prefix + "_launch";
		_pauseTrigger = prefix + "_pause";
        _dashTrigger = prefix + "_dash";
	}

	public float GetDirection (){
		Vector2 vec = new Vector2(
			Input.GetAxis(InputManager.GetKey(_directionAxisX)),
			Input.GetAxis(InputManager.GetKey(_directionAxisY))
		);

		if (IsZeroAngle (vec)) {
			return _directionAngle;
		}
		
		_directionAngle = Mathf.Atan2 (vec.x, vec.y) * Mathf.Rad2Deg;
		return _directionAngle;
	}

	public  Vector2 GetLaunchDirection(){
		Vector2 vec = new Vector2(
			Input.GetAxis(InputManager.GetKey(_launchAxisX)),
			Input.GetAxis(InputManager.GetKey(_launchAxisY))
		);
        return vec.normalized;
	}

	public float GetMoveAcceleration (){

		return Input.GetAxis(InputManager.GetKey(_moveTrigger));

        /*if (Input.GetButton (_moveTrigger)) {
			return 1f;
		} else if (Input.GetButton(_backMoveTrigger)) {
			return -1f;
		}

		return 0f;*/
	}

	public bool GetLaunchTrigger() {
		return Input.GetButtonDown (InputManager.GetKey(_launchTrigger));
	}

    public bool GetDashTrigger() {
        return Input.GetButtonDown(_dashTrigger);
    }

	void Update() {
		if (Input.GetButtonDown (InputManager.GetKey(_pauseTrigger))) {
			EventManager.Fire (new Event_Paused());
		}
	}

	protected bool IsZeroAngle(Vector2 vec) {
		return vec.magnitude - ANGLE_PRECISION <= 0;
	}

	public static string GetKey(string key) {
		if (key.StartsWith ("kb")) {
			return key;
		}

		string postfix = "_";
		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
			postfix += "osx";
		} else {
			postfix += "win";
		}
			
		return key + postfix;
	}
}
