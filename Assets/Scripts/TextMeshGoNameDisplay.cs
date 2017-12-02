using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMeshGoNameDisplay : MonoBehaviour {
	TextMesh _tm = null;
	
	void Start () {
		_tm = GetComponent<TextMesh>();
	}
	
	
	void Update () {
		if (_tm.text != transform.parent.gameObject.name) {
			_tm.text = transform.parent.gameObject.name;
		}
	}
}
