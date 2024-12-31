using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WrongItems : MonoBehaviour
{
    // When Paddle triggers wrong Object: 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Paddle")
        {
            //Display that the wrong item was touched
            ScoreManager.Instance.WrongColor();
            //Debug.Log("DestroyWrongItems");
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Paddle")
        {
            //Display that the wrong item was touched
            ScoreManager.Instance.WrongColor();
            //Debug.Log("DestroyWrongItems");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit");
        if (other.gameObject.tag == "Paddle")
        {
            //Display that the wrong item was touched
            ScoreManager.Instance.UpdateScoreDisplay();
            //Debug.Log("DestroyWrongItems done");
        }
    }
}