using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGender : MonoBehaviour {
    // True = Male - False = Female
    public bool gender { get { return _gender; } }
    [SerializeField]
    [Tooltip("True = Male --- False = Female")]
    bool _gender = false;
}
