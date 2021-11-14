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
            Debug.LogError("Could not generate a new client! Error: Instance of ClientExample GO did not have the ChatClient script.");
            Destroy(newClient);
            return;
        }

        clientScript.name = (InvalidClientName()) ? ("User" + Random.Range(0, 999999)) : clientName.text;

        clientName.text = "Enter Client Name";
        newClient.SetActive(true);   
    }

    private bool InvalidClientName()
    {
        return (clientName.text == "" || clientName.text == "Enter Client Name");                               // Wrote this method to prepare the name filters for banned words.
    }
}
