using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {
    
    float moveSpeed = 6;
    float gravity = -20;
    // use struct to pass positions and directions around 
    Vector3 velocity;


    Controller2D controller;
    void Start() {
        controller = GetComponent<Controller2D> ();
    }

    void Update() {
        // use struct to represent 2D positions and vectors
        // Input is the class to read the axes from the keyboard, multi-touch (mobile) or mouse
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        velocity.x = input.x * moveSpeed;
        // Time is the class that gets the time information
        // deltaTime is the time in seconds it took to complete the last frame (read only) 
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}