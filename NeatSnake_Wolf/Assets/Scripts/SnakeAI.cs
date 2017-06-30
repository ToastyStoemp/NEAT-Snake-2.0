using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class SnakeAI : MonoBehaviour{

    private enum Directions
    {
        Forward,
        Left,
        Right
    }

    public bool Finished;

    public GameObject SnakeHead;
    private GameObject _apple;
    private GameObject _ui;

    public Text IndexCount;
    public Text FitnessCount;
    public Text ApplesCount;
    public Image SpeciesColor;


    private Genome _myAi;

    private Vector2 _pos;
    private Vector2 _direction = new Vector2(1,0);

    public int AppleCounter;
    private Vector2 _applePos;
    private List<Vector2> _applePositions;

    private List<Vector2> _positions = new List<Vector2>();
    private List<Directions> _directions = new List<Directions>();

    private int _lifeCounter;

    private int _hungerBar; //Snake needs food, if no food die
    //We want to be able to detect if the AI got stuck somewhere, so we count the amount of ticks,
    //If it didn't get an apple recently it dies

    private readonly int _fieldSize = 3; //Radius of the field sort of speak

    public void Initialze(Genome myAi, List<Vector2> applePositions)
    {
        //Instantiate the apple, and set it to the correct parrent
        _myAi = myAi;
        _applePositions = applePositions;
        
        _apple = Instantiate(Resources.Load("Apple")) as GameObject;
        _apple.transform.parent = gameObject.transform;
        NextApple();
        DisplayAi();
    }

    private void NextApple()
    {
        _applePos = _applePositions[AppleCounter++];
        Vector2 targetPos = _applePos + new Vector2(transform.position.x, transform.position.z);
        _apple.transform.position = new Vector3(targetPos.x, 0.5f, targetPos.y);
        _hungerBar = 0;
    }

	// Update is called once per frame
	public void Tick ()
	{
	    if (Finished) return;
	    //Tick for all the Snakes

	    //Check if DEAD?
	    if (IsDead())
	    {
	        Finished = true;
	        CalcFitness();
	    }
	    else //Not Dead yet, what do we do now
	    {
	        //Set Inputs for AI
	        List<float> inputs = new List<float>();
	        //Set own Position
	        //Inputs.Add(Pos.x);
	        //Inputs.Add(Pos.y);

	        //Set Apple Postion
	        inputs.Add(_applePos.x);
	        inputs.Add(_applePos.y);

	        //Set Vissable 
	        //forward
	        Vector2 tempPos = _pos;
	        int step = 0;
	        while (!IsDead(tempPos, true))
	        {
	            tempPos += _direction;
	            step++;
	        }
	        inputs.Add(step);

	        //left 
	        Vector2 leftDir = new Vector2(_direction.y, -_direction.x);
	        tempPos = _pos;
	        step = 0;
	        while (!IsDead(tempPos, true))
	        {
	            tempPos += leftDir;
	            step++;
	        }
	        inputs.Add(step);

	        //right  
	        Vector2 rightDir = new Vector2(-_direction.y, _direction.x);
	        tempPos = _pos;
	        step = 0;
	        while (!IsDead(tempPos, true))
	        {
	            tempPos += rightDir;
	            step++;
	        }
	        inputs.Add(step);

	        //Set Inputs for teh AI
	        _myAi.SetInputs(inputs);
	        _myAi.Calculate();
	        List<float> outPuts = _myAi.GetOutputs();

	        //Analyize Outputs
	        //First Output = Should change direction
	            
	        Directions targetDirection = Directions.Forward;

	        if (outPuts[0] > 0.1f)
	        {
	            //GoForward
	            targetDirection = Directions.Forward;
	        }
	        else if (outPuts[1] > 0.1f)
	        {
	            //GoRight
	            _direction = rightDir;
	            targetDirection = Directions.Right;
	        }
	        else if (outPuts[2] > 0.1f)
	        {
	            //GoLeft
	            _direction = leftDir;
	            targetDirection = Directions.Left;
	        }


	        //Move the Head 1 Forward
	        _pos += _direction;
	        transform.GetChild(0).transform.position = new Vector3(_pos.x, 0.5f, _pos.y) + transform.position;

	        _positions.Add(_pos);
	        _directions.Add(targetDirection);

	        //Check if we ate the apple!
	        if (_pos == _applePos)
	            NextApple();

	        _hungerBar++;
	        _lifeCounter++; //Amount of ticks alive


	        UpdateUi();
	    }
	}

    public void NextVersion(Genome myAi, List<Vector2> appleSpawnPoints)
    {
        _myAi = myAi;
        AppleCounter = 0;
        _applePositions = appleSpawnPoints;
        NextApple();
        _hungerBar = 0;
        _lifeCounter = 0;
        _direction = new Vector2(1, 0);
        _pos = new Vector2();
        transform.GetChild(0).transform.position = new Vector3(0, 0.5f, 0) + transform.position;
        Finished = false;

        _positions = new List<Vector2>();
        _directions = new List<Directions>();

        Destroy(_ui);
        DisplayAi();
    }

    bool IsDead()
    {
        return IsDead(_pos);
    }

    bool IsDead(Vector2 pos, bool skipLoopCheck = false)
    {
        bool alive = !(pos.x > _fieldSize || pos.x < -_fieldSize || pos.y > _fieldSize || pos.y < -_fieldSize);
        if (alive)
            alive = !(_hungerBar >= 15 + 8 * AppleCounter);
        if (alive && !skipLoopCheck && _directions.Count >= 5 && _hungerBar > 5)
        {
            int dirCount = _directions.Count-1;
            if (_directions[dirCount] == _directions[dirCount - 1] &&
                _directions[dirCount - 2] == _directions[dirCount - 3] &&
                _directions[dirCount ] == _directions[dirCount - 2])
            {
                alive = !(pos == _positions[_positions.Count - 5]);
            }
        }
        return !alive;
    }

    public void CalcFitness()
    {
        _myAi.FitnessCalc(-(Mathf.Abs(_applePos.x - _pos.x) + Mathf.Abs(_applePos.y - _pos.y)) + 10 * AppleCounter);
    }

    public void DisplayAi()
    {
        float leftOffset = 5.0f;
        float nodeDistance = 1.0f;

        int hiddenNodeCount = _myAi.NodeCollection.Count - Parameters.NumInputs - Parameters.NumOutputs;

        _ui = new GameObject();
        _ui.transform.parent = gameObject.transform;
        
        List<Vector3> nodePositions = new List<Vector3>();

        //Print Input Nodes;
        for (int i = 0; i < Parameters.NumInputs; i++)
        {
            GameObject input = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            input.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            input.transform.parent = _ui.transform;
            input.GetComponent<Renderer>().material.color = Color.red;
            Vector3 myPos = new Vector3(leftOffset, 0, transform.position.z + 2.0f - i * nodeDistance);
            input.transform.position = myPos;
            nodePositions.Add(myPos);
        }

        //Print Output Nodes;
        float outPutOffset = leftOffset + nodeDistance + hiddenNodeCount * nodeDistance;
        outPutOffset = outPutOffset > (leftOffset + 5 * nodeDistance) ? outPutOffset : (leftOffset + 5 * nodeDistance);
        for (int i = 0; i < Parameters.NumOutputs; i++)
        {
            GameObject input = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            input.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            input.transform.parent = _ui.transform;
            input.GetComponent<Renderer>().material.color = Color.green;
            Vector3 myPos = new Vector3(outPutOffset, 0, transform.position.z - i * nodeDistance);
            input.transform.position = myPos;
            nodePositions.Add(myPos);
        }

        //Print Hidden Nodes;
        for (int i = 0; i < hiddenNodeCount; i++)
        {
            GameObject input = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            input.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            input.transform.parent = _ui.transform;
            input.GetComponent<Renderer>().material.color = Color.cyan;
            Vector3 myPos = new Vector3(leftOffset + nodeDistance + nodeDistance * i, 0, FindHiddenNodePos(i, nodePositions));
            input.transform.position = myPos;
            nodePositions.Add(myPos);
        }

        //Print the Links
        foreach (Link link in _myAi.LinkNetwork)
        {
            GameObject lineObj = new GameObject("Line");
            lineObj.transform.parent = _ui.transform;
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            if (!link.Enabled)
            {
                line.startColor = Color.black;
                line.endColor = Color.black;
            }
            else
            {
                line.startColor = link.Weight > 0 ? Color.green : Color.red;
                line.endColor = link.Weight > 0 ? Color.green : Color.red;
            }
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.SetPosition(0, nodePositions[link.In.NodeIndex]);
            line.SetPosition(1, nodePositions[link.Out.NodeIndex]);
        }
    }

    private void UpdateUi()
    {
        IndexCount.text = _myAi.Index.ToString() + " " + _myAi.SpeciesIndex.ToString();
        CalcFitness();
        FitnessCount.text = _myAi.Fitness.ToString();
        ApplesCount.text = (AppleCounter - 1).ToString() + " " + _hungerBar.ToString() + " " + (19 + 3 * AppleCounter).ToString();

        switch (_myAi.SpeciesIndex % 8)
        {
            case 0:
                SpeciesColor.color = Color.yellow;
                break;
            case 1:
                SpeciesColor.color = Color.black;
                break;
            case 2:
                SpeciesColor.color = Color.blue;
                break;
            case 3:
                SpeciesColor.color = Color.cyan;
                break;
            case 4:
                SpeciesColor.color = Color.gray;
                break;
            case 5:
                SpeciesColor.color = Color.green;
                break;
            case 6:
                SpeciesColor.color = Color.magenta;
                break;
            case 7:
                SpeciesColor.color = Color.red;
                break;
        }
    }

    float FindHiddenNodePos(int nodeIndex, List<Vector3> nodePositions )
    {
        float zPos = 0;
        int inputCounter = 0;
        foreach (Link link in _myAi.LinkNetwork)
        {
            if (link.Out.NodeIndex != nodeIndex + Parameters.NumInputs + Parameters.NumOutputs) continue;
            if (link.In.NodeIndex >= nodePositions.Count) continue;
            zPos += nodePositions[link.In.NodeIndex].z;
            inputCounter++;
        }
        
        return zPos / inputCounter;
    }
}
