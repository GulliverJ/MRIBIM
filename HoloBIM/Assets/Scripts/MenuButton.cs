using UnityEngine;
using System.Collections;

public class MenuButton : MonoBehaviour {

    public float vertDist;
    public float forwardDist;
    private Vector3 camForward;
    public GameObject menu;

	// Use this for initialization
	void Start () {

        foreach (Renderer r in menu.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }

        camForward = Vector3.Normalize(Camera.main.transform.forward - new Vector3(0f,0f,Camera.main.transform.forward.z));
        transform.position = Camera.main.transform.position - new Vector3(0f, vertDist, 0f) + camForward * forwardDist;
    }
	
	// Update is called once per frame
	void Update () {
        camForward = Vector3.Normalize(Camera.main.transform.forward - new Vector3(0f, Camera.main.transform.forward.y, 0f));
        transform.position = Camera.main.transform.position - new Vector3(0f, vertDist, 0f) + camForward * forwardDist;
    }

    void OnSelect(Vector3 p)
    {
        Debug.Log("Opening Menu");

        foreach (Renderer r in menu.GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
        foreach (Collider c in menu.GetComponentsInChildren<Collider>())
        {
            c.enabled = true;
        }

        menu.transform.position = Camera.main.transform.position + camForward * 2f;
        menu.transform.rotation = Quaternion.LookRotation(menu.transform.position - Camera.main.transform.position);
    }

    public void OpenMenu() {
        Debug.Log("Opening Menu");

        foreach (Renderer r in menu.GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }

        menu.transform.position = Camera.main.transform.position + camForward * 1.5f;
        menu.transform.rotation = Quaternion.LookRotation(menu.transform.position - Camera.main.transform.position);

    }

}
