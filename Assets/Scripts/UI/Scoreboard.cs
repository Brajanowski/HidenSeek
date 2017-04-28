using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour {
  public GameObject playerScoreboardItem;

  public GameObject seekersContent;
  public GameObject hidersContent;

  public GameObject mainObject;

  public Color aliveColor;
  public Color deathColor;
  public Color ourColor;

  void Start() {
    HideScoreboard();
  }

  public void ShowScoreboard() {
    mainObject.SetActive(true);
    GenerateScoreboard();
  }

  public void HideScoreboard() {
    mainObject.SetActive(false);
    foreach (Transform child in seekersContent.transform) {
      Destroy(child.gameObject);
    }

    foreach (Transform child in hidersContent.transform) {
      Destroy(child.gameObject);
    }
  }

  void GenerateScoreboard() {
    // hiders
    int count = 0;
    foreach (Player player in GameManager.GetPlayers()) {
      if (!player.isReady)
        continue;

      if (player.team != Player.Team.Hiders)
        continue;

      GameObject item = Instantiate(playerScoreboardItem);
      item.transform.SetParent(hidersContent.transform);

      RectTransform trans = item.GetComponent<RectTransform>();
      trans.offsetMin = new Vector2(15, trans.offsetMin.y);
      trans.offsetMax = new Vector2(15, trans.offsetMax.y);
      trans.anchoredPosition = new Vector2(0, -32 - (count * 48));

      Text nick = item.transform.FindChild("Nick").gameObject.GetComponent<Text>();

      if (player == GameManager.mainPlayer) {
        nick.color = ourColor;
      } else if (player.isDead) {
        nick.color = deathColor;
      } else {
        nick.color = aliveColor;
      }

      nick.text = player.nickname;
      item.transform.FindChild("Score").gameObject.GetComponent<Text>().text = player.score.ToString();

      count++;
    }

    // seekers
    count = 0;
    foreach (Player player in GameManager.GetPlayers()) {
      if (!player.isReady)
        continue;

      if (player.team != Player.Team.Seekers)
        continue;

      GameObject item = Instantiate(playerScoreboardItem);
      item.transform.SetParent(seekersContent.transform);

      RectTransform trans = item.GetComponent<RectTransform>();
      trans.offsetMin = new Vector2(15, trans.offsetMin.y);
      trans.offsetMax = new Vector2(15, trans.offsetMax.y);
      trans.anchoredPosition = new Vector2(0, -32 - (count * 48));

      Text nick = item.transform.FindChild("Nick").gameObject.GetComponent<Text>();

      if (player == GameManager.mainPlayer) {
        nick.color = ourColor;
      } else if (player.isDead) {
        nick.color = deathColor;
      } else {
        nick.color = aliveColor;
      }

      nick.text = player.nickname;

      item.transform.FindChild("Score").gameObject.GetComponent<Text>().text = player.score.ToString();

      count++;
    }
  }

}
