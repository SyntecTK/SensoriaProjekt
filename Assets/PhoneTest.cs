using UnityEngine;

// Testskript: dreht das Objekt mit dem Handy mit und färbt es rot bei Berührung.
public class PhoneTest : MonoBehaviour
{
    public PhoneControllerReceiver phone;
    Renderer rend;
    bool wasConnected;

    void Start() => rend = GetComponent<Renderer>();

    void Update()
    {
        if (phone.Connected != wasConnected)
        {
            Debug.Log(phone.Connected ? "Handy verbunden" : "Handy getrennt");
            wasConnected = phone.Connected;
        }
        transform.rotation = phone.Attitude;
        rend.material.color = phone.Touching ? Color.red : Color.white;
    }
}