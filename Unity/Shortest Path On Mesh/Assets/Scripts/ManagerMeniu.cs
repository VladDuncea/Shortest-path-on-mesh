﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerMeniu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickStart()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void ClickMeniu()
    {

    }

    public void ClickInchide()
    {
        Application.Quit();
    }
}
