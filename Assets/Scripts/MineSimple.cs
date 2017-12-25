using UnityEngine;

public class MineSimple: Mine {

	public MineSimple(): base() {
        mineType = Mine.MINE_TYPE_SIMPLE;
        mineTypeColor = Color.grey;
    }
}