using System;
using System.Collections.Generic;
using System.Threading;
using Photon.Realtime;

public class NetworkHandler : IConnectionCallbacks
{
    public static String APP_ID = "FIXME";
    
    private readonly LoadBalancingClient client = new LoadBalancingClient();
    private bool quit;

    ~NetworkHandler()
    {
        this.client.Disconnect();
        this.client.RemoveCallbackTarget(this);
    }

    public void StartClient()
    {
        this.client.AddCallbackTarget(this);
        this.client.StateChanged += this.OnStateChange;

        this.client.ConnectUsingSettings(new AppSettings() { AppIdRealtime = APP_ID });

        Thread t = new Thread(this.Loop);
        t.Start();

        Console.WriteLine("Running until key pressed.");
        Console.ReadKey();
        this.quit = true;
    }

    private void Loop(object state)
    {
        while (!this.quit)
        {
            this.client.Service();
            Thread.Sleep(33);
        }
    }

    private void OnStateChange(ClientState arg1, ClientState arg2)
    {
        Console.WriteLine(arg1 + " -> " + arg2);
    }

    #region IConnectionCallbacks

    public void OnConnectedToMaster()
    {
        Console.WriteLine("OnConnectedToMaster Server: " + this.client.LoadBalancingPeer.ServerIpAddress);
    }

    public void OnConnected()
    {
    }

    public void OnDisconnected(DisconnectCause cause)
    {
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }

    #endregion
}