using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 2f;

   void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.B) && SceneManager.GetActiveScene().buildIndex==0)
            StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
        
    }
    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }
}
