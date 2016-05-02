using UnityEngine;
using System.Collections;
//for networking
using UnityEngine.Networking;


[RequireComponent (typeof (Controller2D))]
//was monobehaviour
public class Player : NetworkBehaviour {
    
	//affect gravity and jump velocity
	public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
	public float timeToJumpApex = .4f; //.4 of a second
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
    float moveSpeed = 6;

    // three Vector2 - represent the type of wall interactions
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    // to add stick when jumping on walls
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    float gravity;
	float jumpVelocity;
    float minJumpVelocity;
    // use struct to pass positions and directions around 
    Vector3 velocity;
	float velocityXSmoothing; //for x movement

    Controller2D controller;
    void Start() {
        controller = GetComponent<Controller2D> ();
		//alter physics
		gravity = -(2*maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
		print("Gravity: " + gravity + " Jump Velocity: " + jumpVelocity);
    }

    void Update() {
        //for network
        if(!isLocalPlayer)
        {
            return;
        }

        
        // use struct to represent 2D positions and vectors
        // Input is the class to read the axes from the keyboard, multi-touch (mobile) or mouse
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        int wallDirX = (controller.collisions.left) ? -1 : 1;

        float targetVelocityX = input.x * moveSpeed;
        //SmoothDamp function lookup
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        bool wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
            wallSliding = true;
            if(velocity.y < -wallSlideSpeedMax){
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0){
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (input.x != wallDirX && input.x != 0) {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else {
                timeToWallUnstick = wallStickTime;
            }
        }
        
        //if we're standing on something and player hits space, we jump
		if(Input.GetKeyDown(KeyCode.Space)){
            if (wallSliding){
                if(wallDirX == input.x){
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if(input.x == 0){
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }

            }
            if (controller.collisions.below){
                velocity.y = jumpVelocity;
            }
           
		}
        if (Input.GetKeyUp(KeyCode.Space)){
            if(velocity.y > minJumpVelocity){
                velocity.y = minJumpVelocity;               
            }
        }
       
        // Time is the class that gets the time information
        // deltaTime is the time in seconds it took to complete the last frame (read only) 
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime, input);
        //fall off more elegantly
        if (controller.collisions.above || controller.collisions.below){ 
			//reset velocity on the y axis
			velocity.y = 0;
		}
    }
}