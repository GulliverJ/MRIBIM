using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


// COMPONENT is parent of information for an element of the BIM
// Should also store history, I suppose
public class BIMComponent : MonoBehaviour {

    public DateTime dateModified;
    public Tag[] tags;
    public string name;
    public string code;
    public List<BIMComponent> history;
    public Material defaultMat;
    public Material selectMat;
    private BIMManager bimMan;
    private GameObject infoWindow;
    public bool selected;

	// Use this for initialization
	void Start () {

        history = new List<BIMComponent>();
        dateModified = new DateTime(2016, 9, 4, 12, 15, 23);
        bimMan = GameObject.FindGameObjectWithTag("BIMManager").GetComponent<BIMManager>();
        code = "SD-004c";
        infoWindow = GameObject.FindGameObjectWithTag("InfoWindow");

        GetComponent<Renderer>().sharedMaterial = defaultMat;

        // Set visible/set materials appropriately
        if (tags.Contains<Tag>(Tag.Wall) || tags.Contains<Tag>(bimMan.curLayer))
        {
            GetComponent<Renderer>().material.SetColor("_LineColor", bimMan.BIMColors[bimMan.curLayer]);
            GetComponent<Renderer>().enabled = true;
        } else
        {
            GetComponent<Renderer>().enabled = false;
        }
        
        

        selected = false;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnSelect(Vector3 hitPos)
    {

        // TOOD Temporary fix; don't open window if layer currently hidden.
        if (!GetComponent<Renderer>().enabled)
        {
            return;
        } else if (selected)
        {
            // Temporary fix; let someone deselect an object if they click on it a second time.
            OnDeselect();
            return;
        } else if (bimMan.curSelected != null && bimMan.curSelected != gameObject)
        {
            bimMan.curSelected.GetComponent<BIMComponent>().OnDeselect();
        } 

        GetComponent<Renderer>().sharedMaterial = selectMat;

        foreach(Renderer r in infoWindow.GetComponentsInChildren<Renderer>()) {
            r.enabled = true;
        }

        Vector3 camDir = Vector3.Normalize(hitPos - Camera.main.transform.position);
        if(hitPos.y < Camera.main.transform.position.y)
        {
            Debug.Log("InfoWindow low");
            infoWindow.transform.position = hitPos + new Vector3(0.0f, 0.1f, 0) - (0.3f * camDir) - (0.3f * Camera.main.transform.right);
        }
        else {
            Debug.Log("InfoWindow high");
            infoWindow.transform.position = hitPos + new Vector3(0.0f, -0.25f, 0) - (0.3f * camDir) - (0.3f * Camera.main.transform.right);
        }
        infoWindow.transform.rotation = Quaternion.LookRotation(camDir);

        infoWindow.GetComponentsInChildren<TextMesh>()[0].text = name;
        infoWindow.GetComponentsInChildren<TextMesh>()[1].text = code;
        infoWindow.GetComponentsInChildren<TextMesh>()[2].text = "04/09/2016";

        selected = true;
        bimMan.curSelected = gameObject;
    }

    void OnDeselect()
    {
        Debug.Log("Deselecting!");
        // Do other things to deselect.
        GetComponent<Renderer>().sharedMaterial = defaultMat;
        GetComponent<Renderer>().material.SetColor("_LineColor", bimMan.BIMColors[bimMan.curLayer]);

        foreach (Renderer r in infoWindow.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }

        selected = false;
        if(bimMan.curSelected == gameObject)
        {
            bimMan.curSelected = null;
        }
    }

    public void OnGaze()
    {
        // On select, make infowindow open? For now, colour it in.
        if(GetComponent<Renderer>().enabled)
        {
            Debug.Log("OnGaze Triggered");
            GetComponent<Renderer>().material.SetColor("_GridColor", bimMan.BIMColors[bimMan.curLayer] * new Vector4(1f, 1f, 1f, 0.2f));
        }
        
    }

    public void OnGazeEnd()
    {
        // On select, make infowindow open? For now, colour it in.
        if(!selected && GetComponent<Renderer>().enabled)
        {
            Debug.Log("OnGazeEnd Triggered");
            GetComponent<Renderer>().sharedMaterial = defaultMat;
            GetComponent<Renderer>().material.SetColor("_LineColor", bimMan.BIMColors[bimMan.curLayer]);
        }
        

    }
}
