using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private List<Transform> _checkpoints = new List<Transform>();

    private void Start()
    {
        //PlayerController.Instance.OnPlayerFell += RestarLevel;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[0].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer(CauseOfDeath.Teleport));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[1].position);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[2].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer(CauseOfDeath.Teleport));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[3].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer(CauseOfDeath.Teleport));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[4].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer(CauseOfDeath.Teleport));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[5].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer(CauseOfDeath.Teleport));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[6].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer(CauseOfDeath.Teleport));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[7].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer(CauseOfDeath.Teleport));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            PlayerController.Instance.SetCheckPointPos(_checkpoints[1].position);
            StartCoroutine(PlayerController.Instance.RespawnPlayer(CauseOfDeath.Teleport));
        }
    }

    public void RestarLevel()
    {
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }
}
