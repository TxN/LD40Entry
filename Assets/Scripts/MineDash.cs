using UnityEngine;

public class MineDash: Mine {
    
	public MineDash(): base() {
		mineType = MineTypes.Dash;
        mineTypeColor = Color.red;
    }
}