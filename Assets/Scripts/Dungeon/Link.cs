using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Link : MonoBehaviour, IEquatable<Link>
{
    public Tile tileA;
    public Tile tileB;

    [Header ( "Params" )]
    [SerializeField] bool overrideLength = false;
    [SerializeField] float _length;
    [SerializeField] float lengthOverride;
    public float length { get { return overrideLength ? lengthOverride : _length; } set { _length = value; } }
    public bool isEnabled { get { return mainSwitch && tileSwitch; } }
    [SerializeField] bool mainSwitch = true;
    [SerializeField] bool tileSwitch { get { return tileA.isEnabled && tileB.isEnabled; } } // False if one tile is disabled

    [SerializeField] private LineRenderer lineRenderer;

    private void Start ()
    {
        lineRenderer.enabled = false;
        mainSwitch = true;

        if ( tileA && tileB )
        {
            tileA.addLink ( this );
            tileB.addLink ( this );
        }
        setColor ();
    }

    public Link ( Tile a , Tile b )
    {
        setTiles ( a , b );
    }

    /// <summary>
    /// Set the Tiles of this link. The link then calculate it's Length and set the position of it's LineRenderer's points.
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    public void setTiles ( Tile A , Tile B )
    {
        if ( A.links.Contains ( this ) || B.links.Contains ( this ) )
        {
            Destroy ( gameObject );
            return;
        }

        tileA = A;
        tileB = B;
        A.addLink ( this );
        B.addLink ( this );

        length = Vector3.Distance ( tileA.transform.position , tileB.transform.position );

        transform.position = ( tileA.transform.position + tileB.transform.position ) / 2;
        lineRenderer.SetPosition ( 0 , tileA.transform.position );
        lineRenderer.SetPosition ( 1 , tileB.transform.position ); 
        setColor ();
    }

    /// <summary>
    /// Enable or Disable the Link based on it's current state.
    /// </summary>
    public void switchLink ()
    {
        if ( isEnabled )
            disable ();
        else
            enable ();
    }

    /// <summary>
    /// Enable the Link
    /// </summary>
    public void enable ()
    {
        mainSwitch = true;
#if UNITY_EDITOR
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications ( this );
#endif
        setColor ();
    }

    /// <summary>
    /// Disable the Link
    /// </summary>
    public void disable ()
    {
        mainSwitch = false;
#if UNITY_EDITOR
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications ( this );
#endif
        setColor ();
    }

    /// <summary>
    /// Returns True if the param Tile is equal to TileA or TileB
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool containsTile ( Tile t )
    {
        return t == tileA || t == tileB;
    }

    /// <summary>
    /// Returns True if both Tiles or Enabled
    /// </summary>
    /// <returns></returns>
    public bool areTilesEnabled ()
    {
        return tileA.isEnabled && tileB.isEnabled;
    }

    /// <summary>
    /// Returns the Tile of this Link that's different from the pram Tile, or Null if neither is equal to the param Tile
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Tile getOtherTile ( Tile t )
    {
        if ( tileA != t && tileB != t )
            return null;

        if ( tileA != t )
            return tileA;
        else
            return tileB;
    }

    public void setColor ()
    {
        lineRenderer.SetColors ( isEnabled ? DungeonParams.linkOnColor : DungeonParams.linkOffColor , isEnabled ? DungeonParams.linkOnColor : DungeonParams.linkOffColor );
    }

    //
    // OVERRIDES
    //
    public override bool Equals ( object obj )
    {
        if ( obj is Link )
        {
            Link l = ( Link ) obj;
            return tileA == l.tileA && tileB == l.tileB || tileA == l.tileB && tileB == l.tileA;
        }
        return false;
    }

    public bool Equals ( Link l )
    {
        return tileA == l.tileA && tileB == l.tileB || tileA == l.tileB && tileB == l.tileA;
    }

    public override int GetHashCode ()
    {
        return tileA.pos.x ^ tileA.pos.y + tileB.pos.x ^ tileB.pos.y;
    }
}