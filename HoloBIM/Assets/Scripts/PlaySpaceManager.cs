using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using HoloToolkit.Unity;
using System.IO;
using System.Text;
using System.Collections;

// Struct used for storing corner coordinates and whether or not they are well defined
public struct Corner
{
    public float x, y;
    public bool wellDefined;

    public Corner(float x, float y, bool wellDefined)
    {
        this.x = x;
        this.y = y;
        this.wellDefined = wellDefined;
    }
}

public class PlaySpaceManager : Singleton<PlaySpaceManager>
{
    [Tooltip("When checked, the SurfaceObserver will stop running after a specified amount of time.")]
    public bool limitScanningByTime = true;

    [Tooltip("How much time (in seconds) that the SurfaceObserver will run after being started; used when 'Limit Scanning By Time' is checked.")]
    public float scanTime = 30.0f;

    [Tooltip("Material to use when rendering Spatial Mapping meshes while the observer is running.")]
    public Material defaultMaterial;

    [Tooltip("Optional Material to use when rendering Spatial Mapping meshes after the observer has been stopped.")]
    public Material secondaryMaterial;

    [Tooltip("Minimum number of floor planes required in order to exit scanning/processing mode.")]
    public uint minimumFloors = 1;

    [Tooltip("Minimum number of wall planes required in order to exit scanning/processing mode.")]
    public uint minimumWalls = 1;

    public BIMManager bimMan;

    //public GameObject spatialMapping;

    /// <summary>
    /// Indicates if processing of the surface meshes is complete.
    /// </summary>
    private bool meshesProcessed = false;

    /// <summary>
    /// GameObject initialization.
    /// </summary>
    private void Start()
    {
        // Update surfaceObserver and storedMeshes to use the same material during scanning.
        SpatialMappingManager.Instance.SetSurfaceMaterial(defaultMaterial);

        // Register for the MakePlanesComplete event.
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += SurfaceMeshesToPlanes_MakePlanesComplete;
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    /// 

    private bool savedPlanes = false;

    /*
     * NOTE TO EXAMINERS: This file is largely a Windows library file, part of the HoloToolkit
     * Modifications have been made within it to take advantage of the spatial mapping implementations
     * 
     */

    private void Update()
    {

            // Check to see if the spatial mapping data has been processed
            // and if we are limiting how much time the user can spend scanning.
            if (!meshesProcessed && limitScanningByTime)
            {
                // If we have not processed the spatial mapping data
                // and scanning time is limited...

                // Check to see if enough scanning time has passed
                // since starting the observer.
                if (limitScanningByTime && ((Time.time - SpatialMappingManager.Instance.StartTime) < scanTime))
                {
                    // If we have a limited scanning time, then we should wait until
                    // enough time has passed before processing the mesh.
                }
                else
                {
                    // The user should be done scanning their environment,
                    // so start processing the spatial mapping data...


                    // 3.a: Check if IsObserverRunning() is true on the
                    // SpatialMappingManager.Instance.
                    if (SpatialMappingManager.Instance.IsObserverRunning())
                    {
                        // 3.a: If running, Stop the observer by calling
                        // StopObserver() on the SpatialMappingManager.Instance.
                        SpatialMappingManager.Instance.StopObserver();
                    }

                    // 3.a: Call CreatePlanes() to generate planes.
                    CreatePlanes();


                    // 3.a: Set meshesProcessed to true.
                    meshesProcessed = true;

                    Debug.Log("Starting plane processing...");
                    StartCoroutine(ProcessRoomPlanes());

                }
            }
        
    }

    //// GULLIVER IMPLEMENTATION BEGINS ////////////////////////////////////////////////////////////////

    // Quick helper method for getting distance between points
    private float Dist(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
    }
    // Overloaded with vectors too
    private float Dist(Vector2 a, Vector2 b)
    {
        return Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
    }

    // Helper methods for checking if abs value within threshold
    private bool WithinThreshold(float x, float threshold)
    {
        if(x < 0)
        {
            x *= -1;
        }
        if (x < threshold)
        {
            return true;
        } else
        {
            return false;
        }
    }

    private bool WithinThreshold(float x, float y, float threshold)
    {
        if(x < y)
        {
            if((y - x) < threshold)
            {
                return true;
            } else
            {
                return false;
            }
        }

        if((x - y) < threshold)
        {
            return true;
        }
        return false;
    }

    // Max angle to which planes are snapped to 90 degrees
    private float angleThreshold = 5.0f;

    // Given two planes, returns true if orthogonal
    private bool AreOrthgonal(GameObject p1, GameObject p2)
    {
        float positiveDiff = (p1.transform.rotation.y - p2.transform.rotation.y);
        if (positiveDiff < 0)
        {
            positiveDiff *= -1;
        }

        if (WithinThreshold(positiveDiff - 90, angleThreshold))
        {
            return true;
        }
        return false;
    }

    // Given two planes, returns 1 if parallel in same plane, -1 if parallel opposite, and 0 if not parallel
    private int AreParallel(GameObject p1, GameObject p2)
    {
        float positiveDiff = (p1.transform.rotation.y - p2.transform.rotation.y);
        if (positiveDiff < 0)
        {
            positiveDiff *= -1;
        }

        if(WithinThreshold(positiveDiff - 180, angleThreshold))
        {
            return -1;
        } else if (WithinThreshold(positiveDiff, angleThreshold))
        {
            return 1;
        }
        return 0;
    }

    List<Wall> walls = new List<Wall>();
    float baseOrientation = -1;

    // POST PROCESSING ON PLANES to match general room layout
    private IEnumerator ProcessRoomPlanes()
    {

        // Deactivate the mesh
        GameObject map = GameObject.FindGameObjectWithTag("Map");
        map.SetActive(false);

        List<Vector2> corners = new List<Vector2>();

        SurfaceMeshesToPlanes mesher = gameObject.GetComponent<SurfaceMeshesToPlanes>();

        while (mesher.makingPlanes)
        {
            yield return null;
        }


        List<GameObject> planes = mesher.ActivePlanes;

        MeshFilter[] meshFilters = new MeshFilter[planes.Count];
        int meshIndex = 0;

        

        // First step should be to join different parts of the same plane into one
        // E.g. sides of windows should be joined  in testing environment
        int index = 0;
        foreach (GameObject plane in planes)
        {
            index++;

            // Begin by setting vertical height to half-way between ceiling and floor
            float middleY = (mesher.FloorYPosition + mesher.CeilingYPosition) / 2;
            float roomHeight = (mesher.CeilingYPosition - mesher.FloorYPosition) > 0 ? mesher.CeilingYPosition - mesher.FloorYPosition : mesher.FloorYPosition - mesher.CeilingYPosition;
            float minWallHeightRatio = 0.75f;

            // If it's rotated in x around 90 or 270 degrees, probably a ceiling or floor, so skip
            if (    (plane.transform.rotation.eulerAngles.x % 180 ) > 85f
                || (plane.transform.rotation.eulerAngles.x % 180) < -85f 
                || ( plane.transform.localScale.x < (minWallHeightRatio * roomHeight) 
                && ((plane.transform.localScale.x / 2) + plane.transform.position.y) < (mesher.CeilingYPosition - (0.25 * roomHeight)) )   )
            {
                
                plane.SetActive(false);
                continue;
            }

            float width = plane.transform.localScale.y * 0.5f;

            float angleRads = (plane.transform.rotation.eulerAngles.y / 180f) * Mathf.PI;

            float v1x = Mathf.Cos(angleRads);
            float v1z = Mathf.Sin(angleRads);

            // A and B refer to the two vertical edge coordinates of these planes; x and z components
            float aX = (plane.transform.position.x - (v1x * width));
            float aZ = (plane.transform.position.z + (v1z * width));
            float bX = (plane.transform.position.x + (v1x * width));
            float bZ = (plane.transform.position.z - (v1z * width));

            int thisOrientation = 0;
            float angle = plane.transform.rotation.eulerAngles.y;

            if(baseOrientation < 0)
            {
                baseOrientation = plane.transform.rotation.eulerAngles.y;
                baseOrientation = angle;
            } else
            {
                // Compare this wall's orientation to the origin wall's orientation
                float angleDif = (angle - baseOrientation) > 0 ? angle - baseOrientation : baseOrientation - angle;
                if(WithinThreshold(angleDif, 90, angleThreshold))
                {
                    thisOrientation = 1;
                } else if(WithinThreshold(angleDif, 180, angleThreshold))
                {
                    thisOrientation = 2;
                } else if(WithinThreshold(angleDif, 270, angleThreshold))
                {
                    thisOrientation = 3;
                } else
                {
                    thisOrientation = -1;
                }
            }

            // Create walls list; combine very close planes of the same orientation
            walls.Add(new Wall(plane, new Vector2(aX, aZ), new Vector2(bX, bZ), thisOrientation));

            meshFilters[meshIndex++] = plane.GetComponent<MeshFilter>();

            // Remove the planes from User's view
            plane.SetActive(false);

        }

        ////
        //// Now extract corners from planes
        ////

        // Max distance between corners
        float maxDist = 1f;
        // Distance increased on each iteration
        float incDist = 0.2f;
        float distThreshold = 0;

        Wall[] wallsArray = walls.ToArray();

        // Used to track unpaired edges on a plane
        int neighbourCount = 0;

        // Iterate, expanding neighbourhood distance threshold until we've reached the max
        while (distThreshold < maxDist)
        {
            distThreshold += incDist;
            
            // For each candidate wall...
            for (int i = 0; i < wallsArray.Length; i++)
            {
                Wall wall = wallsArray[i];

                // Iterate through every other candidate wall to find a pair
                for (int j = i + 1; j < wallsArray.Length; j++)
                {
                    Wall nextWall = wallsArray[j];

                    if(wall.aNeighbour == null && nextWall.bNeighbour == null)
                    {
                        float distRight = Dist(wall.a, nextWall.b);

                        // If within min distance threshold, we found a neighbour
                        if (distRight < distThreshold)
                        {
                            Debug.Log("Joining start of " + i + " with end of " + j);
                            neighbourCount++;
                            wall.aNeighbour = nextWall;
                            nextWall.bNeighbour = wall;
                        }
                        
                    }
                    if(wall.bNeighbour == null && nextWall.aNeighbour == null)
                    {
                        float distLeft = Dist(wall.b, nextWall.a);
                        // If within threshold, we found a neighbour
                        if (distLeft < distThreshold)
                        {
                            Debug.Log("Joining end of " + i + " with start of " + j);
                            neighbourCount++;
                            wall.bNeighbour = nextWall;
                            nextWall.aNeighbour = wall;
                        }
                    }                   
                }
            }
        }

        // Find the starting wall
        Wall curWall = wallsArray[0];
        if (neighbourCount < planes.Count)
        {
            while (curWall.aNeighbour != null)
            {
                curWall = curWall.aNeighbour;
            }
        }

        List<Vector2> trueCorners = new List<Vector2>();
        List<Corner> testCorners = new List<Corner>();

        testCorners.Add(new Corner(curWall.a.x, curWall.a.y, false));

        // For each corner, extrapolate the true corner as the interesection of the wall planes
        int count = 0;
        while (curWall.bNeighbour != null && count < wallsArray.Length) {
            count++;
            Wall nextWall = curWall.bNeighbour;
            if ((curWall.orientation - nextWall.orientation) % 2 != 0)
            {
                // Get corner point
                // Note, implementation reference for Stack Overflow: http://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines
                float a1 = curWall.b.y - curWall.a.y;
                float b1 = curWall.a.x - curWall.b.x;
                float c1 = a1 * curWall.a.x + b1 * curWall.a.y;

                float a2 = nextWall.b.y - nextWall.a.y;
                float b2 = nextWall.a.x - nextWall.b.x;
                float c2 = a2 * nextWall.a.x + b2 * nextWall.a.y;

                float det = a1 * b2 - a2 * b1;
                float cornerX = (b2 * c1 - b1 * c2) / det;
                float cornerY = (a1 * c2 - a2 * c1) / det;
                trueCorners.Add(new Vector2(cornerX, cornerY));
                testCorners.Add(new Corner(cornerX, cornerY, true));

            }
            curWall = curWall.bNeighbour;
        }

        if(curWall.bNeighbour == null)
        {
            testCorners.Add(new Corner(curWall.b.x, curWall.b.y, false));
        }

        // Build a linked list of corners 
        if(trueCorners.Count >= 2)
        {
            Vector2[] cornersArray = trueCorners.ToArray();
            Corner[] testCornersArray = testCorners.ToArray();

            Corner[] bimWalls = bimMan.GetCornersArray();

            float minDiff = 100f;
            int bestMatchBIMA = -1;
            int bestMatchBIMB = -1;
            int bestMatchScanA = -1;
            int bestMatchScanB = -1;


            for (int i = 0; i < bimWalls.Length; i++)
            {
                int nextIndex = (i + 1) % bimWalls.Length;
                float edgeDistBIM = Dist(bimWalls[i].x, bimWalls[i].y, bimWalls[nextIndex].x, bimWalls[nextIndex].y);

                //for (int j = 0; j < cornersArray.Length; j++)
                for(int j = 0; j < testCornersArray.Length; j++)
                {
                    //int nextCorner = (j + 1) % cornersArray.Length;
                    int nextCorner = (j + 1) % testCornersArray.Length;

                    // Skip corner if not well defined
                    if(!testCornersArray[j].wellDefined || !testCornersArray[nextCorner].wellDefined)
                    {
                        continue;
                    }

                    // NOTE Hardcoded for test case again
                    if (nextCorner == 0 && testCornersArray.Length == 4)
                    {
                        break;
                    }

                    //float edgeDistScan = Dist(cornersArray[j], cornersArray[nextCorner]);
                    float edgeDistScan = Dist(new Vector2(testCornersArray[j].x, testCornersArray[j].y), new Vector2(testCornersArray[nextCorner].x,testCornersArray[nextCorner].y));
                    float edgeDiff = Mathf.Abs(edgeDistScan - edgeDistBIM);

                    if (edgeDiff < minDiff)
                    {
                        minDiff = edgeDiff;
                        bestMatchBIMA = i;
                        bestMatchBIMB = nextIndex;
                        bestMatchScanA = j;
                        bestMatchScanB = nextCorner;
                    }


                }
            }

            float tX = bimWalls[bestMatchBIMA].x;
            float tZ = bimWalls[bestMatchBIMA].y;

            tX += testCornersArray[bestMatchScanA].x;
            tZ += testCornersArray[bestMatchScanA].y;


            ////
            //// Calculate rotation component using average of all edges
            ////   - Note that below implementation expects the test case of 3 edges only and is not robust in that regard

            float cumRotDif = 0;

            // TODO Case enforced; must be generalised
            // scanVec: vector for wall extracted from the scan
            // bimVec: Vector for the corresponding wall in the BIM
            // cumRotDif: Cumulative rotational difference, used for averaging rotational error over all three walls in test case

            Vector2 scanVec = new Vector2(testCornersArray[bestMatchScanA].x - testCornersArray[bestMatchScanA - 1].x, testCornersArray[bestMatchScanA].y - testCornersArray[bestMatchScanA - 1].y);
            Vector2 bimVec = new Vector2(bimWalls[bestMatchBIMA].x - bimWalls[(bestMatchBIMA - 1)].x, bimWalls[bestMatchBIMA].y - bimWalls[bestMatchBIMA - 1].y);
            cumRotDif += Mathf.Acos(Vector2.Dot(scanVec, bimVec) / (bimVec.magnitude * scanVec.magnitude));

            scanVec = new Vector2(testCornersArray[bestMatchScanB].x - testCornersArray[bestMatchScanA].x, testCornersArray[bestMatchScanB].y - testCornersArray[bestMatchScanA].y);
            bimVec = new Vector2(bimWalls[bestMatchBIMA].x - bimWalls[bestMatchBIMB].x, bimWalls[bestMatchBIMA].y - bimWalls[bestMatchBIMB].y);
            cumRotDif += Mathf.Acos(Vector2.Dot(scanVec, bimVec) / (bimVec.magnitude * scanVec.magnitude));

            scanVec = new Vector2(testCornersArray[bestMatchScanB + 1].x - testCornersArray[bestMatchScanB].x, testCornersArray[bestMatchScanB + 1].y - testCornersArray[bestMatchScanB].y);
            bimVec = new Vector2(bimWalls[(bestMatchBIMB + 1) % bimWalls.Length].x - bimWalls[bestMatchBIMB].x, bimWalls[(bestMatchBIMB + 1) % bimWalls.Length].y - bimWalls[bestMatchBIMB].y);
            cumRotDif += Mathf.Acos(Vector2.Dot(scanVec, bimVec) / (bimVec.magnitude * scanVec.magnitude));
            
            float rotDifDeg = (cumRotDif / 3f) / Mathf.PI * 180f;

            //////////////////// SET HEIGHT

            float tY = 0;

            float scanHeight = Mathf.Abs(mesher.CeilingYPosition - mesher.FloorYPosition);

            float floorOffset = (bimMan.curBIM.roomHeight - scanHeight) / 2f;

            if (mesher.FloorYPosition == 0)
            {
                Debug.Log("Couldn't find floor! Using ceiling only...");
                tY = mesher.CeilingYPosition - bimMan.curBIM.roomHeight;
            }
            else if (mesher.CeilingYPosition == 0)
            {
                Debug.Log("Couldn't find ceiling! Using floor only...");
                tY = mesher.FloorYPosition;
            }
            else
            {
                tY = mesher.FloorYPosition - floorOffset;
            }

            ///////////////////////////

            // Translate to position of well-defined corners
            // Note: Shortcut implementation for ease of use in prototyping
            Debug.Log("Translating by vector [" + tX + ", " + tY + ", " + tZ + "]");
            bimMan.setTranslation(new Vector3(tX, tY, tZ));

            // Rotate around this corner
            bimMan.RotateAround(new Vector3(testCornersArray[bestMatchScanA].x, 0, testCornersArray[bestMatchScanA].y), rotDifDeg);

            // Set bimAligned to true; if this value is saved, alignment shouldn't be performed again
            bimMan.curBIM.aligned = true;
            
        }
        else
        {
            Debug.Log("CORNER SCAN FAILED; EXITING");
        }
    }


    // Mod for positive values
    float modPositive(float a, int n)
    {
        return ((a % n) + n) % n;
    }

    //// GULLIVER IMPLEMENTATION ENDS //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Handler for the SurfaceMeshesToPlanes MakePlanesComplete event.
    /// </summary>
    /// <param name="source">Source of the event.</param>
    /// <param name="args">Args for the event.</param>
    private void SurfaceMeshesToPlanes_MakePlanesComplete(object source, System.EventArgs args)
    {

        Debug.Log("Running MakePlanesComplete...");

        /* TODO: 3.a DEVELOPER CODING EXERCISE 3.a */

        // Collection of floor and table planes that we can use to set horizontal items on.
        List<GameObject> horizontal = new List<GameObject>();

        // Collection of wall planes that we can use to set vertical items on.
        List<GameObject> vertical = new List<GameObject>();

        // 3.a: Get all floor and table planes by calling
        // SurfaceMeshesToPlanes.Instance.GetActivePlanes().
        // Assign the result to the 'horizontal' list.
        horizontal = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Table | PlaneTypes.Floor);

        // 3.a: Get all wall planes by calling
        // SurfaceMeshesToPlanes.Instance.GetActivePlanes().
        // Assign the result to the 'vertical' list.
        vertical = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);

        // Check to see if we have enough horizontal planes (minimumFloors)
        // and vertical planes (minimumWalls), to set holograms on in the world.
        if (horizontal.Count >= minimumFloors && vertical.Count >= minimumWalls)
        {
            // We have enough floors and walls to place our holograms on...

            // 3.a: Let's reduce our triangle count by removing triangles
            // from SpatialMapping meshes that intersect with our active planes.
            // Call RemoveVertices().
            // Pass in all activePlanes found by SurfaceMeshesToPlanes.Instance.
            RemoveVertices(SurfaceMeshesToPlanes.Instance.ActivePlanes);

            // 3.a: We can indicate to the user that scanning is over by
            // changing the material applied to the Spatial Mapping meshes.
            // Call SpatialMappingManager.Instance.SetSurfaceMaterial().
            // Pass in the secondaryMaterial.
            SpatialMappingManager.Instance.SetSurfaceMaterial(secondaryMaterial);

            // 3.a: We are all done processing the mesh, so we can now
            // initialize a collection of Placeable holograms in the world
            // and use horizontal/vertical planes to set their starting positions.
            // Call SpaceCollectionManager.Instance.GenerateItemsInWorld().
            // Pass in the lists of horizontal and vertical planes that we found earlier.
            //SpaceCollectionManager.Instance.GenerateItemsInWorld(horizontal, vertical);
        }
        else
        {
            // TODO RESCAN IF THIS HAPPENS



            // We do not have enough floors/walls to place our holograms on...

            // 3.a: Re-enter scanning mode so the user can find more surfaces by 
            // calling StartObserver() on the SpatialMappingManager.Instance.
            SpatialMappingManager.Instance.StartObserver();

            // 3.a: Re-process spatial data after scanning completes by
            // re-setting meshesProcessed to false.
            //meshesProcessed = false;
        }
    }

    /// <summary>
    /// Creates planes from the spatial mapping surfaces.
    /// </summary>
    private void CreatePlanes()
    {
        // Generate planes based on the spatial map.
        SurfaceMeshesToPlanes surfaceToPlanes = SurfaceMeshesToPlanes.Instance;
        if (surfaceToPlanes != null && surfaceToPlanes.enabled)
        {
            surfaceToPlanes.MakePlanes();
        }
    }

    /// <summary>
    /// Removes triangles from the spatial mapping surfaces.
    /// </summary>
    /// <param name="boundingObjects"></param>
    private void RemoveVertices(IEnumerable<GameObject> boundingObjects)
    {
        RemoveSurfaceVertices removeVerts = RemoveSurfaceVertices.Instance;
        if (removeVerts != null && removeVerts.enabled)
        {
            removeVerts.RemoveSurfaceVerticesWithinBounds(boundingObjects);
        }
    }

    /// <summary>
    /// Called when the GameObject is unloaded.
    /// </summary>
    private void OnDestroy()
    {
        if (SurfaceMeshesToPlanes.Instance != null)
        {
            SurfaceMeshesToPlanes.Instance.MakePlanesComplete -= SurfaceMeshesToPlanes_MakePlanesComplete;
        }
    }
}