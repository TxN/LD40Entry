using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public class MinesSetup : EditorWindow {
    int playersCount = 1;
    int additionalMines = 40;
	
	[MenuItem("Tools/Mines Tools")]
	public static void SetupMines() {
        EditorWindow.GetWindow(typeof(MinesSetup));
	}
    
    void OnGUI()
    {
        GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
        playersCount = EditorGUILayout.IntSlider ("Number of players", playersCount, 1, 4);
        additionalMines = EditorGUILayout.IntSlider("Number of additional mines", additionalMines, 0, 100);
        
        if (GUILayout.Button("Create mines")) {
            if (EditorSceneManager.GetActiveScene().name == "JoinScreen") {
                Debug.Log("This action is allowed only on scenes with track and GameState object");
                return;
            }
            
            GameState gameState = FindObjectOfType<GameState>();
            gameState.SpawnMines(
                new List<TrackNode>(FindObjectsOfType<TrackNode>()).OrderBy(o=>o.GetIndex()).ToList(),
                playersCount,
                additionalMines,
                true
            );
            MarkActiveSceneDirty();
        }

        if (GUILayout.Button("Remove existing mines")) {
            if (EditorSceneManager.GetActiveScene().name == "JoinScreen") {
                Debug.Log("This action is allowed only on scenes with track and GameState object");
                return;
            }

            DestructMines();
            MarkActiveSceneDirty();
        }
    }
    void DestructMines() {
        Mine[] existingMines = FindObjectsOfType<Mine>();
        for (int i = 0; i < existingMines.Length; i++) {
            DestroyImmediate(existingMines[i].gameObject);
        }
        Debug.Log("Mines were deleted successfully.");
    }

    void MarkActiveSceneDirty() {
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
    }
}
