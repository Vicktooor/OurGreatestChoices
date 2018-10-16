using Assets.Scripts.Game;
using Assets.Scripts.Game.Objects;
using Assets.Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;

public class DeforestationArea
{
    private static List<Cell> CELLS_USED = new List<Cell>();
    public static int NB_FOREST_CUT = 0;
    protected float _ratio;

    public Cell initialCell;

    private int cellIterator = 0;
    private int iteration = 0;
    private List<ObjectArray<Cell>> iterationCells = new List<ObjectArray<Cell>>();

    public DeforestationArea(int cID)
    {
        ObjectArray<Cell> newArray = new ObjectArray<Cell>();
        initialCell = EarthManager.Instance.Cells.Find(c => c.ID == cID);
        newArray.Add(initialCell);
        iterationCells.Add(newArray);
    }

    public void Start()
    {
        if (iteration == 0 && cellIterator == 0)
        {
            iterationCells.Clear();
            ObjectArray<Cell> newArray = new ObjectArray<Cell>();
            newArray.Add(initialCell);
            iterationCells.Add(newArray);
        }
    }

    public void Action()
    {
        GetRatio();
        bool toCut = HaveToCut();

        float targetRatio = GetTargetRatio();
        if (toCut)
        {
            if (_ratio <= targetRatio) return;
        } 
        else
        {
            if (_ratio >= targetRatio) return;
        }

        ObjectArray<Cell> iArray = iterationCells[iteration];
        if (toCut)
        {
            if (cellIterator < iArray.Length && cellIterator >= 0)
            {
                foreach (KeyValuePair<Props, string> p in iArray.Objs[cellIterator].obj.Props)
                {
                    if (p.Key.GetType() == typeof(PoolTree))
                    {
                        PoolTree pt = (PoolTree)p.Key;
                        pt.SetDeforestation(toCut);
                    }
                }
                cellIterator++;
            }
            else ExtendDeforestation(iArray.Objs);
        }
        else
        {
            if (cellIterator < iArray.Length && cellIterator >= 0)
            {
                foreach (KeyValuePair<Props, string> p in iArray.Objs[cellIterator].obj.Props)
                {
                    if (p.Key.GetType() == typeof(PoolTree))
                    {
                        PoolTree pt = (PoolTree)p.Key;
                        pt.SetDeforestation(toCut);
                    }
                }
                cellIterator++;
            }
            else RetractDeforestation();
        }
    }

    protected void ExtendDeforestation(NamedObject<Cell>[] tCells)
    {
        ObjectArray<Cell> newArray = new ObjectArray<Cell>();

        int nbCell = tCells.Length;
        for (int i = 0; i < nbCell; i++)
        {
            int nbICell = tCells[i].obj.Neighbors.Count;
            for (int j = 0; j < nbICell; j++)
            {
                Cell neighbor = tCells[i].obj.Neighbors[j];
                if (!newArray.Contains(neighbor) && PoolTree.ForestCells.Contains(neighbor) && !CELLS_USED.Contains(neighbor))
                {
                    newArray.Add(neighbor);
                    CELLS_USED.Add(neighbor);
                }
            }
        }

        if (newArray.Length > 0)
        {
            iteration++;
            cellIterator = 0;
            newArray.CheckIt();
            iterationCells.Add(newArray);
        }
        else ForceExtend();
    }

    protected void ForceExtend()
    {
        List<Cell> catchCells = new List<Cell>();
        foreach (NamedObject<Cell> c in PoolTree.ForestCells.Objs)
        {
            if (!CELLS_USED.Contains(c.obj))
            {
                bool containUncutAsset = false;
                List<PoolTree> treesAsset = c.obj.GetProps<PoolTree>();
                foreach (PoolTree pt in treesAsset)
                {
                    if (!pt.IsCut)
                    {
                        containUncutAsset = true;
                        break;
                    }
                }
                if (containUncutAsset)
                {
                    catchCells.Add(c.obj);
                    CELLS_USED.Add(c.obj);
                }
            }
        }

        if (catchCells.Count > 0)
        {
            iteration++;
            cellIterator = 0;
            ObjectArray<Cell> newArray = new ObjectArray<Cell>();
            int lLength = catchCells.Count;
            for (int i = 0; i < lLength; i++) newArray.Add(catchCells[i]);
            iterationCells.Add(newArray);
        }
    }

    protected void RetractDeforestation()
    {
        if (iteration > 0)
        {
            int lLength = iterationCells[iteration].Length;
            for (int i = 0; i < lLength; i++) CELLS_USED.Remove(iterationCells[iteration].Objs[i].obj);

            iterationCells.Remove(iterationCells[iteration]);
            iteration--;
            cellIterator = 0;
        }
        else if (NB_FOREST_CUT > 0) ForceRetract();
    }

    private void ForceRetract()
    {
        List<Cell> catchCells = new List<Cell>();
        foreach (NamedObject<Cell> c in PoolTree.ForestCells.Objs)
        {
            if (!CELLS_USED.Contains(c.obj))
            {
                bool containCutAsset = false;
                List<PoolTree> treesAsset = c.obj.GetProps<PoolTree>();
                foreach (PoolTree pt in treesAsset)
                {
                    if (pt.IsCut)
                    {
                        containCutAsset = true;
                        break;
                    }
                }
                if (containCutAsset)
                {
                    catchCells.Add(c.obj);
                    CELLS_USED.Add(c.obj);
                }
            }
        }

        if (catchCells.Count > 0)
        {
            iteration++;
            cellIterator = 0;
            ObjectArray<Cell> newArray = new ObjectArray<Cell>();
            int lLength = catchCells.Count;
            for (int i = 0; i < lLength; i++) newArray.Add(catchCells[i]);
            iterationCells.Add(newArray);
        }
    }

    protected void GetRatio()
    {
        _ratio = (float)(EarthManager.NB_FOREST_PROPS - NB_FOREST_CUT) / (float)EarthManager.NB_FOREST_PROPS;
    }

    protected bool HaveToCut()
    {        
        if (WorldValues.STATE_FOREST <= -2f) return true;
        else if (WorldValues.STATE_FOREST <= -1f) return true;
        else if (WorldValues.STATE_FOREST <= 0f) return true;
        else if (WorldValues.STATE_FOREST >= 2f) return false;
        else if (WorldValues.STATE_FOREST >= 1f) return false;
        else return false;
    }

    protected float GetTargetRatio()
    {
        if (WorldValues.STATE_FOREST <= -2f) return 0f;
        else if (WorldValues.STATE_FOREST <= -1f) return 0.25f;
        else if (WorldValues.STATE_FOREST >= 2f) return 1f;
        else if (WorldValues.STATE_FOREST >= 1f) return 0.75f;
        else return 0.5f;
    }
}