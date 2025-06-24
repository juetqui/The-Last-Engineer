using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    //[SerializeField] private Animator _transition;
    [SerializeField] private float _transitionTime = 0f;

    //void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.P))
    //    {
    //        LoadNextLevel();
    //    }   
    //}

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        //_transition.SetTrigger("Start");

        yield return new WaitForSeconds(_transitionTime);

        SceneManager.LoadScene(levelIndex);
    } 
}
