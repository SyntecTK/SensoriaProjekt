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
    [SerializeField] private Vector3 phoneBackDirection = Vector3.up;
    [SerializeField] private Vector3 phoneTopDirection = Vector3.left;

    [Header("Phone Pitch Amplification")]
    [Tooltip("Multiplier for leaning back (nose up). >1 -> less physical tilt needed")]
    [SerializeField] private float backwardPitchMultiplier = 4f;
    [SerializeField] private float forwardPitchMultiplier = 1f;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float maxBackwardPitch = 60f;

    [Header("Phone Roll Steering")]
    [SerializeField] private float rollDeadzone = 2f;
    [SerializeField] private float maxSteerRoll = 25f;
    [SerializeField] private float maxTurnRate = 140f;
    [SerializeField] private float steerExponent = 1.2f;
    [SerializeField] private float steerResponse = 6f;
    [SerializeField] private float lateralForce = 30f;
    [Tooltip("0 -> momentum keeps its direction (drifting), 1 -> velocity turns fully with the board")]
    [SerializeField, Range(0f, 1f)] private float velocityGrip = 1f;

    [Header("Steering Runtime")]
    [SerializeField] private float steerYaw = 0f;
    [SerializeField] private float currentTurnRate = 0f;


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

        steerYaw = 0f;
        currentTurnRate = 0f;
    }

    private Quaternion AmplifyPitch(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        float pitch = Mathf.DeltaAngle(0f, euler.x);

        // Unity: negative X = nose up (leaning back), positive X = nose down
        pitch *= pitch < 0f ? backwardPitchMultiplier : forwardPitchMultiplier;
        euler.x = Mathf.Clamp(pitch, -maxBackwardPitch, maxPitch);

        return Quaternion.Euler(euler);
    }

    // Returns -1..1, positive = leaning right
    private float GetSteerInput(Quaternion rotation)
    {
        // Unity: positive Z = right side up (leaning left), negative Z = leaning right
        float roll = -Mathf.DeltaAngle(0f, rotation.eulerAngles.z);
        float magnitude = Mathf.InverseLerp(rollDeadzone, maxSteerRoll, Mathf.Abs(roll));

        return Mathf.Sign(roll) * Mathf.Pow(magnitude, steerExponent);
    }

    private void Steer(float steerInput, float deltaTime)
    {
        // Turn rate builds up while the tilt is held -> slight tilt drifts sideways first, then curves
        float targetTurnRate = steerInput * maxTurnRate;
        currentTurnRate = Mathf.Lerp(currentTurnRate, targetTurnRate, 1f - Mathf.Exp(-steerResponse * deltaTime));
        float yawStep = currentTurnRate * deltaTime;
        steerYaw = Mathf.Repeat(steerYaw + yawStep, 360f);

        // Carry the existing momentum into the curve, otherwise the board only rotates but keeps sliding straight
        if (velocityGrip > 0f)
        {
            PhysicsRigidbody.linearVelocity = Quaternion.AngleAxis(yawStep * velocityGrip, Vector3.up) * PhysicsRigidbody.linearVelocity;
        }

        if (lateralForce != 0f && steerInput != 0f)
        {
            Vector3 right = Quaternion.Euler(0f, steerYaw, 0f) * Vector3.right;
            PhysicsRigidbody.AddForce(right * (steerInput * lateralForce * deltaTime), ForceMode.Impulse);
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
        if (phoneMovement == null) return;

        // Auto-calibrate as soon as the first phone data arrives
        if (!isCalibrated)
        {
            if (!phoneMovement.Connected) return;
            HandleCalibrate();
        }

        // Rotation since calibration, expressed in the phone's own axes
        Quaternion phoneDelta = Quaternion.Inverse(calibrationReference) * phoneMovement.Attitude;

        // Change of basis: phone axes -> board axes
        Quaternion phoneToBoard = Quaternion.LookRotation(phoneBackDirection, phoneTopDirection);
        Quaternion boardDelta = phoneToBoard * phoneDelta * Quaternion.Inverse(phoneToBoard);

        Steer(GetSteerInput(boardDelta), deltaTime);

        // Steering yaw is applied in world space, the phone tilt stays relative to it
        PhysicsRigidbody.MoveRotation(Quaternion.Euler(0f, steerYaw, 0f) * AmplifyPitch(boardDelta));
        
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
