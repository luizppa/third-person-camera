using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
  [SerializeField] Camera gameCamera = null;
  [SerializeField] float moveSpeed = 1f;

  private Rigidbody rb = null;

  void Start()
  {
    rb = GetComponent<Rigidbody>();
  }

  void FixedUpdate()
  {
    ControlPlayer();
  }

  void ControlPlayer()
  {
    Rotate();
    Move();
  }

  void Rotate()
  {
    Vector3 direction = Vector3.ProjectOnPlane(GetMoveDirection(), transform.up);
    if (direction.magnitude > 0.1f)
    {
      transform.rotation = Quaternion.LookRotation(direction);
    }
  }

  void Move()
  {
    Vector3 direction = Vector3.ProjectOnPlane(GetMoveDirection(), transform.up);
    if (direction.magnitude > 0.1f)
    {
      Vector3 velocity = direction * GetMoveSpeed() * Time.deltaTime * 60f;
      rb.AddForce(velocity, ForceMode.VelocityChange);
    }
  }

  private Vector3 GetMoveDirection()
  {
    Vector3 direction = (gameCamera.transform.right * Input.GetAxis("Horizontal")) + (gameCamera.transform.forward * Input.GetAxis("Vertical"));
    return direction;
  }

  private float GetMoveSpeed()
  {
    return moveSpeed;
  }
}
