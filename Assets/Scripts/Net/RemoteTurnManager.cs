using System;
using System.Collections.Generic;
using System.Threading;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

// handle communication w/ a remote player
public class RemoteTurnManager : TurnManager, IConnectionCallbacks, IMatchmakingCallbacks {
    public String AppId = "f755bd34-887c-44db-92dc-83be9a36f77b"; // TODO change this in an actual build

    private readonly LoadBalancingClient client = new LoadBalancingClient();
    private bool quit;
    private bool ready;

    private int localPlayerActorId;
    private Room room;
    private int remotePlayerActorId;

    public virtual Play ActOn(Player player, Player opponent, Board board) {
        // TODO send the board
        // TODO Wait for player command

        return null;
    }

    public override void OnGameStarting() {
        Debug.Log("Game starting...");
        StartClient();
    }

    public override bool IsReady() {
        return ready;
    }

    // wait for player
    public override void GameStarted(Player player, Player opponent, Board board) {
        // TODO 
        Debug.Log("Game started");

        // join a random room or create one
        // when someone else enters the room, THAT player becomes the opponent
    }

    ~RemoteTurnManager() {
        this.client.Disconnect();
        this.client.RemoveCallbackTarget(this);
    }

    private void StartClient() {
        this.client.AddCallbackTarget(this);
        // this.client.StateChanged += this.OnStateChange;

        this.client.UserId = Random.Range(0, 20).ToString();

        this.client.ConnectUsingSettings(new AppSettings() { AppIdRealtime = AppId });

        Thread t = new Thread(this.Loop);
        t.Start();
    }

    private void Loop(object state) {
        while (!this.quit) {
            this.client.Service();
            Thread.Sleep(33);
        }
    }

    public string Username() {
        if (ready) {
            return "Remote user!";
        }

        if (!this.client.IsConnected) {
            return "Connecting...";
        }

        if (this.room != null && !ready) {
            return "Waiting for opponent...";
        }

        return "Loading...";
    }

    #region IConnectionCallbacks

    public void OnConnectedToMaster() {
        Debug.Log("OnConnectedToMaster Server: " + this.client.LoadBalancingPeer.ServerIpAddress);
        this.localPlayerActorId = this.client.LocalPlayer.ActorNumber;

        JoinRoom();
    }

    private void JoinRoom() {
        var cp = new OpJoinRandomRoomParams();
        cp.ExpectedMaxPlayers = 2;

        var res = this.client.OpJoinRandomRoom(cp);
    }

    private void StartGame() {
        var players = this.room.Players;
        foreach (var kv in players) {
            if (kv.Key != localPlayerActorId) {
                this.remotePlayerActorId = kv.Key;
                break;
            }
        }

        ready = true;
    }

    public void OnConnected() {
        Debug.Log("Connected!");
    }

    public void OnDisconnected(DisconnectCause cause) {
        Debug.Log("Disconnected! " + cause);
    }

    public void OnRegionListReceived(RegionHandler regionHandler) {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {
    }

    public void OnCustomAuthenticationFailed(string debugMessage) {
    }

    #endregion

    #region IMatchmakingCallbacks

    public void OnFriendListUpdate(List<FriendInfo> friendList) {
    }

    public void OnCreatedRoom() {
        Debug.Log("Created room");
    }

    public void OnCreateRoomFailed(short returnCode, string message) {
        Debug.Log("couldnt create room: " + message);
    }

    public void OnJoinedRoom() {
        this.room = this.client.CurrentRoom;
        Debug.Log("joined room: " + this.room.Name);

        if (this.room?.PlayerCount == 2) {
            StartGame();
        }
    }

    public void OnJoinRoomFailed(short returnCode, string message) {
        Debug.Log("couldnt join room!");
    }

    public void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log("couldnt join random, creating...");
        var ep = new EnterRoomParams();

        this.client.OpCreateRoom(ep);
    }

    public void OnLeftRoom() {
        // TODO end game
    }

    #endregion
}