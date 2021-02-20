using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Serialization;

public class MenuButtonHandler : MonoBehaviour {
    public static string PLAYER = "Human";
    public static string COMPUTER = "Computer";

    public Button ExitButton;
    public Button NewGame;
    public GameObject NewGameSettings;

    public Button PlayerOneMode;
    public Button PlayerTwoMode;

    public Button PlayerOneArrangement;
    public Button PlayerTwoArrangement;

    private Type PlayerOneManager = typeof(TurnManager);
    private Type PlayerTwoManager = typeof(TurnManager);
    private Type AiType = typeof(RandomMoves);


    private ArrangementType PlayerOneArrangementType = ArrangementType.Vanilla;
    private ArrangementType PlayerTwoArrangementType = ArrangementType.Vanilla;

    public Button StartButton;

    public List<ArrangementType> arrangementTypes;

    private void Start() {
        if (NewGameSettings == null) {
            Debug.Log("Why is this getting called?!?!?"); // TODO
            return;
        }

        this.arrangementTypes = new List<ArrangementType>{
            ArrangementType.Vanilla,
            ArrangementType.Random,
            ArrangementType.Tiny,
            ArrangementType.Checkers
        };

        NewGameSettings.SetActive(false);
        SetCaption(PlayerOneMode, turnManagerToString(PlayerOneManager));
        SetCaption(PlayerTwoMode, turnManagerToString(PlayerTwoManager));

        SetCaption(PlayerOneArrangement, ArrangementToString(PlayerOneArrangementType));
        SetCaption(PlayerTwoArrangement, ArrangementToString(PlayerTwoArrangementType));

        ExitButton.onClick.AddListener(delegate () { Exit(); });
        NewGame.onClick.AddListener(delegate () { ShowGameOptions(); });
        StartButton.onClick.AddListener(delegate () { StartGame(); });

        PlayerOneMode.onClick.AddListener(delegate () { PlayerOneManager = toggleSelection(PlayerOneMode, PlayerOneManager); });
        PlayerTwoMode.onClick.AddListener(delegate () { PlayerTwoManager = toggleSelection(PlayerTwoMode, PlayerTwoManager); });

        PlayerOneArrangement.onClick.AddListener(delegate () { PlayerOneArrangementType = toggleArrangementSelection(PlayerOneArrangement, PlayerOneArrangementType); });
        PlayerTwoArrangement.onClick.AddListener(delegate () { PlayerTwoArrangementType = toggleArrangementSelection(PlayerTwoArrangement, PlayerTwoArrangementType); });
    }

    private ArrangementType toggleArrangementSelection(Button btn, ArrangementType current) {
        int curr = this.arrangementTypes.IndexOf(current);
        if (curr == this.arrangementTypes.Count - 1) {
            curr = 0;
        } else {
            curr += 1;
        }

        SetCaption(btn, ArrangementToString(this.arrangementTypes[curr]));

        return this.arrangementTypes[curr];
    }

    private string ArrangementToString(ArrangementType arrangementType) {
        switch (arrangementType) {
            case ArrangementType.Vanilla:
                return "Standard pieces";
            case ArrangementType.Random:
                return "Random set";
            case ArrangementType.Tiny:
                return "Tiny army";
            case ArrangementType.Checkers:
                return "Checkers";
            default:
                return "uh oh";
        }
    }

    private void SetCaption(Button bt, string text) {
        bt.GetComponentInChildren<Text>().text = text;
    }

    private void ShowGameOptions() {
        NewGameSettings.SetActive(true);
    }

    private string turnManagerToString(Type newTurnManager) {
        var kind = PLAYER;
        if (newTurnManager != typeof(TurnManager)) // any non-vanilla is AI
        {
            kind = COMPUTER;
        }

        return kind;
    }

    private Type toggleSelection(Button btn, Type currentTurnManager) {
        Type newTurnManager;
        if (currentTurnManager == typeof(TurnManager)) {
            newTurnManager = AiType;
        } else {
            newTurnManager = typeof(TurnManager);
        }

        SetCaption(btn, turnManagerToString(newTurnManager));
        return newTurnManager;
    }

    public void Exit() {
#if UNITY_EDITOR
         // Application.Quit() does not work in the editor so
         // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
         UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void StartGame() {
        var prefs = PlayerPreferences.Instance;
        prefs.PlayerOneManager = (TurnManager)Activator.CreateInstance(PlayerOneManager);
        prefs.PlayerOneArrangement = (PieceArrangement)Activator.CreateInstance(ToArrangementType(PlayerOneArrangementType));
        prefs.PlayerTwoManager = (TurnManager)Activator.CreateInstance(PlayerTwoManager);
        prefs.PlayerTwoArrangement = (PieceArrangement)Activator.CreateInstance(ToArrangementType(PlayerTwoArrangementType));


        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    private Type ToArrangementType(ArrangementType type) {
        switch (type) {
            case ArrangementType.Vanilla:
                return typeof(StandardPieceArrangement);
            case ArrangementType.Tiny:
                return typeof(TinyArmyArrangement);
            case ArrangementType.Random:
                return typeof(RandomArmyArrangement);
            case ArrangementType.Checkers:
                return typeof(CheckersArrangement);
            default:
                return null;
        }
    }
}
