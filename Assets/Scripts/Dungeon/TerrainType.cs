using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainType
{
    public enum terrainTypes { Ground, Void, Water, ShallowWater }
    terrainTypes type;
    public Color color
    {
        get
        {
            switch ( type )
            {
                case terrainTypes.Void:
                    return Color.black;
                case terrainTypes.Ground:
                    return Color.yellow;
                case terrainTypes.Water:
                    return Color.blue;
                case terrainTypes.ShallowWater:
                    return Color.cyan;
            }
            return Color.white;
        }
    }

    public TerrainType ( terrainTypes tt )
    {
        type = tt;
    }

    public List<Board.moveTypes> allowedMoveTypes
    {
        get
        {
            List<Board.moveTypes> list = new List<Board.moveTypes> ();
            switch ( type )
            {
                case terrainTypes.Void:
                    list.Add ( Board.moveTypes.Fly );
                    break;
                case terrainTypes.Ground:
                    list.Add ( Board.moveTypes.Fly );
                    list.Add ( Board.moveTypes.Walk );
                    break;
                case terrainTypes.Water:
                    list.Add ( Board.moveTypes.Fly );
                    list.Add ( Board.moveTypes.Swim );
                    break;
                case terrainTypes.ShallowWater:
                    list.Add ( Board.moveTypes.Fly );
                    list.Add ( Board.moveTypes.Swim );
                    list.Add ( Board.moveTypes.Walk );
                    break;
            }
            list.Add ( Board.moveTypes.Teleport );
            return list;
        }
    }
}
