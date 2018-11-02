using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script;

public class PropParticlesDisplay : MonoBehaviour {

    GameObject ps;

    string NEGATIVE_FBX = "FX/PFB_FX_NegativeChange01";
    string POSITIVE_FBX = "FX/PFB_FX_PositiveChange01";

    public void DisplayFX(bool pIsPositive) {
        if (pIsPositive) ps = Instantiate(Resources.Load(POSITIVE_FBX), new Vector3(transform.position.x, transform.position.y- (transform.position.y/2), transform.position.z), Quaternion.identity) as GameObject;
        else ps = Instantiate(Resources.Load(NEGATIVE_FBX), transform.position, Quaternion.identity) as GameObject;
        ps.transform.Rotate(new Vector3(-90f, -180f, 0));
        ps.GetComponent<ParticleSystem>().Play();
    }

    // Update is called once per frame
    void Update() {
        if (ps != null) {
            if (ps.GetComponent<ParticleSystem>().isStopped) Destroy(ps.gameObject);
        }
    }
}
