using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }
    private static bool isConnected = false;
    public TextMeshProUGUI connectedText;
   
    public void Awake()
    {
        Instance = this;
     }

      
 
    public void Start()
    {
        if ((string.IsNullOrEmpty(MovesenseSensors.Instance.connectedSensor))&&(!isConnected))
        {
            isConnected = true;
            Debug.Log("isConnected = true " + isConnected);
            InitialText();
        }
        else
        {
            ConnectionSuccessfull();
            Debug.Log("isConnected = what " + isConnected);

        }
}

    //Methods for available Buttons in MainMenu to quit the Game/load a different scene 
    public void ExitButton()
    {
        //Debug.Log("Exit " + exit);
        MovesenseController.Disconnect(MovesenseSensors.Instance.connectedSensor);
        //Debug.Log("Disconnected");
        Application.Quit();
        
    }
    public void StartFirstLevel()
    {
        SceneManager.LoadScene("Level1");
    }

    public void InitialText()
    {
        //Called as long as the sensor is disconnected/connecting
        connectedText.text = " Please wait for the sensor to connect! The connection message will be displayed here. ";

    }

    public void ConnectionSuccessfull()
    {
        //Called when the sensor connected
        connectedText.text = " You can start playing now, your sensor successfully connected! ";
    }
}