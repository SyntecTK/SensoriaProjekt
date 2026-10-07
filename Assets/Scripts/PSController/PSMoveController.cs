using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class PSMoveControllers : MonoBehaviour
{
    private const string Library = "psmoveapi";

    private const uint ApiVersion = 0x04000C;

    // PSMove_Button aus psmove.h
    private const uint BtnTriangle = 1u << 4;

    [Serializable]
    public class ControllerSlot
    {
        public Transform testObject;

        [Range(0f, 1f)]
        public float triggerValue;

        public UnityEvent onTriggerPressed = new UnityEvent();

        public UnityEvent onTrianglePressed = new UnityEvent();

        [NonSerialized] public IntPtr handle;
        [NonSerialized] public bool triggerWasPressed;
        [NonSerialized] public bool triangleWasPressed;
        [NonSerialized] public Vector3 originalScale;
    }

    [SerializeField]
    private ControllerSlot[] controllers =
    {
        new ControllerSlot(),
        new ControllerSlot()
    };

    [SerializeField, Range(0.05f, 1f)]
    private float triggerThreshold = 0.5f;

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    private static extern int psmove_init(uint version);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    private static extern int psmove_count_connected();

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr psmove_connect_by_id(int id);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    private static extern int psmove_poll(IntPtr move);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    private static extern byte psmove_get_trigger(IntPtr move);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint psmove_get_buttons(IntPtr move);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
    private static extern void psmove_disconnect(IntPtr move);


    private void Start()
    {
        try
        {
            if (psmove_init(ApiVersion) == 0)
            {
                Debug.LogError("PS Move API konnte nicht initialisiert werden.");
                enabled = false;
                return;
            }

            int count = psmove_count_connected();

            Debug.Log($"PS Move: {count} Verbindung(en) gefunden.");

            if (count < controllers.Length)
            {
                Debug.LogWarning(
                    $"Erwartet: {controllers.Length} Controller. " +
                    $"Gefunden: {count}.");
            }

            int slotCount = Mathf.Min(count, controllers.Length);

            for (int i = 0; i < slotCount; i++)
            {
                ControllerSlot slot = controllers[i];

                slot.handle = psmove_connect_by_id(i);

                if (slot.handle == IntPtr.Zero)
                {
                    Debug.LogWarning(
                        $"Controller {i + 1} konnte nicht geöffnet werden.");
                    continue;
                }

                slot.triggerWasPressed = false;
                slot.triangleWasPressed = false;
                slot.triggerValue = 0f;

                if (slot.testObject != null)
                    slot.originalScale = slot.testObject.localScale;

                Debug.Log($"Controller {i + 1} geöffnet.");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            CloseControllers();
            enabled = false;
        }
    }

    private void Update()
    {
        for (int i = 0; i < controllers.Length; i++)
        {
            ControllerSlot slot = controllers[i];

            if (slot.handle == IntPtr.Zero)
                continue;

            for (int report = 0; report < 64; report++)
            {
                if (psmove_poll(slot.handle) == 0)
                    break;

                slot.triggerValue =
                    psmove_get_trigger(slot.handle) / 255f;

                bool pressed =
                    slot.triggerValue >= triggerThreshold;

                if (pressed && !slot.triggerWasPressed)
                {
                    Debug.Log($"Controller {i + 1}: Trigger gedrückt.");
                    slot.onTriggerPressed.Invoke();
                }

                slot.triggerWasPressed = pressed;

                uint buttons = psmove_get_buttons(slot.handle);
                bool trianglePressed = (buttons & BtnTriangle) != 0;

                if (trianglePressed && !slot.triangleWasPressed)
                {
                    Debug.Log($"Controller {i + 1}: Dreieck gedrückt.");
                    slot.onTrianglePressed.Invoke();
                    EventManager.Calibrate();
                }

                slot.triangleWasPressed = trianglePressed;
            }

            if (slot.testObject != null)
            {
                float scaleFactor = 1f + slot.triggerValue;
                slot.testObject.localScale =
                    slot.originalScale * scaleFactor;
            }
        }
    }

    private void OnDestroy()
    {
        CloseControllers();
    }

    private void CloseControllers()
    {
        foreach (ControllerSlot slot in controllers)
        {
            if (slot.handle == IntPtr.Zero)
                continue;

            psmove_disconnect(slot.handle);
            slot.handle = IntPtr.Zero;

            if (slot.testObject != null)
                slot.testObject.localScale = slot.originalScale;
        }
    }
}