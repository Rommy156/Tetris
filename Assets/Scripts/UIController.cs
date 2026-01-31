using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TetrisManager tetrisManager;
    public GameObject endGamePanel;

    public void UIUpdateScore()
    {
        scoreText.text = "Score: " + tetrisManager.score.ToString();
    }
    public void UpdateGameOver()
    {
        endGamePanel.SetActive(tetrisManager.gameOver);
    }
    public void PlayAgain()
    {
       tetrisManager.SetGameOver(false);
    }
}
