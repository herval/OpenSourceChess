using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MenuButtonHandler : MonoBehaviour
{
    PlayerPreferences prefs;

    public Button exit;
    public Button newGame;
    public GameObject newGameSettings;


    Color buttonDefaultColor;
    public Color buttonSelectedColor;

    public Button pvpMode;
    public Button pvcMode;

    public Button start;

    private void Start()
    {
        if(this.pvpMode == null)
        {
            Debug.Log("Why is this getting called?!?!?"); // TODO
            return; 
        }

        prefs = PlayerPreferences.Instance;

        buttonDefaultColor = pvpMode.colors.normalColor;

        newGameSettings.active = false;

        exit.onClick.AddListener(delegate() { Exit(); });
        newGame.onClick.AddListener(delegate () { ShowGameOptions(); });
        start.onClick.AddListener(delegate () { StartGame(); });
        pvcMode.onClick.AddListener(delegate () { toggleSelection(pvcMode, pvpMode); });
        pvpMode.onClick.AddListener(delegate () { toggleSelection(pvpMode, pvcMode); });
    }

    private void ShowGameOptions()
    {
        if(prefs.gameMode == GameMode.PlayerVersusComputer)
        {
            toggleSelection(pvcMode, pvpMode);
        } else
        {
            toggleSelection(pvpMode, pvcMode);
        }
        newGameSettings.active = true;
    }

    private void toggleSelection(Button selected, Button unselected)
    {
        var colors = selected.colors;
        colors.normalColor = buttonSelectedColor;
        colors.selectedColor = buttonSelectedColor;
        colors.pressedColor = buttonSelectedColor;
        colors.highlightedColor = buttonSelectedColor;
        selected.colors = colors;

        colors = unselected.colors;
        colors.normalColor = buttonDefaultColor;
        colors.selectedColor = buttonDefaultColor;
        colors.highlightedColor = buttonDefaultColor;
        colors.pressedColor = buttonDefaultColor;
        unselected.colors = colors;
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
        // ew
        prefs.gameMode = pvpMode.colors.normalColor == buttonSelectedColor ? GameMode.PlayerVersusPlayer : GameMode.PlayerVersusComputer;

        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

}
