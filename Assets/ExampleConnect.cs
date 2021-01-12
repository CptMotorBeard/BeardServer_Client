using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleConnect : MonoBehaviour
{
    public async void ConnectToServer()
    {
        BeardServer.Client.Instance.ConnectToServer(System.Net.IPAddress.Loopback, 42069);

        BeardServer.GeneralResponse resp = await BeardServer.ClientSend.SendGeneralRequestToServer();

        if (resp.IsSuccess)
            Debug.Log("Got a success");
        else
            Debug.Log("Got a failure");
    }
}
