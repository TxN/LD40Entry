using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputManager : MonoBehaviour {
	public abstract float GetDirection ();
	public abstract float GetLaunchDirection();
	public abstract float GetMoveAcceleration ();
	public abstract bool GetLaunchTrigger();
}
