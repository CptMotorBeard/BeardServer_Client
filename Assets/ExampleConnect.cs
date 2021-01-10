using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleConnect : MonoBehaviour
{
    public void ConnectToServer()
    {
        BeardServer.Client.Instance.ConnectToServer(System.Net.IPAddress.Loopback, 42069);
    }
}
