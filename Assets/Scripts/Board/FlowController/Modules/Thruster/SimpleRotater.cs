using UnityEngine;

public class SimpleRotater : MonoBehaviour, IReferenceRigidbody, IHandleInput, ISimulateable
{
    public Rigidbody PhysicsRigidbody { get; set; }
    [SerializeField] private GetGamepadParameter inputLogic = new GetGamepadParameter();
    [SerializeField] private PhoneControllerReceiver phoneMovement;

    [Header("Runtime Variables")]
    [SerializeField] private Vector3 RotThrottle = Vector3.zero;
    private Quaternion calibrationReference = Quaternion.identity;
    private bool isCalibrated = false;


    [Header("Settings")]
    [SerializeField] private bool usesPhysicsRotation = true;
    [SerializeField] private float pitchSpeed = 0f;
    [SerializeField] private float yawSpeed = 0f;
    [SerializeField] private float rollSpeed = 0f;

    [Tooltip("1 -> left.x, 2 -> left.y, 3 -> right.x, 4 -> right.y")]
    [SerializeField] private int PitchStickAxis = 2, YawStickAxis = 3, RollStickAxis = 1;

    [Header("Phone Axes (in board space)")]
    [Tooltip("Board direction the phone's back (camera side, Unity +Z of Attitude) points to. Screen down -> up")]
    [SerializeField] private Vector3 phoneBackDirection = Vector3.up;
    [Tooltip("Board direction the phone's top edge (Unity +Y of Attitude) points to")]
    [SerializeField] private Vector3 phoneTopDirection = Vector3.left;


    private void OnEnable()
    {
        EventManager.OnCalibrate += HandleCalibrate;
    }

    private void OnDisable()
    {
        EventManager.OnCalibrate -= HandleCalibrate;
    }

    private void HandleCalibrate()
    {
        if (phoneMovement != null)
        {
            // The complete current phone orientation becomes the new zero
            calibrationReference = phoneMovement.Attitude;
            isCalibrated = true;
        }

        if (PhysicsRigidbody != null)
        {
            PhysicsRigidbody.rotation = Quaternion.identity;
            PhysicsRigidbody.angularVelocity = Vector3.zero;
        }
    }

    public void HandleInput(in GamepadInput input, float deltaTime)
    {
        RotThrottle = new Vector3(inputLogic.CheckAxis(PitchStickAxis, input) * pitchSpeed,
            inputLogic.CheckAxis(YawStickAxis, input) * yawSpeed,
            inputLogic.CheckAxis(RollStickAxis, input) * rollSpeed);
    }
    public void Simulate(float deltaTime)
    {
        if (phoneMovement == null || !isCalibrated) return;

        // Rotation since calibration, expressed in the phone's own axes
        Quaternion phoneDelta = Quaternion.Inverse(calibrationReference) * phoneMovement.Attitude;

        // Change of basis: phone axes -> board axes
        Quaternion phoneToBoard = Quaternion.LookRotation(phoneBackDirection, phoneTopDirection);
        Quaternion boardDelta = phoneToBoard * phoneDelta * Quaternion.Inverse(phoneToBoard);

        PhysicsRigidbody.MoveRotation(boardDelta);
        
        // if (usesPhysicsRotation)
        // {
        //     PhysicsRigidbody.AddRelativeTorque(RotThrottle * deltaTime, ForceMode.Impulse);
        // }
        // else
        // {
        //     PhysicsRigidbody.transform.rotation = Quaternion.Euler(RotThrottle);
        // }
    }
}
