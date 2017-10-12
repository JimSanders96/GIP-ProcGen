using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionNodeData
{

    public MissionNodeTypes type { get; set; }
    public int keyNr { get; set; }
    public Mechanics[] challengeMechanics { get; set; }

    public MissionNodeData(MissionNodeTypes type, int keyNr = -1, Mechanics[] challengeMechanics = null)
    {
        this.type = type;
        this.keyNr = keyNr;
        this.challengeMechanics = challengeMechanics;
    }

    public override string ToString()
    {
        string s = "";
        s += type;

        if (type == MissionNodeTypes.KEY)
        {
            s += " | KeyNr: " + keyNr + " | Challenge: ";
            foreach (Mechanics mechanic in challengeMechanics)
                s += mechanic + ", ";
        }
        else if (type == MissionNodeTypes.LOCK)
        {
            s += " | Lock: " + keyNr;
        }
        return s;
    }
}
