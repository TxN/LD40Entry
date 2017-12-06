using UnityEngine;
using EventSys;

public class IngameMenu : MonoBehaviour {

    public void OnQuitButton() {
        Application.Quit();
    }

    public void OnUnpauseButton() {
        if (GameState.Instance.PauseEnabled) {
            EventManager.Fire<Event_Paused>(new Event_Paused());
        }
    }

    public void OnGoToMenuButton() {
        GameState.Instance.EndGame();
    }
}
