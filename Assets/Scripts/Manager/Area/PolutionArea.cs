using Assets.Scripts.Game;
using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PolutionArea
{
    public static int SEA_POLUTED = 0;
    public static int SNOW_POLUTED = 0;
    public static int FIELD_POLUTED = 0;

    private static List<Cell> _CELLS_USED = new List<Cell>();
    public static List<Cell> CELLS_USED { get { return _CELLS_USED; } }
    protected float _ratio = 0;

    public Cell initialCell;

    private List<CellState> polutionStates;
    private int cellIterator = 0;
    private int iteration = 0;
    private List<List<Cell>> iterationCells = new List<List<Cell>>();

    public PolutionArea(int cID, List<CellState> polutionTypes)
    {
        ObjectArray<Cell> newArray = new ObjectArray<Cell>();
        initialCell = EarthManager.Instance.Cells.Find(c => c.ID == cID);
        polutionStates = polutionTypes;
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

        List<Cell> iArray = iterationCells[iteration];
        if (toPoluted)
        {
            if (cellIterator < iArray.Count && cellIterator >= 0)
            {
                iArray[cellIterator].SetPolution(toPoluted);
                cellIterator++;
            }
            else ExtendPolution(iArray);
        }
        else
        {
            if (cellIterator <= iArray.Count && cellIterator > 0)
            {
                cellIterator--;
                iArray[cellIterator].SetPolution(toPoluted);
            }
            else RetractPolution();
        }
    }

    protected void ExtendPolution(List<Cell> tCells)
    {
        List<Cell> newArray = new List<Cell>();

        int nbCell = tCells.Count;
        for (int i = 0; i < nbCell; i++)
        {
            int nbICell = tCells[i].Neighbors.Count;
            for (int j = 0; j < nbICell; j++)
            {
                Cell neighbor = tCells[i].Neighbors[j];
                if (!neighbor.Poluted && !newArray.Contains(neighbor) && !CELLS_USED.Contains(neighbor) && polutionStates.Contains(neighbor.State))
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
        List<Cell> catchCells = EarthManager.Instance.Cells.FindAll(c => polutionStates.Contains(c.State) && !_CELLS_USED.Contains(c));
        if (catchCells.Count > 0)
        {
            iteration++;
            cellIterator = 0;
            _CELLS_USED.Add(catchCells[0]);
            List<Cell> newArray = new List<Cell>();
            newArray.Add(catchCells[0]);
            iterationCells.Add(newArray);
            Action();
        }
    }

    protected void RetractPolution()
    {
        if (iteration > 0)
        {
            int lLength = iterationCells[iteration].Count;
            for (int i = 0; i < lLength; i++) CELLS_USED.Remove(iterationCells[iteration][i]);

            iterationCells.Remove(iterationCells[iteration]);
            iteration--;
            cellIterator = iterationCells[iteration].Count;
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
            List<Cell> newArray = new List<Cell>();
            int lLength = catchCells.Count;
            for (int i = 0; i < lLength; i++) newArray.Add(catchCells[i]);
            iterationCells.Add(newArray);
            Action();
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

    private void Clear()
    {
        cellIterator = 0;
        iteration = 0;
        iterationCells.Clear();
    }

    public static void ClearStatic()
    {
        _CELLS_USED.Clear();
        SEA_POLUTED = 0;
        SNOW_POLUTED = 0;
        FIELD_POLUTED = 0;
    }
}