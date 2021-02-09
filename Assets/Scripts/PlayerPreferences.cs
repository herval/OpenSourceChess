public class PlayerPreferences : Singleton<PlayerPreferences>
{
    public TurnManager PlayerOneManager = new TurnManager();
    public TurnManager PlayerTwoManager = new RandomMoves();
}
