using UnityEngine;

public class MineSimple: Mine {

	public MineSimple(): base() {
		mineType = (int)MineTypes.Simple;
        mineTypeColor = Color.grey;
    }
}