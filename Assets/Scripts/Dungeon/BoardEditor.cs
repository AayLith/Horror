using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class BoardEditor : MonoBehaviour
{
    public static Vector3 mouseWorldPos = Vector3.zero; // Mouse position on the board
    public static Point pointWorldPos { get { return new Point ( mouseWorldPos ); } }
    public static Tile currentTile; // Tiles currently hoverred by the mouse
    public Room currentRoom;
    List<Tile> selectedTiles = new List<Tile> ();
    Dictionary<Point , Tile> tiles { get { return currentRoom.roomTiles; } }

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
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Link linkPrefab;
    [SerializeField] private Room roomPrefab;
    [SerializeField] private GameObject firstSelector;
    [SerializeField] private GameObject secondSelector;
    [SerializeField] private GameObject tileHighlight;
    [SerializeField] private GameObject groundPlane; // Collider plane for mouse position

    [Header ( "Params" )]
    [SerializeField] private bool starryBoard = false; // If true, diagonal tiles are considered adjacent
    [SerializeField] private Vector3 posScale { get { return DungeonParams.pointY ? new Vector3 ( 1 , 1 , 0 ) : new Vector3 ( 1 , 0 , 1 ); } }

    private Tile firstSelectedTile;
    private Tile secondSelectedTile;

    public enum states { addTile, remTile, selectTile, link, switchTile }
    [HideInInspector] public states currentState = states.addTile;
    [Space ( 50 )]
    public bool editorEnabled = false;

    private void Reset ()
    {
        runInEditMode = true;
    }

    void OnGUI ()
    {
        if ( EditorApplication.isPlaying )
            return;
        if ( editorEnabled == false )
            return;
        if ( !Application.isFocused )
            return;
        /*
        updateWorldPos ();
        updateCurrentTile ();
        
        if ( Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint )
            UnityEditor.EditorUtility.SetDirty ( this );
        else if ( Event.current.type == EventType.MouseDown )
            addTile ( pointWorldPos );*/
    }

    public void OnSceneGUI ( SceneView sceneView )
    {
        if ( EditorApplication.isPlaying )
            return;
        if ( editorEnabled == false )
            return;
        if ( EditorWindow.focusedWindow != SceneView.currentDrawingSceneView )
            return;

        updateWorldPos ();
        updateCurrentTile ();

        if ( Event.current.type == EventType.MouseDown && Event.current.button == 0 )
            switch ( currentState )
            {
                case states.addTile:
                    addTile ( pointWorldPos );
                    break;
                case states.remTile:
                    remTile ( pointWorldPos );
                    break;
                case states.selectTile:
                    selecTile ( pointWorldPos );
                    break;
                case states.switchTile:
                    switchTile ( pointWorldPos );
                    break;
            }
    }

    // Ajoute une tuile à la position P si elle n'existe pas déjà et forme des liens vers les cases adjacentes
    void addTile ( Point p )
    {
        if ( tiles.ContainsKey ( p ) )
            return;

        GameObject tile = ( PrefabUtility.InstantiatePrefab ( tilePrefab ) as Tile ).gameObject;
        tile.transform.parent = currentRoom.tiles;
        tile.GetComponent<Tile> ().setPos ( pointWorldPos );
        tiles.Add ( tile.GetComponent<Tile> ().pos , tile.GetComponent<Tile> () );
        tile.name = "Tile " + pointWorldPos.x + " , " + pointWorldPos.y;
        setLinks ( tile.GetComponent<Tile> () );
    }

    // Supprime la tuile à la position P si elle existe et supprime les liens qu'elle possède.
    void remTile ( Point p )
    {
        if ( !tiles.ContainsKey ( p ) )
            return;

        Tile t = tiles[ p ];
        for ( int i = t.links.Count - 1 ; i >= 0 ; i-- )
        {
            t.links[ i ].getOtherTile ( t ).removeLink ( t.links[ i ] );
            DestroyImmediate ( t.links[ i ].gameObject );
        }
        tiles.Remove ( p );
        DestroyImmediate ( t.gameObject );
    }

    // Crée un lien entre deux cases
    public void createLink ()
    {
        if ( firstSelectedTile == null || secondSelectedTile == null )
            return;

        addLink ( firstSelectedTile , secondSelectedTile );
    }

    public void setLinks ( Tile tile )
    {
        foreach ( Tile t in getAdjacentTiles ( tile ) )
            addLink ( tile , t );
    }

    void addLink ( Tile t1 , Tile t2 )
    {
        Link link = Instantiate ( linkPrefab , currentRoom.links ).GetComponent<Link> ();
        link.setTiles ( t1 , t2 );
    }

    // Désactive la case aux coordonnées P si elle est activé, sinon l'active
    void switchTile ( Point p )
    {
        if ( !tiles.ContainsKey ( p ) )
            return;

        tiles[ p ].switchTile ();
    }

    void selecTile ( Point p )
    {
        if ( !tiles.ContainsKey ( p ) )
            return;

        if ( firstSelectedTile != null && firstSelectedTile.pos == p ) // Si on clique sur la première sélection, elle est remplacée par la seoncde qui est désafectée
        {
            firstSelectedTile = secondSelectedTile;
            secondSelectedTile = null;
            selectorTile ( tiles[ p ] , firstSelector );
        }
        else if ( secondSelectedTile != null && secondSelectedTile.pos == p ) // Si  on vlique sur la seconde sélection, elle est désafectée
        {
            secondSelectedTile = null;
        }
        else if ( firstSelectedTile == null ) // Première sélection
        {
            firstSelectedTile = tiles[ p ];
            selectorTile ( tiles[ p ] , firstSelector );
        }
        else if ( secondSelectedTile == null ) // Seconde sélection
        {
            secondSelectedTile = tiles[ p ];
            selectorTile ( tiles[ p ] , secondSelector );
        }
        else // Remplacer la première sélection par la seconde, la nouvelle est affectée à la seconde
        {
            firstSelectedTile = secondSelectedTile;
            secondSelectedTile = tiles[ p ];
            selectorTile ( firstSelectedTile , firstSelector );
            selectorTile ( secondSelectedTile , secondSelector );
        }

        if ( firstSelectedTile == null )
            deselect ( firstSelector );
        if ( secondSelectedTile == null )
            deselect ( secondSelector );
    }

    void deselect ( GameObject selector )
    {
        selector.gameObject.SetActive ( false );
    }

    void selectorTile ( Tile t , GameObject selector )
    {
        selector.gameObject.SetActive ( true );
        selector.transform.position = t.transform.position;
    }

    public void reset ()
    {
        DestroyImmediate ( currentRoom.gameObject );
        tiles.Clear ();
        currentRoom = PrefabUtility.InstantiatePrefab ( roomPrefab ) as Room;
    }

    private void OnEnable ()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable ()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
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

    /// <summary>
    /// Update the position in the world where the mouse is hovering.
    /// </summary>
    private void updateWorldPos ()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay ( Event.current.mousePosition ); ;
        RaycastHit hit;
        if ( groundPlane.GetComponent<Collider> ().Raycast ( ray , out hit , 10000.0F ) )
            mouseWorldPos = Vector3.Scale ( hit.point , posScale );
        //else
        //  mouseWorldPos = SceneView Camera.main.ScreenToWorldPoint ( Event.current.mousePosition );
    }

    /// <summary>
    /// Update the current Tile the mouse is hoverring. If there is a Tile under the mouse, the tileHighlight is enabled and moved at it's position. If there is none, it is disables.
    /// </summary>
    private void updateCurrentTile ()
    {
        currentTile = getTile ( mouseWorldPos );
        if ( currentTile != null )
            tileHighlight.transform.position = Vector3.Scale ( currentTile.transform.position , posScale );
        else
            tileHighlight.transform.position = Vector3.Scale ( new Point ( mouseWorldPos ).pointToVect () , posScale );
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
}
