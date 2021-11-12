using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Client_Generator : MonoBehaviour
{
    public GameObject   clientExample;
    public Text         clientName;

    void Start()
    {

    }

    void Update()
    {

    }

    public void GenerateClient()
    {
        GameObject newClient    = GameObject.Instantiate(clientExample);
        TCP_Client clientScript = newClient.GetComponent<TCP_Client>();
        if (clientScript == null)
        {
            Destroy(newClient);
            return;
        }

        clientScript.name = clientName.text;
        newClient.SetActive(true);   
    }
}
