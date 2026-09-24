using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;

/*
 * NOTE: This codebase was written when I was 15 years old, prior to my involvement
 * in competitive programming and interest in code optimalization. 
 * My coding standards, software architecture, and optimization techniques have evolved since then.
 */


public class Manager : MonoBehaviour
{
    public GameObject car;
    public int AmountOfBots = 1;
    public int BestPoints=1;
    public float BestLifetime=10000;
    private static bool created = false;
    public bool ReplayBest = false;
    private string PlayerPrefsConnections = "";
    private string PlayerPrefsBiases = "";
    private string NotificationText = "";
    public int DeleteSaveCounter = 0;
    public int ShowNotificationCounter = 0;

    //(DON'T CHANGE! Hardcoded values that worked the best!)
    private const int HIDDEN_NODES_COUNT = 6;
    private const int OUTPUT_NODES_COUNT = 4;

    public List<float> biases = new List<float>();
    public List<float> hiddenNodes = new List<float>();
    public List<List<float>> connections = new List<List<float>>();
    public List<float> DebugConnections = new List<float>();
    private GameObject Notification;
    private GameObject SlotsBG;
    private GameObject BestCarCamera;
    private GameObject MainCam;
    private GameObject BestCarIndicator;

    private void Awake()
    {
        if (!created)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 240;
            DontDestroyOnLoad(this.gameObject);
            
            created = true;
        }
        else
        {
            if (biases.Count == 0)
            {
                Destroy(this.gameObject);
            }
        }
        LoadBrainFromPlayerPrefs();

    }
    void LoadSlotsUI()
    {
        SlotsBG = FindObjectsOfType<GameObject>(true).Where(item => item.name == "SlotsBG").ToArray()[0];
        int i = 1;
        print("SettingNames, " + 1.ToString() + " - " + PlayerPrefs.GetString("Slot" + 1.ToString() + "Name"));
        foreach (var item in FindObjectsOfType<GameObject>(true).Where(item => item.name == "Slot").ToArray().OrderBy(obj => obj.GetComponent<TextMeshProUGUI>().text[0]))
        {
            item.GetComponent<TextMeshProUGUI>().text = i.ToString() + " - " + PlayerPrefs.GetString("Slot" + i.ToString() + "Name");
            i++;
        }
    }
    void LoadBrainFromPlayerPrefs()
    {
        if (PlayerPrefs.GetString("Connections") != "")
        {
            connections.Clear();
            PlayerPrefsConnections = PlayerPrefs.GetString("Connections");
            string[] PlayerPrefsConnectionsArray = PlayerPrefsConnections.Replace(".", ",").Split('|');
            for (int i = 0; i < 3*HIDDEN_NODES_COUNT; i++)
            {
                connections.Add(new List<float>());
                if (i < HIDDEN_NODES_COUNT)
                {
                    for (int j = 0; j < HIDDEN_NODES_COUNT; j++)
                    {
                        connections[i].Add(float.Parse(PlayerPrefsConnectionsArray[j]));
                    }
                    PlayerPrefsConnectionsArray = PlayerPrefsConnectionsArray.Skip(6).ToArray();
                }
                else if (i >= HIDDEN_NODES_COUNT && i < 2*HIDDEN_NODES_COUNT)
                {
                    for (int j = 0; j < HIDDEN_NODES_COUNT; j++)
                    {
                        connections[i].Add(float.Parse(PlayerPrefsConnectionsArray[j]));
                    }
                    PlayerPrefsConnectionsArray = PlayerPrefsConnectionsArray.Skip(6).ToArray();
                }
                else
                {
                    for (int j = 0; j < OUTPUT_NODES_COUNT; j++)
                    {
                        connections[i].Add(float.Parse(PlayerPrefsConnectionsArray[j]));
                    }
                    PlayerPrefsConnectionsArray = PlayerPrefsConnectionsArray.Skip(4).ToArray();
                }



            }
        }

        print("plprefs biases: " + PlayerPrefs.GetString("Biases"));
        if (PlayerPrefs.GetString("Biases") != "")
        {
            if (biases.Count > 0)
                print(biases[0] + "   " + PlayerPrefs.GetString("Biases"));
            biases.Clear();
            PlayerPrefsBiases = PlayerPrefs.GetString("Biases");
            string[] PlayerPrefsBiasesArray = PlayerPrefsBiases.Replace(".", ",").Split('|');
            foreach (var item in PlayerPrefsBiasesArray)
            {
                biases.Add(float.Parse(item));
            }
            print(biases[0] + "   " + PlayerPrefs.GetString("Biases"));
        }

        if (!PlayerPrefs.HasKey("BestPoints"))
        {
            PlayerPrefs.SetInt("BestPoints", BestPoints);
            PlayerPrefs.SetFloat("BestLifetime", BestLifetime);
            print("SET BestPoints and BestLifetime!!!!");
        }
        else
        {
            BestPoints = PlayerPrefs.GetInt("BestPoints");
            BestLifetime = PlayerPrefs.GetFloat("BestLifetime");
        }
    }
    //float PercentageBetween
    // Update is called once per frame
    void Update()
    {
        if (biases.Count > 0)
        {
            if (biases[0].ToString() != PlayerPrefs.GetString("Biases").Replace(".", ",").Split('|')[0])
            {
                LoadBrainFromPlayerPrefs();
            }
        }

        //if (GameObject.FindGameObjectsWithTag("Manager").Length > 1)
        //{
        //    DrawOnCanvas();
        //}
        if (Notification == null)
        {
            Notification = FindObjectsOfType<GameObject>(true).Where(item => item.name == "Notification").ToArray()[0];
            BestCarCamera = FindObjectsOfType<GameObject>(true).Where(item => item.name == "BestCarCamera").ToArray()[0];
            MainCam = Camera.main.gameObject;
            BestCarIndicator = GameObject.Find("BestCarIndicator");
            LoadSlotsUI();
        }
            
            
        int BestActualPoints = 0;
        int BestCarIndex = 0;
        GameObject[] ArrayOfCars = GameObject.FindGameObjectsWithTag("Car");
        for (int i = 0; i < ArrayOfCars.Length; i++)
        {
            if (ArrayOfCars[i].GetComponent<Car>().Points > BestActualPoints)
            {
                BestActualPoints = ArrayOfCars[i].GetComponent<Car>().Points;
                BestCarIndex = i;
            }
            if (ArrayOfCars[i].gameObject.name == "BestCar!")
            {
                ArrayOfCars[i].gameObject.name = "FormerBest";
            }
            if(i== ArrayOfCars.Length - 1)
            {
                ArrayOfCars[BestCarIndex].gameObject.name = "BestCar!";
                ArrayOfCars[BestCarIndex].GetComponent<Car>().DrawOnCanvas();
            }
        }
        if (GameObject.FindGameObjectsWithTag("Car").Length+ GameObject.FindGameObjectsWithTag("DedCar").Length < 1 )
        {
            Instantiate(car, new Vector3(-325, -50, 0), Quaternion.identity);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene((buildIndex+1)%3);
        }
        if (Input.GetKeyDown(KeyCode.R) && ShowNotificationCounter ==0)
        {
            NotificationText = "Replay Mode set to: " +ReplayBest;
            ShowNotificationCounter = 300;
            ReplayBest = !ReplayBest;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            MainCam.SetActive(!MainCam.activeInHierarchy);
            BestCarCamera.SetActive(!BestCarCamera.activeInHierarchy);
        }
        if (BestCarCamera.activeInHierarchy)
        {
            BestCarCamera.transform.position = Vector3.Lerp(BestCarCamera.transform.position, new Vector3(GameObject.Find("BestCar!").transform.position.x, GameObject.Find("BestCar!").transform.position.y,-10),20f*Time.deltaTime);
            if( GameObject.FindGameObjectsWithTag("Car").Length > 1)
                BestCarIndicator.transform.position = new Vector3(GameObject.Find("BestCar!").transform.position.x, GameObject.Find("BestCar!").transform.position.y, -2);
        }
        else
        {
            BestCarIndicator.transform.position = new Vector3(0, 0, -100);
        }

        //////
        if (Input.GetKey(KeyCode.Backspace))
        {
            DeleteSaveCounter++;
            if (DeleteSaveCounter > 500)
            {
                PlayerPrefs.SetFloat("BestLifetime", 10000);
                PlayerPrefs.SetInt("BestPoints", 0);
                PlayerPrefs.SetString("Biases", "");
                PlayerPrefs.SetString("Connections", "");

                print("DELETED ACTIVE SAVED BRAINS!!!!");
                DeleteSaveCounter = 0;
                connections.Clear();
                biases.Clear();
                BestLifetime = 10000;
                BestPoints = 1;
                DebugConnections.Clear();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            DeleteSaveCounter = 0;
        }
        //////
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.L))
        {
            SlotsBG.SetActive(true);
        } 
        else
            SlotsBG.SetActive(false);


        if (Input.GetKey(KeyCode.S) && ShowNotificationCounter < 100)
        {
            string mode = "0";

            if (Input.GetKey(KeyCode.Alpha1)) mode = "1";
            if (Input.GetKey(KeyCode.Alpha2)) mode = "2";
            if (Input.GetKey(KeyCode.Alpha3)) mode = "3";
            if (Input.GetKey(KeyCode.Alpha4)) mode = "4";
            if (Input.GetKey(KeyCode.Alpha5)) mode = "5";
            if (Input.GetKey(KeyCode.Alpha6)) mode = "6";

            if (mode != "0")
            {
                NotificationText = "Saved slot "+ mode;
                PlayerPrefs.SetString("Slot" + mode + "Name", SceneManager.GetActiveScene().name);
                PlayerPrefs.SetFloat("BestLifetime"+mode, PlayerPrefs.GetFloat("BestLifetime"));
                PlayerPrefs.SetInt("BestPoints"+mode, PlayerPrefs.GetInt("BestPoints"));
                PlayerPrefs.SetString("Biases"+mode, PlayerPrefs.GetString("Biases"));
                PlayerPrefs.SetString("Connections"+mode, PlayerPrefs.GetString("Connections"));
                LoadSlotsUI();
                ShowNotificationCounter = 300;
            }

        }
        if (Input.GetKey(KeyCode.L) && !Input.GetKey(KeyCode.S) && ShowNotificationCounter<100)
        {

            string mode = "0";

            if (Input.GetKey(KeyCode.Alpha1)) mode = "1";
            if (Input.GetKey(KeyCode.Alpha2)) mode = "2";
            if (Input.GetKey(KeyCode.Alpha3)) mode = "3";
            if (Input.GetKey(KeyCode.Alpha4)) mode = "4";
            if (Input.GetKey(KeyCode.Alpha5)) mode = "5";
            if (Input.GetKey(KeyCode.Alpha6)) mode = "6";

            if (mode != "0")
            {
                if (PlayerPrefs.GetString("Connections"+mode) != "")
                {
                    NotificationText = "Loaded slot "+mode;
                    PlayerPrefs.SetFloat("BestLifetime", PlayerPrefs.GetFloat("BestLifetime"+mode));
                    PlayerPrefs.SetInt("BestPoints", PlayerPrefs.GetInt("BestPoints"+mode));
                    PlayerPrefs.SetString("Biases", PlayerPrefs.GetString("Biases"+mode));
                    PlayerPrefs.SetString("Connections", PlayerPrefs.GetString("Connections"+mode));
                    LoadSlotsUI();
                    ShowNotificationCounter = 300;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
        if (ShowNotificationCounter > 0)
        {
            ShowNotificationCounter--;
            if (!Notification.activeInHierarchy)
            {
                Notification.SetActive(true);
                Notification.GetComponent<TextMeshProUGUI>().text = NotificationText;
            }
                
        }
        else
        {
            if (Notification.activeInHierarchy)
                Notification.SetActive(false);
        }


        if (Time.timeSinceLevelLoad >= 30 && !ReplayBest)
        {
            foreach (var item in ArrayOfCars)
            {
                item.GetComponent<Car>().DieCar();
            }
        }
        if (DebugConnections.Count < 1)
        {
            PlayerPrefsConnections = "";
            for (int i = 0; i < connections.Count; i++)
            {
                print("manager here: connections.len:" + connections.Count + " connections[i].len: " + connections[i].Count);
                for (int j = 0; j < connections[i].Count; j++)
                {
                    DebugConnections.Add(connections[i][j]);
                    PlayerPrefsConnections += connections[i][j].ToString()+"|";
                }
            }

            if (connections.Count != 0)
            {
                PlayerPrefsConnections.Remove(PlayerPrefsConnections.Length - 1);
                PlayerPrefsBiases = "";
                foreach (var item in biases)
                {
                    PlayerPrefsBiases += item + "|";
                    PlayerPrefsBiases.Remove(PlayerPrefsBiases.Length - 1);
                }
                
                
                if (PlayerPrefs.GetString("Connections") == "")
                {
                    PlayerPrefs.SetString("Connections", PlayerPrefsConnections);
                    print("SET Connections!");
                }
                if (PlayerPrefs.GetString("Biases") == "")
                {
                    PlayerPrefs.SetString("Biases", PlayerPrefsBiases);
                    print("SET Biases");
                }
            }

            
        }

    }
}
