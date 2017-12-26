using UnityEngine;

public class MineSpeed: Mine {

    public MineSpeed(): base() {
		mineType = (int)MineTypes.Speed;
        mineTypeColor = Color.blue;
    }
}