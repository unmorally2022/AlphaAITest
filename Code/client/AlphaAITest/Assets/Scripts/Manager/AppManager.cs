using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


public static class AppManager
{
    public static string PlayerIOid;
    public static string PlayerIOName;

    public enum CharacterState
    {
        notready, ready, finish
    }

    public enum GameplayState
    {
        iddle,//when user first run and didnt do anything yet
        pause,//when game paused
        play//when user playing the gamne        
    }
    public static GameplayState gameplayState;

    static AppManager()
    {
        PlayerIOid = "PL_AlphaAI_" + System.DateTime.UtcNow.ToString(@"yyyyMddhhmmss");
        Debug.Log(string.Format("Application Manager {0}", PlayerIOid));        
    }

    public static string RandomString(int length)
    {
        System.Random random = new System.Random();
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static IEnumerator LoadYourAsyncScene(string SceneName)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {            
            //Debug.Log(asyncLoad.progress);
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

    }
    
}
