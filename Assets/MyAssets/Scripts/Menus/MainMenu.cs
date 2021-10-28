using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [SerializeField] MenuManager menu;

    // this class holds all options to be performed from the main menu
    // each public function is a 'OnClick' button callback

    public void StartTrainingGame()
    {
        if(!UserManagement.selfReference.LoggedIn())
        {
            menu.ShowUserSelection();
            menu.UserSelection.GetComponent<UserSelection>().loginCallback = LoadTrainingGame;
            // userCreation shall always use the same callback action as login
            menu.UserCreation.GetComponent<UserCreation>().createCallback = LoadTrainingGame;
        }
        else
        {
            LoadTrainingGame(true);
        }

    }

    public void LoadTrainingGame(bool success)
    {
        Debug.Log("Enter callback");
        if(success)
        {
            SceneManager.LoadSceneAsync("TrainingScene");
            return;
        }

        Debug.Log("Login Failed. Dont load Scene...");
    }

    public void ShowPlayerProgress()
    {
        if(!UserManagement.selfReference.LoggedIn())
        {
            menu.ShowUserSelection();
            menu.UserSelection.GetComponent<UserSelection>().loginCallback = LoadPlayerProgress;
            menu.UserCreation.GetComponent<UserCreation>().createCallback = LoadPlayerProgress;
        }
        else
        {
            LoadPlayerProgress(true);
        }
    }

    public void LoadPlayerProgress(bool success)
    {
        if (success)
        {
            menu.ShowProgess();
            return;
        }

        Debug.Log("Login Failed. Dont show player progress...");
    }

    public void QuitApp ()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

}
