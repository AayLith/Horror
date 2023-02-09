using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class Board : MonoBehaviour
{
    // Static
    public static Board instance;
    public static Tile currentTile; // Tiles currently hoverred by the mouse
    public static Tile previousTile; // Tiles hoverred by the mouse during the previous frame
    public static Vector3 mouseWorldPos = Vector3.zero; // Mouse position on the board

    private Dictionary<Point , Tile> tiles = new Dictionary<Point , Tile> ();

    // Right, top, left and bottom tiles
    static Point[] square = new Point[ 4 ]
    {
            new Point(0, 1),
            new Point(1, 0),
            new Point(0, -1),
            new Point(-1, 0),
    };
    // Top-right, top-left, bottom-left and bottom-right tiles
    static Point[] star = new Point[ 4 ]
    {
            new Point(1, 1),
            new Point(1, -1),
            new Point(-1, -1),
            new Point(-1, 1),
    };

    [Header ( "GameObjects" )]
    [SerializeField] private GameObject tileHighlight;
    [SerializeField] private GameObject groundPlane; // Collider plane for mouse position
    [SerializeField] private Transform linksHolder;
    [SerializeField] private GameObject linkPrefab;

    [Header ( "Params" )]
    [SerializeField] private bool starryBoard = false; // If true, diagonal tiles are considered adjacent
    [SerializeField] private float diagonalWeight = 1.4142f; // Distance approximate for diagonal tiles
    [SerializeField] private float linkLengthWeight = 1; // Multiplies all links length by this value. If 0, all links have a length of 1
    [SerializeField] private Vector3 posScale = new Vector3 ( 1 , 1 , 0 );
    public bool debugMode = false;

    private void Awake ()
    {
        if ( instance != null )
            Destroy ( this );
        instance = this;
    }

    private void Update ()
    {
        updateWorldPos ();
        updateCurrentTile ();
    }

    /// <summary>
    /// Update the position in the world where the mouse is hovering.
    /// </summary>
    private void updateWorldPos ()
    {
        Ray ray = Camera.main.ScreenPointToRay ( Input.mousePosition );
        RaycastHit hit;
        if ( groundPlane.GetComponent<Collider> ().Raycast ( ray , out hit , 10000.0F ) )
            mouseWorldPos = Vector3.Scale ( hit.point , posScale );
    }

    /// <summary>
    /// Update the current Tile the mouse is hoverring. If there is a Tile under the mouse, the tileHighlight is enabled and moved at it's position. If there is none, it is disables.
    /// </summary>
    private void updateCurrentTile ()
    {
        previousTile = currentTile;
        currentTile = getTile ( mouseWorldPos );
        if ( currentTile != null )
        {
            tileHighlight.SetActive ( true );
            tileHighlight.transform.position = Vector3.Scale ( currentTile.transform.position , posScale );
        }
        else
            tileHighlight.SetActive ( false );
    }

    /// <summary>
    /// Returns the tile at the given Point if it exists, else returns null.
    /// </summary>
    /// <param name="point">The point to look for</param>
    /// <returns></returns>
    public Tile getTile ( Point point ) { return tiles.ContainsKey ( point ) ? tiles[ point ] : null; }
    public Tile getTile ( int x , int y ) { return getTile ( new Point ( x , y ) ); }
    public Tile getTile ( Vector3 pos ) { return getTile ( new Point ( pos ) ); }
    public Tile getTile ( Vector2 pos ) { return getTile ( new Point ( pos ) ); }

    /// <summary>
    /// Add a Tile to the Board. 
    /// </summary>
    /// <param name="t">The Tile to add</param>
    public void addTile ( Tile t )
    {
        tiles.Add ( t.pos , t );
        setLinks ( t );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start">Starting Tile</param>
    /// <param name="radius">Search radius</param>
    /// <param name="dontAddIfOccupied">If true, Tiles with contents are nor returned</param>
    /// <param name="stopSearchIfOccupied">If true, the search is alted at Tiles with contents</param>
    /// <param name="addFirstTile">If true, the Start Tile is added</param>
    /// <returns></returns>
    public List<Tile> getTilesInRange ( Tile start , int radius , bool dontAddIfOccupied , bool stopSearchIfOccupied , bool addFirstTile = true )
    {
        if ( start == null ) return null;
        return SearchArea ( start , delegate ( SearchTile arg )
        {
            return arg.totalLength <= radius;
        } , dontAddIfOccupied , stopSearchIfOccupied , addFirstTile );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start">Starting Tile</param>
    /// <param name="radius">Search radius</param>
    /// <param name="dontAddIfOccupied">If true, Tiles with contents are nor returned</param>
    /// <param name="stopSearchIfOccupied">If true, the search is alted at Tiles with contents</param>
    /// <param name="addFirstTile">If true, the Start Tile is added</param>
    /// <returns></returns>
    public List<Tile> getTilesForMove ( Tile start , int radius , bool dontAddIfOccupied , bool stopSearchIfOccupied , bool addFirstTile = true )
    {
        if ( start == null ) return null;
        return SearchAreaLinks ( start , delegate ( SearchTile arg )
        {
            return arg.totalLength <= radius;
        } , dontAddIfOccupied , stopSearchIfOccupied , addFirstTile );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start">Starting Tile</param>
    /// <param name="addTile">Search radius</param>
    /// <param name="dontAddIfOccupied">If true, Tiles with contents are nor returned</param>
    /// <param name="stopSearchIfOccupied">If true, the search is alted at Tiles with contents</param>
    /// <param name="addFirstTile">If true, the Start Tile is added</param>
    /// <returns></returns>
    public List<Tile> SearchArea ( Tile start , Func<SearchTile , bool> addTile , bool dontAddIfOccupied = false , bool stopSearchIfOccupied = false , bool addFirstTile = true )
    {
        List<SearchTile> tilesToReturn = new List<SearchTile> ();
        if ( addFirstTile )
            tilesToReturn.Add ( new SearchTile ( start , 0 ) );

        Queue<SearchTile> checkNext = new Queue<SearchTile> ();
        Queue<SearchTile> checkNow = new Queue<SearchTile> ();
        checkNow.Enqueue ( new SearchTile ( start , 0 ) );

        while ( checkNow.Count > 0 )
        {
            SearchTile t = checkNow.Dequeue ();

            searchDir ( square , t , addTile , 1 , ref tilesToReturn , dontAddIfOccupied , stopSearchIfOccupied , ref checkNext );
            if ( starryBoard )
                searchDir ( star , t , addTile , diagonalWeight , ref tilesToReturn , dontAddIfOccupied , stopSearchIfOccupied , ref checkNext );

            if ( checkNow.Count == 0 )
                SwapReference ( ref checkNow , ref checkNext );
        }

        List<Tile> tiles = new List<Tile> ();
        foreach ( SearchTile st in tilesToReturn )
            tiles.Add ( st.tile );
        return tiles;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start">Starting Tile</param>
    /// <param name="addTile">Search radius</param>
    /// <param name="dontAddIfOccupied">If true, Tiles with contents are nor returned</param>
    /// <param name="stopSearchIfOccupied">If true, the search is alted at Tiles with contents</param>
    /// <param name="addFirstTile">If true, the Start Tile is added</param>
    /// <returns></returns>
    public List<Tile> SearchAreaLinks ( Tile start , Func<SearchTile , bool> addTile , bool dontAddIfOccupied = false , bool stopSearchIfOccupied = false , bool addFirstTile = true )
    {
        List<SearchTile> tilesToReturn = new List<SearchTile> ();
        if ( addFirstTile )
            tilesToReturn.Add ( new SearchTile ( start , 0 ) );

        Queue<SearchTile> checkNext = new Queue<SearchTile> ();
        Queue<SearchTile> checkNow = new Queue<SearchTile> ();
        checkNow.Enqueue ( new SearchTile ( start , 0 ) );

        while ( checkNow.Count > 0 )
        {
            SearchTile t = checkNow.Dequeue ();

            searchLinks ( t , addTile , ref tilesToReturn , dontAddIfOccupied , stopSearchIfOccupied , ref checkNext );
            if ( starryBoard )
                searchLinks ( t , addTile , ref tilesToReturn , dontAddIfOccupied , stopSearchIfOccupied , ref checkNext );

            if ( checkNow.Count == 0 )
                SwapReference ( ref checkNow , ref checkNext );
        }
        List<Tile> tiles = new List<Tile> ();
        foreach ( SearchTile st in tilesToReturn )
            tiles.Add ( st.tile );
        return tiles;
    }

    /// <summary>
    /// Search the tiles in directions relative to Start. If their total move cost is iferior to radius, they are added to the list.
    /// </summary>
    /// <param name="dirs"></param>
    /// <param name="start"></param>
    /// <param name="weight"></param>
    /// <param name="tilesToReturn"></param>
    /// <param name="dontAddIfOccupied"></param>
    /// <param name="stopSearchIfOccupied"></param>
    /// <param name="checkNext"></param>
    void searchDir ( Point[] dirs , SearchTile start , Func<SearchTile , bool> addTile , float weight , ref List<SearchTile> tilesToReturn , bool dontAddIfOccupied , bool stopSearchIfOccupied , ref Queue<SearchTile> checkNext )
    {
        foreach ( Point dir in dirs )
        {
            SearchTile next = new SearchTile ( getTile ( dir + start.pos ) );

            if ( next.tile == null )
                continue;
            if ( tilesToReturn.Contains ( next ) ) // If the tile was already found
            {
                if ( start.totalLength + weight < next.totalLength ) // And we have a shorter path that the previous one, the tile is put back is the queue
                {
                    next.totalLength = start.totalLength + weight;
                    next.prev = start;
                    checkNext.Enqueue ( start );
                    checkNext.Enqueue ( next );
                }
                continue;
            }

            if ( next.content != null )
                if ( stopSearchIfOccupied )
                {
                    if ( !dontAddIfOccupied )
                        tilesToReturn.Add ( next );
                    continue;
                }

            next.totalLength = start.totalLength + weight;
            next.prev = start;

            if ( addTile ( next ) )
            {
                checkNext.Enqueue ( next );
                if ( !( dontAddIfOccupied && next.content != null ) )
                    tilesToReturn.Add ( next );
            }
        }
    }

    /// <summary>
    /// Search the tiles in the links of Start. If their total move cost is iferior to radius, they are added to the list.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="tilesToReturn"></param>
    /// <param name="dontAddIfOccupied"></param>
    /// <param name="stopSearchIfOccupied"></param>
    /// <param name="checkNext"></param>
    void searchLinks ( SearchTile start , Func<SearchTile , bool> addTile , ref List<SearchTile> tilesToReturn , bool dontAddIfOccupied , bool stopSearchIfOccupied , ref Queue<SearchTile> checkNext )
    {
        foreach ( Link l in start.links )
        {
            SearchTile next = new SearchTile ( l.getOtherTile ( start.tile ) );

            if ( next.tile == null )
                continue;
            if ( tilesToReturn.Contains ( next ) ) // If the tile was already found
            {
                if ( start.totalLength + l.length * start.moveCost < next.totalLength ) // And we have a shorter path that the previous one, the tile is put back is the queue
                {
                    next.totalLength = start.totalLength + Mathf.Max ( 1 , linkLengthWeight * l.length ) * start.moveCost;
                    next.prev = start;
                    checkNext.Enqueue ( start );
                    checkNext.Enqueue ( next );
                }
                continue;
            }

            if ( next.content != null )
                if ( stopSearchIfOccupied )
                {
                    if ( !dontAddIfOccupied )
                        tilesToReturn.Add ( next );
                    continue;
                }

            next.totalLength = start.totalLength + Mathf.Max ( 1 , linkLengthWeight * l.length ) * start.moveCost;
            next.prev = start;

            if ( addTile ( next ) )
            {
                checkNext.Enqueue ( next );
                if ( !( dontAddIfOccupied && next.content != null ) )
                    tilesToReturn.Add ( next );
            }
        }
    }

    /// <summary>
    /// Swaps out reference between A and B
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    void SwapReference ( ref Queue<SearchTile> a , ref Queue<SearchTile> b )
    {
        Queue<SearchTile> temp = a;
        a = b;
        b = temp;
    }

    /// <summary>
    /// Get Tiles drawing a square centered on Start Tile.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="ability"></param>
    /// <returns></returns>
    public List<Tile> getTilesInSquare ( Tile start , int aoeSize , bool hasLineOfSight )
    {
        if ( start == null )
            return new List<Tile> ();
        List<Tile> ret = new List<Tile> ();
        if ( aoeSize <= 1 )
            return new List<Tile> () { start };
        Tile tile;

        if ( aoeSize % 2 == 0 ) // If even, there is no central square and the result will be shifted. TODO building by tiles intersection.
            for ( int x = -( aoeSize - 2 ) / 2 ; x <= ( aoeSize ) / 2 ; x++ )
                for ( int y = -( aoeSize - 2 ) / 2 ; y <= ( aoeSize ) / 2 ; y++ )
                {
                    tile = getTile ( new Point ( x , y ) + start.pos );
                    if ( tile == null )
                        continue;
                    ret.Add ( tile );
                }
        else // If odd, the square central Tile is the same has the currently hoverred Tile
            for ( int x = -( aoeSize - 1 ) / 2 ; x <= ( aoeSize - 1 ) / 2 ; x++ )
                for ( int y = -( aoeSize - 1 ) / 2 ; y <= ( aoeSize - 1 ) / 2 ; y++ )
                {
                    tile = getTile ( new Point ( x , y ) + start.pos );
                    if ( tile == null )
                        continue;
                    ret.Add ( tile );
                }

        for ( int i = ret.Count - 1 ; i >= 0 ; i-- ) // Removes null tiles or with no line of sight
            if ( ret[ i ] == null || hasLineOfSight && false == Utilities.checkLineOfSight ( start , ret[ i ] ) )
                ret.RemoveAt ( i );
        return ret;
    }

    /// <summary>
    /// Get Tiles drawing a circle centered on Start Tile.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="ability"></param>
    /// <returns></returns>
    public List<Tile> getTilesInCircle ( Tile start , int aoeSize , bool hasLineOfSight ) // TODO Addapt to starryBoard. find all tiles no further away from start that ability.aoeSize
    {
        if ( start == null )
            return new List<Tile> ();
        if ( aoeSize <= 0 )
            return new List<Tile> () { start };
        List<Tile> ret = new List<Tile> ();

        ret = getTilesInRange ( start , aoeSize , false , false , true );

        for ( int i = ret.Count - 1 ; i >= 0 ; i-- ) // Removes null tiles or with no line of sight
            if ( ret[ i ] == null || hasLineOfSight && false == Utilities.checkLineOfSight ( start , ret[ i ] ) )
                ret.RemoveAt ( i );
        return ret;
    }

    /// <summary>
    /// Get Tiles drawing a cone, the base of the cone is the Start Tile.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="origin"></param>
    /// <param name="ability"></param>
    /// <returns></returns>
    public List<Tile> getTilesInCone ( Tile start , Tile origin , int aoeSize , bool hasLineOfSight )
    {
        if ( start == null )
            return new List<Tile> ();
        List<Tile> ret = new List<Tile> ();
        if ( aoeSize <= 1 )
            return new List<Tile> () { start };
        Tile tile;
        Point dir;

        dir = Point.direction ( origin.pos , start.pos ).normalized;
        ret.Add ( start );
        if ( dir.sqrtMagnitude == 1 ) // Standard cone drawing
        {
            for ( int x = dir.x != 0 ? dir.x : dir.y ; x < aoeSize ; x++ )
                for ( int y = -x ; y <= x ; y++ )
                {
                    if ( dir.y == 0 )
                        tile = getTile ( new Point ( x * dir.x , y ) + start.pos );
                    else
                        tile = getTile ( new Point ( y , x * dir.y ) + start.pos );
                    if ( tile == null )
                        continue;
                    ret.Add ( tile );
                }
        }
        else // If the start Tile is directly diagonal to the origin Tile (casting character), the cone is drawn at an angle
            for ( int x = aoeSize - 1 ; x >= 0 ; x-- )
                for ( int y = 0 ; y < aoeSize - x ; y++ )
                    ret.Add ( getTile ( new Point ( x * dir.x , y * dir.y ) + start.pos ) );

        for ( int i = ret.Count - 1 ; i >= 0 ; i-- ) // Removes null tiles or with no line of sight
            if ( ret[ i ] == null || hasLineOfSight && false == Utilities.checkLineOfSight ( start , ret[ i ] ) )
                ret.RemoveAt ( i );
        return ret;
    }

    /// <summary>
    /// Get Tiles drawing a line.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="origin"></param>
    /// <param name="ability"></param>
    /// <returns></returns>
    public List<Tile> getTilesInLine ( Tile start , Tile origin , int aoeSize , bool hasLineOfSight , bool aoeLineOfSight )
    {
        if ( start == null )
            return new List<Tile> ();
        List<Tile> ret = new List<Tile> ();
        if ( aoeSize <= 1 )
            return new List<Tile> () { start };
        Tile tile;
        Point dir;

        dir = Point.direction ( origin.pos , start.pos ).normalized;
        ret.Add ( start );

        for ( int i = 1 ; i < aoeSize ; i++ )
        {
            tile = getTile ( start.pos + dir * i );
            if ( tile == null )
                if ( aoeLineOfSight ) // If no tile and need line of sight, stop search
                    break;
                else
                    continue;
            ret.Add ( tile );
            if ( tile.content != null && aoeLineOfSight ) // If content and need line of sight, stop search
                break;
        }

        for ( int i = ret.Count - 1 ; i >= 0 ; i-- ) // Removes null tiles or with no line of sight
            if ( ret[ i ] == null || hasLineOfSight && false == Utilities.checkLineOfSight ( start , ret[ i ] ) )
                ret.RemoveAt ( i );
        return ret;
    }

    /// <summary>
    /// Perform an A* search on the Board by following tiles links.
    /// </summary>
    /// <param name="start">Starting Tile</param>
    /// <param name="end">Target ending Tile</param>
    /// <param name="maxRange">Maximum search radius</param>
    /// <returns></returns>
    public static List<Tile> ASTARSEARCH ( Tile start , Tile end , int maxRange )
    {
        List<Tile> path = new List<Tile> ();
        PriorityQueue<SearchTile , float> openList = new PriorityQueue<SearchTile , float> ();
        List<SearchTile> closedList = new List<SearchTile> ();
        List<Link> adjacencies;
        SearchTile current = new SearchTile ( start );

        // add start node to Open List
        openList.Enqueue ( current , current.distanceFactor );

        while ( openList.Count != 0 && !closedList.Exists ( x => x.pos == end.pos ) )
        {
            current = openList.Dequeue ();
            closedList.Add ( current );
            adjacencies = current.links;

            foreach ( Link l in adjacencies )
            {
                if ( !l.isEnabled ) // Don't move through inactive links
                    continue;

                SearchTile t = new SearchTile ( l.getOtherTile ( current.tile ) );
                if ( t.tile == null )
                    continue;
                if ( !closedList.Contains ( t ) && t.walkable )
                {
                    bool isFound = false;
                    foreach ( var openListTile in openList.UnorderedItems )
                        if ( openListTile.Element == t )
                            isFound = true;

                    if ( !isFound )
                    {
                        t.prev = current;
                        t.distanceToEnd = Math.Abs ( t.pos.x - end.pos.x ) + Math.Abs ( t.pos.y - end.pos.y ); // Approximating the distance to end
                        t.totalCost = t.moveCost * l.length + t.prev.totalCost;
                        t.distanceToStart = current.distanceToStart + 1;
                        openList.Enqueue ( t , t.distanceFactor );
                    }
                }
            }
        }

        // construct path, if end was not closed return null
        if ( !closedList.Exists ( x => x.pos == end.pos ) )
        {
            Debug.Log ( "No path to target" );
            return null;
        }

        // if all good, return path
        SearchTile temp = closedList[ closedList.IndexOf ( current ) ];
        if ( temp.tile == null ) return null;
        do
        {
            path.Add ( temp.tile );
            temp = temp.prev;
        } while ( temp.tile != start && temp.tile != null );

        path.Add ( start );
        path.Reverse ();
        for ( int i = path.Count - 1 ; i > maxRange ; i-- ) // Removes tiles that are too far from the start
            path.RemoveAt ( i );
        return path;
    }

    /// <summary>
    /// Returns the Tiles adjacents to the given Tile in parameter.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private List<Tile> getAdjacentTiles ( Tile t )
    {
        List<Tile> temp = new List<Tile> ();

        foreach ( Point link in square )
        {
            Tile otherTile = getTile ( link + t.pos );
            if ( otherTile == null )
                continue;
            if ( otherTile.content != null )
                continue;
            temp.Add ( otherTile );
        }

        if ( starryBoard )
            foreach ( Point link in star )
            {
                Tile otherTile = getTile ( link + t.pos );
                if ( otherTile == null )
                    continue;
                if ( otherTile.content != null )
                    continue;
                temp.Add ( otherTile );
            }

        return temp;
    }

    public void setLinks ( Tile tile )
    {
        foreach ( Tile t in getAdjacentTiles ( tile ) )
        {
            Link link = Instantiate ( linkPrefab , linksHolder ).GetComponent<Link> ();
            link.setTiles ( tile , t );
        }
    }
}
