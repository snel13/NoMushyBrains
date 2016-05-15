using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerNetworkSetup : NetworkBehaviour {

    public override void OnStartLocalPlayer () {
        GetComponent <Controller2D> ().enabled  = true;
        GetComponent <Player> ().enabled = true;
    }
}