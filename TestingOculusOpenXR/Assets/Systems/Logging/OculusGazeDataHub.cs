using UnityEngine;
using System;

public class OculusGazeData
{
    public Vector3 GazeDirection { get; set; }
    public Vector3 GazeOrigin { get; set; }  
}

public class OculusGazeDataHub : MonoBehaviour
{
    public static OculusGazeDataHub Instance { get; private set; }
    public OculusGazeData GazeData { get; private set; } = new OculusGazeData();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public bool EyeTrackingEnabled => OVRPlugin.eyeTrackingEnabled;
    public EyeId Eye;
    public float Confidence { get; private set; }

    /// <summary>
    /// GameObject will not change if detected eye state confidence is below this threshold.
    /// </summary>
    [Range(0f, 1f)]
    public float ConfidenceThreshold = 0.5f;

    private OVRPlugin.EyeGazesState _currentEyeGazesState;

    public Transform ReferenceFrame;
    private Quaternion _initialRotationOffset;
    private Transform _viewTransform;
    public Transform eyeTrackedObject;


    private static int _trackingInstanceCount;

    public void UpdateGazeData(Vector3 origin, Vector3 direction)
    {
        GazeData.GazeOrigin = origin;
        GazeData.GazeDirection = direction;
    }
    private void Start()
    {
        PrepareHeadDirection();
    }

    private bool StartEyeTracking()
    {
        if (!OVRPlugin.StartEyeTracking())
        {
            Debug.LogWarning($"[{nameof(OVREyeGaze)}] Failed to start eye tracking.");
            return false;
        }

        return true;
    }

    private void OnDisable()
    {
        if (--_trackingInstanceCount == 0)
        {
            OVRPlugin.StopEyeTracking();
        }
    }


    private void Update()
    {
        if (!OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _currentEyeGazesState))
            return;

        var eyeGaze = _currentEyeGazesState.EyeGazes[(int)Eye];

        if (!eyeGaze.IsValid)
            return;

        Confidence = eyeGaze.Confidence;
        if (Confidence < ConfidenceThreshold)
            return;

        var pose = eyeGaze.Pose.ToOVRPose();
        pose = pose.ToWorldSpacePose();
        
        var GazeOriginPose = pose.ToWorldSpacePose(Camera.main);
        var GazeDirectionPose = pose.ToWorldSpacePose(Camera.main);
        
        GazeData.GazeOrigin = GazeOriginPose.position;
        GazeData.GazeDirection = GazeDirectionPose.orientation * Vector3.forward;
    }

    private Quaternion CalculateEyeRotation(Quaternion eyeRotation)
    {
        var eyeRotationWorldSpace = _viewTransform.rotation * eyeRotation;
        var lookDirection = eyeRotationWorldSpace * Vector3.forward;
        var targetRotation = Quaternion.LookRotation(lookDirection, _viewTransform.up);

        return targetRotation * _initialRotationOffset;
    }

    private void PrepareHeadDirection()
    {
        string transformName = "HeadLookAtDirection";

        _viewTransform = new GameObject(transformName).transform;

        if (ReferenceFrame)
        {
            _viewTransform.SetPositionAndRotation(ReferenceFrame.position, ReferenceFrame.rotation);
        }
        else
        {
            _viewTransform.SetPositionAndRotation(transform.position, Quaternion.identity);
        }

        _viewTransform.parent = transform.parent;
        _initialRotationOffset = Quaternion.Inverse(_viewTransform.rotation) * transform.rotation;
    }


    /// <summary>
    /// List of eyes
    /// </summary>
    public enum EyeId
    {
        Left = OVRPlugin.Eye.Left,
        Right = OVRPlugin.Eye.Right
    }
    
        
}
