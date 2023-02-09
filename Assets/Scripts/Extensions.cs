using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector3 pointToVect ( this Point p )
    {
        return new Vector3 ( p.x , DungeonParams.pointY ? p.y : 0 , DungeonParams.pointY ? 0 : p.y );
    }
}
