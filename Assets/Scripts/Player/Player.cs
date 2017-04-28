using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
  public enum Team {
    Hiders,
    Seekers
  };

  [SyncVar] public Team team = Team.Seekers;
  [SyncVar] public string nickname = "unknown";
  [SyncVar] public int health = 100;
  [SyncVar] public int score = 0;
  [SyncVar] public bool isReady = false;
  [SyncVar] public bool isDead = false;

  PlayerGraphics graphics;
  PlayerController controller;
  PlayerUI ui;
  PlayerAudio playerAudio;
  Camera mainCamera;
  GameManager gameManager;

  void Awake() {
    graphics = GetComponent<PlayerGraphics>();
    controller = GetComponent<PlayerController>();
    ui = GetComponent<PlayerUI>();
    playerAudio = GetComponent<PlayerAudio>();
    mainCamera = Camera.main;
    gameManager = GameManager.instance;
  }

  void Start() {
    if (!isLocalPlayer)
      return;

    CmdSetNickname(PlayerPrefs.GetString("Nickname"));
  }

  [ClientCallback]
  void Update() {
    if (!isLocalPlayer)
      return;

    if (!isReady || isDead) {
      transform.position = Vector3.zero;
      return;
    }

    if (health <= 0 && !isDead) {
      Player p = null;
      foreach (Player player in GameManager.GetPlayers()) {
        if (!player.isDead && player.isReady && player != this) {
          p = player;
          break;
        }
      }
      if (p != null)
        GetComponent<PlayerSpectatorMode>().EnableSpectatorMode(p.transform);

      ui.ShowDeathScreen("Maybe next time...");
      CmdPlayerDie();
    }

    if (team == Team.Seekers && !gameManager.allowSeekers) {
      controller.enabled = false;
    } else if (!HUDController.instance.IsDisconnectWindowVisible()) {
      controller.enabled = true;
    }

    if (team == Team.Hiders) {
      if (Input.GetMouseButtonDown(0) && !HUDController.instance.IsDisconnectWindowVisible()) {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        int layerMask = 1 << LayerMask.NameToLayer("Player");
        layerMask = ~layerMask;

        if (Physics.Raycast(ray, out hit, 10.0f, layerMask)) {          
          if (hit.transform.tag == "Interactable") {
            ObjectID objectID = hit.transform.GetComponent<ObjectID>();

            if (objectID != null) {
              graphics.CmdTransformInto(objectID.id);
            } else {
              Debug.LogError("Cannot find ObjectID component in interactable object");
            }
          }
        }
      }

      if (Input.GetKeyDown(KeyCode.R)) {
        controller.playerRotationEnabled = !controller.playerRotationEnabled;
      }
    } else if (team == Team.Seekers) {
      if (Input.GetMouseButtonDown(0) && 
          !HUDController.instance.IsDisconnectWindowVisible() &&
          gameManager.allowSeekers) {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        string target = null;

        int layerMask = 1 << 8;
        layerMask = ~layerMask;

        playerAudio.CmdPlayShootEffect();

        if (Physics.Raycast(ray, out hit, 100.0f)) {
          graphics.CmdShootEffect(hit.point);
          if (hit.transform.tag == "Player") {
            target = hit.transform.GetComponent<NetworkIdentity>().netId.ToString();
          }
        } else {
          graphics.CmdShootEffect(transform.position + ray.direction * 1000.0f);
        }

        CmdShoot(target);
      }
    }
  }

  [Command]
  void CmdShoot(string target) {
    if (team != Team.Seekers)
      return;

    if (target != null && target != "") {
      Player p = GameManager.GetPlayer(target);

      if (p == null) {
        Debug.LogError("Cannot find player named: " + target);
        return;
      }

      if (GameManager.instance.isRoundOn)
        p.health -= 1;

      if (p.health <= 0 && !p.isDead) {
        score++;
        p.CmdPlayerDie();
      }
    } else {
      if (GameManager.instance.isRoundOn)
        health--;
    }
  }

  [Command]
  void CmdPlayerDie() {
    isDead = true;
    graphics.CmdHideAllGraphics();

    RpcPlayerDie();
  }

  [ClientRpc]
  void RpcPlayerDie() {
    if (!isLocalPlayer)
      return;
    // ...
  }

  [Command]
  void CmdSetNickname(string nick) {
    nickname = nick;
  }

  [Command]
  public void CmdSetReadyStatus(bool status) {
    isReady = status;
  }

  [Command]
  public void CmdSetTeam(Team t) {
    team = t;

    RpcSetTeam(t);
  }

  [ClientRpc]
  public void RpcSetTeam(Team t) {
    graphics.UpdateGraphics(t, true);

    if (isLocalPlayer) {
      if (t == Team.Hiders) {
        controller.SetCameraType(PlayerController.CameraType.ThirdPerson);
        Camera.main.transform.GetChild(0).gameObject.SetActive(false);
      } else if (t == Team.Seekers) {
        controller.SetCameraType(PlayerController.CameraType.FirstPerson);
        Camera.main.transform.GetChild(0).gameObject.SetActive(true);
      }
    }
  }

  [Command]
  void CmdResetStats() {
    isDead = false;

    if (team == Team.Hiders) {
      health = 10;
    } else if (team == Team.Seekers) {
      health = 100;
    }
  }

  [Command]
  public void CmdNewRound() {
    CmdResetStats();
    RpcNewRound();
  }

  [ClientRpc]
  void RpcNewRound() {
    // do it only on our player
    if (!isLocalPlayer)
      return;

    //graphics.UpdateGraphics(team, true);
    //graphics.CmdRestartGraphics(team);
    GetComponent<PlayerSpawner>().Spawn();
    GetComponent<PlayerSpectatorMode>().DisableSpectatorMode();
  }
}
