using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAudio : NetworkBehaviour {
  public AudioClip shoot;

  AudioSource audioSource;

  void Awake() {
    audioSource = GetComponent<AudioSource>();
  }

  [Command]
  public void CmdPlayShootEffect() {
    RpcPlayShootEffect();
  }

  [ClientRpc]
  void RpcPlayShootEffect() {
    audioSource.clip = shoot;
    audioSource.Play();
  }
}
