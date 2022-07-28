using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class RingConfiguration
{
  public float radius = 3f;
  public float height = 2.5f;
  public Color color = Color.red;

  public RingConfiguration(float radius, float height, Color color)
  {
    this.radius = radius;
    this.height = height;
    this.color = color;
  }

  public RingConfiguration(float radius, float height)
  {
    this.radius = radius;
    this.height = height;
    this.color = Color.green;
  }

  public float GetBorderDistanceToReference()
  {
    return Mathf.Sqrt((radius * radius) + (height * height));
  }
}

public class ThirdPersonCamera : MonoBehaviour
{

  [Header("Targets")]
  [SerializeField] GameObject follow = null;
  [SerializeField] GameObject lookAt = null;

  [Header("Orbits")]
  [SerializeField] RingConfiguration topRing = new RingConfiguration(2f, 1.4f, Color.red);
  [SerializeField] RingConfiguration middleRing = new RingConfiguration(5f, 3f, Color.red);
  [SerializeField] RingConfiguration bottomRing = new RingConfiguration(1f, -1f, Color.red);

  [Header("Positioning")]
  [SerializeField] bool avoidClipping = true;
  [ShowIf(nameof(avoidClipping))][SerializeField] float clippingOffset = 0f;
  [SerializeField][Range(-180, 180)] float horizontalTilt = 0f;
  [SerializeField][Range(-180, 180)] float verticalTilt = 0f;
  [SerializeField] bool useTargetNormal = true;

  [Header("Controls")]
  [SerializeField] bool captureCursor = false;

  [Header("X axis")]
  [SerializeField] string horizontalAxis = "Mouse X";
  [SerializeField] float horizontalSensitivity = 1f;
  [SerializeField] bool invertX = false;
  [Header("Y axis")]
  [SerializeField] string verticalAxis = "Mouse Y";
  [SerializeField] float verticalSensitivity = 0.8f;
  [SerializeField] bool invertY = true;

  [Header("Effects")]
  [SerializeField] bool zoomOutOnHighSpeed = true;
  [ShowIf(nameof(zoomOutOnHighSpeed))][SerializeField] float zoomOutStartSpeed = 10f;
  [ShowIf(nameof(zoomOutOnHighSpeed))][SerializeField] float zoomOutCapSpeed = 15f;
  [ShowIf(nameof(zoomOutOnHighSpeed))][SerializeField] float zoomStartDistanceRatio = 0.1f;
  [ShowIf(nameof(zoomOutOnHighSpeed))][SerializeField] float zoomCapDistanceRatio = 0.3f;


  [Header("Editor Settings")]
  [SerializeField] bool showGizmos = true;

  private float cameraTranslation = 90f;
  private float verticalMultiplier = 10f;
  private float referenceHeight = 0f;
  private float referenceDistance;
  private float noClippingHeight;
  private float noClippingDistance;
  private RingConfiguration cameraRing = null;
  private Vector3 up;
  private Vector3 right;
  private Vector3 forward;

  void Start()
  {
    InitPosition();
  }

  void Update()
  {
    if (captureCursor)
    {
      Cursor.lockState = CursorLockMode.Locked;
    }
    SetNormalVectors();
    SetPosition();
    SetRotation();
  }

  private void InitPosition()
  {
    referenceHeight = middleRing.height;
  }

  private void SetNormalVectors()
  {
    up = useTargetNormal ? follow.transform.up : Vector3.up;
    right = Vector3.Cross(up, Vector3.right);
    forward = Vector3.Cross(up, right);
  }

  private void SetPosition()
  {
    ReadInputs();
    referenceDistance = 0f;

    cameraRing = GetCameraRing();

    referenceHeight = cameraRing.height;
    float distance = cameraRing.GetBorderDistanceToReference();
    referenceDistance = Mathf.Sqrt((distance * distance) - (referenceHeight * referenceHeight));
    referenceDistance = ApplyDistanceEffects(referenceDistance);
    CorrectClipping(distance);

    Vector3 heightVector = up * (avoidClipping ? noClippingHeight : referenceHeight);
    Vector3 distanceVector = -forward * (avoidClipping ? noClippingDistance : referenceDistance);

    transform.position = follow.transform.position + heightVector + distanceVector;
    transform.RotateAround(follow.transform.position, up, cameraTranslation);
  }

  private void SetRotation()
  {
    LookAt(up, lookAt.transform);

    Vector3 verticalAngles = right * verticalTilt;
    Vector3 horizontalAngles = up * horizontalTilt;

    Vector3 eulerRotation = verticalAngles + horizontalAngles;
    transform.Rotate(eulerRotation.x, eulerRotation.y, eulerRotation.z);
  }

  private void LookAt(Vector3 normal, Transform lookAt)
  {
    Vector3 targetDirection = (lookAt.position - transform.position).normalized;
    transform.localRotation = Quaternion.LookRotation(targetDirection, normal);
  }

  private void ReadInputs()
  {
    referenceHeight += Input.GetAxis(verticalAxis) * verticalSensitivity * (invertY ? -1 : 1);
    cameraTranslation += Input.GetAxis(horizontalAxis) * verticalMultiplier * horizontalSensitivity * (invertX ? -1 : 1);

    if (cameraTranslation > 360f)
    {
      cameraTranslation -= 360f;
    }
    else if (cameraTranslation < 0f)
    {
      cameraTranslation += 360f;
    }
  }

  private void CorrectClipping(float raycastDistance)
  {
    RaycastHit hit;
    Ray ray = new Ray(follow.transform.position, (transform.position - follow.transform.position).normalized);

    if (avoidClipping && Physics.Raycast(ray, out hit, raycastDistance))
    {
      float safeDistance = hit.distance - clippingOffset;
      float sinAngl = referenceHeight / raycastDistance;
      float cosAngl = referenceDistance / raycastDistance;

      noClippingHeight = safeDistance * sinAngl;
      noClippingDistance = safeDistance * cosAngl;
    }
    else
    {
      noClippingHeight = referenceHeight;
      noClippingDistance = referenceDistance;
    }
  }

  private float ApplyDistanceEffects(float distance)
  {
    if (zoomOutOnHighSpeed)
    {
      Rigidbody rb = follow.GetComponent<Rigidbody>();
      if (rb == null)
      {
        return distance;
      }
      float speed = follow.GetComponent<Rigidbody>().velocity.magnitude;
      Debug.Log(speed);
      float speedRatio = Mathf.Clamp01((speed - zoomOutStartSpeed) / (zoomOutCapSpeed - zoomOutStartSpeed));
      float distanceIncrease = Mathf.Lerp(zoomStartDistanceRatio, zoomCapDistanceRatio, speedRatio);
      return distance += (distanceIncrease * distance);
    }
    else
    {
      return distance;
    }
  }

  private float EaseLerpRingRadius(RingConfiguration r1, RingConfiguration r2)
  {
    float lerpState = Mathf.InverseLerp(r1.height, r2.height, referenceHeight);
    if (r1.radius > r2.radius)
    {
      lerpState = lerpState * lerpState;
    }
    else
    {
      lerpState = Mathf.Sqrt(lerpState);
    }
    float radius = Mathf.Lerp(r1.radius, r2.radius, lerpState);
    return radius;
  }

  private RingConfiguration GetCameraRing()
  {
    if (referenceHeight >= topRing.height)
    {
      return new RingConfiguration(topRing.radius, topRing.height);
    }
    else if (referenceHeight >= middleRing.height)
    {
      float radius = EaseLerpRingRadius(middleRing, topRing);
      return new RingConfiguration(radius, referenceHeight);
    }
    else if (referenceHeight >= bottomRing.height)
    {
      float radius = EaseLerpRingRadius(bottomRing, middleRing);
      return new RingConfiguration(radius, referenceHeight);
    }
    else
    {
      return new RingConfiguration(bottomRing.radius, bottomRing.height);
    }
  }

  private void OnDrawGizmos()
  {
    if (follow != null && showGizmos)
    {
      DrawRing(topRing);
      DrawRing(middleRing);
      DrawRing(bottomRing);
    }
  }

  private void DrawRing(RingConfiguration ring)
  {
    Vector3 up = useTargetNormal ? follow.transform.up : Vector3.up;
    Handles.color = ring.color;
    Vector3 position = follow.transform.position + (up * ring.height);
    Handles.DrawWireDisc(position, up, ring.radius);
  }
}
