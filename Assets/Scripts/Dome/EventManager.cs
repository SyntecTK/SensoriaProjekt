using System;

public static class EventManager 
{
    public static event Action OnCalibrate;

    public static void Calibrate()
    {
        OnCalibrate?.Invoke();
    }
}
