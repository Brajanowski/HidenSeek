using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveObject : MonoBehaviour {
  public float timeToDestroy = 0.5f;

  public void BeginDestroying() {
    StartCoroutine(Destroy());
  }

  IEnumerator Destroy() {
    yield return new WaitForSeconds(timeToDestroy);
    Destroy(gameObject);
  }
}
