using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerUI : NetworkBehaviour {
  HUDController hudController;
  GameManager gameManager;

  Player player;
  PlayerController controller;

  Text health;
  Text roundtime;

  void Start() {
    if (!isLocalPlayer)
      return;

    hudController = HUDController.instance;
    hudController.playerController = GetComponent<PlayerController>();
    gameManager = GameManager.instance;
    player = GetComponent<Player>();
    controller = GetComponent<PlayerController>();
    health = hudController.hud.transform.FindChild("Health").GetComponent<Text>();
    roundtime = hudController.hud.transform.FindChild("Round time").GetComponent<Text>();

    hudController.hud.SetActive(false);

    hudController.deathScreen.SetActive(false);

    // show team choose menu and bind buttons
    GetComponent<PlayerController>().enabled = false;

    hudController.teamChoose.SetActive(true);
    hudController.teamChoose.transform.FindChild("Hiders").GetComponent<Button>().onClick.AddListener(ChooseHidersTeamListener);
    hudController.teamChoose.transform.FindChild("Seekers").GetComponent<Button>().onClick.AddListener(ChooseSeekersTeamListener);
  }

  [ClientCallback]
	void Update () {
    if (!isLocalPlayer)
      return;

    if (gameManager.isRoundOn) {
      string minutes = (((int)gameManager.roundTime) / 60 % 60).ToString();
      string seconds = (((int)gameManager.roundTime) % 60).ToString();

      roundtime.text = "ROUND TIME:\n" + minutes + "m " + seconds + "s";

      if (player.team == Player.Team.Seekers && !gameManager.allowSeekers) {
        hudController.blackScreenForSeekers.SetActive(true);
      } else {
        hudController.blackScreenForSeekers.SetActive(false);
      }

      if (player.team == Player.Team.Hiders) {
        health.text = "Health: " + player.health;
      } else if (player.team == Player.Team.Seekers) {
        health.text = "Ammuniton: " + player.health;
      }
    } else {
      roundtime.text = "WAITING FOR PLAYERS";
      hudController.blackScreenForSeekers.SetActive(false);
      hudController.deathScreen.SetActive(false);
    }

		if (Input.GetKeyDown(KeyCode.Escape)) {
      hudController.ShowDisconnectWindow();
    }

    if (Input.GetKeyDown(KeyCode.Tab)) {
      hudController.scoreboard.ShowScoreboard();
    }

    if (Input.GetKeyUp(KeyCode.Tab)) {
      hudController.scoreboard.HideScoreboard();
    }
	}

  void ChooseHidersTeamListener() {
    hudController.teamChoose.SetActive(false);
    player.CmdSetTeam(Player.Team.Hiders);
    player.CmdSetReadyStatus(true);
    GetComponent<PlayerController>().enabled = true;
    hudController.hud.SetActive(true);

    GetComponent<PlayerSpawner>().Spawn();
  }
  
  void ChooseSeekersTeamListener() {
    hudController.teamChoose.SetActive(false);
    player.CmdSetTeam(Player.Team.Seekers);
    player.CmdSetReadyStatus(true);
    GetComponent<PlayerController>().enabled = true;
    hudController.hud.SetActive(true);

    GetComponent<PlayerSpawner>().Spawn();
  }

  public void ShowDeathScreen(string textToShow) {
    hudController.deathScreen.SetActive(true);
    hudController.deathScreen.transform.GetChild(0).GetComponent<Text>().text = textToShow;

    StartCoroutine(HideDeathScreen());
  }

  IEnumerator HideDeathScreen() {
    yield return new WaitForSeconds(3.0f);

    hudController.deathScreen.SetActive(false);
  }
}
