using UnityEngine;
using System.Collections;


public class Controller2D : RaycastController {
	float maxClimbAngle = 80;
    float maxDescendAngle = 75;
	
    public CollisionInfo collisions; //public reference to our collision info
    
    public override void Start(){
        base.Start();
        collisions.faceDir = 1;
    }

    // use Move function to keep track of the ray casts
    public void Move(Vector3 velocity, bool standingOnPlatform = false){
        UpdateRaycastOrigins();
		collisions.Reset(); //blank slate each time
        collisions.velocityOld = velocity;

        if (velocity.x != 0){
            collisions.faceDir = (int)Mathf.Sign(velocity.x);
        }

        if (velocity.y < 0){
            DescendSlope(ref velocity);
        }
       
            HorizontalCollisions(ref velocity);
        
        if (velocity.y != 0){
            VerticalCollisions(ref velocity);
        }
        transform.Translate(velocity);
        
        if(standingOnPlatform){
            collisions.below = true;
        }

    }

    // HorizontalCollisions function uses the RaycastHit2D struct to detect objects in the raycast area (right to left)
    void HorizontalCollisions(ref Vector3 velocity) {
        float directionX = collisions.faceDir;
        // make sure length is a positive number
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;
            
        if (Mathf.Abs(velocity.x) < skinWidth){
            rayLength = 2 * skinWidth;
        }
        for (int i = 0; i < horizontalRayCount; i++){
            // see what direction we are moving in
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            // used for debugging, draws the rays in red
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit){
                
                if(hit.distance == 0){
                    continue;
                }
                
				//angled movement
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if(i == 0 && slopeAngle <= maxClimbAngle){
                    if(collisions.descendingSlope){
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
					float distanceToSlopeStart = 0;
					//starting to climb a new slope 
					if(slopeAngle != collisions.slopeAngleOld){
						distanceToSlopeStart = hit.distance-skinWidth;
						velocity.x -= distanceToSlopeStart * directionX; //only uses velocity x that it has once it reaches the slope
					}
					ClimbSlope(ref velocity, slopeAngle);//print(slopeAngle);
					velocity.x += distanceToSlopeStart * directionX; //add back on
				}
				//check other rays if we're not climbing slope
				if(!collisions.climbingSlope || slopeAngle > maxClimbAngle){
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;
					//update velocity on y axis
					if(collisions.climbingSlope){
						velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}
					//set depending on the direction
					collisions.left = directionX == -1; //if we've hit something and we're going left then this will be true
					collisions.right = directionX == 1;					
				}
            }
        }
    }

    // VerticalCollisions function uses the RaycastHit2D struct to detect objects in the raycast area (bottom to top)
    void VerticalCollisions(ref Vector3 velocity){
        float directionY = Mathf.Sign(velocity.y);
        // make sure length is a positive number
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;
        for (int i = 0; i < verticalRayCount; i++){
            // see what direction we are moving in
            Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            // used for debugging, draws the rays in red
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit){ 
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
				
				if(collisions.climbingSlope){
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}
				collisions.below = directionY == -1; 
				collisions.above = directionY == 1;
            }
        }
        if(collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            
            if(hit){
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle != collisions.slopeAngle){
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

	void ClimbSlope(ref Vector3 velocity, float slopeAngle){
		//figure out new velocity x and y for slopes 
		float moveDistance = Mathf.Abs(velocity.x);
		float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
		if(velocity.y <= climbVelocityY){
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
			collisions.below = true; //fix jumping	
			collisions.climbingSlope = true; //set to true
			collisions.slopeAngle = slopeAngle;
		}
	}
    
    void DescendSlope(ref Vector3 velocity){
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomRight:raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
        if(hit){
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if(slopeAngle != 0 && slopeAngle <= maxDescendAngle){
                if(Mathf.Sign(hit.normal.x) == directionX){
                    if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad)*Mathf.Abs(velocity.x)){
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVecolityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVecolityY;
                        
                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

	public struct CollisionInfo{
		public bool above, below;
		public bool left, right;
		
		public bool climbingSlope;
        public bool descendingSlope;
		public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;
        // 1 means faing left, -1 means facing right
        public int faceDir;
		
		//reset bools to false
		public void Reset(){
			above = below = false;
			left = right = false;
			climbingSlope = false;
            descendingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
       
}
