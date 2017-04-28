using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class RoomListItem : MonoBehaviour {
  // pointer to join function located in JoinGame script
  public delegate void JoinGameDelegate(MatchInfoSnapshot info);
  JoinGameDelegate joinGameDelegate;

  MatchInfoSnapshot matchInfo;

  public void Init(MatchInfoSnapshot info, JoinGameDelegate _joinGameDelegate) {
    matchInfo = info;
    joinGameDelegate = _joinGameDelegate;
  }

  public void Join() {
    joinGameDelegate.Invoke(matchInfo);
  }
}
