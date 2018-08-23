using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }

    public Action OnHouseCountChanged;
    public Action<string> OnCameraMove;
    public Action<string> OnCameraMoveClear;

    public void GameOver()
    {
        Debug.Log("Game Over");
        Application.Quit();
    }
}
