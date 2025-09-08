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
            PlayerController.Instance.SetCheckPointPos(_checkpoints[0].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[1].position);
            //StartCoroutine(PlayerTDController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[1]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[2].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[2]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[3].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[3]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[4].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[4]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[5].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[4]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[6].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer());
            //SceneManager.LoadScene(sceneNames[4]);
        }
        //else if (Input.GetKeyDown(KeyCode.Alpha8))
        //{
        //    PlayerTDController.Instance.SetCheckPointPos(_checkpoints[7].position);
        //    StartCoroutine(PlayerTDController.Instance.RespawnPlayer());
        //    //SceneManager.LoadScene(sceneNames[4]);
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha9))
        //{
        //    PlayerTDController.Instance.SetCheckPointPos(_checkpoints[1].position);
        //    //StartCoroutine(PlayerTDController.Instance.RespawnPlayer());
        //    //SceneManager.LoadScene(sceneNames[1]);
        //}
    }
}
