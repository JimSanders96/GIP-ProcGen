using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionMarker : MonoBehaviour {

    public Text displayText;

    public void Init(MissionNodeData data)
    {
        displayText.text = data.type.ToString() + " - " + data.keyNr; 
    }
}
