using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    private string[] sceneNames;

    private void Start()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        sceneNames = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(sceneNames[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene(sceneNames[1]);
        }
    }
}
