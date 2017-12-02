namespace EventSys {
    public struct Event_PlayerDead {
        public Player Player;
        public int PlayerIndex;
    }

    public struct Event_PlayerMineCollect {
        public int playerIndex;
    }

    public struct Event_MaximumMinesCount_Change {
        public int count;
    }

	public struct Event_Paused {
	}
}
