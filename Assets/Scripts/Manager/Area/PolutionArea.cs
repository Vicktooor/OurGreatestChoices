using Assets.Scripts.Game;
using Assets.Scripts.Manager;
using System.Collections.Generic;
using UnityEngine;

public class PolutionArea
{
    public static int SEA_POLUTED = 0;
    public static int SNOW_POLUTED = 0;
    public static int FIELD_POLUTED = 0;

    private static List<Cell> CELLS_USED = new List<Cell>();
    protected float _ratio = 0;

    public Cell initialCell;

    private List<CellState> polutionStates;
    private int cellIterator = 0;
    private int iteration = 0;
    private List<ObjectArray<Cell>> iterationCells = new List<ObjectArray<Cell>>();

    public PolutionArea(int cID, List<CellState> polutionTypes)
    {
        ObjectArray<Cell> newArray = new ObjectArray<Cell>();
        initialCell = EarthManager.Instance.Cells.Find(c => c.ID == cID);
        polutionStates = polutionTypes;
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
        bool toPoluted = HaveToPolute();

        float targetRatio = GetTargetRatio();
        if (toPoluted)
        {
            if (_ratio >= targetRatio) return;
        }
        else
        {
            if (_ratio <= targetRatio) return;
        }

        ObjectArray<Cell> iArray = iterationCells[iteration];
        if (toPoluted)
        {
            if (cellIterator < iArray.Length && cellIterator >= 0)
            {
                iArray.Objs[cellIterator].obj.SetPolution(toPoluted);
                cellIterator++;
            }
            else ExtendPolution(iArray.Objs);
        }
        else
        {
            if (cellIterator <= iArray.Length && cellIterator > 0)
            {
                cellIterator--;
                iArray.Objs[cellIterator].obj.SetPolution(toPoluted);
            }
            else RetractPolution();
        }
    }

    protected void ExtendPolution(NamedObject<Cell>[] tCells)
    {
        ObjectArray<Cell> newArray = new ObjectArray<Cell>();

        int nbCell = tCells.Length;
        for (int i = 0; i < nbCell; i++)
        {
            int nbICell = tCells[i].obj.Neighbors.Count;
            for (int j = 0; j < nbICell; j++)
            {
                Cell neighbor = tCells[i].obj.Neighbors[j];
                if (!neighbor.Poluted && !newArray.Contains(neighbor) && !CELLS_USED.Contains(neighbor) && polutionStates.Contains(neighbor.State))
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
        List<Cell> catchCells = EarthManager.Instance.Cells.FindAll(c => polutionStates.Contains(c.State) && !CELLS_USED.Contains(c));
        if (catchCells.Count > 0)
        {
            iteration++;
            cellIterator = 0;
            CELLS_USED.Add(catchCells[0]);
            ObjectArray<Cell> newArray = new ObjectArray<Cell>();
            newArray.Add(catchCells[0]);
            iterationCells.Add(newArray);
        }
    }

    protected void RetractPolution()
    {
        if (iteration > 0)
        {
            int lLength = iterationCells[iteration].Length;
            for (int i = 0; i < lLength; i++) CELLS_USED.Remove(iterationCells[iteration].Objs[i].obj);

            iterationCells.Remove(iterationCells[iteration]);
            iteration--;
            cellIterator = iterationCells[iteration].Length;
        }
        else ForceRetract();
    }

    private void ForceRetract()
    {
        List<Cell> catchCells = EarthManager.Instance.Cells.FindAll(c => polutionStates.Contains(c.State) && c.Poluted);
        if (catchCells.Count > 0)
        {
            iteration++;
            cellIterator = catchCells.Count;
            ObjectArray<Cell> newArray = new ObjectArray<Cell>();
            int lLength = catchCells.Count;
            for (int i = 0; i < lLength; i++) newArray.Add(catchCells[i]);
            iterationCells.Add(newArray);
        }
    }

    protected void GetRatio()
    {
        if (polutionStates.Contains(CellState.GRASS) && polutionStates.Contains(CellState.MOSS))
        {
            _ratio = (float)FIELD_POLUTED / (float)EarthManager.NB_FIELD_GROUND;
        }
        else if (polutionStates.Contains(CellState.SEA))
        {
            _ratio = (float)SEA_POLUTED / (float)EarthManager.NB_SEA_GROUND;
        }
        else if (polutionStates.Contains(CellState.SNOW))
        {
            _ratio = (float)SNOW_POLUTED / (float)EarthManager.NB_SNOW_GROUND;
        }
    }

    protected bool HaveToPolute()
    {
        if (WorldValues.STATE_CLEANLINESS <= -2f) return true;
        else if (WorldValues.STATE_CLEANLINESS <= -1f) return true;
        else if (WorldValues.STATE_CLEANLINESS <= 0f) return true;
        else if (WorldValues.STATE_CLEANLINESS >= 2f) return false;
        else if (WorldValues.STATE_CLEANLINESS >= 1f) return false;
        else return false;
    }

    protected float GetTargetRatio()
    {
        if (WorldValues.STATE_CLEANLINESS <= -2f) return 1f;
        else if (WorldValues.STATE_CLEANLINESS <= -1f) return 0.75f;
        else if (WorldValues.STATE_CLEANLINESS >= 2f) return 0f;
        else if (WorldValues.STATE_CLEANLINESS >= 1f) return 0.25f;
        else return 0.5f;
    }
}