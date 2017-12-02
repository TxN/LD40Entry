using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
	const float ANGLE_PRECISION = 0.1f;

	private string _directionAxisX;
	private string _directionAxisY;
	private string _launchAxisX;
	private string _launchAxisY;
	private string _moveTrigger;
	private string _backMoveTrigger; //TODO
	private string _launchTrigger;

	private float _directionAngle = 0f;
	private float _launchAngle = 0f;

	public void Init(string prefix)
	{
		_directionAxisX = prefix + "_direction_x";
		_directionAxisY = prefix + "_direction_y";
		_launchAxisX = prefix + "_launch_x";
		_launchAxisY = prefix + "_launch_y";
		_moveTrigger = prefix + "_move";
		_launchTrigger = prefix + "_launch";
	}

	public float GetDirection (){
		Vector2 vec = new Vector2(Input.GetAxis(_directionAxisX), Input.GetAxis(_directionAxisY));

		if (IsZeroAngle (vec)) {
			return _directionAngle;
		}

		_directionAngle = Mathf.Atan2 (vec.y, vec.x);
		return _directionAngle;
	}

	public  float GetLaunchDirection(){
		Vector2 vec = new Vector2(Input.GetAxis(_launchAxisX), Input.GetAxis(_launchAxisY));

		if (IsZeroAngle(vec)) {
			return _launchAngle;
		}

		_launchAngle = Mathf.Atan2 (vec.y, vec.x);
		return _launchAngle;
	}

	public float GetMoveAcceleration (){
		if (Input.GetKeyDown (_moveTrigger)) {
			return 1f;
		}

		return 0f;
	}

	public bool GetLaunchTrigger() {
		return Input.GetKeyDown (_launchTrigger);
	}

	void Update() {
		
	}

	protected bool IsZeroAngle(Vector2 vec) {
		return vec.magnitude - ANGLE_PRECISION <= 0;
	}
}
