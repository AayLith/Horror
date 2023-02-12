using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Board;

public class Tile : MonoBehaviour
{
    public Point pos { get; private set; }

    [Header ( "Params" )]
    public bool isEnabled;
    public int moveCost = 1;
    public bool walkable = true;
    public TerrainType terrain;

    [Header ( "Objects" )]
    public SpriteRenderer tileSprite;
    public SpriteRenderer debugSprite;
    public List<Link> links = new List<Link> ();
    public DungeonElement content { get; private set; }
    public List<DungeonElement> objects { get; private set; }
    public TMPro.TMP_Text debugText;

    public int attractiveness // Used by AI's to choose where to move
    {
        get
        { return _attractiveness; }
        set
        {
            _attractiveness = value; debugText.text = "" + value; Debug.Log ( pos + " " + value );
        }
    }
    int _attractiveness;

    private void Awake ()
    {
        pos = new Point ( transform.position );
        setColor ();
    }

    void Start ()
    {
        Board.instance.addTile ( this );
        if ( Board.instance.debugMode )
            debugText.text = "" + pos.x + ',' + pos.y;
    }

    public void setContent ( DungeonElement d )
    {
        content = d;
        walkable = content == null;
    }

    public void setTerrain ( TerrainType tt )
    {
        terrain = tt;
        tileSprite.color = terrain.color;
    }

    public bool canMoveThrough ( Board.moveTypes t )
    {
        List<Board.moveTypes> list = terrain.allowedMoveTypes;
        foreach ( moveTypes mt in content.revokedMoveTypes )
            try { list.Remove ( mt ); }
            catch { }

        return list.Contains ( t );
    }

    /// <summary>
    /// Enable or Disable the Tile based on it's current state
    /// </summary>
    public void switchTile ()
    {
        if ( isEnabled )
            disable ();
        else
            enable ();

#if UNITY_EDITOR
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications ( this );
#endif
    }

    /// <summary>
    /// Set the Pos of this Tile
    /// </summary>
    /// <param name="p"></param>
    public void setPos ( Point p )
    {
        pos = p;
        transform.position = DungeonParams.pointY ? new Vector3 ( p.x , p.y , 0 ) : new Vector3 ( p.x , 0 , p.y );
        setColor ();
    }

    public bool addLink ( Link l )
    {
        if ( !links.Contains ( l ) )
        {
            links.Add ( l );
            return true;
        }
        return false;
    }

    public bool removeLink ( Link l )
    {
        if ( links.Contains ( l ) )
        {
            links.Remove ( l );
            return true;
        }
        return false;
    }

    public void enable ()
    {
        isEnabled = true;
        setColor ();
    }

    public void disable ()
    {
        isEnabled = false;
        setColor ();
    }

    void setColor ()
    {
        tileSprite.color = isEnabled ? DungeonParams.tileOnColor : DungeonParams.tileOffColor;
        foreach ( Link l in links )
            l.setColor ();
    }

    public List<Tile> getAdjacentTiles ()
    {
        List<Tile> tiles = new List<Tile> ();
        foreach ( Link l in links )
            tiles.Add ( l.getOtherTile ( this ) );
        return tiles;
    }
}
