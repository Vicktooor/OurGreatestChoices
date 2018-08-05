using UnityEngine;

/// <summary>
/// 
/// </summary>
public class TextFile : MonoBehaviour {

    #region Public Variable

    [HideInInspector]
    public string[] textFiles;

    public TextAsset frenchTextFile;

    public TextAsset englishTextFile;

    #endregion

    #region Private Variable

    #endregion

    // Use this for initialization
    void Start() {
        //InitArray();
    }

    void InitArray() {
        textFiles[0] = frenchTextFile.text;
        textFiles[1] = englishTextFile.text;
    }

    void Update() {

    }
}