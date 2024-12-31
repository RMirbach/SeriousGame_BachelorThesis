using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class PrefabManager : MonoBehaviour
{
    // Defining public storageplaces for different game objects
    public GameObject prefab;
    public GameObject prefabBlue;
    public GameObject prefabYellow;

    // Define variable for Backgroundscreen
    public GameOver GameOver;

    // Defining lists for game objects (prefabs), integers (countIndices) and Vector3 positions (listsOfPositions)
    public List<int> countIndices;
    public List<GameObject> prefabs;
    public List<List<Vector3>> listsOfPositions;

    // Positions in first list (Motor III for level 1 and 2)
    public List<Vector3> getFirstListMotor3()
    {
        List<Vector3> firstListMotor3 = new List<Vector3>();
        firstListMotor3.Add(new Vector3(0.5f, 3.4f, 0));
        firstListMotor3.Add(new Vector3(-0.5f, 3.4f, 0));
        firstListMotor3.Add(new Vector3(0.8f, 3.34f, 0));
        firstListMotor3.Add(new Vector3(-0.8f, 3.34f, 0));
        firstListMotor3.Add(new Vector3(1.1f, 3.25f, 0));
        firstListMotor3.Add(new Vector3(-1.1f, 3.25f, 0));
        firstListMotor3.Add(new Vector3(1.4f, 3.14f, 0));
        firstListMotor3.Add(new Vector3(-1.4f, 3.14f, 0));
        return firstListMotor3;
    }

    // Positions in second list (Motor III for level 2)
    public List<Vector3> getSecondListMotor3()
    {
        List<Vector3> secondListMotor3 = new List<Vector3>();
        secondListMotor3.Add(new Vector3(-0.5f, 3.4f, 0));
        secondListMotor3.Add(new Vector3(1.1f, 3.25f, 0));
        secondListMotor3.Add(new Vector3(-1.4f, 3.14f, 0));
        secondListMotor3.Add(new Vector3(1.1f, 3.25f, 0));
        secondListMotor3.Add(new Vector3(-1.4f, 3.14f, 0));
        secondListMotor3.Add(new Vector3(1.4f, 3.14f, 0));
        secondListMotor3.Add(new Vector3(-1.7f, 2.99f, 0));
        secondListMotor3.Add(new Vector3(1.7f, 2.99f, 0));
        return secondListMotor3;
    }


    // Positions in first list (Motor II for level 3 and 4)
    public List<Vector3> getFirstListMotor2()
    {
        List<Vector3> firstListMotor2 = new List<Vector3>();
        firstListMotor2.Add(new Vector3(0.5f, 3.4f, 0));
        firstListMotor2.Add(new Vector3(-0.5f, 3.4f, 0));
        firstListMotor2.Add(new Vector3(1.1f, 3.25f, 0));
        firstListMotor2.Add(new Vector3(-1.1f, 3.25f, 0));
        firstListMotor2.Add(new Vector3(1.7f, 2.99f, 0));
        firstListMotor2.Add(new Vector3(-1.7f, 2.99f, 0));
        firstListMotor2.Add(new Vector3(2.3f, 2.7f, 0));
        firstListMotor2.Add(new Vector3(-2.3f, 2.7f, 0));
        return firstListMotor2;
    }

    // Positions in second list (Motor II for level 4)
    public List<Vector3> getSecondListMotor2()
    {
        List<Vector3> secondListMotor2 = new List<Vector3>();
        secondListMotor2.Add(new Vector3(-0.5f, 3.4f, 0));
        secondListMotor2.Add(new Vector3(1.7f, 2.99f, 0));
        secondListMotor2.Add(new Vector3(-2.3f, 2.7f, 0));
        secondListMotor2.Add(new Vector3(1.7f, 2.99f, 0));
        secondListMotor2.Add(new Vector3(-2.3f, 2.7f, 0));
        secondListMotor2.Add(new Vector3(2.3f, 2.7f, 0));
        secondListMotor2.Add(new Vector3(-3f, 1.67f, 0));
        secondListMotor2.Add(new Vector3(3f, 1.67f, 0));
        return secondListMotor2;
    }

    // Positions in first list  (Motor I for level 5 and 6)
    public List<Vector3> getFirstListMotor1()
    {
        List<Vector3> firstListMotor1 = new List<Vector3>();
        firstListMotor1.Add(new Vector3(0.5f, 3.4f, 0));
        firstListMotor1.Add(new Vector3(-0.5f, 3.4f, 0));
        firstListMotor1.Add(new Vector3(1.5f, 2.8f, 0));
        firstListMotor1.Add(new Vector3(-1.5f, 2.8f, 0));
        firstListMotor1.Add(new Vector3(2.5f, 2f, 0));
        firstListMotor1.Add(new Vector3(-2.5f, 2f, 0));
        firstListMotor1.Add(new Vector3(3f, 0.5f, 0));
        firstListMotor1.Add(new Vector3(-3f, 0.5f, 0));
        return firstListMotor1;
    }

    // Positions in second list  (Motor I for level 6)
    public List<Vector3> getSecondListMotor1()
    {
        List<Vector3> secondListMotor1 = new List<Vector3>();
        secondListMotor1.Add(new Vector3(-0.5f, 3.4f, 0));
        secondListMotor1.Add(new Vector3(1.5f, 2.8f, 0));
        secondListMotor1.Add(new Vector3(-1.5f, 2.8f, 0));
        secondListMotor1.Add(new Vector3(3f, 0.5f, 0));
        secondListMotor1.Add(new Vector3(-2.5f, 2f, 0));
        secondListMotor1.Add(new Vector3(3f, 0.5f, 0));
        secondListMotor1.Add(new Vector3(-3f, 0.5f, 0));
        secondListMotor1.Add(new Vector3(3f, 0.5f, 0));
        
        
        return secondListMotor1;
    }




    // Method initialising the switch cases (different levels)
    public void init(int level)
    {
        // Defining new lists
        listsOfPositions = new List<List<Vector3>>();
        countIndices = new List<int>();
        prefabs = new List<GameObject>();
     
        switch (level)
        {
            // First level Motor I level 1
            case 1: 
                listsOfPositions.Add(getFirstListMotor3());
                countIndices.Add(0);
                prefabs.Add(prefab);
                break;
            // cognitive level Motor I level 2
            case 2:
                listsOfPositions.Add(getSecondListMotor3());
                listsOfPositions.Add(getFirstListMotor3());
                countIndices.Add(0);
                countIndices.Add(0);
                prefabs.Add(prefabBlue);
                prefabs.Add(prefabYellow);
                break;
            // First level Motor II level 3
            case 3:
                listsOfPositions.Add(getFirstListMotor2());
                countIndices.Add(0);
                prefabs.Add(prefab);
                break;
            //cognitive level Motor II level 4
            case 4:
                listsOfPositions.Add(getSecondListMotor2());
                listsOfPositions.Add(getFirstListMotor2());
                countIndices.Add(0);
                countIndices.Add(0);
                prefabs.Add(prefabBlue);
                prefabs.Add(prefabYellow);
                break;
            // First level Motor I level 5
            case 5:
                listsOfPositions.Add(getFirstListMotor1());
                countIndices.Add(0);
                prefabs.Add(prefab);
                break;
            //cognitive level Motor I level 6
            case 6:
                listsOfPositions.Add(getSecondListMotor1());
                listsOfPositions.Add(getFirstListMotor1());
                countIndices.Add(0);
                countIndices.Add(0);
                prefabs.Add(prefabBlue);
                prefabs.Add(prefabYellow);
                break;
            // otherwise: break
            default:
                break;
        }
    }


    public static PrefabManager Instance { get; private set; }
   
    //Get the sphere collider component and assign it to sphereCollider variable

    public void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // Calling method to initialize items for current level
        init((SceneManager.GetActiveScene().buildIndex) - 1);

        // Initialize first position
        Spawning();
     }

    // Spawn items
    public void Spawning()
    {
            
        // Game over?
        if (checkLevelDone()) 
        {
            RotatePaddle.Instance.NoMoreMovement();
            GameOverScreen();
        }
        else
        {
            for (int i = 0; i < listsOfPositions.Count; i++) {
                Instantiate(PrefabManager.InstanceType.prefabs[i], transform.position = listsOfPositions[i][countIndices[i]], Quaternion.identity);
                countIndices[i]++;
             }
         }
    }

    public bool checkLevelDone()
    {
        for (int i = 0; i < countIndices.Count; i++) {
            if (countIndices[i] != listsOfPositions[i].Count)    return false;
        }
        return true;
     }

    public void GameOverScreen()
    {
        GameOver.Setup(countIndices[0]);
    }

    private static PrefabManager m_Instance = null;

    public static PrefabManager InstanceType
    {
        get {
            if (m_Instance == null) {
                m_Instance = (PrefabManager)FindObjectOfType(typeof(PrefabManager));
            }
            return m_Instance;
        }
    }

}