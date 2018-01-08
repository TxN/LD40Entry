using UnityEngine;

public class MineSimple: Mine {

	public MineSimple(): base() {
		mineType = MineTypes.Simple;
        mineTypeColor = Color.grey;
    }
}