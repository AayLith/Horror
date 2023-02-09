using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonElement : MonoBehaviour
{
    public bool bockLOS = false;
    public GameObject LOSComponent;
    public Tile tile;

    private void Awake ()
    {
    }

    protected virtual void Start ()
    {
        tile = Board.instance.getTile ( transform.position );
        tile.setContent ( this );
        LOSComponent.SetActive ( bockLOS );
    }

    public virtual void takeDamage ( int amount )
    {
        NotificationCenter.instance.PostNotification ( this , Notification.notifications.takeDamage , new Hashtable () { { Notification.datas.character , this } } );
    }
}
