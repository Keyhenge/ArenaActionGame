using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenuController : MonoBehaviour
{
    public string mainMenuScene;
    public string arenaScene;
    public GameObject deathMenu;
    public bool playerDied;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (playerDied)
        {
            deathMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void Retry()
    {
        Resume();
        SceneManager.LoadScene(arenaScene);
    }

    public void Return()
    {
        Resume();
        SceneManager.LoadScene(mainMenuScene);
    }

    private void Resume()
    {
        playerDied = false;
        deathMenu.SetActive(false);
        Time.timeScale = 1f;
    }
}
