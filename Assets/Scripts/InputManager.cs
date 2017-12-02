using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
	private string _directionAxisX;
	private string _directionAxisY;
	private string _launchAxisX;
	private string _launchAxisY;

	public void Init(string prefix)
	{
		_directionAxisX = prefix + "_direction_x";
		_directionAxisY = prefix + "_direction_y";
		_launchAxisX = prefix + "_launch_x";
		_launchAxisY = prefix + "_launch_y";
	}

	public float GetDirection (){
		Vector2 vec = new Vector2(Input.GetAxis(_directionAxisX), Input.GetAxis(_directionAxisY));
		return Mathf.Atan2 (vec.y, vec.x);
	}
	public  float GetLaunchDirection(){return 0f;}
	public  float GetMoveAcceleration (){return 0f;}
	public  bool GetLaunchTrigger() {
		return false;
	}
}
