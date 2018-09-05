using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Planet;

public class TrackFollower : MonoBehaviour
{
    public string trackName;

    private List<Vector3> track;
    private int trackStep;

    public void Start()
    {
        if (Road.TRACKS.ContainsKey(trackName))
        {
            track = Road.TRACKS[trackName].tracks;
            StartCoroutine("TrackCoroutine");
        }
    }

    private IEnumerator TrackCoroutine()
    {
        float k = 0;
        float dist = (track[trackStep] - track[trackStep + 1]).magnitude;
        while (true)
        {
            k = Mathf.Clamp01(k + (Time.deltaTime * (0.2f / dist)));
            transform.rotation = Quaternion.LookRotation(track[trackStep] - track[trackStep + 1], transform.position);
            transform.position = Vector3.Lerp(track[trackStep], track[trackStep + 1], k);

            if (k >= 1)
            {
                if (trackStep >= track.Count - 2) trackStep = 0;
                else trackStep++;
                dist = (track[trackStep] - track[trackStep + 1]).magnitude;
                k = 0;
            }

            yield return null;
        }
    }
}
