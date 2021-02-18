using System;
using UnityEngine;

public class NetworkListener : MonoBehaviour {
    private NetworkHandler NetworkHandler;

    void Awake() {
        NetworkHandler = new NetworkHandler();
        NetworkHandler.StartClient();
    }
}