using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {
  [SerializeField] GameObject main;
  [SerializeField] GameObject hostOnline;
  [SerializeField] GameObject joinOnline;
  [SerializeField] GameObject nickname;
  [SerializeField] GameObject online;
  [SerializeField] GameObject lan;
  [SerializeField] GameObject hostLan;
  [SerializeField] GameObject joinLan;

  [SerializeField] InputField nicknameInput;

  [SerializeField] InputField portInput;
  [SerializeField] InputField serverAddressInput;

  [SerializeField] HostGame hostGame;
  [SerializeField] JoinGame joinGame;

  void Start() {
    Goto("Main");

    nicknameInput.text = PlayerPrefs.GetString("Nickname");

    portInput.text = PlayerPrefs.GetInt("port").ToString();
    serverAddressInput.text = PlayerPrefs.GetString("serverAddress");
  }

  public void Goto(string where) {
    if (where == "Main") {
      DisableAll();
      main.SetActive(true);
    } else if (where == "Online") {
      DisableAll();
      online.SetActive(true);
    } else if (where == "HostOnline") {
      DisableAll();
      hostOnline.SetActive(true);
    } else if (where == "JoinOnline") {
      DisableAll();
      joinOnline.SetActive(true);
    } else if (where == "LAN") {
      DisableAll();
      lan.SetActive(true);
    } else if (where == "HostLAN") {
      DisableAll();
      hostLan.SetActive(true);
    } else if (where == "JoinLAN") {
      DisableAll();
      joinLan.SetActive(true);
    } else if (where == "Nickname") {
      DisableAll();
      nickname.SetActive(true);
    } else if (where == "Exit") {
      Application.Quit();
    }
  }

  void DisableAll() {
    foreach (Transform child in transform) {
      if (child.name != "Background") {
        child.gameObject.SetActive(false);
      }
    }
  }

  public void SetNickname(string name) {
    PlayerPrefs.SetString("Nickname", name);
  }

  public void Host() {
    int port = int.Parse(portInput.text);
    hostGame.Host(port);
    PlayerPrefs.SetInt("port", port);
  }

  public void Connect() {
    joinGame.Connect(serverAddressInput.text);
    PlayerPrefs.SetString("serverAddress", serverAddressInput.text);
  }
}
