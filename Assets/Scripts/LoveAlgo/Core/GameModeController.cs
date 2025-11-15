using System;

namespace LoveAlgo.Core
{
    public enum GameMode
    {
        Story,
        FreeAction,
        Event,
        MiniGame,
        Messenger,
        Meta
    }

    public sealed class GameModeController
    {
        private GameMode currentMode = GameMode.Meta;

        public GameMode CurrentMode => currentMode;

        public event Action<GameMode> ModeChanged;

        public void SetMode(GameMode mode)
        {
            if (currentMode == mode)
            {
                return;
            }

            currentMode = mode;
            ModeChanged?.Invoke(currentMode);
        }
    }
}
