using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;


public enum Tag { Structural, Infrastructural, System, Fixture, Wall, Window, Column, Beam, Electric, Gas, Water, Safety };

/*
    BIMManager manages a BIM object
     - Loads, updates, translates BIM model
     - Should instantiate and position all gameObjects in a BIM model
 
*/

public class BIMManager : MonoBehaviour
{

    public Dictionary<Tag, Color> BIMColors;

    public GameObject bim;

    public BIM[] savedBIMs;

    public BIM curBIM;

    public float roomHeight;

    public GameObject infoWindow;

    public GameObject curSelected;

    public Tag curLayer;

    
    
    // On awake, set Tag enum colours
    void Awake()
    {

        BIMColors = new Dictionary<Tag, Color>();

        BIMColors[Tag.Structural] = new Color(1f, 1f, 1f, 1f);
        BIMColors[Tag.Safety] = new Color(0.9f, 0f, 0.1f, 1f);
        BIMColors[Tag.Fixture] = new Color(0.9f, 0.65f, 0.05f, 1f);
        BIMColors[Tag.Electric] = new Color(0.9f, 0.9f, 0.10f, 1f);
        BIMColors[Tag.Water] = new Color(0f, 0.3f, 1f, 1f);
        Debug.Log("Making BimCOLORS");
    }

    void Start()
    {

        // Used for debugging
        curLayer = Tag.Water;
        curSelected = null;

        // Initialise InfoWindow reference
        infoWindow = GameObject.FindGameObjectWithTag("InfoWindow");


        // Create BIM gameObject here
        bim.SetActive(false);
        curBIM.roomHeight = 3.28f;

        foreach (Renderer r in infoWindow.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }

        InitBIM();
    }

   
    private void InitBIM()
    {
        // Set BIM components visible or non-visible according to saved prefs
        foreach (BIMComponent child in bim.GetComponentsInChildren<BIMComponent>())
        {
            if (child.tags.Contains<Tag>(Tag.Wall) || child.tags.Contains<Tag>(curLayer) || true)
            {
                // Set up default wall outlining
                child.GetComponent<Renderer>().enabled = true;
            }
            else
            {
                child.GetComponent<Renderer>().enabled = false;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    /*
     * Code testing serialisation
     * Unused due to XML format problems on Windows devices
     * 
    public void SaveBIM()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Create(Application.persistentDataPath + "/bim.dat");
        bf.Serialize(fs, curBIM);
        fs.Close();
        Debug.Log("Saving BIM");
    }

    public bool LoadBIM()
    {
        if(File.Exists(Application.persistentDataPath + "/bim.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(Application.persistentDataPath + "/bim.dat", FileMode.Open);
            curBIM = (BIM) bf.Deserialize(fs);
            fs.Close();
            Debug.Log("Loading bim " + curBIM.buildingName);
            return true;
        } else
        {
            Debug.Log("No file to load; using default BIM");
            return false;
        }
    }
    */

    // Retrieve corners of floorplan
    // Hardcoded for now, to match the test case
    public List<Corner> GetCorners()
    {
        List<Corner> corners = new List<Corner>();
        //Corner corn = new Corner(0, 0, false);
        corners.Add(new Corner(0, 0, true));
        corners.Add(new Corner(-2.58f, 0, true));
        corners.Add(new Corner(-2.58f, -0.16f, true));
        corners.Add(new Corner(-5.34f, -0.16f, true));
        corners.Add(new Corner(-5.34f, -3.12f, true));
        corners.Add(new Corner(0, -3.12f, true));
        //curBIM.footprint = corners;
        return corners;

    }

    // Hides to activates active BIM components according to newly-set layer
    public void ChangeLayer(Tag layer)
    {
        curLayer = layer;
        Debug.Log("Changing layer to " + layer);
        foreach (BIMComponent child in bim.GetComponentsInChildren<BIMComponent>())
        {
            if (child.tags.Contains<Tag>(layer) || child.tags.Contains<Tag>(Tag.Wall))
            {
                child.GetComponent<Renderer>().material.SetColor("_LineColor", BIMColors[curLayer]);
                child.GetComponent<Renderer>().enabled = true;
            } else
            {
                child.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public Corner[] GetCornersArray()
    {
        return GetCorners().ToArray();
    }

    // Methods called when applying alignment results

    public void Translate(Vector3 trans)
    {
        bim.transform.position += trans;
    }

    public void Rotate(float angle)
    {
        bim.transform.Rotate(new Vector3(0, angle, 0));
    }

    public void RotateAround(Vector3 point, float angle)
    {
        bim.transform.RotateAround(point, Vector3.up, angle);
        curBIM.rotation = angle;
        curBIM.rotationX = point.x;
        curBIM.rotationY = point.y;
        curBIM.rotationZ = point.z;
    }

    public void setTranslation(Vector3 trans)
    {
        bim.transform.position = trans;
        curBIM.alignX = trans.x;
        curBIM.alignY = trans.y;
        curBIM.alignZ = trans.z;
        bim.SetActive(true);
    }
}
