using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 2f;
    public bool inputB = false;

   void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.B))
            inputB = !inputB;
        if (inputB==true && SceneManager.GetActiveScene().name == "Main_Menu")
        {
            StartCoroutine(LoadLevel("PortalScene"));
        }
        
    }
    IEnumerator LoadLevel(string levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }
}