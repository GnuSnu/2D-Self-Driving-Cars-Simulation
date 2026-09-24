using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

/*
 * NOTE: This codebase was written when I was 15 years old, prior to my involvement
 * in competitive programming and interest in code optimalization. 
 * My coding standards, software architecture, and optimization techniques have evolved since then.
 */


public class Simulation : MonoBehaviour
{
    private float MaxSpeed = 0.7f;
    private float Acc = 0.005f;
    private float Speed = 0;
    private float friction = 0.003f;
    private float Angle = 0;
    private float RayLenght = 16f;
    private string PlayerPrefsConnections = "";
    public Transform CarObj;
    public Transform ConnectionObj;
    public LayerMask LayerToHit;
    public int Points=0;
    public int LastPoints = 0;
    public int CompletedLaps = 0;
    public int NotMovingCounter = 0;
    public float Lifetime = 0f;
    private GameObject CheckpointsParent;
    private GameObject Manager;
    private GameObject NeuralNetworkCanvas;

    //(DON'T CHANGE! Hardcoded values that worked the best!)
    private const int HIDDEN_NODES_COUNT = 6; 
    private const int OUTPUT_NODES_COUNT = 4;

    public List<float> DebugConnections = new List<float>();
    public List<float> inputs = new List<float>();
    public List<float> hiddenNodes = new List<float>();
    public List<float> hiddenNodes2 = new List<float>();
    public List<float> outputs = new List<float>();
    public List<float> biases = new List<float>();
    public List<List<float>> connections = new List<List<float>>();

    private List<RaycastHit2D> RayList = new List<RaycastHit2D>();

    void CreateNeuralNetwork()
    {
        if (Manager.GetComponent<Manager>().biases.Count == 0)
        {
            if (biases.Count > 0)
            {
                connections.Clear();
                biases.Clear();
                outputs.Clear();
                inputs.Clear();
                hiddenNodes.Clear();
                hiddenNodes2.Clear();
                DebugConnections.Clear();
            }
            for (int i = 0; i < HIDDEN_NODES_COUNT; i++)
            {
                connections.Add(new List<float>());
                connections.Add(new List<float>());
                connections.Add(new List<float>());
                hiddenNodes.Add(Random.Range(-1f, 1f));
                hiddenNodes2.Add(Random.Range(-1f, 1f));
                inputs.Add(0f);
                if (i < OUTPUT_NODES_COUNT)
                {
                    outputs.Add(0f);
                    biases.Add(Random.Range(-1f, 1f));
                }
            }
            foreach (var item in connections)
            {
                if (connections.IndexOf(item) < HIDDEN_NODES_COUNT)
                {
                    for (int i = 0; i < hiddenNodes.Count; i++)
                    {
                        item.Add(Random.Range(-1f, 1f));
                    }
                }
                else if(connections.IndexOf(item) >= HIDDEN_NODES_COUNT && connections.IndexOf(item) < 2*HIDDEN_NODES_COUNT)
                {
                    for (int i = 0; i < hiddenNodes2.Count; i++)
                    {
                        item.Add(Random.Range(-1f, 1f));
                    }
                }
                else
                {
                    for (int i = 0; i < outputs.Count; i++)
                    {
                        item.Add(Random.Range(-1f, 1f));
                    }
                }
        
            }
            for (int i = 0; i < connections.Count; i++)
            {
                for (int j = 0; j < connections[i].Count; j++)
                {
                    DebugConnections.Add(connections[i][j]);
                }
            }
        }
        else
        {
            
            if (inputs.Count == 0)
            {
                gameObject.name = "FirstCar!";
                for (int i = 0; i < HIDDEN_NODES_COUNT; i++)
                {
                    inputs.Add(0f);
                    hiddenNodes.Add(0f);
                    hiddenNodes2.Add(0f);
                    if (i < OUTPUT_NODES_COUNT)
                    {
                        outputs.Add(0f);
                    }
                }
                foreach (var item in GameObject.Find("_Manager").GetComponent<Manager>().biases)
                {
                    biases.Add(item);
                }
                SpawnNewBots(200);
                gameObject.GetComponent<SpriteRenderer>().color = Color.green;
                connections = new List<List<float>>(GameObject.Find("_Manager").GetComponent<Manager>().connections.ToArray());
                for (int i = 0; i < connections.Count; i++)
                {
                    for (int j = 0; j < connections[i].Count; j++)
                    {

                        if (GameObject.FindGameObjectsWithTag("Car").Length > 1)
                        {
                            connections[i][j] = Manager.GetComponent<Manager>().connections[i][j];
                        }
                        DebugConnections.Add(connections[i][j]);
                    }
                }
                for (int i = 0; i < biases.Count; i++)
                {

                    if (GameObject.FindGameObjectsWithTag("Car").Length < 1)
                    {
                        print("One car is here!");
                        gameObject.name = "FirstCar!";
                        biases[i] = Manager.GetComponent<Manager>().biases[i];
                    }
                }
            }
            else
            {
                connections.Clear();
                DebugConnections.Clear();
                if (GameObject.Find("FirstCar!") && gameObject.name != "FirstCar!")
                    connections = new List<List<float>>();
                for (int i = 0; i < GameObject.Find("FirstCar!").GetComponent<Car>().connections.Count; i++)
                {
                    connections.Add(new List<float>());
                    for (int j = 0; j < GameObject.Find("FirstCar!").GetComponent<Car>().connections[i].Count; j++)
                    {
                        connections[i].Add(new float());
                        connections[i][j] = (float)GameObject.Find("FirstCar!").GetComponent<Car>().connections[i][j];
                    }
                }
                //print(connections.SequenceEqual(GameObject.Find("FirstCar!").GetComponent<Car>().connections));
                for (int i = 0; i < connections.Count; i++)
                {
                    for (int j = 0; j < connections[i].Count; j++)
                    {
                        connections[i][j] = Mathf.Lerp(connections[i][j], Random.Range(-1f, 1f), Random.Range(0.04f,0.25f));
                        DebugConnections.Add(connections[i][j]);
                    }
                }
                for (int i = 0; i < biases.Count; i++)
                {
                    biases[i] = Mathf.Lerp(biases[i], Random.Range(-1f, 1f), Random.Range(0.04f, 0.25f));
                }
            }
        }
       

    }
    void SpawnNewBots(int botAmount)
    {
        if(GameObject.FindGameObjectsWithTag("Car").Length == 1 && !Manager.GetComponent<Manager>().ReplayBest)
        {
            for (int i = 0; i < botAmount; i++)
            {
                Instantiate(this.gameObject);
            }
        }
    }
    void UpdateNeuralNetwork()
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            if (i == 5) //special input for speed
            {
                if (MaxSpeed != 0)
                    inputs[i] = Speed / MaxSpeed;
                else
                    inputs[i] = 0;
                break;
            }
            if(RayList[i].collider != null)
            {
                inputs[i] = (RayLenght-RayList[i].distance)/RayLenght;
            }
            else
            {
                inputs[i] = 0;
            }
        }
        for (int i = 0; i < hiddenNodes.Count; i++)
        {
            float sum = 0f;
            float maxPossiblesum = 0f;
            for (int j = 0; j < inputs.Count; j++)
            {
                //print("i: " + i + " j: " + j + " sum: " + sum + " connections[j][i] " + (float)connections[j][i]);
                sum += inputs[j] * (float)connections[j][i];
                maxPossiblesum++;
            }

            hiddenNodes[i] = sum/maxPossiblesum;

        }
        for (int i = 0; i < hiddenNodes2.Count; i++)
        {
            float sum = 0f;
            float maxPossiblesum = 0f;
            for (int j = 0; j < hiddenNodes.Count; j++)
            {
                //print("i: " + i + " j: " + j + " sum: " + sum + " connections[j][i] " + (float)connections[j][i]);
                sum += hiddenNodes[j] * (float)connections[j+inputs.Count][i];
                maxPossiblesum++;
            }

            hiddenNodes2[i] = sum / maxPossiblesum;

        }
        for (int i = 0; i < outputs.Count; i++)
        {
            float sum = 0f;
            float maxPossiblesum = 0f;
            for (int j = 0; j < hiddenNodes2.Count; j++)
            {
                //print("i: " + i + " j: " + j + " sum: " + sum + " connections[j][i] " + (float)connections[j][i]);
                sum += hiddenNodes2[j] * (float)connections[j+inputs.Count+hiddenNodes.Count][i];
                maxPossiblesum += 1;
            }
            sum += biases[i];
            if (sum >= biases[i])
            {
                outputs[i] = 1;
                //print("Output[" + i + "] is ON!");
            }
            else
            {
                outputs[i] = 0;
            }

        }

    }


    void ManageRays()
    {
        
        if (gameObject.GetComponent<SpriteRenderer>().color != new Color(0.5f, 0.5f, 0.5f, 0.1f))
        {
            RayList.Clear();
            Vector2 LeftCross = (transform.up - transform.right).normalized;
            Vector2 RightCross = (transform.up + transform.right).normalized;
            Vector2[] directions = new Vector2[5]{transform.up,transform.right,-transform.right,LeftCross,RightCross};
            for (int i = 0; i < 5; i++)
            {
                //print(i + "+" + dir);
                Ray2D ray = new Ray2D(transform.position, directions[i]);
                RaycastHit2D hitInfo = Physics2D.Raycast(ray.origin, ray.direction, RayLenght, LayerToHit);
                RayList.Add(hitInfo);
                if (hitInfo.collider != null)
                {
                    Debug.DrawLine(ray.origin, hitInfo.point, Color.gray);
                    Debug.DrawLine(hitInfo.point, hitInfo.point + ray.direction * (RayLenght - (ray.origin - hitInfo.point).magnitude), Color.red);
                }
                else
                {
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * RayLenght, Color.green);
                }
            }
        }
    }

    void Start()
    {
        Manager = GameObject.Find("_Manager");
        CheckpointsParent = GameObject.Find("Checkpoints");
        Physics2D.queriesStartInColliders = false;
        CreateNeuralNetwork();
        SpawnNewBots(200);
        StartCoroutine(Move());
        StartCoroutine(SetDebugConnections());
    }

    IEnumerator SetDebugConnections()
    {
        yield return new WaitForSecondsRealtime(1f);
        DebugConnections.Clear();
        for (int i = 0; i < connections.Count; i++)
        {
            for (int j = 0; j < connections[i].Count; j++)
            {
                DebugConnections.Add(connections[i][j]);
            }
        }
    }
    void Update()
    {
        ManageRays();
        UpdateNeuralNetwork();
        if(GetComponent<SpriteRenderer>().color == Color.green)
        {
            if(GameObject.FindGameObjectsWithTag("Car").Length == 1 && !Manager.GetComponent<Manager>().ReplayBest)
            {
                DieCar();

            }
        }
    }
    public void DrawOnCanvas()
    {
        NeuralNetworkCanvas = GameObject.Find("NeuralNetworkCanvas");
        if (biases.Count > 0)
        {
            List<List<float>> LayersValueSource = new List<List<float>>();
            LayersValueSource.Add(inputs);
            LayersValueSource.Add(hiddenNodes);
            LayersValueSource.Add(hiddenNodes2);
            LayersValueSource.Add(outputs);
            for(int i =0;i<HIDDEN_NODES_COUNT-2;i++)
            {
                for (int j = 0; j < NeuralNetworkCanvas.transform.GetChild(1+i).childCount; j++)
                {
                    NeuralNetworkCanvas.transform.GetChild(1+i).GetChild(j).GetComponent<SpriteRenderer>().color = Color.Lerp(Color.yellow, Color.red, LayersValueSource[i][j]);
                }
            }

            for (int i = 0; i < NeuralNetworkCanvas.transform.GetChild(5).childCount; i++)
            {
                NeuralNetworkCanvas.transform.GetChild(5).GetChild(i).GetComponent<SpriteRenderer>().color = Color.Lerp(Color.yellow, Color.red, (biases[i] + 1f) / (1f + 1f));
            }

            for (int i = 0; i < NeuralNetworkCanvas.transform.GetChild(6).childCount; i++)
            {
                Destroy(NeuralNetworkCanvas.transform.GetChild(6).transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < connections.Count; i++)
            {
                for (int j = 0; j < connections[i].Count; j++)
                {
                    if (i < HIDDEN_NODES_COUNT)
                    {
                        Transform obj = Instantiate(ConnectionObj, Vector3.Lerp(NeuralNetworkCanvas.transform.GetChild(1).GetChild(i).transform.position, NeuralNetworkCanvas.transform.GetChild(2).GetChild(j).transform.position, 0.5f), Quaternion.identity, NeuralNetworkCanvas.transform.GetChild(6).transform);
                        Vector3 difference = NeuralNetworkCanvas.transform.GetChild(2).GetChild(j).transform.position - obj.position;
                        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                        obj.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
                        obj.localScale = new Vector3(Vector3.Distance(NeuralNetworkCanvas.transform.GetChild(1).GetChild(i).transform.position, NeuralNetworkCanvas.transform.GetChild(2).GetChild(j).transform.position), obj.localScale.y, obj.localScale.z);
                        obj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.yellow, Color.red, (connections[i][j] + 1f) / (1f + 1f));
                        obj.name = "i: " + i + " j: " + j;
                    }
                    else if(i>= HIDDEN_NODES_COUNT && i< 2*HIDDEN_NODES_COUNT)
                    {
                        Transform obj = Instantiate(ConnectionObj, Vector3.Lerp(NeuralNetworkCanvas.transform.GetChild(2).GetChild(i- HIDDEN_NODES_COUNT).transform.position,NeuralNetworkCanvas.transform.GetChild(3).GetChild(j).transform.position, 0.5f),Quaternion.identity, NeuralNetworkCanvas.transform.GetChild(6).transform);
                        Vector3 difference = NeuralNetworkCanvas.transform.GetChild(3).GetChild(j).transform.position - obj.position;
                        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                        obj.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
                        obj.localScale = new Vector3(Vector3.Distance(NeuralNetworkCanvas.transform.GetChild(2).GetChild(i- HIDDEN_NODES_COUNT).transform.position, NeuralNetworkCanvas.transform.GetChild(3).GetChild(j).transform.position), obj.localScale.y, obj.localScale.z);
                        obj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.yellow, Color.red, (connections[i][j] + 1f) / (1f + 1f));
                        obj.name = "i: " + i + " j: " + j;
                    }
                    else
                    {
                        Transform obj = Instantiate(ConnectionObj, Vector3.Lerp(NeuralNetworkCanvas.transform.GetChild(3).GetChild(i - 2*HIDDEN_NODES_COUNT).transform.position, NeuralNetworkCanvas.transform.GetChild(4).GetChild(j).transform.position, 0.5f), Quaternion.identity, NeuralNetworkCanvas.transform.GetChild(6).transform);
                        Vector3 difference = NeuralNetworkCanvas.transform.GetChild(4).GetChild(j).transform.position - obj.position;
                        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                        obj.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
                        obj.localScale = new Vector3(Vector3.Distance(NeuralNetworkCanvas.transform.GetChild(3).GetChild(i - 2*HIDDEN_NODES_COUNT).transform.position, NeuralNetworkCanvas.transform.GetChild(4).GetChild(j).transform.position), obj.localScale.y, obj.localScale.z);
                        obj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.yellow, Color.red, (connections[i][j] + 1f) / (1f + 1f));
                        obj.name = "i: " + i + " j: " + j;
                    }
                        

                }
            }
        }
    }
    public void DieCar()
    {
        Lifetime = Time.timeSinceLevelLoad;
        StopAllCoroutines();
        //print("Amount of cars: " + GameObject.FindGameObjectsWithTag("Car").Length);
        if (GameObject.FindGameObjectsWithTag("Car").Length > 1)
        {
            gameObject.name = "DedCar";
            gameObject.tag = "DedCar";
            GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0.5f);
            GameObject Child = transform.Find("front").gameObject;
            Child.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        }
        else
        {
            //if (Manager.GetComponent<Manager>().biases.Count == 0)
            //{
            gameObject.tag = "DedCar";
            gameObject.name = "LAST CAR";
            GameObject[] Cars = GameObject.FindGameObjectsWithTag("DedCar");
            List<float> Bestbiases = Manager.GetComponent<Manager>().biases;
            List<List<float>> Bestconnections = Manager.GetComponent<Manager>().connections;
            int BestPoints = Manager.GetComponent<Manager>().BestPoints;
            float BestLifetime = Manager.GetComponent<Manager>().BestLifetime;
            print("Last car is DEAD!!! ManagerBestPoints: " + BestPoints +" Manager best lifetime: " + BestLifetime);
            foreach (var item in Cars)
            {
                // print("item.Points: " + item.GetComponent<Car>().Points);
                if (item.GetComponent<Car>().Points/item.GetComponent<Car>().Lifetime > (float)(BestPoints/BestLifetime) || item.GetComponent<Car>().Points > BestPoints+GameObject.Find("Checkpoints").transform.childCount/15)
                {
                    BestPoints = item.GetComponent<Car>().Points;
                    BestLifetime = item.GetComponent<Car>().Lifetime;
                    Bestbiases = item.GetComponent<Car>().biases;
                    Bestconnections = item.GetComponent<Car>().connections;
                    /*DebugConnections.Clear();
                    for (int i = 0; i < Bestconnections.Count; i++)
                    {
                        for (int j = 0; j < Bestconnections[i].Count; j++)
                        {
                            DebugConnections.Add(Bestconnections[i][j]);
                        }
                    }*/
                }
            }
            if(connections!= Bestconnections)
            {
                Manager.GetComponent<Manager>().BestLifetime = BestLifetime;
                Manager.GetComponent<Manager>().BestPoints = BestPoints;
                Manager.GetComponent<Manager>().biases = Bestbiases;
                Manager.GetComponent<Manager>().connections = Bestconnections;
                PlayerPrefs.SetFloat("BestLifetime", BestLifetime);
                PlayerPrefs.SetInt("BestPoints", BestPoints);
                PlayerPrefs.SetString("Biases", string.Join("|", Bestbiases.ToArray()));
                print("SET BestLifetime, BestPoints, Biases!");
                
                for (int i = 0; i < Bestconnections.Count; i++)
                {
                    for (int j = 0; j < Bestconnections[i].Count; j++)
                    {
                        PlayerPrefsConnections += Bestconnections[i][j] + "|";
                    }
                }
                PlayerPrefsConnections.Remove(PlayerPrefsConnections.Length - 1);
                PlayerPrefs.SetString("Connections",PlayerPrefsConnections);
                print("SET Connections!!!!!!!!!");
            }

            //}
            //print(Points - CompletedLaps * (CheckpointsParent.transform.childCount - 1)+" checkpoint name: "+ CheckpointsParent.transform.GetChild(Points - CompletedLaps * (CheckpointsParent.transform.childCount - 1)).gameObject.name);
            if (!Manager.GetComponent<Manager>().ReplayBest)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
                
            
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag== "Wall")
        {
            DieCar();
        }
        if (col.gameObject.tag == "Checkpoint")
        {
            
            if (col.gameObject.name == CheckpointsParent.transform.GetChild(Points-CompletedLaps*CheckpointsParent.transform.childCount).gameObject.name)
            {
                Points++;
            }
            if (Points > 0)
            {
                if (col.gameObject.name == CheckpointsParent.transform.GetChild((Points - CompletedLaps * CheckpointsParent.transform.childCount)-1).gameObject.name && Points - CompletedLaps * CheckpointsParent.transform.childCount >= CheckpointsParent.transform.childCount) { CompletedLaps++; }
            }
            
        }
    }

    IEnumerator Move()
    { 
        while (true)
        {

            if (NotMovingCounter > 500)
            {
                DieCar();
            }

            if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
            {
                if (Mathf.Abs(Speed) >= friction)
                {
                    if (Speed > 0)
                    {
                        Speed -= friction;
                    }
                    else
                    {
                        Speed += friction;
                    }

                }
                else
                {
                    Speed = 0;
                }
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                outputs[0] = 0;
            }
            if (Input.GetKey(KeyCode.DownArrow)){
                outputs[1] = 0;
            }
            if (Input.GetKey(KeyCode.UpArrow) || outputs[0]==1)
            {
                if (Speed < MaxSpeed) { Speed += Acc; }
            }
            if (Input.GetKey(KeyCode.DownArrow) || outputs[1] == 1)
            {
                if (Speed <= MaxSpeed) { Speed -= Acc; }
            }
            if (Input.GetKey(KeyCode.RightArrow) || outputs[2] == 1)
            {
                Angle -= 15f*Speed;
            }
            if (Input.GetKey(KeyCode.LeftArrow) || outputs[3] == 1)
            {
                Angle +=15f*Speed;
            }

            

            if (Angle >= 360) { Angle -= 360; }
            if (Angle <= -360) { Angle += 360; }
            if (Speed > MaxSpeed) { Speed = MaxSpeed; } else if (Speed < -MaxSpeed) { Speed = -MaxSpeed; }
            CarObj.eulerAngles = Vector3.forward * Angle;
            CarObj.Rotate(0, 0, -90);
            //CarObj.position += transform.up * Speed;
            CarObj.position += new Vector3((Mathf.Cos(Angle * Mathf.PI / 180)) * Speed, (Mathf.Sin(Angle * Mathf.PI / 180) * Speed));
            yield return new WaitForSeconds(0.005f);

            if (LastPoints == Points)
            {
                NotMovingCounter++;
            }
            else
            {
                NotMovingCounter = 0;
            }
            LastPoints = Points;
        }
        
    }

}
