﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {
    // create a mask that rays will interact with
    public LayerMask collisionMask;

    // skin width is the padding around the game object where the rays emit from
    // padding is inset so object can rest on platform 
    const float skinWidth= .015f;
    // number of rays emitting from player
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
   
    float horizontalRaySpacing;
    float verticalRaySpacing;
    // class BoxCollider2D is a collider for 2D physics representing an axis-aligned rectangle
    BoxCollider2D collider;
    // custom struct that holds the origin values for the player rayCast
    RaycastOrigins raycastOrigins;
    
    // Monobehaviour Start is called on the frame when a script is enabled, before any update methods are called
    void Start() {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }
     
    // use Move function to keep track of the ray casts
    public void Move(Vector3 velocity){
        UpdateRaycastOrigins();
        if (velocity.x != 0)
        {
            HorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }
        transform.Translate(velocity);

    }

    // HorizontalCollisions function uses the RaycastHit2D struct to detect objects in the raycast area (right to left)
    void HorizontalCollisions(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        // make sure length is a positive number
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;
        for (int i = 0; i < horizontalRayCount; i++)
        {
            // see what direction we are moving in
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            // used for debugging, draws the rays in red
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit)
            {
                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;
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
            }
        }
    }

    // UpdateRaycastOrigins function is used in Move() function to detect space around player
    void UpdateRaycastOrigins() {
        // Bounds struct represents an axis aligned bounding box, this box "overlays" the player
        // Expandincreases the size by a given amount along  each side
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);
        // set the raycastOrigins struct's to the bounds values
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x,bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x,bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x,bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x,bounds.max.y);
    }
    
    // CalculateRaySpacing function is used in Move() function to determine ray spacing
    void CalculateRaySpacing() {
        // once again set up Bounds struct to represent the bounding box
        Bounds bounds = collider.bounds;
        bounds.Expand (skinWidth * -2);
        // use Clamp() to clamp a value between min and max 
        // clamp the value of horizontalRayCount/verticalRayCount between 2 and 2,147,483,647
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);
        
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        
    }
    
    // struct used in UpdateRayCastOrigins to set the bounds to the player object
    struct RaycastOrigins{
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
       
}
