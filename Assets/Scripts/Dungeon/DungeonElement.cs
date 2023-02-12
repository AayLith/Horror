using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonElement : MonoBehaviour
{
    public bool bockLOS = false;
    public Collider2D collider2d;
    public Tile tile;
    public List<Board.moveTypes> revokedMoveTypes = new List<Board.moveTypes> ();

    private void Awake ()
    {
    }

    protected virtual void Start ()
    {
        tile = Board.instance.getTile ( transform.position );
        tile.setContent ( this );
        collider2d.enabled = bockLOS;
    }

    public virtual void takeDamage ( int amount )
    {
        NotificationCenter.instance.PostNotification ( this , Notification.notifications.takeDamage , new Hashtable () { { Notification.datas.character , this } } );
    }
}
