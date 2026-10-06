using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// Läuft im PC-Spiel. Empfängt die Daten vom Handy in einem Hintergrund-Thread
// und stellt sie im Main-Thread als Properties bereit.
// Diese Version zeigt zusätzlich Diagnose-Infos in der Console und oben links im Game-Fenster.
public class PhoneControllerReceiver : MonoBehaviour
{
    public int port = 5555;

    public Quaternion Attitude { get; private set; } = Quaternion.identity;
    public Vector3 RotationRate { get; private set; }
    public bool Touching { get; private set; }
    public bool Connected => Time.time - lastPacketTime < 1f;

    [SerializeField] private SimpleRotater rotater;
    
    UdpClient udp;
    Thread thread;
    volatile bool running;
    readonly object lockObj = new object();
    float[] latest;
    bool hasNew;
    float lastPacketTime = -10f;

    long received;
    string lastSender = "-";
    string status = "nicht gestartet";

    void Start()
    {
        try
        {
            udp = new UdpClient(port);
            running = true;
            thread = new Thread(ReceiveLoop) { IsBackground = true };
            thread.Start();
            status = "lauscht auf Port " + port;
            Debug.Log("[Phone] Empfänger gestartet, lauscht auf UDP-Port " + port);
        }
        catch (Exception e)
        {
            status = "FEHLER: " + e.Message;
            Debug.LogError("[Phone] Empfänger konnte nicht starten: " + e.Message);
        }
    }

    void ReceiveLoop()
    {
        var ep = new IPEndPoint(IPAddress.Any, 0);
        while (running)
        {
            try
            {
                string s = Encoding.ASCII.GetString(udp.Receive(ref ep));
                if (Interlocked.Increment(ref received) == 1)
                    Debug.Log("[Phone] Erstes Paket empfangen von " + ep + ": " + s);
                lastSender = ep.ToString();

                string[] p = s.Split(',');
                if (p.Length < 8) { Debug.LogWarning("[Phone] Unvollständiges Paket: " + s); continue; }

                var v = new float[8];
                for (int i = 0; i < 8; i++)
                    v[i] = float.Parse(p[i], CultureInfo.InvariantCulture);

                lock (lockObj) { latest = v; hasNew = true; }
            }
            catch (FormatException) { Debug.LogWarning("[Phone] Paket konnte nicht gelesen werden"); }
            catch (Exception e)
            {
                if (!running) break;
                Debug.LogWarning("[Phone] Empfangsfehler: " + e.Message);
            }
        }
    }

    void Update()
    {
        float[] v;
        lock (lockObj)
        {
            if (!hasNew) return;
            v = latest;
            hasNew = false;
        }
        lastPacketTime = Time.time;

        // Handy-Koordinaten (rechtshändig) -> Unity (linkshändig)
        var a = new Quaternion(v[0], v[1], -v[2], -v[3]);
        Attitude = Quaternion.Euler(90f, 0f, 0f) * a;
        RotationRate = new Vector3(v[4], v[5], v[6]);
        Touching = v[7] > 0.5f;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 600, 25), "Empfänger: " + status);
        GUI.Label(new Rect(10, 30, 600, 25), "Pakete: " + Interlocked.Read(ref received) + "   Absender: " + lastSender);
        GUI.Label(new Rect(10, 50, 600, 25), "Verbunden: " + (Connected ? "JA" : "nein"));
    }

    void OnDestroy()
    {
        running = false;
        udp?.Close();
    }
}