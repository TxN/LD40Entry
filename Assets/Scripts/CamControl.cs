using UnityEngine;
using System.Collections.Generic;
using EventSys;

public class CamControl : MonoBehaviour {

    public static CamControl Instance;

    public AnimationCurve lerpCoef;
    public Transform player;

    public float initDelta = 10f;

    Camera _camera;

    float initZ = 0;
    float moveError;

    Vector3 _lastPos = new Vector3();

    float _multiplier = 1f;

    public List<float> CamCoeffs = new List<float>() { 0.9f, 0.75f, 0.65f, 0.55f };

    void Awake() {
        Instance = this;
    }

	void Start () {
        initZ = transform.position.z;
        _camera = GetComponent<Camera>();
        EventManager.Subscribe<Event_LapPassed>(this, OnLapChanged);
	}
	
	void LateUpdate () {
        if (player == null) {
            return;
        }
        moveError = Vector3.Distance(_lastPos, player.position);
        float cLerp = lerpCoef.Evaluate(moveError);
        Vector3 newPos = Vector3.Lerp(transform.position, player.position, cLerp * Time.deltaTime *8f);
        newPos.z = Mathf.Lerp(transform.position.z, initZ * _multiplier, 3f * Time.deltaTime);
        transform.position = newPos;
        _lastPos = player.position;
	}

    void OnDestroy() {
        EventManager.Unsubscribe<Event_LapPassed>(OnLapChanged);
    }

    public float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget, bool clamp = false) {
        float val =  (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        if (clamp) {
            val = Mathf.Clamp(val, fromTarget, toTarget);
        }
        return val;
    }

    public void MultiplyInitZ(float coef) {
        _multiplier = coef;
    }

    void OnLapChanged(Event_LapPassed e) {
        MultiplyInitZ(CamCoeffs[Mathf.Clamp(e.lap - 1, 0, CamCoeffs.Count - 1)]);
    }
}
