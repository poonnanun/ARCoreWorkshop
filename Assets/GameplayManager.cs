using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class GameplayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject prefabs;
    [SerializeField]
    private GameObject indicator;

    private GameObject _currentIndicator;
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateApplicationLifecycle();
        ActiveGridAndSpawnObject(prefabs);
    }
    private void UpdateApplicationLifecycle()
    {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
    private bool ActiveGridAndSpawnObject(GameObject prefabs)
    {
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;
        Vector3 position = cam.transform.position;
        bool found = Frame.Raycast(position.x + (cam.pixelWidth / 2), position.y + (cam.pixelHeight / 2), raycastFilter, out TrackableHit hit);
        if (found)
        {
            if (_currentIndicator == null)
            {
                _currentIndicator = Instantiate(indicator, hit.Pose.position, hit.Pose.rotation);
            }
            else
            {
                _currentIndicator.transform.position = hit.Pose.position;
                _currentIndicator.transform.rotation = hit.Pose.rotation;
            }
            _currentIndicator.SetActive(true);
        }

        if (Input.touchCount < 1 || (Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return false;
        }

       
        if (found)
        {
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(cam.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                // Choose the prefab based on the Trackable that got hit.
                GameObject prefab;
                if (hit.Trackable is FeaturePoint)
                {
                    prefab = prefabs;
                }
                else if (hit.Trackable is DetectedPlane)
                {
                    DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                    if (detectedPlane.PlaneType == DetectedPlaneType.HorizontalUpwardFacing)
                    {
                        prefab = prefabs;
                    }
                    else
                    {
                        prefab = null;
                    }
                }
                else
                {
                    prefab = prefabs;
                }
                // Instantiate prefab at the hit pose.
                if (prefab != null)
                {
                    var tmpObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                    // Make game object a child of the anchor.
                    tmpObject.transform.parent = anchor.transform;
                    return true;
                }
            }
        }
        return false;
    }

}
