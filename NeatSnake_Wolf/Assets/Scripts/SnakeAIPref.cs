using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[System.Serializable]
public class SnakeAIPref {

    private enum Directions
    {
        Forward,
        Left,
        Right
    }

    public bool Finished;
    
    private Genome _myAi;

    private Vector2 _pos;
    private Vector2 _direction = new Vector2(1, 0);

    public int AppleCounter;
    private Vector2 _applePos;
    private List<Vector2> _applePositions;

    private List<Vector2> _positions = new List<Vector2>();
    private List<Directions> _directions = new List<Directions>();

    private int _lifeCounter;

    private int _hungerBar; //Snake needs food, if no food die
    //We want to be able to detect if the AI got stuck somewhere, so we count the amount of ticks,
    //If it didn't get an apple recently it dies

    private int _fieldSize = 3; //Radius of the field sort of speak

    public void Initialze(Genome myAi, List<Vector2> applePositions)
    {
        //Instantiate the apple, and set it to the correct parrent
        _myAi = myAi;
        //ApplePositions = applePositions;
        _applePositions = applePositions;


        NextApple();
        Thread tick = new Thread(Tick);
        tick.Start();
    }

    void NextApple()
    {
        _applePos = _applePositions[AppleCounter++];
        _hungerBar = 0;
    }

    // Update is called once per frame
    public void Tick()
    {
        while (!Finished)
        {
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
                
                _positions.Add(_pos);
                _directions.Add(targetDirection);

                //Check if we ate the apple!
                if (_pos == _applePos)
                    NextApple();

                _hungerBar++;
                _lifeCounter++; //Amount of ticks alive
            }
        }
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
            int dirCount = _directions.Count - 1;
            if (_directions[dirCount] == _directions[dirCount - 1] &&
                _directions[dirCount - 2] == _directions[dirCount - 3] &&
                _directions[dirCount] == _directions[dirCount - 2])
            {
                alive = !(pos == _positions[_positions.Count - 5]);
            }
        }
        return !alive;
    }

    public void NextVersion(Genome myAi, List<Vector2> applePositions)
    {
        _myAi = myAi;
        AppleCounter = 0;
        _applePositions = applePositions;
        NextApple();
        _hungerBar = 0;
        _lifeCounter = 0;
        _direction = new Vector2(1, 0);
        _pos = new Vector2();
        Finished = false;

        _positions = new List<Vector2>();
        _directions = new List<Directions>();

        Thread tick = new Thread(Tick);
        tick.Start();
    }

    public void CalcFitness()
    {
        _myAi.FitnessCalc(-(Mathf.Abs(_applePos.x - _pos.x) + Mathf.Abs(_applePos.y - _pos.y)) + 10 * AppleCounter);
    }
}
