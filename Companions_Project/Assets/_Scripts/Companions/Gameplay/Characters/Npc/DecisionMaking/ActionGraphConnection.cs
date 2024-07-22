using UnityEngine;

[System.Serializable]
public struct ActionGraphConnection
{
    public ActionGraphConnectionPort inputPort;
    public ActionGraphConnectionPort outputPort;

    public ActionGraphConnection(ActionGraphConnectionPort input, ActionGraphConnectionPort output)
    {
        inputPort = input;
        outputPort = output;
    }

    public ActionGraphConnection(string inputPortId, int inputIndex, string outputPortId, int outputIndex)
    {
        inputPort = new(inputPortId, inputIndex);
        outputPort = new(outputPortId, outputIndex);
    }
}

[System.Serializable]
public struct ActionGraphConnectionPort
{
    public string nodeId;
    public int portIndex;

    public ActionGraphConnectionPort(string id, int index)
    {
        this.nodeId = id;
        this.portIndex = index;
    }
}
