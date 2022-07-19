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
        return Mathf.Sqrt((radius*radius) + (height*height));
    }
}

public class ThirdPersonCamera : MonoBehaviour
{
    
    [Header("Targets")]
    [SerializeField] GameObject follow = null;
    [SerializeField] GameObject lookAt = null;

    [Header("Orbits")]
    [SerializeField] RingConfiguration topRing = new RingConfiguration{ radius = 2f, height = 1.4f, color = Color.red };
    [SerializeField] RingConfiguration middleRing = new RingConfiguration{ radius = 2f, height = 1.4f, color = Color.red };
    [SerializeField] RingConfiguration bottomRing = new RingConfiguration{ radius = 2f, height = 1.4f, color = Color.red };

    [Header("Editor Settings")]
    [SerializeField] bool showGizmos = true;

    void Start()
    {
        InitPosition();
    }

    // Update is called once per frame
    void Update()
    {
        SetPosition();
    }

    private void InitPosition()
    {
        transform.position = follow.transform.position - (follow.transform.forward * middleRing.GetBorderDistanceToCenter());
    }

    private void SetPosition(){
        Vector3 movement = follow.transform.up * Input.GetAxis("Mouse Y");
        Plane referencePlane = new Plane(follow.transform.up, follow.transform.position);

        transform.position += movement;
        float referenceHeight = referencePlane.GetDistanceToPoint(transform.position);

        if(referenceHeight >= topRing.height){

        }
        if(referenceHeight >= middleRing.height && referenceHeight < topRing.height){

        }
        if(referenceHeight >= bottomRing.height && referenceHeight < middleRing.height){

        }
        if(referenceHeight < bottomRing.height){

        }
    }

    private void SetRotation(){

    }

    private void InitRotation(){

    }

    private void OnDrawGizmos()
    {
        /*Gizmos.DrawCube(-1f * Vector3.right, Vector3.one * cubeSize);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Vector3.right, 0.5f);
        
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(3f * Vector3.right, 0.5f);*/
        
        if(follow != null && showGizmos)
        {
            DrawRing(topRing);
            DrawRing(middleRing);
            DrawRing(bottomRing);
        }
    }

    private void DrawRing(RingConfiguration ring){
        Handles.color = ring.color;
        Vector3 position = follow.transform.position + (follow.transform.up * ring.height);
        Handles.DrawWireDisc(position, follow.transform.up, ring.radius);
    }
}
