using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
  public static GameManager instance;

  void Awake() {
    if (instance == null)
      instance = this;
    else
      Debug.LogError("There is already GameManager on scene");

    //DontDestroyOnLoad(this);
  }

  enum EndRoundReason {
    Timeout,
    NoHiders,
    NoSeekers,
    NoPlayers,
    NewRound
  };

  [SyncVar] public float roundTime = 300.0f;
  [SyncVar] public bool isRoundOn = false;
  [SyncVar] public bool allowSeekers = true;

  public GameObject interactableObjects;
  public GameObject interactableObjectsPrefab;

  [ServerCallback]
  void Update() {
    if (!isServer) {
      return;
    }

    if (!isRoundOn) {
      if (GetPlayers().Length > 1) {
        int hidersNumber = 0;
        int seekersNumber = 0;

        foreach (Player player in GetPlayers()) {
          if (!player.isReady)
            continue;
          else if (player.team == Player.Team.Hiders)
            hidersNumber++;
          else if (player.team == Player.Team.Seekers)
            seekersNumber++;
        }

        if (hidersNumber > 0 && seekersNumber > 0)
          CmdEndRound(EndRoundReason.NewRound, false);
      }
    } else {
      if (GetPlayers().Length <= 1) {
        CmdEndRound(EndRoundReason.NoPlayers, false);
        return;
      }
      
      int aliveHiders = 0;
      int aliveSeekers = 0;

      foreach (Player player in GetPlayers()) {
        if (!player.isDead && player.isReady)
          if (player.team == Player.Team.Hiders)
            aliveHiders++;
          else if (player.team == Player.Team.Seekers)
            aliveSeekers++;
      }

      if (aliveHiders == 0) {
        CmdEndRound(EndRoundReason.NoHiders, true);
        CmdSwapTeams();
      } else if (aliveSeekers == 0) {
        CmdEndRound(EndRoundReason.NoSeekers, true);
        CmdSwapTeams();
      } else if (roundTime <= 0.0f) {
        CmdEndRound(EndRoundReason.Timeout, true);
        CmdSwapTeams();
      }

      if (roundTime <= 270.0f && !allowSeekers) {
        CmdAllowSeekers(true);
      }

      roundTime -= Time.deltaTime;
    }
  }
  
  [Command]
  void CmdAllowSeekers(bool allow) {
    allowSeekers = allow;
  }

  [Command]
  void CmdSwapTeams() {
    foreach (Player player in GetPlayers()) {
      Player.Team newTeam = player.team == Player.Team.Seekers ? Player.Team.Hiders : Player.Team.Seekers;

      player.CmdSetTeam(newTeam);
    }
  }

  [Command]
  void CmdEndRound(EndRoundReason reason, bool swapTeams) {
    roundTime = 300.0f;

    allowSeekers = false;
    //isRoundOn = true;

    if (reason == EndRoundReason.Timeout) {
      IncreasePointsForTeam(Player.Team.Hiders, 1, true);
    } else if (reason == EndRoundReason.NoHiders) {
      IncreasePointsForTeam(Player.Team.Seekers, 1, true);
    } else if (reason == EndRoundReason.NoSeekers) {
      IncreasePointsForTeam(Player.Team.Hiders, 1, true);
    } else if (reason == EndRoundReason.NoPlayers) {
      isRoundOn = false;
      allowSeekers = true;
    } else if (reason == EndRoundReason.NewRound) {
      isRoundOn = true;
    }

    if (swapTeams)
      CmdSwapTeams();

    CmdCallNewRound();

    NetworkServer.Destroy(interactableObjects);
    interactableObjects = Instantiate(interactableObjectsPrefab);
    NetworkServer.Spawn(interactableObjects);
  }

  void IncreasePointsForTeam(Player.Team team, int points, bool hasToBeAlive = false) {
    foreach (Player player in GetPlayers()) {
      if (hasToBeAlive) {
        if (!player.isDead)
          player.score += points;
      } else {
        player.score += points;
      }
    }
  }

  [Command]
  void CmdCallNewRound() {
    foreach (Player player in GetPlayers()) {
      player.CmdNewRound();
    }
  }

  #region RegisteringPlayer
  private static Dictionary<string, Player> players = new Dictionary<string, Player>();
  public static Player mainPlayer;

  public static void RegisterPlayer(string id, Player player) {
    string playerID = "Player " + id;
    players.Add(playerID, player);
    player.transform.name = playerID;
  }

  public static void UnregisterPlayer(string id) {
    players.Remove("Player " + id);
  }

  public static Player GetPlayer(string playerid) {
    Player p;
    players.TryGetValue("Player " + playerid, out p);
    return p;
  }

  public static Player[] GetPlayers() {
    return players.Values.ToArray();
  }
  #endregion
}
