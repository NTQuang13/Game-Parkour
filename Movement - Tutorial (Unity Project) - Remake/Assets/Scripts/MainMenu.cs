using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;

public class MainMenu : MonoBehaviour
{

    [SerializeField] GameObject NW;
    [SerializeField] GameObject Cam;
    [SerializeField] GameObject Audio;
    [SerializeField] GameObject Event;
    [SerializeField] GameObject Menu;
    [SerializeField] GameObject Mess;
    // Start is called before the first frame update
    private bool pass=false;
    public bool connected = false;
    private void Start()
    {
        NW.GetComponent<NetworkCommunicator>().OnReceiveServerMessage += MainMenu_OnReceiveServerMessage;
        
    }

    private void MainMenu_OnReceiveServerMessage(object sender, NetworkCommunicator.OnReceiveServerMessageEventArgs e)
    {
        if (e.message == "ALREADY")
        {
            TextMeshProUGUI TMP = Mess.GetComponentInChildren<TextMeshProUGUI>();
            TMP.text = "ALREADY";
            connected = true;

        }
        else
        {
            connected= true;
            UnityEngine.Debug.Log("Passed");
            pass=true;
        }
    }

    public void PlaySingle()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public async void  Join()
    {
        
        
        NW.SetActive(true);

        /*if ()
        {
        
        }*/
        

        
        
        
    }
    public async Task Proccess()
    {
        while (!connected)
        {
           
            
        }
        UnityEngine.Debug.Log("done waiting");
    }
    private void Update()
    {
        if (connected)  
        {
            if (!pass)
            {
                NW.SetActive(false);
                
            }
            else {
                Cam.SetActive(false);
                Audio.SetActive(false);
                Event.SetActive(false);
                Menu.SetActive(false);
            }
            
        }
    }
    public void Quit()
    {
        Application.Quit();
        UnityEngine.Debug.Log("Player has quit the game");
    }
}
