using System;

public static class EventManager 
{
    public static event Action OnCalibrate;
    public static event Action OnCollectibleCollected;
    public static event Action OnRoundStarted;
    public static event Action OnIntroEnded;
    public static event Action OnWallrideStarted;

    public static void Calibrate()
    {
        OnCalibrate?.Invoke();
    }

    public static void CollectibleCollected()
    {
        OnCollectibleCollected?.Invoke();
    }

    public static void RoundStarted()
    {
        OnRoundStarted?.Invoke();
    }
    
    public static void IntroEnded()
    {
        OnIntroEnded?.Invoke();
    }

    public static void WallrideStarted()
    {
        OnWallrideStarted?.Invoke();
    }
}
