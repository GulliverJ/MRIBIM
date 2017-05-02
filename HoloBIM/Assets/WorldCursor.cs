using UnityEngine;

public class WorldCursor : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    public GameObject target;
    public GameObject menuTarget;
    private BIMManager bimMan;


    // Use this for initialization
    void Start()
    {
        // Grab the mesh renderer that's on the same object as this script.
        meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        target = null;
        menuTarget = null;
        bimMan = GameObject.FindGameObjectWithTag("BIMManager").GetComponent<BIMManager>();

    }

    // Update is called once per frame
    void Update()
    {
        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;

        if(Input.GetKeyDown(KeyCode.F))
        {

            if(bimMan.curLayer == Tag.Safety)
            {
                bimMan.ChangeLayer(Tag.Electric);
            } else
            {
                bimMan.ChangeLayer(Tag.Safety);
            }
        }
        
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram...
            // Display the cursor mesh.
            meshRenderer.enabled = true;

            // If we're hitting a BIMComponent..
            if (hitInfo.collider.gameObject.GetComponent<BIMComponent>() != null)
            {
                // If it's not our target...
                if(hitInfo.collider.gameObject != target)
                {
                    // If the target was previously set, ungaze...
                    if (target != null)
                    {
                        target.GetComponent<BIMComponent>().OnGazeEnd();
                    }

                    // Set BIMComponent as our new target
                    target = hitInfo.collider.gameObject;
                    target.GetComponent<BIMComponent>().OnGaze();
                }
            } else if(hitInfo.collider.gameObject.GetComponent<MenuButton>() != null)
            {
                // Looking at the menu
                //GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0.8f, 0.8f, 1f));
                //hitInfo.collider.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0.1f, 0.1f, 0.9f));

            } else if (hitInfo.collider.gameObject.GetComponent<MenuItem>() != null)
            {
                menuTarget = hitInfo.collider.gameObject;
            }
            else
            {
                if (target != null)
                {
                    target.GetComponent<BIMComponent>().OnGazeEnd();
                }
                target = null;
                menuTarget = null;
            }

            // Set cursor colour
            if(hitInfo.collider.gameObject == target) {
                GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0.8f, 0.8f, 1f));
            } else {
                GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f, 1f));
            }


            // Move the cursor to the point where the raycast hit.
            this.transform.position = hitInfo.point;

            // Rotate the cursor to hug the surface of the hologram.
            this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

        }
        else
        {
            // If the raycast did not hit a hologram, hide the cursor mesh.
            meshRenderer.enabled = false;

            if(target != null)
            {
                if(target.GetComponent<BIMComponent>() != null && !target.GetComponent<BIMComponent>().selected)
                {
                    target.GetComponent<BIMComponent>().OnGazeEnd();
                    target = null;
                }
                target = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            // Send an OnSelect message to the focused object and its ancestors.
            if (target != null)
            {
                if (target.GetComponent<BIMComponent>() != null && !target.GetComponent<BIMComponent>().selected && target.GetComponent<Renderer>().enabled)
                {
                    Debug.Log("Selecting");
                    target.SendMessageUpwards("OnSelect", hitInfo.point);
                }
                else
                {
                    Debug.Log("Deselecting");
                    target.SendMessageUpwards("OnDeselect");
                }
            }
            else if (menuTarget != null)
            {
                //menuTarget.GetComponent<MenuItem>().OnSelect();
            }
            else
            {
                Debug.Log("No target");
            }
        }

        if(Input.GetKeyDown(KeyCode.M))
        {
            hitInfo.collider.gameObject.GetComponent<MenuButton>().OpenMenu();
        }
    }
}