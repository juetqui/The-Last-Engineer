using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private List<Transform> _checkpoints = new List<Transform>();
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
            PlayerTDController.Instance.SetCheckPointPos(_checkpoints[0].position);
            StartCoroutine(PlayerTDController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerTDController.Instance.SetCheckPointPos(_checkpoints[1].position);
            StartCoroutine(PlayerTDController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[1]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayerTDController.Instance.SetCheckPointPos(_checkpoints[2].position);
            StartCoroutine(PlayerTDController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[2]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayerTDController.Instance.SetCheckPointPos(_checkpoints[3].position);
            StartCoroutine(PlayerTDController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[3]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlayerTDController.Instance.SetCheckPointPos(_checkpoints[4].position);
            StartCoroutine(PlayerTDController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[4]);
        }
    }
}
