using System;
using UnityEngine;

/// <summary>
/// This class updates the transform of the GameObject to point toward an eye direction.
/// </summary>
/// <remarks>
/// See <see cref="OVRPlugin.EyeGazeState"/> structure for list of eye state parameters.
/// </remarks>
[HelpURL("https://developer.oculus.com/reference/unity/latest/class_o_v_r_eye_gaze")]
public class EyeTrackGazeWithOculus : MonoBehaviour
{
    /// <summary>
    /// True if eye tracking is enabled, otherwise false.
    /// </summary>
    public bool EyeTrackingEnabled => OVRPlugin.eyeTrackingEnabled;

    /// <summary>
    /// GameObject will automatically change position and rotate according to the selected eye.
    /// </summary>
    public EyeId Eye;

    /// <summary>
    /// A confidence value ranging from 0..1 indicating the reliability of the eye tracking data.
    /// </summary>
    public float Confidence { get; private set; }

    /// <summary>
    /// GameObject will not change if detected eye state confidence is below this threshold.
    /// </summary>
    [Range(0f, 1f)]
    public float ConfidenceThreshold = 0.5f;

    /// <summary>
    /// GameObject will automatically change position.
    /// </summary>
    public bool ApplyPosition = true;

    /// <summary>
    /// GameObject will automatically rotate.
    /// </summary>
    public bool ApplyRotation = true;

    private OVRPlugin.EyeGazesState _currentEyeGazesState;

    /// <summary>
    /// Reference frame for eye. If it's null, then world reference frame will be used.
    /// </summary>
    [Tooltip("Reference frame for eye. " +
             "Reference frame should be set in the forward direction of the eye. It is there to calculate the initial offset of the eye GameObject. " +
             "If it's null, then world reference frame will be used.")]
    public Transform ReferenceFrame;

    /// <summary>
    /// HeadSpace: Track eye relative to head space.
    /// WorldSpace: Track eye in world space.
    /// TrackingSpace: Track eye relative to OVRCameraRig.
    /// </summary>
    [Tooltip(
        "HeadSpace: Tracking mode will convert the eye pose from tracking space to local space " +
        "which is relative to the VR camera rig. For example, we can use this setting to correctly " +
        "show the eye movement of a character which is facing in another direction than the source.\n" +
        "WorldSpace: Tracking mode will convert the eye pose from tracking space to world space.\n" +
        "TrackingSpace: Track eye is relative to OVRCameraRig. This is raw pose information from VR tracking space.")]
    public EyeTrackingMode TrackingMode;

    private Quaternion _initialRotationOffset;
    private Transform _viewTransform;
    public Transform eyeTrackedObject;

    [Tooltip("Layer mask to specify which layers the raycast should hit")]
    public LayerMask raycastLayerMask;


    private static int _trackingInstanceCount;

    private void Awake()
    {
    }

    private void Start()
    {
        PrepareHeadDirection();
    }

    private void OnEnable()
    {
        _trackingInstanceCount++;

        if (!StartEyeTracking())
        {
            enabled = false;
        }
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
        switch (TrackingMode)
        {
            case EyeTrackingMode.HeadSpace:
                pose = pose.ToHeadSpacePose();
                break;
            case EyeTrackingMode.WorldSpace:
                pose = pose.ToWorldSpacePose(Camera.main);
                break;
        }
        Debug.DrawRay(pose.position, pose.orientation * Vector3.forward, Color.red, 2, false);

        RaycastHit hit;
        if (Physics.Raycast(pose.position, pose.orientation * Vector3.forward, out hit, raycastLayerMask))
        {
            eyeTrackedObject.position = Vector3.Lerp(eyeTrackedObject.position, hit.point, 0.5f );
        }
        else
        {
            eyeTrackedObject.position = new Vector3(-1000, -1000, -1000);
        }

        // if (ApplyPosition)
        // {
        //     transform.position = pose.position;
        // }

        // if (ApplyRotation)
        // {
        //     transform.rotation = CalculateEyeRotation(pose.orientation);
        // }
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

    public enum EyeTrackingMode
    {
        HeadSpace,
        WorldSpace,
        TrackingSpace
    }
}
