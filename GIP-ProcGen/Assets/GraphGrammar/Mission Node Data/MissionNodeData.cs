using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionNodeData  {

    public MissionNodeTypes type { get; set; }
    public int keyNr { get; set; }
    public Mechanics[] challengeMechanics { get; set; }

    public MissionNodeData(MissionNodeTypes type, int keyNr = -1, Mechanics[] challengeMechanics = null)
    {
        this.type = type;
        this.keyNr = keyNr;
        this.challengeMechanics = challengeMechanics;
    }

}
