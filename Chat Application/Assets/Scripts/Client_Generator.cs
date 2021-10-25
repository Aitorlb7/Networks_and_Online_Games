using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Client_Generator : MonoBehaviour
{
    public GameObject clientExample;
    public Text clientName;

    void Start()
    {

    }

    void Update()
    {

    }

    public void GenerateClient()
    {
        GameObject newClient = GameObject.Instantiate(clientExample);
        newClient.SetActive(true);
    }
}
