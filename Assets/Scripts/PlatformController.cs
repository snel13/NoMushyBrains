using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {
   
    public LayerMask passengerMask;
    
    
    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;
    
    public float speed;
    // create bool to state if we want a cyclic platform behaviour
    public bool cyclic;
    public float waitTime;
    // clamp between 1 and 3 
    [Range(0,2)]
    public float easeAmount;

    // index of the global waypoint that we are moving away from
    int fromWaypointIndex;
    // percent is between 0 - 1 1 = 100%
    float percentBetweenWaypoints;
    float nextMoveTime;


    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();
    
	// Use this for initialization
	public override void Start () {
	    base.Start();
        globalWaypoints = new Vector3[localWaypoints.Length];
        for(int i = 0; i < localWaypoints.Length; i++){
            globalWaypoints[i] = localWaypoints[i] + transform.position;
            
        }
	}
	
	// Update is called once per frame
	void Update () {
        UpdateRaycastOrigins();
        
	    Vector3 velocity = CalculatePlatformMovement();
        
        CalculatePassengerMovement(velocity);
        
        MovePassengers(true);
        transform.Translate(velocity);
	    MovePassengers(false);
    }

    // function to depress the movement of the platform 
    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    // function will replace the move variable
    Vector3 CalculatePlatformMovement() {

        // adding a pause in the platform movement
        if (Time.time < nextMoveTime) {
            return Vector3.zero;
        }

        // reset to zero, otherwise platform will go out of bounds 
        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        // gets faster the closer to the waypoints
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        // we want to make sure we are clamping between 0 and 1 
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);
       
        // Lerp method is used to find a point between two end points (return the percent btwe waypoints
        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if(percentBetweenWaypoints >= 1) {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!cyclic){
                // check for edge case
                if (fromWaypointIndex >= globalWaypoints.Length - 1){
                    fromWaypointIndex = 0;
                    // reverse our array of way points, to retrace out steps
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }


        return newPos - transform.position;
    }

    void MovePassengers(bool beforeMovePlatform){
        foreach (PassengerMovement passenger in passengerMovement)
        {
            if(!passengerDictionary.ContainsKey(passenger.transform)){
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }
            if(passenger.moveBeforePlatform == beforeMovePlatform){
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }
    
    void CalculatePassengerMovement(Vector3 velocity){
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement> ();
        
        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);
        
        //veritcally moving platform
        if(velocity.y != 0){
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;
            
            for (int i = 0; i < verticalRayCount; i++){
                // see what direction we are moving in
                Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                
                //testing for a collision with the platform (on the platform) AND player is not traveling through the platform (meaning they jumped through the platform)
                if(hit && hit.distance != 0){
                    if(!movedPassengers.Contains(hit.transform)){
                        movedPassengers.Add(hit.transform);
                        float pushX = (directionY == 1)?velocity.x:0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                        
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY),directionY == 1,true));
                    }
                }
            }    
        }
        //horizontally moving platform
        if(velocity.x != 0){
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;
            for (int i = 0; i < horizontalRayCount; i++){
                // see what direction we are moving in
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                
                if(hit && hit.distance != 0){
                    if(!movedPassengers.Contains(hit.transform)){
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;
                        
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY),false,true));
                    }
                }
            }
        }
        
        //passenger on top of a horizontally or downward moving platform
        if(directionY == -1 || velocity.y == 0 && velocity.x != 0){
            float rayLength = skinWidth * 2;
            
            for (int i = 0; i < verticalRayCount; i++){
                // see what direction we are moving in
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
                
                if(hit && hit.distance != 0){
                    if(!movedPassengers.Contains(hit.transform)){
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;
                        
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY),true,false));
                    }
                }
            }    
        }
    }



    struct PassengerMovement{
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;
        
        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform){
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    void OnDrawGizmos(){
        if(localWaypoints != null){
            Gizmos.color = Color.red;
            float size = .3f;
                
            for(int i = 0; i < localWaypoints.Length; i++){
                Vector3 globalWaypointPos = (Application.isPlaying)? globalWaypoints[i]: localWaypoints[i] + transform.position;
                // gizmos used for showing where the platform will be moving between
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);                
            }
        }
    }
}