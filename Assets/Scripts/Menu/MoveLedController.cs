using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MoveLedController : MonoBehaviour
{
    [SerializeField] int moveIndex = 0;
    [SerializeField] Color idleColor = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] float blendSpeed = 12f;
    [SerializeField] float sendInterval = 0.033f;
    [SerializeField] float keepAliveInterval = 3f;       // NEU

    // NEU: Versionskonstante aus include/psmove.h (PSMOVE_CURRENT_VERSION) prüfen
    [SerializeField] int psmoveVersion = 0x04000C;

    IntPtr move = IntPtr.Zero;                            // NEU: statt UniMoveController
    Color current, target, lastSent;
    float timer, sinceLastSend;

    void Awake() { current = target = lastSent = idleColor; }

    void Start()
    {
        try
        {
            bool ok = PSMoveNative.psmove_init(psmoveVersion);
            Debug.Log($"[MoveLED] psmove_init(0x{psmoveVersion:X6}) = {ok}");   // NEU

            int count = PSMoveNative.psmove_count_connected();
            Debug.Log($"[MoveLED] Controller gefunden: {count}");               // NEU

            if (count <= moveIndex)
            {
                Debug.LogWarning("[MoveLED] Kein Move-Controller gefunden.");
                enabled = false;
                return;
            }

            move = PSMoveNative.psmove_connect_by_id(moveIndex);
            Debug.Log($"[MoveLED] Handle: {move}");                             // NEU
            if (move == IntPtr.Zero) { enabled = false; return; }
            Send(idleColor);
        }
        catch (DllNotFoundException e)
        {
            Debug.LogError("psmoveapi.dll nicht gefunden oder Abhängigkeit fehlt: " + e.Message);
            enabled = false;
        }
        catch (EntryPointNotFoundException e)
        {
            Debug.LogError("Funktion in der DLL nicht gefunden: " + e.Message);
            enabled = false;
        }
    }

    [ContextMenu("Alle Controller testen")]
    void TestAll()
    {
        int count = PSMoveNative.psmove_count_connected();
        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };

        for (int i = 0; i < count; i++)
        {
            IntPtr h = PSMoveNative.psmove_connect_by_id(i);
            if (h == IntPtr.Zero) { Debug.Log($"[MoveLED] Index {i}: Verbindung fehlgeschlagen"); continue; }

            string serial = Marshal.PtrToStringAnsi(PSMoveNative.psmove_get_serial(h));
            int conn = PSMoveNative.psmove_connection_type(h);
            Color c = colors[i % colors.Length];

            PSMoveNative.psmove_set_leds(h, (byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255));
            int result = PSMoveNative.psmove_update_leds(h);

            Debug.Log($"[MoveLED] Index {i}: Seriennummer={serial}, Verbindungstyp={conn}, Farbe={c}, update_leds={result}");
        }
    }

    void OnEnable()
    {
        MenuButtonHover.HoverStarted += OnHoverStarted;
        MenuButtonHover.HoverEnded += OnHoverEnded;
    }

    void OnDisable()
    {
        MenuButtonHover.HoverStarted -= OnHoverStarted;
        MenuButtonHover.HoverEnded -= OnHoverEnded;
    }

    void OnHoverStarted(Color c) => target = c;
    void OnHoverEnded() => target = idleColor;

    void Update()
    {
        if (move == IntPtr.Zero) return;

        current = Color.Lerp(current, target, 1f - Mathf.Exp(-blendSpeed * Time.unscaledDeltaTime));

        timer += Time.unscaledDeltaTime;
        sinceLastSend += Time.unscaledDeltaTime;
        if (timer < sendInterval) return;
        timer = 0f;

        float diff = Mathf.Abs(current.r - lastSent.r) + Mathf.Abs(current.g - lastSent.g) + Mathf.Abs(current.b - lastSent.b);
        if (diff < 0.01f && sinceLastSend < keepAliveInterval) return;   // NEU: Keep-Alive

        Send(current);
    }

    void Send(Color c)   // NEU
    {
        PSMoveNative.psmove_set_leds(move,
            (byte)Mathf.RoundToInt(c.r * 255f),
            (byte)Mathf.RoundToInt(c.g * 255f),
            (byte)Mathf.RoundToInt(c.b * 255f));
        PSMoveNative.psmove_update_leds(move);
        lastSent = c;
        sinceLastSend = 0f;
    }

    void OnDestroy()
    {
        if (move == IntPtr.Zero) return;
        PSMoveNative.psmove_set_leds(move, 0, 0, 0);
        PSMoveNative.psmove_update_leds(move);
        PSMoveNative.psmove_disconnect(move);
        move = IntPtr.Zero;
    }
}