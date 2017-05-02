using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


// Represents one building information model
// The content of this class is what gets saved/loaded by BIMManager

[Serializable]
public class BIM {

    //public List<Component> components;
    public float roomHeight;
    public string buildingName;
    //public List<Corner> footprint;

    public float alignX;
    public float alignY;
    public float alignZ;

    public float rotation;
    public float rotationX;
    public float rotationY;
    public float rotationZ;

    public bool aligned;
    

    public DateTime lastModified;

    // TODO Need to store all of the positions and such of the geometry in some data structure here
}
