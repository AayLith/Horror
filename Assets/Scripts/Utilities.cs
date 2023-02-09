using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static bool checkLineOfSight ( Tile startTile , Tile endTile )
    {
        Vector3 start = startTile.transform.position - Vector3.forward / 2;
        Vector3 end = endTile.transform.position - Vector3.forward / 2;
        int layer_mask = LayerMask.GetMask ( "Line of Sight" );
        RaycastHit hit1;
        RaycastHit hit2;
        Debug.DrawLine ( start , end , Color.green , 10 );
        if ( Physics.Raycast ( start , end - start , out hit1 , Vector3.Distance ( start , end ) , layer_mask ) )
        {
            if ( startTile.content == null && endTile.content == null )
                return false;
            if ( startTile.content != null && endTile.content == null && hit1.transform != startTile.content.LOSComponent.transform )
                return false;
            if ( startTile.content == null && endTile.content != null && hit1.transform != endTile.content.LOSComponent.transform )
                return false;
            if ( startTile.content != null && endTile.content != null )
                if ( hit1.transform != startTile.content.LOSComponent.transform && hit1.transform != endTile.content.LOSComponent.transform )
                    return false;
        }
        if ( Physics.Raycast ( end , start - end , out hit2 , Vector3.Distance ( start , end ) , layer_mask ) )
        {
            if ( startTile.content == null && endTile.content == null )
                return false;
            if ( startTile.content != null && endTile.content == null && hit2.transform != startTile.content.LOSComponent.transform )
                return false;
            if ( startTile.content == null && endTile.content != null && hit2.transform != endTile.content.LOSComponent.transform )
                return false;
            if ( startTile.content != null && endTile.content != null )
                if ( hit1.transform != startTile.content.LOSComponent.transform && hit2.transform != endTile.content.LOSComponent.transform )
                    return false;
            /*
            if ( ( startTile.content != null && hit2.transform != startTile.content.LOSComponent.transform ) ||
                ( endTile.content != null && hit2.transform != endTile.content.LOSComponent.transform ) )
                return false;*/
        }
        return true;
    }
}