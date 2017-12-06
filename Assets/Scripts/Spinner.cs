using UnityEngine;

public class Spinner : MonoBehaviour
{
	public Vector3 EulersPerSecond;
	public bool RandomizeStartRotation = false;
	public bool RandomizeStartScale    = false;
	public bool RandomizeRotSpeed      = false;

	private void Start() {
		if (RandomizeStartRotation) {
			transform.rotation = Random.rotationUniform;
		}
		if ( RandomizeStartScale) {
			transform.localScale = Vector3.one * Random.Range(0.85f, 1.15f);
		}
		if ( RandomizeRotSpeed ) {
			EulersPerSecond = Random.Range(0.75f, 1.25f) * EulersPerSecond;
		}
	}

	void Update() {
		transform.Rotate(EulersPerSecond * Time.deltaTime);
    }
}
