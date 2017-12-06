using UnityEngine;

public class PlayerInfo{
	public Color color;
	public string prefix;
    public bool ready = false;
	public PlayerInfo(Color col, string pref){
		color = col;
		prefix = pref;
	}
}
