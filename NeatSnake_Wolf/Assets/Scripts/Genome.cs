using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

[System.Serializable]
[System.Diagnostics.DebuggerDisplay("fitness = {Fitness}. rank = {GlobalRank}")]
public class Genome {

    public List<Link> LinkNetwork;
    public List<Node> NodeCollection;
	public float Probability;
	public float Fitness;

    public int Index;
    public int GlobalRank;

    public int SpeciesIndex;


    public Genome(int index)
    {
        LinkNetwork = new List<Link>();
        NodeCollection = new List<Node>();

        Index = index;
    }
    /// <summary>
    /// Fill the NodeCollection with the beginning Nodes
    /// </summary>
    public void CreateBasicNodes()
    {
        //Make the start input and output nodes
        List<Node> inputNodes = new List<Node>();
        for (int i = 0; i < Parameters.NumInputs; i++)
        {
            int inov = InnovationManager.CheckInnovation(i, i, InnovationType.Node);
            inov = inov < 0 ? InnovationManager.CreateNewInnovation(i, i, InnovationType.Node, NodeType.Input) : inov;
            inputNodes.Add(new Node(i, inov, NodeType.Input));
        }
        List<Node> outputNodes = new List<Node>();
        for (int i = 0; i < Parameters.NumOutputs; i++)
        {
            int index = Parameters.NumInputs + i;
            int inov = InnovationManager.CheckInnovation(index, index, InnovationType.Node);
            inov = inov < 0 ? InnovationManager.CreateNewInnovation(index, index, InnovationType.Node, NodeType.OutPut) : inov;
            outputNodes.Add(new Node(index, inov, NodeType.OutPut));
        }

        //Merge the nodes into one list 
        NodeCollection = inputNodes.Union(outputNodes).ToList();
    }
    /// <summary>
    /// Generate a random link to a random OutPut node
    /// </summary>
    public void CreateRandomLink()
    {
        //Generate a random link to a random OutPut node

        Node from = NodeCollection[Random.Range(0, Parameters.NumInputs)]; //Select random Input Node
        Node to = NodeCollection[Random.Range(NodeCollection.Count - Parameters.NumOutputs, NodeCollection.Count)]; //Select random Output Node
        int innovNum = InnovationManager.CheckInnovation(from.InovationNum, to.InovationNum, InnovationType.Link); //Check if the link already exists in the Innovation manager
        innovNum = innovNum > 0 ? innovNum : InnovationManager.CreateNewInnovation(from.InovationNum, to.InovationNum, InnovationType.Link); //If it does not exist, create new link
        LinkNetwork.Add(new Link(from, to, Random.value * 2 - 1.0f, innovNum));

    }

    /// <summary>
    /// Set the input data for the AI
    /// </summary>
    public void SetInputs(List<float> inputList)
    {
        if (inputList.Count != Parameters.NumInputs)
        {
            Debug.Log("Not enough inputs given! " + inputList.ToArray());
            return; 
        }

        //Since we added the Inputs first, we can just access them through the index no need for a seperate list ( technically the nodeType is also not necesary )
        for (int i = 0; i < Parameters.NumInputs; i++)
        {
            NodeCollection[i].Value = inputList[i];
        }
    }
    /// <summary>
    /// After running the calculations throughout all the nodes we can obtain the outputs.
    /// </summary>
    public List<float> GetOutputs()
    {
        List<float> results = new List<float>();

        //Since we added the outputs after the inputs the indexes are easily found
        for (int i = Parameters.NumInputs; i - Parameters.NumInputs < Parameters.NumOutputs; i++)
        {
            //results.Add(((NodeCollection[i + NodeCollection.Count - Parameters.iNumOutputs]._value) + 1.0f) / 2.0f); // Normalize the outputs
            results.Add(Mathf.Clamp((NodeCollection[i].Value), 0, 1)); // Clamp the outputs
        }
        return results;
    }
    /// <summary>
    /// Calculates the end values for every node
    /// </summary>
    public void Calculate()
    {
        Reset(); //Reset all the node vallues
        foreach (Link link in LinkNetwork)
        {
            if (link.Enabled) //Make sure to process only the enabled networks
            {
                Node inputNode = link.In;
                Node targetNode = link.Out;

                targetNode.Value += inputNode.Value * link.Weight;
            }
        }
        foreach (Link link in LinkNetwork)
        {
            if (link.Enabled) //Make sure to process only the enabled networks
            {
                Node targetNode = link.Out;
                if (!targetNode.Calculated)
                {
                    targetNode.Value = Sigmoid(targetNode.Value); //Apply sigmoid function on the output
                    targetNode.Calculated = true;
                }
            }
        }
    }

    /// <summary>
    /// Calc the fitness
    /// </summary>
    public void FitnessCalc(float calculatedFitness)
    {
        Fitness = calculatedFitness;
        //Do not change this!
        //Modifiy the fitness so that more complex AI's are punished for their complexity
        int networkNodeSize = NodeCollection.Count - Parameters.NumInputs - Parameters.NumOutputs;
        int networkLinkSize = LinkNetwork.Count - 1;
        float networkNodeSizePenalty = networkNodeSize * Parameters.NodeSizePenalty * Fitness;
        float networkLinkSizePenalty = networkLinkSize * Parameters.LinkSizePenalty * Fitness;

        Fitness -= networkNodeSizePenalty + networkLinkSizePenalty;

    }
    /// <summary>
    /// Reseting all the values ( in case of copy ), safety measure
    /// </summary>
    public void Reset()
    {
        //Reseting all the values ( in case of copy ), safety measure
        for (int i = Parameters.NumInputs; i < NodeCollection.Count; i++)
        {
            NodeCollection[i].Value = 0.0f;
            NodeCollection[i].Calculated = false;
        }
    }

    /// <summary>
    /// Mates/combines 2 genomes into 2 different OffSpring Aka Crossover
    /// </summary>
    public void Mate(Genome partner, Genome offSpring1, Genome offSpring2)
    {
        List<int> inovationNumbersList = GetInovationList().Union(partner.GetInovationList()).ToList();

        foreach (int inovation in inovationNumbersList)
        {
            Link myLink = GetLink(inovation);
            Link partnerLink = partner.GetLink(inovation);

            if (myLink != null && partnerLink != null) //Both Genomes have the selected Link, a random one will be selected, the oposing one is added to the other OffSpring
            {
                if (Random.value > 0.5f) //50% chance 
                {
                    offSpring1.AddLink(myLink, true);
                    offSpring2.AddLink(partnerLink, true);
                }
                else
                {
                    offSpring1.AddLink(partnerLink, true);
                    offSpring2.AddLink(myLink, true);
                }

            }
            else if (myLink != null)//the Link is not present in the Partner, it's labeled as either disjoint or excess, it will be added to both
            {
                offSpring1.AddLink(myLink, true);
                offSpring2.AddLink(myLink, true);
            }
            else if (partnerLink != null)
            {
                offSpring1.AddLink(partnerLink, true);
                offSpring2.AddLink(partnerLink, true);
            }
        }
    }

    /// <summary>
    /// Mates/combines 2 genomes into 1 different OffSpring Aka Crossover
    /// </summary>
    /// OLD CROSS OVER
    //public Genome Crossover(Genome Partner)
    //{
    //    Genome offSpring = new Genome(0);
    //    offSpring.CreateBasicNodes();

    //    List<int> InnovationNumbersList = GetInovationList().Union(Partner.GetInovationList()).ToList();

    //    List<Node> ToAddNodes = new List<Node>();

    //    foreach (int inovation in InnovationNumbersList)
    //    {
    //        Link myLink = GetLink(inovation);
    //        Link partnerLink = Partner.GetLink(inovation);

    //        if (myLink != null && partnerLink != null) //Both Genomes have the selected Link, a random one will be selected, the oposing one is added to the other OffSpring
    //        {
    //            if (Random.value > 0.5f)
    //            {
    //                //50% chance 
    //                offSpring.AddLink(myLink, true);
    //                foreach (Node node in offSpring.NodeCollection)
    //                {
    //                    if (node)
    //                    {
                        
    //                    }
    //                }
    //            }


    //            else
    //                offSpring.AddLink(partnerLink, true);
    //        }
    //        else if (myLink != null)//the Link is not present in the Partner, it's labeled as either disjoint or excess, it will be added to both
    //            offSpring.AddLink(myLink, true);
    //        else if (partnerLink != null)
    //            offSpring.AddLink(partnerLink, true);
    //    }
        
    //    offSpring.CreateExtendedNodes();
    //    return offSpring;






    //}

    /// <summary>
    /// Creates all non existing nodes
    /// </summary>
    //public void CreateExtendedNodes()
    //{
    //    foreach (Link Link in LinkNetwork)
    //    {
    //        if (GetNode(Link._in) == null) 
    //        {
    //            NodeCollection.Add(new Node(Link._in));
    //        }
    //        if (GetNode(Link._out) == null)
    //        {
    //            NodeCollection.Add(new Node(Link._out));
    //        }
    //    }
    //}

    /// <summary>
    /// Returns the node based on index
    /// </summary>
    public Node GetNode(int index)
    {
        foreach (Node node in NodeCollection)
        {
            if (node.NodeIndex == index)
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
        //int newNodeCounter = 0;
        //int newLinkCounter = 0;
        //int weightsChanged = 0;
        //int weightsPertured = 0;
        //int weightsRandomized = 0;
        //int LinksEnabled = 0;
        //int dissabledLinks = 0;

        //Node Mutation is handled first, to prevent the same issues as adding Link Links while looping over the Links

        //Chance for new Link -- Should be written recursivly so that if a Link already exists it will keep trying
        if (Random.value < Parameters.ChanceAddLink) //chance for it to spawn a new Link (default is 200%)
        {
            //newLinkCounter++;
            Node startNode = GetRandomNode(true, false); //Can be an input node, but not an output node
            Node endNode = GetRandomNode(false, true, startNode.NodeIndex);
            //can by any node, but not an input node

            if (!LinkExists(startNode, endNode))
            {
                int innovNum = InnovationManager.CheckInnovation(startNode.InovationNum, endNode.InovationNum, InnovationType.Link); //Check if the Link already exists in the Innovation manager
                innovNum = innovNum > 0 ? innovNum : InnovationManager.CreateNewInnovation(startNode.InovationNum, endNode.InovationNum, InnovationType.Link); //If it does not exist, create new Link
                LinkNetwork.Add(new Link(startNode, endNode, Random.value * 2 - 1.0f, innovNum));
            }
        }
        
        //pick randomLink
        Link tempLink = GetRandomLinkLink();

        //Random Chance to dissable a Link
        if (LinkNetwork.Count > 1)
        {
            if (Random.value < Parameters.ChanceDissableLink)
            {
                //dissabledLinks++;
                tempLink.Enabled = false;
                tempLink = GetRandomLinkLink();
            }
        }

        //Link Mutation

        //Dissabled Links have a chance to get enabled againg (Default value is 20%)
        if (!tempLink.Enabled && Random.value < Parameters.ChanceEnableLink) tempLink.Enabled = true;

        //There is a high percentage that the weights of a Link is changed (Default value is 80%)
        if (Random.value < Parameters.ChanceWeightChange)
        {
            //weightsChanged++;
            /*2 types of mutation
            Perturbed With the highest chance of occuring ( Default value is 90% ) */
            if (Random.value < Parameters.ChanceWeightReplaced) //Randomized, the weight is set to a random value ( Default value is 10% )
            {
                tempLink.Weight = Random.value * 4 - 2.0f;
                //weightsRandomized++;
            }
            else
            {
                float random = Random.value * 2 - 1.0f;
                tempLink.Weight *= random;

                //weightsPertured++;
            }
           
        }

        //New Node with protection
        if (Random.value < Parameters.ChanceAddNode) //chance for it to spawn a new node (default value is 1%)
        {
            tempLink = GetRandomLinkLink();
            /*Create a new node, set the input and output. After adding a new Link to the network, it is bound to drop in fitness, 
            to give it some breathing room, we set the weight of the new Link to the weight of the previous Link
            The previous Link is dissabled, and a new Link from the origin to this node is set with a weight of 1. 
            This will result in fitness close to fitness achieved in the previous generation */
            int inovationNumber = InnovationManager.CreateNewInnovation(0, 0, InnovationType.Node, NodeType.Hidden);
            Node mutationNode = new Node(NodeCollection.Count, inovationNumber);
            NodeCollection.Add(mutationNode);

            //newNodeCounter++; //DEBUG
            //Make apropriate Links
            Node from = tempLink.In; //this will be the input for the new node
            Node to = tempLink.Out; //this will be the output for the new node

            //Increment the inovation number, for future tracking such ass diversity calculations
            inovationNumber = InnovationManager.CheckInnovation(from.InovationNum, mutationNode.InovationNum, InnovationType.Link); //Check if the Link already exists in the Innovation manager
            inovationNumber = inovationNumber > 0 ? inovationNumber : InnovationManager.CreateNewInnovation(from.InovationNum, mutationNode.InovationNum, InnovationType.Link); //If it does not exist, create new Link
            LinkNetwork.Add(new Link(from, mutationNode, Random.value * 1.0f, inovationNumber));

            //Set the weight to 1.0f to protect the new node

            inovationNumber = InnovationManager.CheckInnovation(mutationNode.InovationNum, to.InovationNum, InnovationType.Link); //Check if the Link already exists in the Innovation manager
            inovationNumber = inovationNumber > 0 ? inovationNumber : InnovationManager.CreateNewInnovation(mutationNode.InovationNum, to.InovationNum, InnovationType.Link); //If it does not exist, create new Link
            LinkNetwork.Add(new Link(mutationNode, to, tempLink.Weight, inovationNumber)); //Set the weight to the original weight to maintain fitness

            tempLink.Enabled = false; //Disable the original Link
        }


        ////Some information output //DEBUG
        //Debug.Log("This mutation, genome#" + _index);
        //if (weightsChanged != 0)
        //{
        //    Debug.Log("Weights changed: " + weightsChanged);
        //    Debug.Log("Weights pertured: " + weightsPertured + "\nWeights randomized: " + weightsRandomized);
        //}
        //if (newLinkCounter != 0 ) Debug.Log("new Links: " + newLinkCounter);
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
                Link myLink = GetLink(inovationNumber);
                Link targetLink = target.GetLink(inovationNumber);

                sum += Mathf.Abs(myLink.Weight - targetLink.Weight);
                totalCount++;
            }
        }

        if (totalCount == 0) return 0;
        return sum / totalCount;
    }
    /// <summary>
    /// returns the amount of disjoints between 2 Genomes
    /// </summary>
    public int DisJoint(Genome target)
    {
        var myInovationList = GetInovationList();
        var targetInovationList = target.GetInovationList();
     

        var myHighest = GetHighestFromList(myInovationList); //Find the highest inovation number in this genome ( to compare with the target genome for excess )

        var resultInts = myInovationList.Where(thisInovationNumber => !targetInovationList.Contains(thisInovationNumber)).ToList();

        resultInts.AddRange(targetInovationList.Where(thisInovationNumber => !myInovationList.Contains(thisInovationNumber) && thisInovationNumber < myHighest));

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
        return list.Concat(new[] {0}).Max();
    }
    /// <summary>
    /// deletes all elements higher then X
    /// </summary>
    public List<int> GetElementsBelow(List<int> list, int value = 0)
    {
        return list.Where(t => t < value).ToList();
    }
    /// <summary>
    /// Adds a Link to the Link Network 
    /// </summary>
    public bool AddLink(Link link, bool canExist = false)
    {
        if (canExist)
        {
            LinkNetwork.Add(link);
            return true;
        }
        if (GetInovationList().IndexOf(link.InovationNum) != -1)
        {
            LinkNetwork.Add(link);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Gets the Links base on the inovation number
    /// </summary>
    public Link GetLink(int inovation)
    {
        return LinkNetwork.FirstOrDefault(link => link.InovationNum == inovation);
    }
    /// <summary>
    /// Returns a random Link, used for adding a node into the network
    /// </summary>
    public Link GetRandomLinkLink()
    {
        //Might have to double check that that the Link is not dissabled!
        return LinkNetwork[Random.Range(0, LinkNetwork.Count)];
    }
    /// <summary>
    /// Returns a random node, used for adding a new Link into the network
    /// </summary>
    public Node GetRandomNode(bool canBeInputNode = true, bool canBeOutputNode = true, int ignoreIndex = -1)
    {
        var tempList = new List<Node>();
        foreach (var node in NodeCollection)
        {
            switch (node.Type)
            {
                case NodeType.Input:
                    if (canBeInputNode)
                        tempList.Add(node);
                    break;
                case NodeType.OutPut:
                    if (canBeOutputNode)
                        tempList.Add(node);
                    break;
                default:
                    tempList.Add(node);
                    break;
            }
        }
        return tempList[Random.Range(0, tempList.Count)];
    }
    /// <summary>
    /// Checks if there is already an Link between these 2 nodes ( both ways )
    /// </summary>
    public bool LinkExists(Node input, Node output)
    {
        foreach (Link link in LinkNetwork)
        {
            if (link.In == input && link.Out == output)
            {
                return true;
            }
            else if (link.In == output && link.Out == input)
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// returns the Link count
    /// </summary>
    public int GetLinkCount()
    {
        return LinkNetwork.Count;
    }
    /// <summary>
    /// returns the Link count with a list of inovation numbers to ignore
    /// </summary>
    public int GetLinkCount(List<int> ignoreList)
    {
        return LinkNetwork.Count(link => ignoreList.IndexOf(link.InovationNum) != -1);
    }
    /// <summary>
    /// returns a list of inovation numbers within the Link
    /// </summary>
    public List<int> GetInovationList()
    {
        return LinkNetwork.Select(link => link.InovationNum).ToList();
    }
    /// <summary>
    /// Sigmoid function, since Unity math does not have one -_-
    /// </summary>
    public static float Sigmoid(float x) {
        return 2 / (1 + Mathf.Exp(-4.9f * x)) - 1; //See https://www.jair.org/media/1338/live-1338-2278-jair.pdf page 95
    }
    /// <summary>>
    /// Graphically represents the Link network
    /// </summary>
    public void Print(Vector3 pos, int sizeOffset, int speciesCount, bool ignoreY = false)
	{

        List<Vector3> positions = new List<Vector3> ();

		var nodeDistance = 1.0f;
        var radius = 0.2f;
        var boxsize = 0.6f;
        var xOffset = sizeOffset * nodeDistance;
        var yOffset = Parameters.NumInputs * nodeDistance * 2 * Index;
        if (ignoreY)
            yOffset = 0.0f;


        Vector3 myPos1 = new Vector3(pos.x - nodeDistance + xOffset, pos.y - yOffset + nodeDistance * 2, pos.z);
        TextGizmo.Draw(myPos1, Index.ToString());

        Vector3 myPos2 = new Vector3(pos.x + xOffset, pos.y - yOffset + nodeDistance * 2, pos.z);
        TextGizmo.Draw(myPos2, speciesCount.ToString());

        myPos1 = new Vector3(pos.x - nodeDistance + xOffset, pos.y - yOffset + nodeDistance, pos.z);
        TextGizmo.Draw(myPos1, Fitness.ToString());

        myPos1 = new Vector3(pos.x - nodeDistance + xOffset, pos.y - yOffset + nodeDistance /2, pos.z);
        TextGizmo.Draw(myPos1, Probability.ToString());

        myPos1 = new Vector3(pos.x - nodeDistance + xOffset + (NodeCollection.Count - Parameters.NumInputs - Parameters.NumOutputs + 2) * nodeDistance, pos.y - yOffset + nodeDistance / 2, pos.z);
        List<float> results = GetOutputs();
        Gizmos.color = new Color(results[0], results[1], results[2]);
        Gizmos.DrawCube(myPos1, new Vector3(boxsize, boxsize, boxsize));
        

        Gizmos.color = new Color (0.2f, 0.2f, 0.2f);
		//Draw inputs
		for (var i = 0; i < Parameters.NumInputs; i++) {
			var myPos = new Vector3 (pos.x + xOffset + i * 0.2f, pos.y - nodeDistance * i - yOffset, pos.z);
			positions.Add (myPos);
            Gizmos.color = Color.red;
			Gizmos.DrawSphere (myPos, radius);
            //TextGizmo.Draw( myPos,  NodeCollection[i]._value.ToString());
            TextGizmo.Draw(myPos, NodeCollection[i].NodeIndex.ToString());
        }
		//Draw OutPuts
		for (var i = 0; i < Parameters.NumOutputs; i++) {
			var myPos = new Vector3 (pos.x - nodeDistance + xOffset + (NodeCollection.Count - Parameters.NumInputs - Parameters.NumOutputs + 2) * nodeDistance + i * 0.2f, pos.y - nodeDistance * i - yOffset, pos.z);
			positions.Add (myPos);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere (myPos, radius);
            TextGizmo.Draw(myPos, NodeCollection[i + Parameters.NumInputs].NodeIndex.ToString());
            //TextGizmo.Draw(myPos, NodeCollection[NodeCollection.Count - _inputCount + i]._value.ToString());
		}
        //Draw Hidden Nodes
        for (var i = 0; i < NodeCollection.Count - Parameters.NumInputs - Parameters.NumOutputs; i++)
        {
            var myPos = new Vector3(pos.x + nodeDistance * (i + 1) + xOffset + i * 0.2f, pos.y - (positions[0].y - positions[Parameters.NumInputs - 1].y) 
                * ((float)i / (NodeCollection.Count - Parameters.NumInputs - Parameters.NumOutputs)) - nodeDistance / 2 - yOffset, pos.z);
            positions.Add(myPos);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(myPos, radius);
            //TextGizmo.Draw(myPos, NodeCollection[i + _inputCount]._value.ToString());
            TextGizmo.Draw(myPos, NodeCollection[i + Parameters.NumInputs + Parameters.NumOutputs].NodeIndex.ToString());
        }


        for (var i = LinkNetwork.Count - 1; i >= 0; i--)
        {
            if (LinkNetwork[i].Weight > 0)
            {
                Gizmos.color = new Color(0, 1.0f, 0);
            }
            else if (LinkNetwork[i].Weight < 0)
            {
                Gizmos.color = new Color(1.0f, 0, 0); 
            }
            if (!LinkNetwork[i].Enabled)
            {
                Gizmos.color = new Color(0, 0, 0);
            }
            Gizmos.DrawLine(positions[LinkNetwork[i].In.NodeIndex], positions[LinkNetwork[i].Out.NodeIndex]);
        }
	}
}