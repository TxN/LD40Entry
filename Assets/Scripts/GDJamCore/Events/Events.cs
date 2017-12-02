namespace EventSys {
    public struct Event_PlayerDead {
        public Player Player;
    }

    public struct Event_PlayerMineCollect {
        int playerIndex;
    }

    public struct Event_MaximumMinesCount_Change {
        public int count;
    }
}
