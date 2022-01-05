using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // unload all Scenes except preload
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (SceneManager.GetSceneByBuildIndex(i).isLoaded && SceneManager.GetSceneByBuildIndex(i).name != "VRLogin")
            {
                Debug.Log("Unload scene: " + i);
                SceneManager.UnloadSceneAsync(i);
            }
        }

        if(!SceneManager.GetSceneByName("VRLogin").isLoaded)
        {
            SceneManager.LoadSceneAsync("VRLogin");
        }
        
    }
}
