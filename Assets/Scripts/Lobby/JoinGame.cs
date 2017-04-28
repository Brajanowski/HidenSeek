using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using System.Collections.Generic;

public class JoinGame : MonoBehaviour {
  List<GameObject> roomList = new List<GameObject>();

  [SerializeField]
  Text status;

  [SerializeField]
  GameObject roomListItemPrefab;

  [SerializeField]
  Transform roomListParent;

  private NetworkManager networkManager;

  void Start() {
    networkManager = NetworkManager.singleton;

    if (networkManager.matchMaker == null) {
      networkManager.StartMatchMaker();
    }
  }

  public void ClearRoomList() {
    foreach (GameObject room in roomList) {
      Destroy(room);
    }

    roomList.Clear();
  }

  public void RefreshRoomList() {
    ClearRoomList();

    networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);

    status.text = "Loading room list...";
  }

  public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches) {
    status.text = "";

    if (!success) {
      status.text = "Failed on listing rooms...";
      return;
    }

    int count = 0;
    foreach (MatchInfoSnapshot match in matches) {
      GameObject item = Instantiate(roomListItemPrefab);
      item.SetActive(true);
      item.transform.SetParent(roomListParent);

      item.GetComponentInChildren<Text>().text = "Room: " + match.name + " | " + match.currentSize + "/" + match.maxSize;
      item.GetComponent<RoomListItem>().Init(match, Join);

      RectTransform trans = item.GetComponent<RectTransform>();
      trans.offsetMin = new Vector2(15, trans.offsetMin.y);
      trans.offsetMax = new Vector2(15, trans.offsetMax.y);
      trans.anchoredPosition = new Vector2(0, -32 - (count * 48));

      roomList.Add(item);

      count++;
    }

    if (roomList.Count == 0) {
      status.text = "There is no available rooms";
    }
  }

  public void Join(MatchInfoSnapshot matchInfo) {
    networkManager.matchMaker.JoinMatch(matchInfo.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
    ClearRoomList();
    status.text = "Connecting...";
  }

  public void Connect(string serverAddress) {
    // port is necessary
    if (!serverAddress.Contains(":"))
      return;

    string ipAddress = serverAddress.Split(':')[0];
    int port = int.Parse(serverAddress.Split(':')[1]);

    NetworkManager.singleton.networkAddress = ipAddress;
    NetworkManager.singleton.networkPort = port;
    
    NetworkManager.singleton.StartClient();
  }

  void OnError(NetworkMessage msg) {
    Debug.Log("There was a problem with connection");
  }
}
