using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;

public class SnakeMaster : MonoBehaviour
{

    public NeatAi Ai;

    public List<SnakeAI> SnakePool = new List<SnakeAI>();
    public List<SnakeAIPref> SnakePoolPref = new List<SnakeAIPref>();
    public bool IsRunning = false;

    public float TickTime = 0.5f;
    private float _timer;

    public bool PreformanceMode = true;

    private List<Vector2> appleSpawnPoints = new List<Vector2>();

    // Use this for initialization
    public void Start()
    {
        //Make the AI 
        Ai = new NeatAi();
        Ai.Initialize(); //Set the input size, output size, and poolsize

        //Create a list of Apple SpawnPoints
        for (int i = 0; i < 20; i++)
            appleSpawnPoints.Add(new Vector2(Mathf.Floor(Random.value * 6 - 3), Mathf.Floor(Random.value * 6 - 3)));

        //Get all the Genomes
        List<Genome> aiList = Ai.CurrentGeneration.GetUnsorted();
        for (int i = 0; i < Parameters.PoolSize; i++)
        {
            //Create the SnakeAI 
            if (PreformanceMode && i != 0)
            {
                SnakeAIPref tempAi = new SnakeAIPref();
                tempAi.Initialze(aiList[i], appleSpawnPoints);
                SnakePoolPref.Add(tempAi);
            }
            else
            {
                GameObject tempSnakeField = Instantiate(Resources.Load("SnakeField")) as GameObject;
                tempSnakeField.transform.position = new Vector3(0, 0, i * 10);
                SnakeAI tempAi = tempSnakeField.GetComponent<SnakeAI>();
                tempAi.Initialze(aiList[i], appleSpawnPoints);
                SnakePool.Add(tempAi);
            }
        }
    }

    // Update is called once per frame
    public void Update ()
    {
        //Check if all AI's are finished
	    if (CheckFinished())
	    {
	        Ai.Evolve();
	        List<Genome> aiList = Ai.CurrentGeneration.GetUnsorted();
	        int aIcounter = 0;

            if (PreformanceMode)
            {
                SnakePool[0].NextVersion(aiList[aIcounter++], appleSpawnPoints);
                foreach (SnakeAIPref snakeAi in SnakePoolPref)
                {
                    snakeAi.NextVersion(aiList[aIcounter++], appleSpawnPoints);
                }
            }
            else
            {
                foreach (SnakeAI snakeAi in SnakePool)
                {
                    snakeAi.NextVersion(aiList[aIcounter++], appleSpawnPoints);
                }
            }
            
        }
	    else //Some AI's are still alive, keep ticking
	    {
	        if (PreformanceMode)
	        {
	            SnakePool[0].Tick();
	        }
	        else
	        {
	            _timer += Time.deltaTime;
	            if (_timer > TickTime)
	            {
	                _timer = 0.0f;
	                foreach (SnakeAI snakeAi in SnakePool)
	                {
	                    snakeAi.Tick();
	                }
	            }
            }
            
        }
	}

    bool CheckFinished()
    {
        foreach (SnakeAI snakeAi in SnakePool)
        {
            if (!snakeAi.Finished)
                return false;
        }
        foreach (SnakeAIPref snakeAi in SnakePoolPref)
        {
            if (!snakeAi.Finished)
                return false;
        }
        return true;
    }
}
