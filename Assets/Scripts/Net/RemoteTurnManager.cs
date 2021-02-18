// handle communication w/ a remote player
public class RemoteTurnManager : TurnManager {

    private NetworkHandler NetworkHandler = new NetworkHandler();
    
    public virtual Play ActOn(Player player, Player opponent, Board board) {
        // TODO send the board
        // TODO Wait for player command
        
        return null;
    }

    // wait for player
    public override void GameStarted(Player player, Player opponent, Board board) {
        // TODO 
        NetworkHandler.StartClient();
    }

    public string Username() {
        return "Finding user...";
    }
}