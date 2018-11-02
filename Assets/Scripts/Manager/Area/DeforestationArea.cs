using Assets.Scripts.Game;
using Assets.Scripts.Game.Objects;
using Assets.Scripts.Game.Save;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;

public class DeforestationArea
{
    private static List<Cell> _CELLS_USED = new List<Cell>();
    public static List<Cell> CELLS_USED { get { return _CELLS_USED; } }
    public static int NB_FOREST_CUT = 0;
    protected float _ratio;

    public Cell initialCell;

    private int cellIterator = 0;
    private int iteration = 0;
    private List<List<Cell>> iterationCells = new List<List<Cell>>();

    public DeforestationArea(int cID)
    {
        initialCell = EarthManager.Instance.Cells.Find(c => c.ID == cID);
        iterationCells.Add(new List<Cell>() { initialCell });
    }

    public void Start()
    {
        if (iteration == 0 && cellIterator == 0)
        {
            Clear();
            iterationCells.Add(new List<Cell>() { initialCell });
            Action();
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

        List<Cell> iArray = iterationCells[iteration];
        if (toCut)
        {
            if (cellIterator < iArray.Count && cellIterator >= 0)
            {
                iArray[cellIterator].SetDeforestation(toCut);
                cellIterator++;
            }
            else ExtendDeforestation(iArray);
        }
        else
        {
            if (cellIterator < iArray.Count && cellIterator >= 0)
            {
                iArray[cellIterator].SetDeforestation(toCut);
                cellIterator++;
            }
            else RetractDeforestation();
        }
    }

    protected void ExtendDeforestation(List<Cell> tCells)
    {
        List<Cell> newArray = new List<Cell>();

        int nbCell = tCells.Count;
        for (int i = 0; i < nbCell; i++)
        {
            int nbICell = tCells[i].Neighbors.Count;
            for (int j = 0; j < nbICell; j++)
            {
                Cell neighbor = tCells[i].Neighbors[j];
                if (!newArray.Contains(neighbor) && PoolTree.ForestCells.Contains(neighbor) && !_CELLS_USED.Contains(neighbor))
                {
                    newArray.Add(neighbor);
                    _CELLS_USED.Add(neighbor);
                }
            }
        }

        if (newArray.Count > 0)
        {
            iteration++;
            cellIterator = 0;
            newArray = ObjectArray<Cell>.CheckIt(newArray);
            iterationCells.Add(newArray);
            Action();
        }
        else ForceExtend();
    }

    protected void ForceExtend()
    {
        List<Cell> catchCells = new List<Cell>();
        foreach (Cell c in PoolTree.ForestCells)
        {
            if (!_CELLS_USED.Contains(c))
            {
                bool containUncutAsset = false;
                List<PoolTree> treesAsset = c.GetProps<PoolTree>();
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
                    catchCells.Add(c);
                    _CELLS_USED.Add(c);
                }
            }
        }

        if (catchCells.Count > 0)
        {
            iteration++;
            cellIterator = 0;
            List<Cell> newArray = new List<Cell>();
            int lLength = catchCells.Count;
            for (int i = 0; i < lLength; i++) newArray.Add(catchCells[i]);
            iterationCells.Add(newArray);
            Action();
        }
    }

    protected void RetractDeforestation()
    {
        if (iteration > 0)
        {
            int lLength = iterationCells[iteration].Count;
            for (int i = 0; i < lLength; i++) _CELLS_USED.Remove(iterationCells[iteration][i]);

            iterationCells.Remove(iterationCells[iteration]);
            iteration--;
            cellIterator = 0;
        }
        else if (NB_FOREST_CUT > 0) ForceRetract();
    }

    private void ForceRetract()
    {
        List<Cell> catchCells = new List<Cell>();
        foreach (Cell c in PoolTree.ForestCells)
        {
            if (!_CELLS_USED.Contains(c))
            {
                bool containCutAsset = false;
                List<PoolTree> treesAsset = c.GetProps<PoolTree>();
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
                    catchCells.Add(c);
                    _CELLS_USED.Add(c);
                }
            }
        }

        if (catchCells.Count > 0)
        {
            iteration++;
            cellIterator = 0;
            List<Cell> newArray = new List<Cell>();
            int lLength = catchCells.Count;
            for (int i = 0; i < lLength; i++) newArray.Add(catchCells[i]);
            iterationCells.Add(newArray);
            Action();
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

    private void Clear()
    {
        cellIterator = 0;
        iteration = 0;
        iterationCells.Clear();
    }

    public static void ClearStatic()
    {
        _CELLS_USED.Clear();
        NB_FOREST_CUT = 0;
    }
}