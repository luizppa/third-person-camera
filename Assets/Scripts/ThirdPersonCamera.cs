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
  [SerializeField] RingConfiguration topRing = new RingConfiguration { radius = 2f, height = 1.4f, color = Color.red };
  [SerializeField] RingConfiguration middleRing = new RingConfiguration { radius = 0.5f, height = 3f, color = Color.red };
  [SerializeField] RingConfiguration bottomRing = new RingConfiguration { radius = 1f, height = -1f, color = Color.red };
  [SerializeField] bool avoidClipping = true;

  [Header("Controls")]
  [Header("X axis")]
  [SerializeField] string horizontalAxis = "Mouse X";
  [SerializeField] float horizontalSensibility = 1f;
  [SerializeField] bool invertX = false;
  [Header("Y axis")]
  [SerializeField] string verticalAxis = "Mouse Y";
  [SerializeField] float verticalSensibility = 1f;
  [SerializeField] bool invertY = true;

  [Header("Editor Settings")]
  [SerializeField] bool showGizmos = true;

  private float cameraTranslation = 0f;
  private float verticalMultiplier = 10f;
  private float referenceHeight = 0f;
  private float referenceDistance;
  private float noClippingHeight;
  private float noClippingDistance;

  void Start()
  {
    InitPosition();
  }

  void Update()
  {
    SetPosition();
    SetRotation();
  }

  private void InitPosition()
  {
    transform.position = follow.transform.position - (follow.transform.forward * middleRing.GetBorderDistanceToReference());
  }

  private void SetPosition()
  {
    ReadInputs();
    referenceDistance = 0f;

    RingConfiguration cameraRing = GetCameraRing();

    referenceHeight = cameraRing.height;
    float distance = cameraRing.GetBorderDistanceToReference();
    referenceDistance = Mathf.Sqrt((distance * distance) - (referenceHeight * referenceHeight));
    CorrectClipping(distance);

    Vector3 heightVector = follow.transform.up * (avoidClipping ? noClippingHeight : referenceHeight);
    Vector3 distanceVector = follow.transform.forward * (avoidClipping ? noClippingDistance : referenceDistance);

    transform.position = follow.transform.position + heightVector + distanceVector;
    transform.RotateAround(follow.transform.position, follow.transform.up, cameraTranslation);
  }

  private void ReadInputs()
  {
    referenceHeight += Input.GetAxis(verticalAxis) * verticalSensibility * (invertY ? -1 : 1);
    cameraTranslation += Input.GetAxis(horizontalAxis) * verticalMultiplier * horizontalSensibility * (invertX ? -1 : 1);

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
      float sinAngl = referenceHeight / raycastDistance;
      float cosAngl = referenceDistance / raycastDistance;

      noClippingHeight = hit.distance * sinAngl;
      noClippingDistance = hit.distance * cosAngl;
    }
    else
    {
      noClippingHeight = referenceHeight;
      noClippingDistance = referenceDistance;
    }
  }

  private void SetRotation()
  {
    transform.LookAt(lookAt.transform);
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
      return new RingConfiguration { radius = topRing.radius, height = topRing.height, color = Color.green };
    }
    else if (referenceHeight >= middleRing.height)
    {
      float radius = EaseLerpRingRadius(middleRing, topRing);
      return new RingConfiguration { radius = radius, height = referenceHeight, color = Color.green };
    }
    else if (referenceHeight >= bottomRing.height)
    {
      float radius = EaseLerpRingRadius(bottomRing, middleRing);
      return new RingConfiguration { radius = radius, height = referenceHeight, color = Color.green };
    }
    else
    {
      return new RingConfiguration { radius = bottomRing.radius, height = bottomRing.height, color = Color.green };
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
    Handles.color = ring.color;
    Vector3 position = follow.transform.position + (follow.transform.up * ring.height);
    Handles.DrawWireDisc(position, follow.transform.up, ring.radius);
  }
}
