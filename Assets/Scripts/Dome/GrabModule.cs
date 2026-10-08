using UnityEngine;

public class GrabModule : MonoBehaviour
{
    private bool isInWallrideRange;

    public void Grab(int index)
    {
        if(isInWallrideRange)
        {
            EventManager.WallrideStarted();
        }
    }

    public void IsInWallrideRange(bool isInRange)
    {
        isInWallrideRange = isInRange;
    }


}
