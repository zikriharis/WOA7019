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
    private Vector2 touchPosition = default; 
    private ARRaycastManager arRaycastManager;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    float verticalPlaneThreshold = 0.5f;
    float selectionProximityThreshold = 0.1f;

    private GameObject selectedInstance = null;

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

            GameObject pressedObject = GetSelectedInstance(touchPosition);

            if (pressedObject != null)
            {
                selectedInstance = pressedObject;
                Debug.Log($"selectedInstance is set to: {selectedInstance.name}.");
            }
            else
            {
                selectedInstance = null; 
                PlaceObject();
            }
        }
        // Touch (For XR Simulation, Can be Deleted on Production)
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchPosition = Mouse.current.position.ReadValue();
            Debug.Log($"[New Input System] Mouse Click Began at: {touchPosition}");

            GameObject pressedObject = GetSelectedInstance(touchPosition);
            if (pressedObject != null)
            {
                selectedInstance = pressedObject;
                Debug.Log($"selectedInstance is set to: {selectedInstance.name}.");
            }
            else
            {
                selectedInstance = null;
                PlaceObject();
            }
        }

        // Drag 
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (selectedInstance != null && Touchscreen.current.primaryTouch.delta.ReadValue().magnitude > 0.1f)
            {
                touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                Debug.Log($"[New Input System] Touch Moved to: {touchPosition}");

                MoveSelectedObject(); 
            }
        }
        // Drag (Simulation)
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (selectedInstance != null && Mouse.current.delta.ReadValue().magnitude > 0.1f)
            {
                touchPosition = Mouse.current.position.ReadValue();
                Debug.Log($"[New Input System] Mouse Moved to: {touchPosition}");

                MoveSelectedObject();
            }
        }
    }

    private void PlaceObject()
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
                    
                    selectedInstance = addedPrefab; 
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

    private void MoveSelectedObject()
    {
        // Only proceed if an object is actually selected
        if (selectedInstance == null)
        {
            Debug.LogWarning("No object is currently selected to move.");
            return;
        }

        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

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

            selectedInstance.transform.position = hitPose.position;
            selectedInstance.transform.rotation = finalRotationForMove;
            Debug.Log($"Moved selected object: {selectedInstance.name}.");
        }
    }

    private GameObject GetSelectedInstance(Vector2 screenPosition)
    {
        if (arRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Vector3 hitPoint = hits[0].pose.position;

            GameObject closestInstance = null;
            float minDistance = float.MaxValue;

            foreach (GameObject instance in addedInstances)
            {
                float distance = Vector3.Distance(hitPoint, instance.transform.position);

                if (distance < selectionProximityThreshold && distance < minDistance)
                {
                    minDistance = distance;
                    closestInstance = instance;
                }
            }
            return closestInstance;
        }
        return null;
    }
}