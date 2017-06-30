using System;
using System.Collections.Generic;
using SharpNeat.Phenomes;
using UnityEngine;
using Random = UnityEngine.Random;

public class SnakeAgent : UnitController
{

    private static List<Vector2> _applePositionList = new List<Vector2>() { new Vector2(2, -2), new Vector2(2, -1), new Vector2(0, -3), new Vector2(-1, -2), new Vector2(-1, -2), new Vector2(-1, 2), new Vector2(-2, 1), new Vector2(-2, 2), new Vector2(1, 2), new Vector2(-3, 1), new Vector2(0, 1), new Vector2(-3, -1), new Vector2(-3, -1), new Vector2(2, 0), new Vector2(-3, 2), new Vector2(-2, 0), new Vector2(0, 1), new Vector2(-3, -2), new Vector2(-1, -3), new Vector2(-3, -3), };
    private static GameObject _apple;
    private static int _globalAppleCounter;

    private enum Directions
    {
        Forward,
        Left,
        Right
    }

    private bool _isRunning;
    private IBlackBox _box;

    private Vector2 _pos;
    private Vector2 _direction = new Vector2(1, 0);

    public int AppleCounter;
    private Vector2 _applePos;

    private List<Vector2> _positions = new List<Vector2>();
    private List<Directions> _directions = new List<Directions>();

    private int _lifeCounter;

    private int _hungerBar; //Snake needs food, if no food die
    //We want to be able to detect if the AI got stuck somewhere, so we count the amount of ticks,
    //If it didn't get an apple recently it dies

    private readonly int _fieldSize = 3; //Radius of the field sort of speak


    // Use this for initialization
    public override void Initialize()
    {
        if (!_apple)
        {
            _apple = Instantiate(Resources.Load("Apple")) as GameObject;
        }
        NextApple();
    }

    public override void Activate(IBlackBox box)
    {
        _box = box;
        _isRunning = true;

        AppleCounter = 0;
        if (_globalAppleCounter != 0)
        {
            _globalAppleCounter = 0;
            _applePos = _applePositionList[0];
            _apple.transform.position = new Vector3(_applePos.x, 1.5f, _applePos.y);
        }
        

        NextApple();
        _hungerBar = 0;
        _lifeCounter = 0;
        _direction = new Vector2(1, 0);
        _pos = new Vector2();
        transform.position = new Vector3(0, 1.5f, 0);

        _positions = new List<Vector2>();
        _directions = new List<Directions>();
    }

    public override void Finished()
    {
        
    }

    public override void Stop()
    {

    }

    public override float GetFitness()
    {
        float fitness = (-(Mathf.Abs(_applePos.x - _pos.x) + Mathf.Abs(_applePos.y - _pos.y)) + 20 * AppleCounter);
        return fitness >= 0 ? fitness : 0;
    }

    private void NextApple()
    {
        _applePos = _applePositionList[AppleCounter++];
        if (AppleCounter > _globalAppleCounter)
        {
            _globalAppleCounter = AppleCounter;
            _apple.transform.position = new Vector3(_applePos.x, 1.5f, _applePos.y);
        }
        _hungerBar = 0;
    }

    // Update is called once per frame
    public void FixedUpdate()
    {
        if (!_isRunning) return;
        //Tick for all the Snakes

        //Check if DEAD?
        if (IsDead())
        {
            _isRunning = false;
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

            ISignalArray inputArray = _box.InputSignalArray;
            for (int index = 0; index < inputs.Count; index++)
            {
                inputArray[index] = inputs[index];
            }

            _box.Activate();
            ISignalArray outPutArray = _box.OutputSignalArray;

            //Analyize Outputs
            //First Output = Should change direction

            Directions targetDirection = Directions.Forward;

            if (outPutArray[0] > 0.1f)
            {
                //GoForward
                targetDirection = Directions.Forward;
            }
            if (outPutArray[1] > outPutArray[0])
            {
                //GoRight
                _direction = rightDir;
                targetDirection = Directions.Right;
            }
            if (outPutArray[2] > outPutArray[0] && outPutArray[2] > outPutArray[1])
            {
                //GoLeft
                _direction = leftDir;
                targetDirection = Directions.Left;
            }


            //Move the Head 1 Forward
            _pos += _direction;
            transform.position = new Vector3(_pos.x, 0.5f, _pos.y);

            _positions.Add(_pos);
            _directions.Add(targetDirection);

            //Check if we ate the apple!
            if (_pos == _applePos)
                NextApple();

            _hungerBar++;
            _lifeCounter++; //Amount of ticks alive
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
}
