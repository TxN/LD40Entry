using UnityEngine;

public class InputTest : MonoBehaviour {

    void Update() {
        var joys = Input.GetJoystickNames();
        Debug.Log(joys.Length);
        foreach (var joy in joys) {
             Debug.Log(joy);
	    }
    }
}
