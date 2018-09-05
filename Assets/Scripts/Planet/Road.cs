using Assets.Scripts.Manager;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoadInfo : object
{
    public string name;
    public List<Vector3> tracks;
}

namespace Assets.Scripts.Planet
{
    /// <summary>
    /// 
    /// </summary>

    public class Road : MonoBehaviour
    {
        public static Dictionary<string, RoadInfo> TRACKS = new Dictionary<string, RoadInfo>();

        public RoadInfo roadInfo;
        protected Mesh _personnalMesh;

        public void Awake()
        {
            roadInfo = JsonManager.FromJson<RoadInfo>("Json/Roads/" + roadInfo.name + ".json", RuntimePlatform.WindowsPlayer);
            if (roadInfo != null) TRACKS.Add(roadInfo.name, roadInfo);
        }
    }
}