using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public string mainMenuScene;
    public GameObject pauseMenu;
    public GameObject deathMenu;
    public GameObject overlay;
    public bool isPaused;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !deathMenu.active)
        {
            if(isPaused)
            {
                Resume();
            }
            else
            {
                isPaused = true;
                pauseMenu.SetActive(true);
                overlay.SetActive(false);
                Time.timeScale = 0f;
            }
        }
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        overlay.SetActive(true);
        Time.timeScale = 1f;
    }

    public void Return()
    {
        Resume();
        SceneManager.LoadScene(mainMenuScene);
    }
}
