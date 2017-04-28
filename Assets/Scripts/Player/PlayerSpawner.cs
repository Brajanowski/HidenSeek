using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSpawner : NetworkBehaviour {
  List<Transform> spawnablePoints = new List<Transform>();

  void Start() {
    foreach (Transform point in GameObject.Find("Spawn points").transform) {
      spawnablePoints.Add(point);
    }

    Random.InitState((int)System.DateTime.Now.Ticks);
  }

  public void Spawn() {
    transform.position = spawnablePoints[Random.Range(0, spawnablePoints.Count - 1)].position;
  }

}
