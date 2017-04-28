using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class HUDController : MonoBehaviour {
  public static HUDController instance;

  [HideInInspector]
  public PlayerController playerController;

  void Awake() {
    if (instance == null)
      instance = this;
    else
      Debug.LogError("There is already HUDController on scene");
  }

  [Header("HUD elements")]
  [SerializeField]
  GameObject disconnectWindow;
  public GameObject teamChoose;
  public GameObject hud;
  public Scoreboard scoreboard;
  public GameObject deathScreen;
  public GameObject crosshair;
  public GameObject blackScreenForSeekers;

  public void ShowDisconnectWindow() {
    disconnectWindow.SetActive(true);
    playerController.enabled = false;
  }

  public void HideDisconnectWindow() {
    disconnectWindow.SetActive(false);
    playerController.enabled = true;
  }

  public void Disconnect() {
    MatchInfo matchInfo = NetworkManager.singleton.matchInfo;
    if (matchInfo != null)
      NetworkManager.singleton.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, NetworkManager.singleton.OnDropConnection);
    NetworkManager.singleton.StopHost();
  }

  public bool IsDisconnectWindowVisible() {
    return disconnectWindow.activeSelf;
  }
}
