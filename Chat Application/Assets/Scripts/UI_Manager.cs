using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
     public GameObject user;
    public GameObject chatUI;
    public void OnCreateUser(string _name)
    {
        GameObject.Find("EnterName").SetActive(false);

        user.SetActive(true);

        chatUI.SetActive(true);
        
    }
}
