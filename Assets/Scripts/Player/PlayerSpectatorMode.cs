using UnityEngine;
using UnityEngine.Networking;

public class PlayerSpectatorMode : NetworkBehaviour {
  Transform target;

  PlayerUI ui;
  PlayerController controller;

  // camera
  float x, y;
  Transform cameraTransform;
  float distance = 3.0f;

  void Start() {
    if (!isLocalPlayer)
      return;

    ui = GetComponent<PlayerUI>();
    controller = GetComponent<PlayerController>();
    cameraTransform = Camera.main.transform;
  }

  void Update() {
    if (!isLocalPlayer)
      return;

    if (target != null) {
      x += Input.GetAxis("Mouse X") * controller.sensitivity;
      y -= Input.GetAxis("Mouse Y") * controller.sensitivity;

      y = controller.ClampAngle(y, -50, 80);

      Quaternion rotation = Quaternion.Euler(y, x, 0);

      float currentDistance = distance;
      Vector3 position = target.position - (rotation * Vector3.forward * currentDistance + new Vector3(0, -controller.characterHeight, 0));

      cameraTransform.rotation = rotation;
      cameraTransform.position = position;
    }
  }

  public void EnableSpectatorMode(Transform targetTransform) {
    target = targetTransform;
    HUDController.instance.crosshair.SetActive(false);
    controller.enabled = false;
    cameraTransform.SetParent(null);
  }

  public void DisableSpectatorMode() {
    target = null;
    HUDController.instance.crosshair.SetActive(true);
    controller.enabled = true;
  }

  public bool IsSpectatorModeEnabled() {
    return (target != null);
  }
}
