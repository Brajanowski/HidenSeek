using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour {
	void Start () {
    if (!isLocalPlayer) {
      GetComponent<PlayerController>().enabled = false;
      GetComponent<PlayerUI>().enabled = false;
    } else {
      GameManager.mainPlayer = GetComponent<Player>();
    }
	}

  public override void OnStartClient() {
    base.OnStartClient();
    GameManager.RegisterPlayer(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<Player>());
  }
 
  void OnDisable() {
    GameManager.UnregisterPlayer(GetComponent<NetworkIdentity>().netId.ToString());
  }
}
