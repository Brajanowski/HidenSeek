using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : NetworkBehaviour {
  public enum CameraType {
    FirstPerson,
    ThirdPerson
  };
  public CameraType cameraType = CameraType.FirstPerson;

  [Header("Main")]
  public float sensitivity = 2.0f;
  public Camera mainCamera = null;

  [Space]
  [Header("First person")]
  public Vector3 cameraOffset = new Vector3(0, 0, 0);

  public float minimumX = -90.0f;
  public float maximumX = 90.0f;

  Quaternion characterRotation;
  Quaternion tempCharacterRotation;
  Quaternion cameraRotation;

  [Space]
  [Header("Third person")]
  public float characterHeight = 1.0f;
  public float distance = 3.0f;
  public float minDistance = 2.0f;
  public float maxDistance = 6.0f;
  public int zoomRate = 30;

  float x = 0.0f;
  float y = 0.0f;

  public bool playerRotationEnabled = true;

  [Space]
  [Header("Movement")]
  public float speed = 5.0f;
  public float gravity = 10.0f;
  public float maxVelocityChange = 10.0f;
  public bool canJump = true;
  public float jumpHeight = 2.0f;
  public KeyCode jumpKey = KeyCode.Space;

  bool isGrounded = false;
  Rigidbody rbody;

  void Start() {
    rbody = GetComponent<Rigidbody>();

    if (mainCamera == null) {
      mainCamera = Camera.main;
    }

    SetCameraType(cameraType);

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  void OnEnable() {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  void OnDisable() {
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;  
  }

  void Update() {
    if (cameraType == CameraType.FirstPerson) {
      FirstPersonCamera();
    } else if (cameraType == CameraType.ThirdPerson) {
      ThirdPersonCamera();
    }

    if (transform.position.y > 15.0f) transform.position = new Vector3(transform.position.x, 4.0f, transform.position.z);
    else if (transform.position.y < -5.0f) transform.position = new Vector3(transform.position.x, 4.0f, transform.position.z);
  }

  void FixedUpdate() {
    Movement();
  }

  // camera look
  void FirstPersonCamera() {
    float yRot = Input.GetAxis("Mouse X") * sensitivity;
    float xRot = Input.GetAxis("Mouse Y") * sensitivity;

    characterRotation *= Quaternion.Euler(0, yRot, 0);
    cameraRotation *= Quaternion.Euler(-xRot, 0, 0);

    cameraRotation = ClampRotationAroundXAxis(cameraRotation);

    transform.localRotation = characterRotation;
    mainCamera.transform.localRotation = cameraRotation;
  }

  Quaternion ClampRotationAroundXAxis(Quaternion q) {
    q.x /= q.w;
    q.y /= q.w;
    q.z /= q.w;
    q.w = 1.0f;

    float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
    angleX = Mathf.Clamp(angleX, minimumX, maximumX);
    q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
    return q;
  }

  public float ClampAngle(float angle, float min, float max) {
    if (angle > 360) angle -= 360; else if (angle < -360) angle += 360;
    return Mathf.Clamp(angle, min, max);
  }

  void ThirdPersonCamera() {
    distance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate;
    distance = Mathf.Clamp(distance, minDistance, maxDistance);

    x += Input.GetAxis("Mouse X") * sensitivity;
    y -= Input.GetAxis("Mouse Y") * sensitivity;

    y = ClampAngle(y, -50, 80);

    Quaternion rotation = Quaternion.Euler(y, x, 0);

    float currentDistance = distance;
    Vector3 position = transform.position - (rotation * Vector3.forward * currentDistance + new Vector3(0, -characterHeight, 0));

    mainCamera.transform.rotation = rotation;
    mainCamera.transform.position = position;

    characterRotation = Quaternion.Euler(0, x, 0);

    if (playerRotationEnabled) {
      tempCharacterRotation = characterRotation;
    }

    transform.localRotation = tempCharacterRotation;
  }

  // movement
  void Movement() {
    Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

    Quaternion tempRot = transform.localRotation;
    transform.localRotation = characterRotation;
    targetVelocity = transform.TransformDirection(targetVelocity);
    transform.localRotation = tempRot;

    targetVelocity *= speed;

    Vector3 velocity = rbody.velocity;
    Vector3 velocityChange = (targetVelocity - velocity);
    velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
    velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
    velocityChange.y = 0;

    rbody.AddForce(velocityChange, ForceMode.VelocityChange);
    Debug.DrawRay(transform.position + new Vector3(0, 0.25f, 0), -Vector3.up, Color.red, 0.1f);

    if (canJump && IsGrounded()) {
      if (Input.GetKeyDown(jumpKey)) {
        rbody.AddForce(new Vector3(0, CalculateJumpVerticalSpeed(), 0), ForceMode.Impulse);
        canJump = false;
        StartCoroutine(JumpCoroutine());
      }
    }

    isGrounded = false;
  }

  float CalculateJumpVerticalSpeed() {
    return Mathf.Sqrt(2 * jumpHeight * gravity);
  }
  
  float distanceToGround = 0.5f;

  bool IsGrounded() {
    return Physics.Raycast(transform.position + new Vector3(0, 0.15f, 0), -Vector3.up, distanceToGround + 0.1f);
  }

  void OnCollisionStay() {
    isGrounded = true;
  }

  public void SetCameraType(CameraType type) {
    cameraType = type;

    if (type == CameraType.FirstPerson) {
      mainCamera.transform.parent = transform;
      mainCamera.transform.localPosition = cameraOffset;
      mainCamera.transform.localRotation = Quaternion.identity;

      characterRotation = transform.localRotation;
      cameraRotation = mainCamera.transform.localRotation;
    } else if (type == CameraType.ThirdPerson) {
      mainCamera.transform.parent = null;
      characterRotation = transform.localRotation;
      tempCharacterRotation = transform.localRotation;
      cameraRotation = mainCamera.transform.localRotation;
    }
  }

  IEnumerator JumpCoroutine() {
    yield return new WaitForSeconds(1.0f);
    canJump = true;
  }
}
