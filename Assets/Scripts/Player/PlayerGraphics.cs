using UnityEngine;
using UnityEngine.Networking;

public class PlayerGraphics : NetworkBehaviour {
  [SerializeField]
  Transform graphicsParent;

  [SerializeField]
  GameObject hiderGraphics;

  [SerializeField]
  GameObject seekerGraphics;

  [SerializeField]
  GameObject localPlayerSeeker;

  Player player;

  [SyncVar] NetworkInstanceId transformedObject;
  [SyncVar] bool isTransformed = false;

  [SerializeField] GameObject gunLinePrefab;

  Transform weaponShootPos;

  void Awake() {
    player = GetComponent<Player>();
  }

  void Start() {
    if (!isLocalPlayer)
      return;

    foreach (Player player in GameManager.GetPlayers()) {
      player.GetComponent<PlayerGraphics>().UpdateGraphics(player.team);
    }

    CmdHideAllGraphics();
  }

  void HideAllGraphics() {
    foreach (Transform child in graphicsParent) {
      Destroy(child.gameObject);
    }
  }

  void SetSeekerGraphics() {
    HideAllGraphics();
    
    GameObject obj;
    
    if (isLocalPlayer)
      obj = Instantiate(localPlayerSeeker);
    else {
      obj = Instantiate(seekerGraphics);
      weaponShootPos = seekerGraphics.transform.GetChild(0).GetChild(0);
    }

    obj.transform.SetParent(graphicsParent);
    obj.transform.localPosition = Vector3.zero;
    obj.transform.localRotation = Quaternion.identity;
  }

  void SetHiderGraphics() {
    HideAllGraphics();
    
    GameObject obj = Instantiate(hiderGraphics);

    obj.transform.SetParent(graphicsParent);
    obj.transform.localPosition = Vector3.zero;
    obj.transform.localRotation = Quaternion.identity;
  }

  public void UpdateGraphics(Player.Team team, bool removeTransformedObject = false) {
    if (isTransformed && !removeTransformedObject) {
      TransformInto(transformedObject);
    } else {
      if (isLocalPlayer)
        CmdDisableTransformedObject();

      if (team == Player.Team.Hiders) {
        SetHiderGraphics();
      } else if (team == Player.Team.Seekers) {
        SetSeekerGraphics();
      }
    }
  }

  [Command]
  void CmdDisableTransformedObject() {
    isTransformed = false;
  }

  [Command]
  public void CmdHideAllGraphics() {
    RpcHideAllGraphics();
  }

  [ClientRpc]
  void RpcHideAllGraphics() {
    HideAllGraphics();
  }

  [Command]
  public void CmdRestartGraphics(Player.Team t) {
    RpcRestartGraphics(t);
  }

  [ClientRpc]
  void RpcRestartGraphics(Player.Team t) {
    HideAllGraphics();
    UpdateGraphics(player.team, true);
  }

  [Command]
  public void CmdTransformInto(string id) {
    GameObject prefab = null;

    foreach (GameObject spawnPrefab in NetworkManager.singleton.spawnPrefabs) {
      ObjectID objectID = spawnPrefab.GetComponent<ObjectID>();

      if (objectID == null)
        continue;

      if (objectID.id == id) {
        prefab = spawnPrefab;
        break;
      }
    }

    if (prefab == null) {
      Debug.LogError("Cannot find spawnable prefab for an object with id: " + id);
      return;
    }

    GameObject transformObject = Instantiate(prefab);
    transformObject.transform.localPosition = transform.position + new Vector3(0, 2, 0);
    transformObject.layer = LayerMask.NameToLayer("Player");

    NetworkServer.Spawn(transformObject);
    RpcTransformInto(transformObject.GetComponent<NetworkIdentity>().netId);

    transformedObject = transformObject.GetComponent<NetworkIdentity>().netId;
    isTransformed = true;
  }

  [ClientRpc]
  void RpcTransformInto(NetworkInstanceId netid) {
    TransformInto(netid);
  }

  void TransformInto(NetworkInstanceId netid) {
    GameObject transformObject = ClientScene.FindLocalObject(netid);

    if (transformObject == null) {
      Debug.LogError("Error while trying to finding local object. NetId: " + netid.ToString());
      return;
    }

    NetworkTransform nt = transformObject.GetComponent<NetworkTransform>();
    if (nt != null)
      Destroy(nt);

    Rigidbody rb = transformObject.GetComponent<Rigidbody>();
    if (rb != null)
      Destroy(rb);

    HideAllGraphics();

    transformObject.transform.SetParent(graphicsParent);
    transformObject.transform.localPosition = Vector3.zero;
  }

  [Command]
  public void CmdShootEffect(Vector3 pos) {
    RpcShootEffect(pos);
  }
  
  [ClientRpc]
  void RpcShootEffect(Vector3 pos) {
    GameObject line = Instantiate(gunLinePrefab);
    LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

    if (isLocalPlayer)
      lineRenderer.SetPosition(0, Camera.main.transform.GetChild(0).GetChild(0).position);
    else
      lineRenderer.SetPosition(0, weaponShootPos.position);

    lineRenderer.SetPosition(1, pos);
    
    RemoveObject ro = line.GetComponent<RemoveObject>();
    ro.timeToDestroy = 0.1f;
    ro.BeginDestroying();
  }
}
