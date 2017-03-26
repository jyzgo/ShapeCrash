using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Shape : BaseShape {

    public override bool Moveable()
    {
        return true;
    }

    public SpriteRenderer shapeSp;
    public void Init(ShapeColor col)
    {

        shapeSp.sprite = ResMgr.current.GetShapeColor(col);
    }

}
