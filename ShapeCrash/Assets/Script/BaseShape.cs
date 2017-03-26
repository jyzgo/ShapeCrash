using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShape : MonoBehaviour {

    public virtual bool Moveable()
    {
        return false;
    }
}
