using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {
    
	//affect gravity and jump velocity
	public float jumpHeight = 4;
	public float timeToJumpApex = .4f; //.4 of a second
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
    float moveSpeed = 6;
    
	float gravity;
	float jumpVelocity;
    // use struct to pass positions and directions around 
    Vector3 velocity;
	float velocityXSmoothing; //for x movement

    Controller2D controller;
    void Start() {
        controller = GetComponent<Controller2D> ();
		//alter physics
		gravity = -(2*jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		print("Gravity: " + gravity + " Jump Velocity: " + jumpVelocity);
    }

    void Update() {
		if(controller.collisions.above || controller.collisions.below){ //fall off more elegantly
			//reset velocity on the y axis
			velocity.y = 0;
		}
        // use struct to represent 2D positions and vectors
        // Input is the class to read the axes from the keyboard, multi-touch (mobile) or mouse
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		
		//if we're standing on something and player hits space, we jump
		if(Input.GetKeyDown(KeyCode.Space) && controller.collisions.below){
			velocity.y = jumpVelocity;
		}
        float targetVelocityX = input.x * moveSpeed;
		//SmoothDamp function lookup
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
        // Time is the class that gets the time information
        // deltaTime is the time in seconds it took to complete the last frame (read only) 
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}