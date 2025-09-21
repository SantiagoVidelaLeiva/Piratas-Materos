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
        SceneManager.LoadScene("Menu");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Resume()
    {
        pause.SetActive(false);
        Time.timeScale = 1.0f;
        isPause = false;
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }
    public void Pause()
    {
        pause.SetActive(true);
        Time.timeScale = 0.0f;
        isPause = true;
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;
    }
    public void Explanation()
    {
        SceneManager.LoadScene("Explanation");
    }
    public void Die()
    {
        SceneManager.LoadScene("Lose");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


}
