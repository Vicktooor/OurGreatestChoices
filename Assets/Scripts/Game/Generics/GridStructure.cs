using System;
using UnityEngine;

[Serializable]
public struct IntVector2
{
    public int x;
    public int y;

    public IntVector2(int px, int py)
    {
        x = px;
        y = py;
    }

    public bool Equals(IntVector2 testPos)
    {
        if (testPos.x == x && testPos.y == y) return true;
        return false; 
    }

    public static float Distance(IntVector2 v1, IntVector2 v2)
    {
        Vector2 fv1 = new Vector2(v1.x, v1.y);
        Vector2 fv2 = new Vector2(v2.x, v2.y);
        return Vector2.Distance(fv1, fv2);
    }

    public static IntVector2 operator +(IntVector2 c1, IntVector2 c2) {
        return new IntVector2(c1.x + c2.x, c1.y + c2.y);
    }

    public static IntVector2 operator -(IntVector2 c1, IntVector2 c2) {
        return new IntVector2(c1.x - c2.x, c1.y - c2.y);
    }
}

public struct GridStructure<T> where T : UnityEngine.Object
{
    private IntVector2 _size;
    public IntVector2 Size { get { return _size; } }

    private T[,] _grid;

    public GridStructure(int xSize, int ySize)
    {
        _size = new IntVector2(xSize, ySize);
        _grid = new T[xSize, ySize];
    }

    public T GetObject(IntVector2 pos)
    {
        if (pos.x < _size.x && pos.y < _size.y && pos.x >= 0 && pos.y >= 0)
            return _grid[pos.x, pos.y];
        else return null;
    }

    public bool IsInGrid(IntVector2 gridPos)
    {
        if (gridPos.x < _size.x && gridPos.y < _size.y && gridPos.x >= 0 && gridPos.y >= 0)
            return true;
        else return false;
    }

    public Vector2 GetWorldPos(IntVector2 gridPos)
    {
        Vector2 lMapSize = new Vector2(_size.x, _size.y);
        return new Vector2(gridPos.x, gridPos.y) - (lMapSize / 2f) + (Vector2.one / 2f);
    }

    public Vector2 GetWorldPos(IntVector2 gridPos, IntVector2 refSize)
    {
        Vector2 lMapSize = new Vector2(refSize.x, refSize.y);
        return new Vector2(gridPos.x, gridPos.y) - (lMapSize / 2f) + (Vector2.one / 2f);
    }

    public IntVector2 GetGridPos(Vector2 worldPos)
    {
        Vector2 lMapSize = new Vector2(_size.x, _size.y);
        IntVector2 floorPos = new IntVector2(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        return TransformPos(lMapSize, floorPos);
    }

    public IntVector2 GetGridPos(Vector2 worldPos, IntVector2 refSize)
    {
        Vector2 lMapSize = new Vector2(refSize.x, refSize.y);
        IntVector2 floorPos = new IntVector2(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        return TransformPos(lMapSize, floorPos);
    }

    public void Fill(IntVector2 pPos, T pObject)
    {
        if (pPos.x < _size.x && pPos.y < _size.y && pPos.x >= 0 && pPos.y >= 0)
            _grid[pPos.x, pPos.y] = pObject;
    }

    public void DestroyElement<O>(IntVector2 pPos) where O : Component
    {
        if (pPos.x < _size.x && pPos.y < _size.y && pPos.x >= 0 && pPos.y >= 0)
        {
            O monoBehaviour = _grid[pPos.x, pPos.y] as O;
            if (monoBehaviour != null)
            {
                GameObject.Destroy(monoBehaviour.gameObject);
                Fill(pPos, null);
            }
        }  
    }

    public GridStructure<T> Clone()
    {
        GridStructure<T> cloneGrid = new GridStructure<T>(_size.x, _size.y);
        IntVector2 fPos = new IntVector2();
        for (int i = 0; i < _size.x; i++)
        {
            for (int j = 0; j < _size.y; j++)
            {
                fPos.x = i;
                fPos.y = j;
                cloneGrid.Fill(fPos, GetObject(fPos));
            }
        }
        return cloneGrid;
    }

    public void Clear()
    {
        for (int i = 0; i < _size.x; i++)
        {
            for (int j = 0; j < _size.y; j++)
            {
                _grid[i, j] = null;
            }
        }
    }

    private static IntVector2 TransformPos(Vector2 mapSize, IntVector2 floorPos)
    {
        float x = floorPos.x + (mapSize.x / 2f);
        float y = floorPos.y + (mapSize.y / 2f);
        IntVector2 gridPos = new IntVector2();

        if (x < 0) gridPos.x = (int)(x - 1f);
        else gridPos.x = (int)x;
        if (y < 0) gridPos.y = (int)(y - 1f);
        else gridPos.y = (int)y;

        return gridPos;
    }
}