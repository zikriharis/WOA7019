using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using UnityEngine.UI; 
using UnityEngine.XR.ARFoundation; 
using UnityEngine.XR.ARSubsystems; 
using UnityEngine.InputSystem; // <-- IMPORTANT: Add this namespace! 

[RequireComponent(typeof(ARRaycastManager))] 

public class PlacementWithManyController : MonoBehaviour 

{ 
    [SerializeField] 
    private GameObject placedPrefabs; 

    [SerializeField] 
    private int maxNumberOfBoards = 1; 
    
    private List<GameObject> addedInstances = new List<GameObject>(); 
    private Vector2 touchPosition = default; // Still useful to store current touch pos 
    private ARRaycastManager arRaycastManager; 
    static List<ARRaycastHit> hits = new List<ARRaycastHit>(); 

    float verticalPlaneThreshold = 0.5f; 

    void Awake() 
    { 
        arRaycastManager = GetComponent<ARRaycastManager>(); 
    } 

    void Update() 
    { 
        // Touch
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) 
        { 
            touchPosition = Touchscreen.current.primaryTouch.position.ReadValue(); 
            Debug.Log($"[New Input System] Touch Began at: {touchPosition}"); 

            TryPlaceObject(); 
        } 
        // Touch (Simulation)
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) 
        { 
            touchPosition = Mouse.current.position.ReadValue(); 
            Debug.Log($"[New Input System] Mouse Click Began at: {touchPosition}"); 

            TryPlaceObject(); 
        } 


        // Drag
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) 
        { 
            if (Touchscreen.current.primaryTouch.delta.ReadValue().magnitude > 0.1f)
            { 
                touchPosition = Touchscreen.current.primaryTouch.position.ReadValue(); 
                Debug.Log($"[New Input System] Touch Moved to: {touchPosition}"); 

                TryMoveLastObject(); 
            } 
        } 
        // Drag (Simulation)
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed) 
        { 
            if (Mouse.current.delta.ReadValue().magnitude > 0.1f) // Check if mouse moved 
            { 
                touchPosition = Mouse.current.position.ReadValue(); 
                Debug.Log($"[New Input System] Mouse Moved to: {touchPosition}"); 

                TryMoveLastObject(); 
            } 
        } 
    } 

    // Touch Function
    private void TryPlaceObject() 
    { 
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) 
        { 
            Debug.Log($"Raycast hit something! Number of hits: {hits.Count}"); 
            if (hits.Count > 0) 
            { 
                var hitPose = hits[0].pose; 
                
                Vector3 planeUp = hitPose.up; 
                float dotProduct = Vector3.Dot(planeUp, Vector3.up); 
                Debug.Log($"Plane detected. Dot product with world up: {dotProduct}"); 
                
                Quaternion finalRotation; 
                
                if (Mathf.Abs(dotProduct) < verticalPlaneThreshold) 
                { 
                    Debug.Log("Detected Vertical Plane."); 
                    Quaternion offset = Quaternion.Euler(90, 0, 90); 
                    finalRotation = hitPose.rotation * offset; 
 		        } 
                else 
                { 
                    Debug.Log("Detected Horizontal Plane."); 
                    Quaternion offset = Quaternion.Euler(90, 180, 90); 
                    finalRotation = hitPose.rotation * offset; 
 		        } 

                if (addedInstances.Count < maxNumberOfBoards) 
                { 
                    Debug.Log("Placing new prefab!"); 
                    GameObject addedPrefab = Instantiate(placedPrefabs, hitPose.position, finalRotation); 
                    addedInstances.Add(addedPrefab); 
                } 
                else 
                { 
                    Debug.Log("Max number of boards reached. Cannot place more."); 
                } 
            } 
        } 
        else 
        { 
            Debug.Log("Raycast hit nothing on PlaneWithinPolygon."); 
        } 
    } 

    // Drag Function
    private void TryMoveLastObject() 
    { 
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) 
        { 
            Pose hitPose = hits[0].pose; 

            if (addedInstances.Count > 0) 
            { 
                GameObject lastAdded = addedInstances[addedInstances.Count - 1]; 

                Vector3 planeUp = hitPose.up;
                float dotProduct = Vector3.Dot(planeUp, Vector3.up); 

                Quaternion finalRotationForMove; 

                if (Mathf.Abs(dotProduct) < verticalPlaneThreshold) 
                { 
                    Quaternion offset = Quaternion.Euler(90, 0, 90);  
                    finalRotationForMove = hitPose.rotation * offset; 
                } 
                else 
                { 
                    Quaternion offset = Quaternion.Euler(90, 180, 90);  
                    finalRotationForMove = hitPose.rotation * offset; 
                } 

                lastAdded.transform.position = hitPose.position; 
                lastAdded.transform.rotation = finalRotationForMove; 
                Debug.Log("Moved last object."); 
            } 
        } 
    } 
}