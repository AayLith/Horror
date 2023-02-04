using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The Notification class is the object that is send to receiving objects of a notification type.
// This class contains the sending GameObject, the name of the notification, and optionally a hashtable containing data.
[System.Serializable]
public class Notification
{
    //public Notification (GameObject aSender, string aName, Hashtable aData)
    //{
    //	throw new System.NotImplementedException ();
    //}
    public enum notifications
    {
        startBattle, endBattle, startTurn, endTurn, startExecution, endExecution, death, spawn, takeDamage, useMana, useStamina
    }
    public enum datas
    {
        amount, // when you need to ass or remove
        value, // set to this value
        ressource, // inform about ressource type
        gameObject,
        character
    }

    public Object sender;
    public notifications name;
    public Hashtable data;
    public Notification ( Object aSender , notifications aName ) { sender = aSender; name = aName; data = null; }
    public Notification ( Object aSender , notifications aName , Hashtable aData ) { sender = aSender; name = aName; data = aData; }
}
