using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 2f;
    public bool inputB = false;
    private bool singleton = false;
    private bool single_load = false;
    private AsyncOperation _asyncLoad;

    private void Start()
    {
/*
        _asyncLoad = SceneManager.LoadSceneAsync("PortalScene");
        _asyncLoad.allowSceneActivation = false;
*/
    }
    void Update()
    {
        if (!single_load)
        {
            StartCoroutine(PreLoad());
            single_load = true;
        }
        if (OVRInput.GetDown(OVRInput.RawButton.B))
            inputB = !inputB;
        if (inputB==true && SceneManager.GetActiveScene().name == "Main_Menu" && singleton==false)
        {
            StartCoroutine(LoadLevel("PortalScene"));
            singleton = true;
        }
        
    }
    IEnumerator PreLoad()
    {
        yield return new WaitForSeconds(0.1f);
        _asyncLoad = SceneManager.LoadSceneAsync("PortalScene");
        _asyncLoad.allowSceneActivation = false;
        yield return new WaitForSeconds(0);
    }
    IEnumerator LoadLevel(string levelIndex)
    {
        Debug.Log("SONO QUI AIUTO");
        transition.SetTrigger("Start");

        //yield return new WaitForSeconds(transitionTime);
        yield return new WaitUntil(() => _asyncLoad.progress >=0.9f);
        _asyncLoad.allowSceneActivation = true;
        //SceneManager.LoadScene(levelIndex);
    }
}