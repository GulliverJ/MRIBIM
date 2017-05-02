using UnityEngine;
using System.Collections;

public class MenuItem : MonoBehaviour {

    public Tag thisTag;
    public GameObject bimMan;
    public GameObject infoWindow;

    void OnSelect(Vector3 hitPos)
    {
        Debug.Log("MENU ITEM: Selecting " + thisTag);
        bimMan.GetComponent<BIMManager>().ChangeLayer(thisTag);
        foreach(Renderer r in transform.parent.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;

        }
        foreach(Collider c in transform.parent.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        foreach(Renderer r in infoWindow.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
