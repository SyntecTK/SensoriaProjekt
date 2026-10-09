using UnityEngine;

public class SimpleRailGrind : MonoBehaviour, IReferenceRigidbody, IHandleInput, ISimulateable
{
    public Rigidbody PhysicsRigidbody { get; set; }
    [SerializeField] private GetGamepadParameter inputLogic = new GetGamepadParameter();

    [Header("Runtime Variables")]
    public bool CanGrind = false, IsGrinding, IsRotating;
    [SerializeField] private bool wantsToGrind, isTurningWithRail;
    [SerializeField] private float rotationAngle, rotationTreshold = 90f;
    [SerializeField] private Transform rotationTransform;
    private Vector3 railDirection, flatRailDirection, lastFlatRailDirection;

    [Header("Settings")]
    [Tooltip("1 = Left Shoulder, 2 = Right Shoulder, 3 = Left Stick Button, 4 = Right Stick Button\r\n" +
        "    /// 5 = A Button, 6 = B Button, 7 = X Button, 8 = Y Button\r\n" +
        "    /// 9 = North Button, 10 = East Button, 11 = South Button, 12 = West Button\r\n" +
        "    /// 13 = Start Button, 14 = Select Button\r\n" +
        "    /// 15 = Left Trigger, 16 = Right Trigger\r\n" +
        "    /// 17 = Left Stick X, 18 = Left Stick Y, 19 = Right Stick X, 20 = Right Stick Y")]
    [SerializeField] private int grindButton;

    [SerializeField] private bool autoGrind, holdButtonToGrind = true;
    [SerializeField] private float grindBoostForce = 0f;


    public void HandleInput(in GamepadInput input, float deltaTime)
    {
        if (autoGrind)
        {
            wantsToGrind = CanGrind;
            return;
        }

        if (grindButton > 0)
        {
            if (holdButtonToGrind)
            {
                wantsToGrind = inputLogic.CheckButton(grindButton, input);
            }

            // Switch Logik und so kein Bock gerade
            //else if (inputLogic.CheckButton(grindButton, input))
            //{
            //    wantsToGrind = IsGrinding ? false : true;
            //}
        }
    }

    public void SetRotationTransform(Transform rotationTransform = null)
    {
        this.rotationTransform = rotationTransform;
    }

    public void UpdateRail(bool isOnRail, bool _isTurningWithRail, Vector3 newRailDirection = default)
    {
        CanGrind = isOnRail;
        
        if (_isTurningWithRail)
        {
            if (wantsToGrind && CanGrind) IsRotating = true;

            flatRailDirection = new Vector3(newRailDirection.x, 0f, newRailDirection.z).normalized;
            lastFlatRailDirection = new Vector3(railDirection.x, 0f, railDirection.z).normalized;
        }

        //Debug Only
        isTurningWithRail = _isTurningWithRail;

        railDirection = newRailDirection;
    }

    public void Simulate(float deltaTime)
    {
        Vector3 newForce;

        if (IsGrinding)
        {
            if (!CanGrind) // Getting off the rail
            {
                newForce = lastFlatRailDirection * grindBoostForce;
            }
            else
            {
                Vector3 accumulatedForce = PhysicsRigidbody.GetAccumulatedForce();

                //Debug.Log("Accumulated Force: " + PhysicsRigidbody.GetAccumulatedForce());
                //Debug.DrawRay(PhysicsRigidbody.position, railDirection * 10f, Color.green);

                newForce = Vector3.Project(accumulatedForce, railDirection) + grindBoostForce * railDirection;

                //Debug.DrawRay(PhysicsRigidbody.position, newForce * 10f, Color.red);

                PhysicsRigidbody.linearVelocity = Vector3.zero;

                //PhysicsRigidbody.AddForce(-PhysicsRigidbody.GetAccumulatedForce(), ForceMode.Acceleration);
            }

            PhysicsRigidbody.AddForce(newForce, ForceMode.Impulse);
        }

        float newRotation = 0f;

        if (IsRotating)
        {
            newRotation = Mathf.Abs(Vector3.SignedAngle(lastFlatRailDirection, flatRailDirection, Vector3.up));
            Vector3 newGoalDirection;

            if (newRotation != 0f && rotationAngle + newRotation > Mathf.Abs(rotationTreshold))
            {
                float a = 90f - rotationAngle;
                float f =  a / newRotation;

                newGoalDirection = Vector3.Slerp(lastFlatRailDirection, flatRailDirection, f);

                rotationAngle = 0f;
                IsRotating = false;

                rotationTransform.localEulerAngles = new Vector3(rotationTransform.localEulerAngles.x, rotationTransform.localEulerAngles.y + a, rotationTransform.localEulerAngles.z);
                rotationTransform = null;
            }
            else
            {
                newGoalDirection = flatRailDirection;
                rotationAngle += newRotation;
            }

            if (rotationTransform != null) rotationTransform.localEulerAngles = new Vector3(rotationTransform.localEulerAngles.x, rotationTransform.localEulerAngles.y + newRotation, rotationTransform.localEulerAngles.z);

            //PhysicsRigidbody.rotation *= Quaternion.FromToRotation(lastFlatRailDirection, newGoalDirection);
        }

        IsGrinding = CanGrind && wantsToGrind;
    }
}