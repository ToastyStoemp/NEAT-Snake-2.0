﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
[System.Diagnostics.DebuggerDisplay("fitness = {_fitness}. rank = {_globalRank}")]
public class Genome {

    //GLOBAL VARIABLES TEMP
    public static int globalInovationCounter = 0;


    float matingChance = 0.05f;
    float enableChance = 0.2f;
    float weightChangeChance = 0.8f;
    float perturbedChance = 0.9f;
    float perturbMaxChangeAmount = 0.05f; //HAS TO CHANGE DEPENDING ON TESTS! Smaller might take longer to get to optimal vallues, higher might result in skipping good vallues perhaps apply inverse sigmoid function
    float randomWeightChance = 0.1f; // has to be 1 - perturbedChance
    float newNodeChance = 0.01f;
    float newConnectionChance = 0.1f;
    float sizeIncluenceFactor = 0.05f; //Determines how much the size of a network has influence on the probability
    

    public List<Neuron> NeuralNetwork;
    public List<Node> NodeCollection;
	public float _probability;
	public float _fitness;
    public int _genIndex;
    public int _index;
    public int _globalRank;

    public int _inputCount;
    public int _outputCount;

    public Genome(int gen, int index, int inputCount, int outputCount)
    {
        NeuralNetwork = new List<Neuron>();
        NodeCollection = new List<Node>();

        _index = index;
        _genIndex = gen;
        _inputCount = inputCount;
        _outputCount = outputCount;

    }
    /// <summary>
    /// Fill the NodeCollection with the beginning Nodes
    /// </summary>
    public void CreateBasicNodes()
    {
        //Make the start input and output nodes
        List<Node> inputNodes = new List<Node>();
        for (int i = 0; i < _inputCount; i++)
        {
            inputNodes.Add(new Node(i, NeuronType.Input));
        }
        List<Node> outputNodes = new List<Node>();
        for (int i = 0; i < _outputCount; i++)
        {
            outputNodes.Add(new Node(_inputCount + i, NeuronType.OutPut));
        }

        //Merge the nodes into one list 
        NodeCollection = inputNodes.Union(outputNodes).ToList();
    }
    /// <summary>
    /// Generate a random connection to a random OutPut node
    /// </summary>
    public void CreateRandomConnection()
    {
        //Generate a random connection to a random OutPut node
        globalInovationCounter++;
        Node from = NodeCollection[(int)Random.Range(0, _inputCount)];
        Node to = NodeCollection[(int)Random.Range(NodeCollection.Count - _outputCount, NodeCollection.Count)];
        NeuralNetwork.Add(new Neuron(from._nodeIndex, to._nodeIndex, Random.value * 2 - 1.0f, globalInovationCounter));
    }

    /// <summary>
    /// Set the input data for the AI
    /// </summary>
    public void SetInputs(List<float> inputList)
    {
        if (inputList.Count != _inputCount)
        {
            Debug.Log("Not enough inputs given! " + inputList.ToArray());
            return; 
        }

        //Since we added the Inputs first, we can just access them through the index no need for a seperate list ( technically the nodeType is also not necesary )
        for (int i = 0; i < _inputCount; i++)
        {
            NodeCollection[i]._value = inputList[i];
        }
    }
    /// <summary>
    /// After running the calculations throughout all the nodes we can obtain the outputs.
    /// </summary>
    public List<float> GetOutputs()
    {
        List<float> results = new List<float>();

        //Since we added the outputs after the inputs the indexes are easily found
        for (int i = _inputCount; i - _inputCount < _outputCount; i++)
        {
            //results.Add(((NodeCollection[i + NodeCollection.Count - _outputCount]._value) + 1.0f) / 2.0f); // Normalize the outputs
            results.Add(Mathf.Clamp((NodeCollection[i]._value), 0, 1)); // Clamp the outputs
        }
        return results;
    }
    /// <summary>
    /// Calculates the end values for every node
    /// </summary>
    public void Calculate()
    {
        Reset(); //Reset all the node vallues
        foreach (Neuron connection in NeuralNetwork)
        {
            if (connection._enabled) //Make sure to process only the enabled networks
            {
                Node inputNode = NodeCollection[connection._in];
                Node targetNode = NodeCollection[connection._out];

                float originalValue = targetNode._value;
                targetNode._value += inputNode._value * connection._weight;
            }
        }
        foreach (Neuron connection in NeuralNetwork)
        {
            if (connection._enabled) //Make sure to process only the enabled networks
            {
                Node targetNode = NodeCollection[connection._out];
                if (!targetNode._calculated)
                {
                    targetNode._value = Sigmoid(targetNode._value); //Apply sigmoid function on the output
                    targetNode._calculated = true;
                }
            }
        }
    }

    /// <summary>
    /// Calc the fitness
    /// </summary>
    public void FitnessCalc(List<float> desiredOutcome)
    {
        float sum = 1.0f;
        //Since we added the outputs after the inputs the indexes are easily found
        List<float> outputs = GetOutputs();


        float diff = Mathf.Abs(Vector3.Distance(new Vector3(outputs[0], outputs[1], outputs[2]), new Vector3(desiredOutcome[0], desiredOutcome[1], desiredOutcome[2])));
        _fitness = sum - diff;

        int networkSize = NodeCollection.Count + NeuralNetwork.Count - _inputCount - _outputCount -1;
        if (_fitness > _fitness - networkSize * sizeIncluenceFactor)
            _fitness -= _fitness * networkSize * sizeIncluenceFactor;

    }
    /// <summary>
    /// Reseting all the values ( in case of copy ), safety measure
    /// </summary>
    public void Reset()
    {
        //Reseting all the values ( in case of copy ), safety measure
        for (int i = _inputCount; i < NodeCollection.Count; i++)
        {
            NodeCollection[i]._value = 0.0f;
            NodeCollection[i]._calculated = false;
        }
    }

    /// <summary>
    /// Mates/combines 2 genomes into 2 different OffSpring Aka Crossover
    /// </summary>
    public void Mate(Genome Partner, Genome OffSpring1, Genome OffSpring2)
    {
        List<int> InovationNumbersList = GetInovationList().Union(Partner.GetInovationList()).ToList();

        foreach (int inovation in InovationNumbersList)
        {
            Neuron myNeuron = GetNeuron(inovation);
            Neuron partnerNeuron = Partner.GetNeuron(inovation);

            if (myNeuron != null && partnerNeuron != null) //Both Genomes have the selected neuron, a random one will be selected, the oposing one is added to the other OffSpring
            {
                if (Random.value > 0.5f) //50% chance 
                {
                    OffSpring1.AddNeuron(myNeuron, true);
                    OffSpring2.AddNeuron(partnerNeuron, true);
                }
                else
                {
                    OffSpring1.AddNeuron(partnerNeuron, true);
                    OffSpring2.AddNeuron(myNeuron, true);
                }

            }
            else if (myNeuron != null)//the neuron is not present in the Partner, it's labeled as either disjoint or excess, it will be added to both
            {
                OffSpring1.AddNeuron(myNeuron, true);
                OffSpring2.AddNeuron(myNeuron, true);
            }
            else if (partnerNeuron != null)
            {
                OffSpring1.AddNeuron(partnerNeuron, true);
                OffSpring2.AddNeuron(partnerNeuron, true);
            }
        }
    }

    /// <summary>
    /// Mates/combines 2 genomes into 1 different OffSpring Aka Crossover
    /// </summary>
    public Genome Crossover(Genome Partner)
    {
        Genome offSpring = new Genome(0, 0, _inputCount, _outputCount);

        List<int> InovationNumbersList = GetInovationList().Union(Partner.GetInovationList()).ToList();

        List<Node> ToAddNodes = new List<Node>();

        foreach (int inovation in InovationNumbersList)
        {
            Neuron myNeuron = GetNeuron(inovation);
            Neuron partnerNeuron = Partner.GetNeuron(inovation);

            if (myNeuron != null && partnerNeuron != null) //Both Genomes have the selected neuron, a random one will be selected, the oposing one is added to the other OffSpring
            {
                if (Random.value > 0.5f) //50% chance 
                    offSpring.AddNeuron(myNeuron, true);
                else
                    offSpring.AddNeuron(partnerNeuron, true);
            }
            else if (myNeuron != null)//the neuron is not present in the Partner, it's labeled as either disjoint or excess, it will be added to both
                offSpring.AddNeuron(myNeuron, true);
            else if (partnerNeuron != null)
                offSpring.AddNeuron(partnerNeuron, true);
        }
        offSpring.CreateBasicNodes();
        offSpring.CreateExtendedNodes();
        return offSpring;
    }

    /// <summary>
    /// Creates all non existing nodes
    /// </summary>
    public void CreateExtendedNodes()
    {
        foreach (Neuron connection in NeuralNetwork)
        {
            if (GetNode(connection._in) == null) 
            {
                NodeCollection.Add(new Node(connection._in));
            }
            if (GetNode(connection._out) == null)
            {
                NodeCollection.Add(new Node(connection._out));
            }
        }
    }

    /// <summary>
    /// Returns the node based on index
    /// </summary>
    public Node GetNode(int index)
    {
        foreach (Node node in NodeCollection)
        {
            if (node._nodeIndex == index)
            {
                return node;
            }
        }
        return null;
    }


    /// <summary>
    /// Alters the current Genome with the different mutation steps
    /// </summary>
    public void Mutate()
    {
        //Some parameters for debugging and informatics
        int newNodeCounter = 0;
        int newConnectionCounter = 0;
        int weightsChanged = 0;
        int weightsPertured = 0;
        int weightsRandomized = 0;
        int connectionsEnabled = 0;

        List<Neuron> ToAddNeurons = new List<Neuron>();
            //Not needed for all programming languages, this makes sure that newly added neurons are not subject to mutation in this generation yet.

        //Node Mutation is handled first, to prevent the same issues as adding Neuron connections while looping over the connections
        //foreach (Node node in NodeCollection)
        {
            //Chance for new connection -- Should be written recursivly so that if a connection already exists it will keep trying
            if (Random.value < newConnectionChance) //chance for it to spawn a new connection (default is 10%)
            {
                newConnectionCounter++;
                Node startNode = GetRandomNode(true, false); //Can be an input node, but not an output node
                Node endNode = GetRandomNode(false, true, startNode._nodeIndex);
                    //can by any node, but not an input node

                if (!ConnectionExists(startNode._nodeIndex, endNode._nodeIndex))
                {
                    globalInovationCounter++;
                    Neuron newConnection = new Neuron(startNode._nodeIndex, endNode._nodeIndex, Random.value * 2 - 1.0f,
                        globalInovationCounter); //Set the weight to be random
                    NeuralNetwork.Add(newConnection);
                }
            }
        }

        //Connection Mutation
        //foreach (Neuron neuron in NeuralNetwork)
        {
            Neuron neuron = GetRandomNeuronConnection();
            //Dissabled Neurons have a chance to get enabled againg (Default value is 20%)
            if (!neuron._enabled && Random.value < enableChance) neuron._enabled = true;

            //There is a high percentage that the weights of a neuron is changed (Default value is 80%)
            if (Random.value < weightChangeChance)
            {
                weightsChanged++;
                /*2 types of mutation
                Perturbed With the highest chance of occuring ( Default value is 90% ) */
                if (Random.value < perturbedChance)
                {
                    float random = Random.value * 2 - 1.0f;
                    neuron._weight *= random;

                    weightsPertured++;
                }
                else //Randomized, the weight is set to a random value ( Default value is 10% )
                {
                    neuron._weight = Random.value * 4 - 2.0f;
                    weightsRandomized++;
                }
            }
        }
        {
            //New Node with protection
            if (Random.value < newNodeChance) //chance for it to spawn a new node (default value is 1%)
            {
                Neuron neuron = GetRandomNeuronConnection();
                /*Create a new node, set the input and output. After adding a new Neuron to the network, it is bound to drop in fitness, 
                to give it some breathing room, we set the weight of the new neuron to the weight of the previous connection
               The previous connection is dissabled, and a new connection from the origin to this node is set with a weight of 1. 
               This will result in fitness close to fitness achieved in the previous generation */
                Node mutationNode = new Node(NodeCollection.Count);
                newNodeCounter++; //DEBUG
                //Make apropriate connections
                int input = neuron._in; //this will be the input for the new node
                int output = neuron._out; //this will be the output for the new node
                globalInovationCounter++;
                    //Increment the inovation number, for future tracking such ass diversity calculations
                Neuron inputConnection = new Neuron(input, mutationNode._nodeIndex, 1.0f, globalInovationCounter);
                    //Set the weight to 1.0f to protect the new node
                globalInovationCounter++;
                Neuron outputConnection = new Neuron(mutationNode._nodeIndex, output, neuron._weight,
                    globalInovationCounter); //Set the weight to the original weight to maintain fitness
                neuron._enabled = false; //Disable the original connection
                //Don't forget to add the actual node to the nodeCollection ( took me about an hour to figure this out -_- )
                NodeCollection.Add(mutationNode);

                NeuralNetwork.Add(inputConnection);
                NeuralNetwork.Add(outputConnection);
            }

        }

        ////Some information output //DEBUG
        //Debug.Log("This mutation, genome#" + _index);
        //if (weightsChanged != 0)
        //{
        //    Debug.Log("Weights changed: " + weightsChanged);
        //    Debug.Log("Weights pertured: " + weightsPertured + "\nWeights randomized: " + weightsRandomized);
        //}
        //if (newConnectionCounter != 0 ) Debug.Log("new connections: " + newConnectionCounter);
        //if (newNodeCounter != 0) Debug.Log("new nodes: " + newNodeCounter);
    }

    /// <summary>
    /// get the weight difference between 2 genomes
    /// </summary>
    public float WeightsDifference(Genome target)
    {

        List<int> myInovationList = GetInovationList();
        List<int> targetInovationList = target.GetInovationList();

        float sum = 0;
        int totalCount = 0;

        foreach (int inovationNumber in myInovationList)
        {
            if (targetInovationList.Contains(inovationNumber))
            {
                Neuron myNeuron = GetNeuron(inovationNumber);
                Neuron targetNeuron = target.GetNeuron(inovationNumber);

                sum += Mathf.Abs(myNeuron._weight - targetNeuron._weight);
                totalCount++;
            }
        }

        if (totalCount == 0) return 0;
        return sum / (float)totalCount;
    }
    /// <summary>
    /// returns the amount of disjoints between 2 Genomes
    /// </summary>
    public int DisJoint(Genome target)
    {
        List<int> myInovationList = GetInovationList();
        List<int> targetInovationList = target.GetInovationList();
     

        int myHighest = GetHighestFromList(myInovationList); //Find the highest inovation number in this genome ( to compare with the target genome for excess )

        List<int> resultInts = new List<int>();

        foreach (int thisInovationNumber in myInovationList) //Loop over every inovation number of This Genome
        {
            if (!targetInovationList.Contains(thisInovationNumber)) //if the inovation number is not found in the target genome (disjoin) add it to the list
                resultInts.Add(thisInovationNumber);
        }

        foreach (int thisInovationNumber in targetInovationList) //Loop over every inovation number of This Genome
        {
            if (!myInovationList.Contains(thisInovationNumber) && thisInovationNumber < myHighest) //if the inovation number is not found in the target genome (disjoin) add it to the list
                resultInts.Add(thisInovationNumber);
        }

        //myInovationList.Concat(targetInovationList).ToList();
        //myInovationList.Distinct().ToList();
        // myInovationList = GetElementsBelow(myInovationList, highest);

        return resultInts.Count; //Might need to be devided by the total count or so
    }
    /// <summary>
    /// returns the amount of disjoints between 2 Genomes
    /// </summary>
    public int GetHighestFromList(List<int> list)
    {
        int highest = 0;
        for (int i = 0; i < list.Count; i++)
            if (highest < list[i])
                highest = list[i];

        return highest;
    }
    /// <summary>
    /// deletes all elements higher then X
    /// </summary>
    public List<int> GetElementsBelow(List<int> list, int value = 0)
    {
        List<int> leftOver = new List<int>();
        for (int i = 0; i < list.Count; i++)
            if (list[i] < value)
                leftOver.Add(list[i]);
        return leftOver;
    }
    /// <summary>
    /// Adds a Neuron to the Neural Network 
    /// </summary>
    public bool AddNeuron(Neuron neuron, bool canExist = false)
    {
        if (canExist)
        {
            NeuralNetwork.Add(neuron);
            return true;
        }
        if (GetInovationList().IndexOf(neuron._inovationNum) != -1)
        {
            NeuralNetwork.Add(neuron);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Gets the neurons base on the inovation number
    /// </summary>
    public Neuron GetNeuron(int Inovation)
    {
        foreach (Neuron neuron in NeuralNetwork)
            if (neuron._inovationNum == Inovation)
                return neuron;

        return null;
    }
    /// <summary>
    /// Returns a random neuron, used for adding a node into the network
    /// </summary>
    public Neuron GetRandomNeuronConnection()
    {
        //Might have to double check that that the connection is not dissabled!
        return NeuralNetwork[(int)Random.Range(0, NeuralNetwork.Count)];
    }
    /// <summary>
    /// Returns a random node, used for adding a new connection into the network
    /// </summary>
    public Node GetRandomNode(bool canBeInputNode = true, bool canBeOutputNode = true, int ignoreIndex = -1)
    {
        List<Node> tempList = new List<Node>();
        foreach (Node node in NodeCollection)
        {
            if (node._type == NeuronType.Input)
            {
                if (canBeInputNode)
                    tempList.Add(node);
            }
            else if (node._type == NeuronType.OutPut)
            {
                if (canBeOutputNode)
                    tempList.Add(node);
            }
            else
            {
                tempList.Add(node);
            }
        }
        return tempList[(int)Random.Range(0, tempList.Count)];
    }
    /// <summary>
    /// Checks if there is already an connection between these 2 nodes ( both ways )
    /// </summary>
    public bool ConnectionExists(int input, int output)
    {
        foreach (Neuron connection in NeuralNetwork)
        {
            if (connection._in == input && connection._out == output)
            {
                return true;
            }
            else if (connection._in == output && connection._out == input)
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// returns the neuron count
    /// </summary>
    public int GetNeuronCount()
    {
        return NeuralNetwork.Count;
    }
    /// <summary>
    /// returns the neuron count with a list of inovation numbers to ignore
    /// </summary>
    public int GetNeuronCount(List<int> ignoreList)
    {
        int count = 0;
        foreach (Neuron neuron in NeuralNetwork)
            if (ignoreList.IndexOf(neuron._inovationNum) != -1)
                count++;
        return count;
    }
    /// <summary>
    /// returns a list of inovation numbers within the neuron
    /// </summary>
    public List<int> GetInovationList()
    {
        List<int> InovationList = new List<int>();
        foreach (Neuron neuron in NeuralNetwork)
            InovationList.Add(neuron._inovationNum);
        return InovationList;
    }
    /// <summary>
    /// Sigmoid function, since Unity math does not have one -_-
    /// </summary>
    float Sigmoid(float x) {
        return 2 / (1 + Mathf.Exp(-4.9f * x)) - 1; //See https://www.jair.org/media/1338/live-1338-2278-jair.pdf page 95
    }
    /// <summary>>
    /// Graphically represents the Neural network
    /// </summary>
    public void Print(Vector3 pos, int sizeOffset, int speciesCount, bool ignoreY = false)
	{

        List<Vector3> Positions = new List<Vector3> ();

		float nodeDistance = 1.0f;
        float radius = 0.2f;
        float boxsize = 0.6f;
        float xOffset = sizeOffset * nodeDistance;
        float yOffset = _inputCount * nodeDistance * 2 * _index;
        if (ignoreY)
            yOffset = 0.0f;


        Vector3 myPos1 = new Vector3(pos.x - nodeDistance + xOffset, pos.y - yOffset + nodeDistance * 2, pos.z);
        TextGizmo.Draw(myPos1, _index.ToString());

        Vector3 myPos2 = new Vector3(pos.x + xOffset, pos.y - yOffset + nodeDistance * 2, pos.z);
        TextGizmo.Draw(myPos2, speciesCount.ToString());

        myPos1 = new Vector3(pos.x - nodeDistance + xOffset, pos.y - yOffset + nodeDistance, pos.z);
        TextGizmo.Draw(myPos1, _fitness.ToString());

        myPos1 = new Vector3(pos.x - nodeDistance + xOffset, pos.y - yOffset + nodeDistance /2, pos.z);
        TextGizmo.Draw(myPos1, _probability.ToString());

        myPos1 = new Vector3(pos.x - nodeDistance + xOffset + (NodeCollection.Count - _inputCount - _outputCount + 2) * nodeDistance, pos.y - yOffset + nodeDistance / 2, pos.z);
        List<float> results = GetOutputs();
        Gizmos.color = new Color(results[0], results[1], results[2]);
        Gizmos.DrawCube(myPos1, new Vector3(boxsize, boxsize, boxsize));
        

        Gizmos.color = new Color (0.2f, 0.2f, 0.2f);
		//Draw inputs
		for (int i = 0; i < _inputCount; i++) {
			Vector3 myPos = new Vector3 (pos.x + xOffset + i * 0.2f, pos.y - nodeDistance * i - yOffset, pos.z);
			Positions.Add (myPos);
            Gizmos.color = Color.red;
			Gizmos.DrawSphere (myPos, radius);
            //TextGizmo.Draw( myPos,  NodeCollection[i]._value.ToString());
            TextGizmo.Draw(myPos, NodeCollection[i]._nodeIndex.ToString());
        }
		//Draw OutPuts
		for (int i = 0; i < _outputCount; i++) {
			Vector3 myPos = new Vector3 (pos.x - nodeDistance + xOffset + (NodeCollection.Count - _inputCount - _outputCount + 2) * nodeDistance + i * 0.2f, pos.y - nodeDistance * i - yOffset, pos.z);
			Positions.Add (myPos);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere (myPos, radius);
            TextGizmo.Draw(myPos, NodeCollection[i + _inputCount]._nodeIndex.ToString());
            //TextGizmo.Draw(myPos, NodeCollection[NodeCollection.Count - _inputCount + i]._value.ToString());
		}
        //Draw Hidden Nodes
        for (int i = 0; i < NodeCollection.Count - _inputCount - _outputCount; i++)
        {
            Vector3 myPos = new Vector3(pos.x + nodeDistance * (i + 1) + xOffset + i * 0.2f, pos.y - (Positions[0].y - Positions[_inputCount - 1].y) * ((float)i / (NodeCollection.Count - _inputCount - _outputCount)) - nodeDistance / 2 - yOffset, pos.z);
            Positions.Add(myPos);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(myPos, radius);
            //TextGizmo.Draw(myPos, NodeCollection[i + _inputCount]._value.ToString());
            TextGizmo.Draw(myPos, NodeCollection[i + _inputCount + _outputCount]._nodeIndex.ToString());
        }


        for (int i = 0; i < NeuralNetwork.Count; i++)
        {
            if (NeuralNetwork[i]._weight > 0)
            {
                Gizmos.color = new Color(0, 1.0f, 0);
            }
            else if (NeuralNetwork[i]._weight < 0)
            {
                Gizmos.color = new Color(1.0f, 0, 0); 
            }
            if (!NeuralNetwork[i]._enabled)
            {
                Gizmos.color = new Color(0, 0, 0);
            }
            Gizmos.DrawLine(Positions[NeuralNetwork[i]._in], Positions[NeuralNetwork[i]._out]);
        }
	}
}