using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{

    public static ScoreManager Instance { get; private set; }
    public float Score { get; private set; }
    public float Level { get; private set; }
    private ScoreDisplay scoreDisplay;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    

    public void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Score = 0;
        //BuildIndex minus 1 because the first level is the third scene
        Level = (SceneManager.GetActiveScene().buildIndex) - 1;
        scoreText.text = string.Format("Score: {0}", Score);
        levelText.text = string.Format("Level: {0}", Level);
    }

    public void IncreaseScore(float amount)
    {
        //Add amount of points to Score
        Score += amount;
        UpdateScoreDisplay();
    }

    public void UpdateScoreDisplay()
    {
        //Display score
        scoreText.text = "Score: " + Score;
        //Display level index
        levelText.text = "Level " + ((SceneManager.GetActiveScene().buildIndex) -1);
    }

    public void WrongMovement()
    {
        //Surface touched
        scoreText.text = "Please rotate the paddle in the opposite direction towards the item.";
        levelText.text = "";
        //Debug.Log("WrongMovement");
    }

    public void WrongColor()
    {
        //Wrong Item triggered
        scoreText.text = "This object is blue. Move the paddle to the yellow object to get a point.";
        levelText.text = "";
    }
}
