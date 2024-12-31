using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;



public class DestroyItems : MonoBehaviour
{
    public ParticleSystem effect;
    public MeshRenderer meshRenderer;
    public bool once = true;
    public static DestroyItems Instance { get; private set; }
    public void Awake()
    {
        Instance = this;
    }


    // Method to remove items when touched by Paddle 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Paddle" && once)//check name of the Collider
        {
            if ((SceneManager.GetActiveScene().buildIndex) - 1 == 2 || (SceneManager.GetActiveScene().buildIndex) - 1 == 4 || (SceneManager.GetActiveScene().buildIndex) - 1 == 6)//check if Level is cognitiv 
            {
                //Remove Blue Item
                Destroy(GameObject.FindGameObjectWithTag("BlueSphere")); 
            }
            //define emission of effect particles 
            var em = effect.emission;
            //define time period
            var dur = effect.main.duration;
            //Play effect
            Instantiate(effect, transform.position, transform.rotation);
            em.enabled = true;
            //Debug.Log("Effect played and em = " + em.enabled);            
            once = false;
            Destroy(meshRenderer);

            //Call IEnumerator with time offset to instantiate new Item
            StartCoroutine(delayedInstantiation(1f));
            IEnumerator delayedInstantiation(float time)
            {
                //Time offset 
                yield return new WaitForSeconds(time);
                //Call Spawning to instantiate new Item
                PrefabManager.Instance.Spawning();
                //Debug.Log("Timer");
            }

            //Call IEnumerator with time offset to destroy item that was triggered
            StartCoroutine(destroyObj(2f, tag));

            IEnumerator destroyObj(float time, string tag)
            {
                //Time offset
                yield return new WaitForSeconds(time); 
                //Destroy item
                Destroy(gameObject);
                //check if Level is cognitiv
                if ((SceneManager.GetActiveScene().buildIndex) - 1 == 2) 
                {
                    //If so, also destroy the second item
                    Destroy(GameObject.FindGameObjectWithTag("YellowSphere"));
                }
            }
            //Count Score
            ScoreManager.Instance.IncreaseScore(1);
        }
    }
}

