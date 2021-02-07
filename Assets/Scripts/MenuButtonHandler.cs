using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MenuButtonHandler : MonoBehaviour
{
    public static String PLAYER = "Human";
    public static String COMPUTER = "Computer";
    
    public Button exit;
    public Button newGame;
    public GameObject newGameSettings;


    public Color buttonSelectedColor;

    public Button playerOneMode;
    public Button playerTwoMode;

    private Type playerOneManager = typeof(TurnManager);
    private Type playerTwoManager = typeof(TurnManager);
    private Type aiType = typeof(RandomMoves);

    public Button start;

    private void Start()
    {
        if(newGameSettings == null)
        {
            Debug.Log("Why is this getting called?!?!?"); // TODO
            return; 
        }

        newGameSettings.SetActive(false);
        SetCaption(playerOneMode, playerOneManager);
        SetCaption(playerTwoMode, playerTwoManager);

        exit.onClick.AddListener(delegate() { Exit(); });
        newGame.onClick.AddListener(delegate () { ShowGameOptions(); });
        start.onClick.AddListener(delegate () { StartGame(); });
        playerOneMode.onClick.AddListener(delegate () { playerOneManager = toggleSelection(playerOneMode, playerOneManager); });
        playerTwoMode.onClick.AddListener(delegate () { playerTwoManager = toggleSelection(playerTwoMode, playerTwoManager); });
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
        newGameSettings.SetActive(true);
    }

    private Type toggleSelection(Button btn, Type currentTurnManager)
    {
        Type newTurnManager;
        if (currentTurnManager == typeof(TurnManager))
        {
            newTurnManager = aiType;
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
        prefs.PlayerOneManager = (TurnManager) Activator.CreateInstance(playerOneManager);
        prefs.PlayerTwoManager = (TurnManager) Activator.CreateInstance(playerTwoManager);

        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

}
