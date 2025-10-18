using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Transform currentPos;
    public Transform startPos;
    public Transform menuPos;
    public Transform optionsPos;
    public Transform playPos;
    public Transform creditsPos;
    public Transform startGamePos;

    [SerializeField] private Transform initialPos;
    public float speedFactor = 0.1f;
    public Image blackScreen;
    public float fadeOutDuration = 2f;
    public float movementDelay = 1f;

    private void Awake()
    {
        transform.position = initialPos.position;
        currentPos = initialPos; speedFactor = 1f;
    }

    void Start()
    {
        Move_To_Start();

        //if (blackScreen != null)
        //{
        //    blackScreen.gameObject.SetActive(true);
        //    blackScreen.color = new Color(0, 0, 0, 0);
        //}
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, currentPos.position, speedFactor);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, currentPos.rotation, speedFactor);
    }

    public void Move_To_Start()
    {
        currentPos = startPos;
    }

    public void Move_To_Menu()
    {
        speedFactor = 0.1f;
        currentPos = menuPos;
    }

    public void Move_To_Options()
    {
        currentPos = optionsPos;
    }

    public void Move_To_Credits()
    {
        currentPos = creditsPos;
    }

    public void Move_To_Play()
    {
        currentPos = playPos;
    }

    public void Move_To_StartGame()
    {
        speedFactor = 0.01f;
        currentPos = startGamePos;
        blackScreen.gameObject.SetActive(true);

        StartCoroutine(FadeOutAndLoadLevel());
    }

    private IEnumerator FadeOutAndLoadLevel()
    {
        yield return new WaitForSeconds(movementDelay);

        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeOutDuration;
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        blackScreen.color = new Color(0, 0, 0, 1);

        yield return null;

        StartLevel();
    }

    public void StartLevel()
    {
        SceneManager.LoadScene("Nivel 1 - Entregable");
    }
    public void StartTutorial2()
    {
        SceneManager.LoadScene("NIVEL gabi");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
