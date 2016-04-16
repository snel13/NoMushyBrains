using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {
    // create a mask that rays will interact with
    public LayerMask collisionMask;

    // skin width is the padding around the game object where the rays emit from
    // padding is inset so object can rest on platform 
    public const float skinWidth= .015f;
    // number of rays emitting from player
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    
    [HideInInspector]
	public float horizontalRaySpacing;
    public float verticalRaySpacing;
    // class BoxCollider2D is a collider for 2D physics representing an axis-aligned rectangle
    public BoxCollider2D collider;
    // custom struct that holds the origin values for the player rayCast
    public RaycastOrigins raycastOrigins;
    
    // Monobehaviour Start is called on the frame when a script is enabled, before any update methods are called
    public virtual void Start() {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    // UpdateRaycastOrigins function is used in Move() function to detect space around player
    public void UpdateRaycastOrigins() {
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
    public void CalculateRaySpacing() {
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
    public struct RaycastOrigins{
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
