using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    public static GameOver Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }

    //Method displaying the current level and total amount of points, called when game is over
    public void Setup(int score)
    {
        gameObject.SetActive(true);
        pointsText.text = "Great Job! In Level " + ((SceneManager.GetActiveScene().buildIndex) - 1) + " you scored " +score.ToString() + " Points!";

    }

    //Methods to load a level by clicking a button
    public void RestartButton()
    {
        SceneManager.LoadScene("Level1");
    }

    public void Level2Button()
    {
        SceneManager.LoadScene("Level2");
    }
    public void Level3Button()
    {
        SceneManager.LoadScene("Level3");
    }

    public void Level4Button()
    {
        SceneManager.LoadScene("Level4");
    }

    public void Level5Button()
    {
        SceneManager.LoadScene("Level5");
    }
    public void Level6Button()
    {
        SceneManager.LoadScene("Level6");
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
