using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Random = UnityEngine.Random;

[Serializable]
public class GeneticAlgorithms
{
    private enum ParentType
    {
        Parent1,
        Parent2
    }

    public Genome CrossOver(Genome parent1, Genome parent2)
    {
        ParentType best;

        //Sort The links by their inovation numbers
        parent1.LinkNetwork.Sort((x, y) => x.InovationNum.CompareTo(y.InovationNum));
        parent2.LinkNetwork.Sort((x, y) => x.InovationNum.CompareTo(y.InovationNum));

        if (Math.Abs(parent1.Fitness - parent2.Fitness) < 0.01f) //If same fitness pick random one for best
        {
            best = Random.value > 0.5f ? ParentType.Parent1 : ParentType.Parent2;
        }
        else if (parent1.Fitness > parent2.Fitness) //If the parent 1 is better 
            best = ParentType.Parent1;
        else //parent 2 is the best
            best = ParentType.Parent2;

        var offspringNodes = new List<Node>();
        var offspringLinks = new List<Link>();

        var nodeIDs = new List<int>();
        for (int i = 0; i < Parameters.NumInputs + Parameters.NumOutputs; i++)
        {
            AddNodeId(i, nodeIDs);
        }

        var parent1Itterator = 0;//Parameters.NumInputs + Parameters.NumOutputs;
        var parent1Size = parent1.LinkNetwork.Count;

        var parent2Itterator = 0;//Parameters.NumInputs + Parameters.NumOutputs;
        var parent2Size = parent2.LinkNetwork.Count;

        var currentLink = new Link(null,null,0,0);

        while (!(parent1Itterator == parent1Size && parent2Itterator == parent2Size))
        {
            if (parent1Itterator == parent1Size && parent2Itterator != parent2Size)
            {
                if (best == ParentType.Parent2) //If we reached the end of parent 1, but parent 2 is the best 
                    currentLink = parent2.LinkNetwork[parent2Itterator];
                else
                    break;

                ++parent2Itterator; //Go to next node from parent 2
            }
            else if (parent2Itterator == parent2Size && parent1Itterator != parent1Size)
            {
                if (best == ParentType.Parent1) //If we reached the end of parent 2, but parent 1 is the best
                    currentLink = parent1.LinkNetwork[parent1Itterator];
                else
                    break;

                ++parent1Itterator;
            }
            else
            {
                Link parent1Link = parent1.LinkNetwork[parent1Itterator];
                Link parent2Link = parent2.LinkNetwork[parent2Itterator];

                if (parent1Link.InovationNum < parent2Link.InovationNum)
                {
                    if (best == ParentType.Parent1)
                        currentLink = parent1Link;
                    else
                        break;

                    ++parent1Itterator;
                }
                else if (parent1Link.InovationNum > parent2Link.InovationNum)
                {
                    if (best == ParentType.Parent2)
                        currentLink = parent2Link;
                    else
                        break;

                    ++parent2Itterator;
                }
                else if (parent1Link.InovationNum == parent2Link.InovationNum)
                {
                    currentLink = Random.value > 0.5f ? parent1Link : parent2Link;

                    ++parent1Itterator;
                    ++parent2Itterator;
                }
            }

            if (offspringLinks.Count == 0)
            {
                offspringLinks.Add(ObjectCopier.Clone(currentLink));
            }
            else
            {
                if (offspringLinks[offspringLinks.Count-1].InovationNum != currentLink.InovationNum)
                {
                    offspringLinks.Add(ObjectCopier.Clone(currentLink));
                }
            }

            //Check if the nodes for the current Link are existant, else add them
            AddNodeId(currentLink.In.NodeIndex, nodeIDs);
            AddNodeId(currentLink.Out.NodeIndex, nodeIDs);
        }

        //Reate all the required Nodes, in order
        nodeIDs = nodeIDs.Distinct().ToList();
        nodeIDs.Sort();

        offspringNodes.AddRange(nodeIDs.Select(InnovationManager.CreateNodeFromId));

        Genome offspring = new Genome(0)
        {
            NodeCollection = offspringNodes,
            LinkNetwork = offspringLinks
        };

        return offspring;
    }

    private static void AddNodeId(int nodeId, ICollection<int> nodeIdList)
    {
        if (!nodeIdList.Contains(nodeId))
        {
            nodeIdList.Add(nodeId);
        }
    }

    public void Epoch(List<float> fitnessScores)
    {
        if (fitnessScores.Count != Parameters.PoolSize)
        {
            //NOT ENOUGH FITNESS PASSED
            return;
        }

        ResetAndKill();
    }

    public void ResetAndKill()
    {
        
    }

}