public enum NodeType
{
    Input,
    OutPut,
    Hidden
}

[System.Serializable]
public class Link
{

    public Node In;
    public Node Out;
    public int InovationNum;

    public bool Enabled;
    public float Weight;

    public Link(Node In, Node Out, float weight, int inovationNumber)
    {
        this.In = In;
        this.Out = Out;
        Enabled = true;
        Weight = weight;
        InovationNum = inovationNumber;
    }
}

[System.Serializable]
[System.Diagnostics.DebuggerDisplay("Type = {Type}")]
public class Node
{
    public int  NodeIndex,
                InovationNum;
	public float Value;
	public NodeType Type;

    public bool Calculated = false;

    public Node(int nodeIndex, int inovationNum, NodeType type = NodeType.Hidden)
    {
        NodeIndex = nodeIndex;
        InovationNum = inovationNum;
		Type = type;
    }
}

