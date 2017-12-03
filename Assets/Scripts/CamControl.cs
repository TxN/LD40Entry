using UnityEngine;
using System.Collections;

public class CamControl : MonoBehaviour {

    public static CamControl Instance;

    public AnimationCurve lerpCoef;
    public Transform player;

    public float initDelta = 10f;

    public float MinFOV = 45;
    public float MaxFOV = 65;

    Camera _camera;

    float _prevFov = 0;

    float initZ = 0;
    float moveError;

    void Awake() {
        Instance = this;
    }

	void Start () {
        initZ = transform.position.z;
        _camera = GetComponent<Camera>();
        _prevFov = MinFOV;
	}

    void Update() {
        //float newFOV = Map(_state.GetShipVelocity, 0, 15, MinFOV, MaxFOV, true);
        float newFOV = 20;
        _prevFov = Mathf.Lerp(_prevFov, newFOV, 2 * Time.deltaTime);
        //_camera.fieldOfView = _prevFov;
    }
	
	void LateUpdate () {
        moveError = Vector3.Distance(transform.position, player.position) - initDelta;
        float cLerp = lerpCoef.Evaluate(moveError);
        Vector3 newPos = Vector3.Lerp(transform.position, player.position, cLerp);
        newPos.z = initZ;
        transform.position = newPos;
	}

    public float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget, bool clamp = false) {
        float val =  (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        if (clamp) {
            val = Mathf.Clamp(val, fromTarget, toTarget);
        }
        return val;
    }


}
