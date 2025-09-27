using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField] private GameObject pause;
    private bool isPause =false;
    [SerializeField] private MenuManager menuManager;

    private void Start()
    {
        if(pause != null)
        {
            pause.SetActive(true);
            pause.SetActive(false);
        }

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPause)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Mapa");
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void BackMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Menu");
    }
    public void Resume()
    {
        menuManager.Pause();
        Time.timeScale = 1.0f;
        isPause = false;
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }
    public void Pause()
    {
        menuManager.Pause();
        Time.timeScale = 0.0f;
        isPause = true;
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
    }

    public void Die()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Lose");

    }


}
