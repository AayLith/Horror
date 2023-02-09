using System;
using UnityEngine;

[System.Serializable]
public struct Point : IEquatable<Point>
{
    public static Point[] dirs = new Point[ 4 ]
        {
        new Point(0, 1),
        new Point(1, 0),
        new Point(0, -1),
        new Point(-1, 0),
        };

    public static Point[] diags = new Point[ 4 ]
        {
        new Point(1, 1),
        new Point(1, -1),
        new Point(-1, -1),
        new Point(-1, 1),
        };

    public static Point[] dirsdiags = new Point[ 8 ]
        {
        new Point(1, 0),
        new Point(1, -1),
        new Point(0, -1),
        new Point(-1, -1),
        new Point(-1, 0),
        new Point(-1, 1),
        new Point(0, 1),
        new Point(1, 1),
        };

    public int x;
    public int y;

    public float magnitude { get { return Mathf.Sqrt ( sqrtMagnitude ); } }
    public int sqrtMagnitude { get { return x * x + y * y; } }
    public Point fakeNormalized
    {
        get
        {
            return new Point ( x < 0 ? -1 : x > 0 ? 1 : 0 , y < 0 ? -1 : y > 0 ? 1 : 0 );
        }
    }
    public Point normalized
    {
        get
        {
            float sqrtmag = magnitude;
            if ( sqrtmag == 0 )
                throw new DivideByZeroException ();
            return new Point ( Mathf.RoundToInt ( ( float ) x / sqrtmag ) , Mathf.RoundToInt ( ( float ) y / sqrtmag ) );
        }
    }

    public Point ( int x , int y )
    {
        this.x = x;
        this.y = y;
    }

    public Point ( Vector2 pos )
    {
        x = Mathf.RoundToInt ( pos.x );
        y = Mathf.RoundToInt ( pos.y );
    }

    public Point ( Vector3 pos )
    {
        x = Mathf.RoundToInt ( pos.x );
        y = Mathf.RoundToInt ( DungeonParams.pointY ? pos.y : pos.z );
    }

    //
    // OPERATORS
    //
    public static Point operator + ( Point a , Point b )
    {
        return new Point ( a.x + b.x , a.y + b.y );
    }

    public static Point operator - ( Point p1 , Point p2 )
    {
        return new Point ( p1.x - p2.x , p1.y - p2.y );
    }

    public static Point operator * ( Point p , int mult )
    {
        return new Point ( p.x * mult , p.y * mult );
    }

    public static Point operator * ( int mult , Point p )
    {
        return new Point ( p.x * mult , p.y * mult );
    }

    public static bool operator == ( Point a , Point b )
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator != ( Point a , Point b )
    {
        return !( a == b );
    }

    /// <summary>
    /// Rotates the Point P a number of steps aroud the Point C. P and C needs to be adjacent.
    /// </summary>
    /// <param name="p">Point to rotate</param>
    /// <param name="c">Point center of rotation</param>
    /// <param name="steps">Nuber of steps. 8 steps is a chole turn</param>
    /// <returns></returns>
    public static Point rotateAround ( Point p , Point c , int steps )
    {
        float angleInRadians = 45 * steps * ( Mathf.PI / 180 );
        float cosTheta = Mathf.Cos ( angleInRadians );
        float sinTheta = Mathf.Sin ( angleInRadians );
        return new Point
        {
            x = Mathf.RoundToInt ( cosTheta * ( p.x - c.x ) - sinTheta * ( p.y - c.y ) + c.x ) ,
            y = Mathf.RoundToInt ( sinTheta * ( p.x - c.x ) + cosTheta * ( p.y - c.y ) + c.y )
        };
    }

    public static int distance ( Point p1 , Point p2 )
    {
        return Mathf.Abs ( p1.x - p2.x ) + Mathf.Abs ( p1.y - p2.y );
    }

    public static int distance ( Tile t1 , Tile t2 )
    {
        return distance ( t1.pos , t2.pos );
    }

    public static Point direction ( Point p1 , Point p2 )
    {
        return new Point ( p2.x - p1.x , p2.y - p1.y );
    }

    //
    // OVERRIDES
    //
    public override bool Equals ( object obj )
    {
        if ( obj is Point )
        {
            Point p = ( Point ) obj;
            return x == p.x && y == p.y;
        }
        return false;
    }

    public bool Equals ( Point p )
    {
        return x == p.x && y == p.y;
    }

    public override int GetHashCode ()
    {
        return x ^ y;
    }

    public override string ToString ()
    {
        return string.Format ( "({0},{1})" , x , y );
    }
}