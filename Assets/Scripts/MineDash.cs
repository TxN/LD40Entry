using UnityEngine;

public class MineDash: Mine {
    
	public MineDash(): base() {
		mineType = (int)MineTypes.Dash;
        mineTypeColor = Color.red;
    }
}