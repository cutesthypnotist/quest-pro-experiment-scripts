using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

public class SharedEyeTrackStandalone : MonoBehaviour
{
    public Transform eyeTrackedObject;
    public Vector3 outOfSight;
    [Range(0, 1)]
    public float lerpFactor;
    [SerializeField]
    private bool eyeTrackingDeviceFound;
    const string k_EyeGazeLayoutName = "EyeGaze";
    private List<InputDevice> inputDeviceList = new();
    private UnityEngine.XR.InputDevice eyeTracking;
    private static OVRPlugin.EyeGazesState _currentEyeGazesState;
    static Vector3 lastDirection = Vector3.forward;
    static Vector3 lastPosition = Vector3.zero;
    public static float ConfidenceThreshold = 0.0f;

    static Vector3 GetLookDirection()
    {

        if (!OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _currentEyeGazesState))
            return lastDirection;

        //float lblinkweight;
        //float rblinkweight;
        //GameplayReferences.OVRFaceExpressions.TryGetFaceExpressionWeight(OVRFaceExpressions.FaceExpression.EyesClosedL, out lblinkweight);
        //GameplayReferences.OVRFaceExpressions.TryGetFaceExpressionWeight(OVRFaceExpressions.FaceExpression.EyesClosedR, out rblinkweight);

        var eyeGazeRight = _currentEyeGazesState.EyeGazes[(int)OVRPlugin.Eye.Right];
        var eyeGazeLeft = _currentEyeGazesState.EyeGazes[(int)OVRPlugin.Eye.Left];

        if (eyeGazeRight.IsValid && eyeGazeLeft.IsValid)
        {
            //average directions
            var poseR = eyeGazeRight.Pose.ToOVRPose();
            poseR = poseR.ToWorldSpacePose(Camera.main);
            var poseL = eyeGazeRight.Pose.ToOVRPose();
            poseL = poseL.ToWorldSpacePose(Camera.main);

            Quaternion q = Quaternion.Slerp(poseR.orientation, poseL.orientation, 0.5f);
            lastDirection = q * Vector3.forward;
            return lastDirection;
        }
        return lastDirection;
    } 

    static Vector3 GetLookPosition() {
        if (!OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _currentEyeGazesState))
            return lastPosition;

        var eyeGazeRight = _currentEyeGazesState.EyeGazes[(int)OVRPlugin.Eye.Right];
        var eyeGazeLeft = _currentEyeGazesState.EyeGazes[(int)OVRPlugin.Eye.Left];

        if (eyeGazeRight.IsValid && eyeGazeLeft.IsValid)
        {
            //average directions
            var poseR = eyeGazeRight.Pose.ToOVRPose();
            poseR = poseR.ToWorldSpacePose(Camera.main);
            var poseL = eyeGazeRight.Pose.ToOVRPose();
            poseL = poseL.ToWorldSpacePose(Camera.main);

            Vector3 pos = (poseR.position + poseL.position) / 2;
            return pos;
        }
        return lastPosition;
    }
    void Awake()
    {
        // Check if we have eye tracking support
        inputDeviceList = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, inputDeviceList);
        if (inputDeviceList.Count > 0)
        {
            Debug.Log("Eye tracking device found!", this);
            eyeTrackingDeviceFound = true;
            return;
        }

        foreach (var device in InputSystem.devices)
        {
            if (device.layout == k_EyeGazeLayoutName)
            {
                Debug.Log("Eye gaze device found!", this);
                eyeTrackingDeviceFound = true;
                return;
            }
        }

        Debug.LogWarning($"Could not find a device that supports eye tracking on Awake. {this} has subscribed to device connected events and will activate the GameObject when an eye tracking device is connected.", this);

        InputDevices.deviceConnected += OnDeviceConnected;
        InputSystem.onDeviceChange += OnDeviceChange;        
    }
    void Start()
    {
    }

    void Update()
    {
        // var testDirection = GetLookDirection();
        // Debug.Log("[SharedEyeTrackStandalone] testDirection: " + testDirection);
        // Debug.DrawRay(GetLookPosition(), testDirection, Color.red, 2, false);

        // RaycastHit hit;
        // if (Physics.Raycast(GetLookPosition(), testDirection, out hit))
        // {
        //     eyeTrackedObject.position = Vector3.Lerp(eyeTrackedObject.position, hit.point, lerpFactor);
        // }
        // else
        // {
        //     eyeTrackedObject.position = outOfSight;
        // }
        if (!eyeTracking.isValid)
        {
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, inputDeviceList);
            eyeTracking = inputDeviceList.FirstOrDefault();

            if (!eyeTracking.isValid)
            {
                Debug.LogWarning("Unable to acquire eye tracking device. Have permissions been granted?");
                return;
            }
        }        

        bool hasData = eyeTracking.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked);
        hasData &= eyeTracking.TryGetFeatureValue(EyeTrackingUsages.gazePosition, out Vector3 position);
        hasData &= eyeTracking.TryGetFeatureValue(EyeTrackingUsages.gazeRotation, out Quaternion rotation);

        if (isTracked && hasData)
        {
            // Need to do transformpoint and transformdirection so we can get a ray that is in world space
            Vector3 gazeOriginCombinedLocal = position;
            Vector3 gazeDirectionCombinedLocal = rotation * Vector3.forward;
            Vector3 gazeOriginWorld = Camera.main.transform.TransformPoint(gazeOriginCombinedLocal);
            Vector3 gazeDirectionWorld = Camera.main.transform.TransformDirection(gazeDirectionCombinedLocal);
            RaycastHit hit;
            if (Physics.Raycast(gazeOriginWorld, gazeDirectionWorld, out hit))
            {
                eyeTrackedObject.position = Vector3.Lerp(eyeTrackedObject.position, hit.point, lerpFactor);
            }
            else
            {
                eyeTrackedObject.position = outOfSight;
            }            
        }

    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnDestroy()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void OnDeviceConnected(UnityEngine.XR.InputDevice inputDevice)
    {
        if (eyeTrackingDeviceFound || !inputDevice.characteristics.HasFlag(InputDeviceCharacteristics.EyeTracking))
            return;

        Debug.Log("Eye tracking device found!", this);
        eyeTrackingDeviceFound = true;
        gameObject.SetActive(true);
    }

    void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
    {
        if (eyeTrackingDeviceFound || change != InputDeviceChange.Added)
            return;

        if (device.layout == k_EyeGazeLayoutName)
        {
            Debug.Log("Eye gaze device found!", this);
            eyeTrackingDeviceFound = true;
            gameObject.SetActive(true);
        }
    }
}
