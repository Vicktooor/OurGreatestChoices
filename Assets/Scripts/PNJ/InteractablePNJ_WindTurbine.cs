using Assets.Scripts.Items;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePNJ_WindTurbine : InteractablePNJ {
    private int _level = 0;
    public int level {
        get { return _level; }
    }

    [SerializeField]
    private List<GameObject> _windTurbinesList;

    public override void SearchPropLinked() {
        _windTurbinesList = new List<GameObject>();

        LinkDatabase lLinkDatabase = LinkDatabase.Instance;
        _windTurbinesList = lLinkDatabase.GetLinkObjects(buildingLink, typeof(WindTurbineItemProp));
        DisableWindTurbines();
        ActiveWindTurbines(level);
    }


    protected override void OnEnable() {
        base.OnEnable();
        ActiveWindTurbines(level);
    }

    protected override void OnDisable() {
        base.OnDisable();
        DisableWindTurbines();
    }

    //Hide all the windTurbines to make an empty place
    void DisableWindTurbines() {
        for (int i = 0; i < _windTurbinesList.Count; i++) {
            _windTurbinesList[i].SetActive(false);
        }
    }


    void ActiveWindTurbines(int level) {
        for (int i = 0; i < _windTurbinesList.Count; i++) {
			if (i < level) _windTurbinesList[i].SetActive(true);
			else _windTurbinesList[i].SetActive(false);
		}
    }
}
