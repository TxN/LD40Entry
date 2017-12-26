namespace EventSys {
    public struct Event_PlayerDead {
        public Player Player;
        public int PlayerIndex;
    }

    public struct Event_PlayerMineCollect {
        public int playerIndex;
        public int mineType;
    }

    public struct Event_MaximumMinesCount_Change {
        public int count;
    }

	public struct Event_Paused {
	}

	public struct Event_ChangeSelectedPauseMenuItem {
		public int offset;
	}

	public struct Event_SelectPauseMenuItem {
	}

    public struct Event_LapPassed {
       public int lap;
    }

	public struct Event_ControlsLockState_Change {
		public bool ControlsEnabled;
	}
}
