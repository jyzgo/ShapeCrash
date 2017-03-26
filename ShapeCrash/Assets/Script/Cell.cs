using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {


    public BaseShape _shape;
    private void OnMouseDown()
    {

        LevelMgr.current.CellDown(this);
        
    }

    private void OnMouseDrag()
    {
        if (LevelMgr.current.Playing)
        {
            LevelMgr.current.CellDrag();
        }
    }

    private void OnMouseUp()
    {
        LevelMgr.current.CellRelease(this);
    }
    public int _row = 0;
    public int _col = 0;
    internal void Init(int row, int col)
    {
        _row = row;
        _col = col;
       
    }

    internal void SetShape(BaseShape shape)
    {
        _shape = shape;
        _shape.transform.SetParent(transform);
        _shape.transform.localPosition = Vector3.zero;
    }

    internal void SwapShape(Cell cell)
    {
        var t = cell._shape;
        cell.SetShape(_shape);
        SetShape(t);
       
    }

    internal bool IsShapeSwapable()
    {
        if (_shape != null)
        {
            return _shape.Moveable();
        }
        return false;
    }

    public string Info()
    {
        return "r" + _row + " c" + _col;
    }
}
