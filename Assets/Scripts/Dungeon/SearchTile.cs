using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchTile
{
    public Tile tile;
    public SearchTile prev;
    public float totalLength = 0;
    public float distanceToStart = 0;
    public float distanceToEnd = 0;
    public float totalCost = 0;

    public List<Link> links { get { return tile.links; } }
    public bool walkable { get { return tile.walkable; } }
    public Point pos { get { return tile.pos; } }
    public int moveCost { get { return tile.moveCost; } }
    public DungeonElement content { get { return tile.content; } }

    public SearchTile ( Tile t )
    {
        tile = t;
    }

    public SearchTile ( Tile t , float dist )
    {
        tile = t;
        totalLength = dist;
    }

    public SearchTile ( Tile t , float distToEnd , float cost , float distToStart , SearchTile previous )
    {
        tile = t;
        distanceToEnd = distToEnd;
        totalCost = cost;
        distanceToStart = distToStart;
        prev = previous;
    }

    public float distanceFactor
    {
        get
        {
            if ( distanceToEnd != -1 && totalCost != -1 )
                return distanceToEnd + totalCost;
            else
                return -1;
        }
    }

    public override bool Equals ( object obj )
    {
        if ( obj is SearchTile )
        {
            SearchTile st = ( SearchTile ) obj;
            return st.tile == tile;
        }
        return false;
    }

    public static bool operator == ( SearchTile a , SearchTile b )
    {
        return a.tile == b.tile;
    }

    public static bool operator != ( SearchTile a , SearchTile b )
    {
        return !( a.tile == b.tile );
    }
}
