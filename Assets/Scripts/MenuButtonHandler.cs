using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Serialization;

public class MenuButtonHandler : MonoBehaviour
{
    public static String PLAYER = "Human";
    public static String COMPUTER = "Computer";
    
    [FormerlySerializedAs("exit")] public Button ExitButton;
    [FormerlySerializedAs("newGame")] public Button NewGame;
    [FormerlySerializedAs("newGameSettings")] public GameObject NewGameSettings;

    [FormerlySerializedAs("buttonSelectedColor")] public Color ButtonSelectedColor;

    [FormerlySerializedAs("playerOneMode")] public Button PlayerOneMode;
    [FormerlySerializedAs("playerTwoMode")] public Button PlayerTwoMode;

    private Type PlayerOneManager = typeof(TurnManager);
    private Type PlayerTwoManager = typeof(TurnManager);
    private Type AiType = typeof(RandomMoves);

    [FormerlySerializedAs("start")] public Button StartButton;

    private void Start()
    {
        if(NewGameSettings == null)
        {
            Debug.Log("Why is this getting called?!?!?"); // TODO
            return; 
        }

        NewGameSettings.SetActive(false);
        SetCaption(PlayerOneMode, PlayerOneManager);
        SetCaption(PlayerTwoMode, PlayerTwoManager);

        ExitButton.onClick.AddListener(delegate() { Exit(); });
        NewGame.onClick.AddListener(delegate () { ShowGameOptions(); });
        StartButton.onClick.AddListener(delegate () { StartGame(); });
        PlayerOneMode.onClick.AddListener(delegate () { PlayerOneManager = toggleSelection(PlayerOneMode, PlayerOneManager); });
        PlayerTwoMode.onClick.AddListener(delegate () { PlayerTwoManager = toggleSelection(PlayerTwoMode, PlayerTwoManager); });
    }

    private void SetCaption(Button bt, Type turnManager)
    {
        var kind = PLAYER;
        if (turnManager != typeof(TurnManager)) // any non-vanilla is AI
        {
            kind = COMPUTER;
        }
        bt.GetComponentInChildren<Text>().text = kind;
    }

    private void ShowGameOptions()
    {
        NewGameSettings.SetActive(true);
    }

    private Type toggleSelection(Button btn, Type currentTurnManager)
    {
        Type newTurnManager;
        if (currentTurnManager == typeof(TurnManager))
        {
            newTurnManager = AiType;
        }
        else
        {
            newTurnManager = typeof(TurnManager);
        }
        
        SetCaption(btn, newTurnManager);
        return newTurnManager;
    }

    public void Exit()
    {
#if UNITY_EDITOR
         // Application.Quit() does not work in the editor so
         // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
         UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void StartGame()
    {
        var prefs = PlayerPreferences.Instance;
        prefs.PlayerOneManager = (TurnManager) Activator.CreateInstance(PlayerOneManager);
        prefs.PlayerTwoManager = (TurnManager) Activator.CreateInstance(PlayerTwoManager);

        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

}
