using UnityEngine;

public class MineSpeed: Mine {

    public MineSpeed(): base() {
		mineType = MineTypes.Speed;
        mineTypeColor = Color.blue;
    }
}