using System.Collections.Generic;

public enum InnovationType
{
    Node,
    Link
}


public struct Innovation
{
    public int InputIndex;
    public int OutputIndex;
    public InnovationType InovType;
    public NodeType NodeInovType;

    public int InnovationNumber;
    public int NodeId;

    public Innovation(int inputIndex, int outputIndex, InnovationType inovType, int innovationNumber)
    {
        InputIndex = inputIndex;
        OutputIndex = outputIndex;
        InovType = inovType;
        NodeInovType = NodeType.Hidden;
        InnovationNumber = innovationNumber;
        NodeId = 0;
    }

    public Innovation(int inputIndex, int outputIndex, InnovationType inovType, NodeType nodeType, int innovationNumber)
    {
        InputIndex = inputIndex;
        OutputIndex = outputIndex;
        InovType = inovType;
        NodeInovType = nodeType;
        InnovationNumber = innovationNumber;
        NodeId = 0;
    }
}

public class InnovationManager {
    
    public static List<Innovation> InovationList = new List<Innovation>();

    public static int GlobalNextNodeId;
    public static int GlobalInnovationCounter;


    /// <summary>
    /// Checks if this Link has already been made this generation,
    /// if so, return the inovation number, else return -1
    /// </summary>
    public static int CheckInnovation(int inputIndex, int outputIndex, InnovationType type)
    {
        foreach (Innovation innovation in InovationList)
        {
            if (innovation.InputIndex == inputIndex && innovation.OutputIndex == outputIndex && innovation.InovType == type)
            {
                return innovation.InnovationNumber;
            }
        }

        return -1;
    }

    public static int CreateNewInnovation(int inputIndex, int outputIndex, InnovationType type)
    {
        Innovation tempInnovation = new Innovation(inputIndex, outputIndex, type, GlobalInnovationCounter);

        if (type == InnovationType.Node)
        {
            tempInnovation.NodeId = GlobalNextNodeId;
            ++GlobalNextNodeId;
        }

        InovationList.Add(tempInnovation);

        ++GlobalInnovationCounter;

        return GlobalInnovationCounter - 1 ;
    }


    public static int CreateNewInnovation(int inputIndex, int outputIndex, InnovationType type, NodeType nodeType)
    {
        Innovation tempInnovation = new Innovation(inputIndex, outputIndex, type, nodeType, GlobalInnovationCounter);

        if (type == InnovationType.Node)
        {
            tempInnovation.NodeId = GlobalNextNodeId;
            ++GlobalNextNodeId;
        }

        InovationList.Add(tempInnovation);

        ++GlobalInnovationCounter;

        return (GlobalNextNodeId - 1);
    }

    public static Node CreateNodeFromId(int nodeId)
    {
        Node tempNode = new Node(0,0);
        foreach (Innovation innovation in InovationList)
        {
            if (innovation.NodeId != nodeId) continue;
            tempNode.Type = innovation.NodeInovType;
            tempNode.NodeIndex = innovation.NodeId;
            tempNode.InovationNum = innovation.InnovationNumber;

            return tempNode;
        }
        return tempNode;
    }
}
