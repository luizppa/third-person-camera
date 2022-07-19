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

  public float GetBorderDistanceToCenter()
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
  [SerializeField] RingConfiguration middleRing = new RingConfiguration { radius = 2f, height = 1.4f, color = Color.red };
  [SerializeField] RingConfiguration bottomRing = new RingConfiguration { radius = 2f, height = 1.4f, color = Color.red };

  [Header("Editor Settings")]
  [SerializeField] bool showGizmos = true;

  private float cameraTranslation = 0f;

  void Start()
  {
    InitPosition();
  }

  // Update is called once per frame
  void Update()
  {
    SetPosition();
    SetRotation();
  }

  private void InitPosition()
  {
    transform.position = follow.transform.position - (follow.transform.forward * middleRing.GetBorderDistanceToCenter());
  }

  private void SetPosition()
  {
    Vector3 movement = follow.transform.up * Input.GetAxis("Mouse Y");
    cameraTranslation += Input.GetAxis("Mouse X") * 10f;
    Plane referencePlane = new Plane(follow.transform.up, follow.transform.position);

    float referenceHeight = referencePlane.GetDistanceToPoint(transform.position + movement);
    float referenceDistance = 0f;

    RingConfiguration cameraRing = GetCameraRing(referenceHeight);

    referenceHeight = cameraRing.height;
    float distance = cameraRing.GetBorderDistanceToCenter();
    referenceDistance = Mathf.Sqrt((distance * distance) - (referenceHeight * referenceHeight));

    transform.position = follow.transform.position + (follow.transform.up * referenceHeight) - (follow.transform.forward * referenceDistance);
    transform.RotateAround(follow.transform.position, follow.transform.up, cameraTranslation);
  }

  private RingConfiguration GetCameraRing(float referenceHeight)
  {

    if (referenceHeight >= topRing.height)
    {
      return new RingConfiguration { radius = topRing.radius, height = topRing.height, color = Color.green };
    }
    else if (referenceHeight >= middleRing.height)
    {
      float lerpState = Mathf.InverseLerp(middleRing.height, topRing.height, referenceHeight);
      float radius = Mathf.Lerp(middleRing.radius, topRing.radius, lerpState);
      return new RingConfiguration { radius = radius, height = referenceHeight, color = Color.green };
    }
    else if (referenceHeight >= bottomRing.height)
    {
      float lerpState = Mathf.InverseLerp(bottomRing.height, middleRing.height, referenceHeight);
      float radius = Mathf.Lerp(bottomRing.radius, middleRing.radius, lerpState);
      return new RingConfiguration { radius = radius, height = referenceHeight, color = Color.green };
    }
    else
    {
      return new RingConfiguration { radius = bottomRing.radius, height = bottomRing.height, color = Color.green };
    }
  }

  private void SetRotation()
  {
    transform.LookAt(lookAt.transform);
  }

  private void InitRotation()
  {

  }

  private void OnDrawGizmos()
  {
    /*Gizmos.DrawCube(-1f * Vector3.right, Vector3.one * cubeSize);

    Gizmos.color = Color.red;
    Gizmos.DrawSphere(Vector3.right, 0.5f);

    Gizmos.color = Color.white;
    Gizmos.DrawSphere(3f * Vector3.right, 0.5f);*/

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
