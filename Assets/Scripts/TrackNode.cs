using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TrackNode : MonoBehaviour {
	public GameObject pole;
	public TrackNode previous;
	public TrackNode next;

	public float pole1Shift = -0.5f;
	public float pole2Shift = 0.5f;
	public GameObject pole1;
	public GameObject pole2;
	private Vector2 pos;

    private float _lastP1Shift = 0f;
    private float _lastP2Shift = 0f;

	public void Init(TrackNode prev)
	{
		// FIXME: it is workaround for first empty track-node
		if (prev == null) {
			return;
		}

		previous = prev;
		transform.position = prev.transform.position + prev.transform.TransformDirection(0,1,0)*2;
		transform.rotation = prev.transform.rotation;

		pole1Shift = prev.pole1Shift;
		pole2Shift = prev.pole2Shift;

		pos = new Vector3 (prev.pole1Shift, 0, prev.transform.position.z);
		pole1 = PrefabUtility.InstantiatePrefab(pole) as GameObject;
		pole1.transform.SetParent(transform);
		pole1.transform.localPosition = pos;
		pole1.transform.rotation = prev.transform.rotation;

		pos = new Vector3 (prev.pole2Shift, 0, prev.transform.position.z);
		pole2 = PrefabUtility.InstantiatePrefab(pole) as GameObject;
		pole2.transform.SetParent(transform);
		pole2.transform.localPosition = pos;
		pole2.transform.rotation = prev.transform.rotation;

		previous.next = this;

        var bcol = gameObject.AddComponent<BoxCollider2D>();
        bcol.isTrigger = true;
        float width = -pole1Shift + pole2Shift;

        bcol.size = new Vector2(width, 0.35f);
        bcol.offset = new Vector2((pole1Shift+pole2Shift)*0.5f, 0);

		MovePoles ();
	}

	void Update(){
		// FIXME: it is workaround for first empty track-node
		if (pole1 == null) {
			return;
		}

		MovePoles ();
	}

	public void BuildObject()
	{
		int index = GetIndex ();
		GameObject obj = new GameObject();
		obj.AddComponent<TrackNode> ();
		obj.name = "TrackNode" + (index + 1);
		obj.transform.SetParent(transform.parent, true);
		TrackNode trackNode = obj.GetComponent<TrackNode>();

		trackNode.Init (this);
		UnityEditor.Selection.activeGameObject = obj;
	}

	public int GetIndex()
	{
		int index = 0;
		int.TryParse(gameObject.name.Replace("TrackNode", ""), out index);
		return index;
	}

	protected void MovePoles()
	{
        if (_lastP1Shift == pole1Shift && _lastP2Shift == pole2Shift) {
            return;
        }
        _lastP1Shift = pole1Shift;
        _lastP2Shift = pole2Shift;

		pole1.transform.localPosition = new Vector3 (pole1Shift, 0, 0);
		pole2.transform.localPosition = new Vector3 (pole2Shift, 0, 0);

        var bcol = gameObject.GetComponent<BoxCollider2D>();
        if (!bcol) {
            bcol = gameObject.AddComponent<BoxCollider2D>();
        }
        bcol.isTrigger = true;
        float width = -pole1Shift + pole2Shift;

        bcol.size = new Vector2(width, 0.35f);
        bcol.offset = new Vector2((pole1Shift + pole2Shift) * 0.5f, 0);
	}
}