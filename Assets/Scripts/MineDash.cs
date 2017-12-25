using UnityEngine;

public class MineDash: Mine {
    
	public MineDash(): base() {
        mineType = Mine.MINE_TYPE_DASH;
        mineTypeColor = Color.red;
    }
}