// Running totals across a playthrough, shown on the end-of-game screen.
// Reset when a fresh run starts at level 1.
public static class GameStats
{
    public static int TotalShots;
    public static float TotalTime;

    public static void Reset()
    {
        TotalShots = 0;
        TotalTime = 0f;
    }
}