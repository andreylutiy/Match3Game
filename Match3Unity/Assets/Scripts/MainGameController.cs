using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainGameController : MonoBehaviour
{
    [SerializeField]
    GameUiController uiController;

    Match3Lib.Match3Controller match3Controller;

    int xSize = 15;
    int ySize = 15;
    int gameItems = 5;
    float duration = 30;
    float timeLeft ;

    void Awake()
    {
        match3Controller = new Match3Lib.Match3Controller();
    }

    private void Start()
    {
        xSize = GameSettings.gameLevel;
        ySize = GameSettings.gameLevel;
        var makeStepAction = new Action<int, int, int, int>((x0, y0, x1, y1)  =>
                {
                    match3Controller.MakeStep(new Match3Lib.Point(x0, y0), new Match3Lib.Point(x1, y1));
                });

        uiController.StartGame(new Vector2Int( xSize, ySize), makeStepAction);
        NewGame();
    }

    void NewGame()
    {
        match3Controller.NewGame(new Match3Lib.Point(xSize, ySize), uiController, gameItems);
        timeLeft = duration;
        Invoke("UpdateTime", 1);
        Invoke("GameOver", duration);
    }

    void GameOver()
    {
        var score = match3Controller.ActualScore;
        GameSettings.lastScore = score;
        GameSettings.bestScore = Math.Max(GameSettings.lastScore, GameSettings.bestScore);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2); 
    }

    void UpdateTime()
    {
        timeLeft--;
        uiController.OnTimeChange(timeLeft);
        Invoke("UpdateTime", 1);
    }
}