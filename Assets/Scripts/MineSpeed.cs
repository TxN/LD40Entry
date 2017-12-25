using UnityEngine;

public class MineSpeed: Mine {

    public MineSpeed(): base() {
        mineType = Mine.MINE_TYPE_SPEED;
        mineTypeColor = Color.blue;
    }
}