using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{

    public static GameManager instance { get; private set; }
    [Header("Player")]
    public GameObject _player;
    [Header("UI")]
    public GameObject _menuPause;
    public GameObject _menuDeath;
    //public GameObject _menuVictory;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        else
            instance = this;
        Application.runInBackground = true;
        CursorToggle(false);

    }

    public GameObject Player
    {
        get
        {
            return _player;
        }
        set
        {
            _player = value;
        }
    }

    public TimeManager timeManager
    {
        get
        {
            return _player.GetComponent<TimeManager>();
        }
    }


    public void CursorToggle(bool visible)
    {
        Cursor.visible = visible;
        Player.GetComponent<Inputs>().cursorInputForLook = !visible;

        if (visible)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }
    }


    public void PauseMenu()
    {
        _menuPause.SetActive(true);
    }

    public void DeathMenu()
    {
        _menuDeath.SetActive(true);
    }

    //public void VictoryMenu()
    //{
    //    _menuVictory.SetActive(true);
    //}
}