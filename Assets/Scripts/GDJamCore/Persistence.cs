using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Persistence : MonoBehaviour {

	public static Persistence Instance {
		get {
			return _instance;
		}
	}

	public static Persistence _instance = null;

	private List<Object> _persistentObjects = new List<Object>();

	private void Awake() {
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(this);
		}
	}

	public void AddPersistentObject(Object toAdd) {
		_persistentObjects.Add(toAdd);
		DontDestroyOnLoad(toAdd);
	}

	public void RemoveFromPersistent(Object obj) {
		if ( _persistentObjects.Contains(obj) ) {
			_persistentObjects.Remove(obj);
			if ( obj is GameObject ) {
				SceneManager.MoveGameObjectToScene(obj as GameObject, SceneManager.GetActiveScene());
			}
		}
	}

	public void DestroyAllPersistent(bool destroyManager = true, bool eventsToo = true) {
		foreach (var item in _persistentObjects) {
			if ( item != null ) {
				Destroy(item);
			}
		}
		_persistentObjects.Clear();

		if ( eventsToo ) {
			EventSys.EventManager.Instance.CleanUp();
		}

		if ( destroyManager ) {
			Destroy(gameObject);
		}
	}
}
