using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class FaceCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update ()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
