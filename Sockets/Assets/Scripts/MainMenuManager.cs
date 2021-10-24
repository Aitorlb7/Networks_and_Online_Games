using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject eventSystemGO;
    
    void Start()
    {
        Debug.Log("Welcome to Aitor's and Angel's UDP/TCP Exercise!");
    }
    
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }
    
    public void UDPButton()
    {
        SceneManager.LoadScene("UDPClient"/*, LoadSceneMode.Additive*/);
        SceneManager.LoadScene("UDPServer", LoadSceneMode.Additive);

        eventSystemGO.SetActive(false);
    }

    public void TCPButton()
    {
        SceneManager.LoadScene("TCPClient"/*, LoadSceneMode.Additive*/);
        SceneManager.LoadScene("TCPServer", LoadSceneMode.Additive);

        eventSystemGO.SetActive(false);
    }
    
    public void ExitButton()
    {
        Application.Quit();
    }
}
