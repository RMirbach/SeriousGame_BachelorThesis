using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
//using static UnityEditor.Experimental.RestService.PlayerDataFileLocator;
using static UnityEngine.GraphicsBuffer;

public class RotatePaddle : MonoBehaviour
{
    private GameObject paddle;
    private float speed = 150.0f; 
    private float horizontalInput;
  

    public static RotatePaddle Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }


    // Update: For motion control on computer. Later instead of horizontalInput Data of the Sensors
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        RotateWithKeys();        
    }



    //method to rotate stick with left and right arrow keys
    public void RotateWithKeys()
    {
        //Rotate the stick in desired range by horizontal input (left and right arrow key)
        if ((transform.rotation.z > -0.7f) && transform.rotation.z < 0.7f)
        {
            transform.Rotate(Vector3.back * speed * horizontalInput * Time.deltaTime);
            return;
        }
        //when the surface is touched
       else
        {
            ScoreManager.Instance.WrongMovement();
            Debug.Log("RotatePaddle.WrongMovement" + transform.rotation);
            transform.Rotate(Vector3.back * speed * 0 * horizontalInput * Time.deltaTime);
            if ((Input.GetKey(KeyCode.LeftArrow)) && transform.rotation.z < 0)
            {
                ScoreManager.Instance.IncreaseScore(0);
                transform.Rotate(Vector3.back * speed * horizontalInput * Time.deltaTime);
                return;
            }
            else if ((Input.GetKey(KeyCode.RightArrow)) && transform.rotation.z > 0)
            {
                ScoreManager.Instance.IncreaseScore(0);
                transform.Rotate(Vector3.back * speed * horizontalInput * Time.deltaTime);
                return;
            }
        }
    }

    //Method when wrong item or surface is triggered, calling the stop of movement and the display of text
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "BlueSphere")
        {
            NoMoreMovement();
            ScoreManager.Instance.WrongColor();

        }
        if(col.gameObject.tag == "Surface")
        {
            NoMoreMovement();
            ScoreManager.Instance.WrongMovement();
        }
    }



    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Surface")
        {
            //Display that the wrong item was touched
            ScoreManager.Instance.UpdateScoreDisplay();
           // Debug.Log("Touch surface done");
        }
    }

    //Make the stick stop moving
    public void NoMoreMovement()
    {
        transform.Rotate(Vector3.up * 0);
        //Debug.Log("NoMoreMovement");

    }



    //Method relevant for motion control by Movesense sensor
    public static void UpdatePaddleRotation(Quaternion rotation)
    {
        //check if movement in desired range, rotate by quaternion
        if ((Instance.transform.rotation.z > -0.7f) && Instance.transform.rotation.z < 0.7f)
        {
            ScoreManager.Instance.IncreaseScore(0);
            Instance.transform.rotation = rotation;
            return;
        }        
        else if(Instance.transform.rotation.z <= -0.7f)
        {
            //check if surface is touched
            if (rotation.z > -0.7f)
            {
                Instance.transform.rotation = rotation;
            }
            //Call method to display wrong movement
            ScoreManager.Instance.WrongMovement();
            return;    
        }
        else if (Instance.transform.rotation.z >= 0.7f)
        {
            //check if surface is touched
            if (rotation.z < 0.7f)
            {
                Instance.transform.rotation = rotation;
            }
            ScoreManager.Instance.WrongMovement();
            return;           
        }
    }   
}