using UnityEngine;

public class PlayerInfo{
	public Color color;
	public int playerNumber;
    public bool ready = false;
	public PlayerInfo(Color col, int playerNumb){
		color = col;
		playerNumber = playerNumb;
	}
}
