using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shake : MonoBehaviour {

    public Button button;
    public float damageTime = 0.4f;
    public float shakeRange = 20f;

    protected Quaternion oRot;
    protected Color oColor;

    void Start() {
        if (GetComponent<Button>() != null)
        {
            button = GetComponent<Button>();
            oRot = button.transform.rotation;
            oColor = button.GetComponent<Image>().color;
        }
    }

    public void DoShake() {
        StartCoroutine(Damage());
        StartCoroutine(EnemyShake());
    }

    private IEnumerator Damage() {
        WaitForSeconds wait = new WaitForSeconds(damageTime);
        button.GetComponent<Image>().color = new Color32(255, 0, 0, 255); //adjust color to your needs
        yield return wait;
        button.GetComponent<Image>().color = oColor;
    }

    private IEnumerator EnemyShake() {

        float elapsed = 0.0f;
        while (elapsed < damageTime) {

            elapsed += Time.deltaTime;
            float z = Random.value * shakeRange - (shakeRange / 2);
            button.transform.eulerAngles = new Vector3(oRot.x, oRot.y, oRot.z + z);
            yield return null;
        }

        button.transform.rotation = oRot;
    }

    public void Clear()
    {
        StopAllCoroutines();
        if (button)
        {
            button.transform.rotation = oRot;
            button.GetComponent<Image>().color = oColor;
        }
    }
}
