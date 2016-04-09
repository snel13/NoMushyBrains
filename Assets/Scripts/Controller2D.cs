using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

    BoxCollider2D collider;
    
    void Start() {
        collider = GetComponent<BoxCollider2D>();
    }

}
