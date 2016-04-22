using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    
    public Controller2D target;
    public float verticalOffset;
    public float lookAheadDistanceX;
    public float lookSmoothTimeX;
    public float verticalSmoothTime;
    public Vector2 focusAreaSize;

    //create a camera variable
    FocusArea focusArea;
    
    float currentLookAheadX;
    float targetLookAheadX;
    float lookAheadDirX;
    float smoothLookVelocityX;
    float smoothVelocityY;
    
    bool lookAheadStopped;

    //setup bounds of camera follow (focusArea)
    //player is always the focal point of the camera
    void Start(){
        focusArea = new FocusArea(target.collider.bounds, focusAreaSize);
    }
    
    //used for updating the camera after all player movement has finished
    void LateUpdate(){
        focusArea.Update(target.collider.bounds);
        
        Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;
        
        if(focusArea.velocity.x != 0){
            lookAheadDirX = Mathf.Sign(focusArea.velocity.x);    
            //we only want to set our look ahead direction if our target is currently moving in same direction
            if(Mathf.Sign(target.playerInput.x) == Mathf.Sign(focusArea.velocity.x) && target.playerInput.x !=0){
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDistanceX;
            }    
            else{
                if(!lookAheadStopped){
                    lookAheadStopped = true;
                    targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistanceX - currentLookAheadX)/4f;
                }
                
            }    
        }
        
       
        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);
        
        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
        
        focusPosition += Vector2.right * currentLookAheadX;
        
        //-10 forces camera to be in front of level on the Z axis (jumping out of screen)
        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }
   
    //used for testing
    void OnDrawGizmos(){
        //1 for Red, 0 for Green, 0 for blue, .5 for transparency 
        Gizmos.color = new Color (1, 0, 0, .5f);
        Gizmos.DrawCube(focusArea.center, focusAreaSize);
    }
    
    //keeps track of the center of the focus area along with all of its corners
    struct FocusArea{
        public Vector2 center;
        public Vector2 velocity;
        float left, right;
        float top, bottom;
        
        //takes in bounding box around player and a vector for the size
        //http://docs.unity3d.com/ScriptReference/Bounds.html
        public FocusArea(Bounds targetBounds, Vector2 size){
            left = targetBounds.center.x - size.x/2;
            right = targetBounds.center.x + size.x/2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y + size.y;
            
            velocity = Vector2.zero;
            center = new Vector2((left+right)/2,(top + bottom)/2);
        }
        
        //update the focus area position when target moves against the edges of the bounds object
        public void Update(Bounds targetBounds){
            float shiftX = 0;
            //checks to see if the target is moving against the left or right edges which means the camera should be moved to keep up
            if(targetBounds.min.x < left){
                shiftX = targetBounds.min.x - left;
            } else if (targetBounds.max.x > right){
                shiftX = targetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;
            
            float shiftY = 0;
            //checks to see if the target is moving against the top or bottom edges which means the camera should be moved to keep up
            if(targetBounds.min.y < bottom){
                shiftY = targetBounds.min.y - bottom;
            } else if (targetBounds.max.y > top){
                shiftY = targetBounds.max.y - top;
            }
            top += shiftY;
            bottom += shiftY;
            center = new Vector2((left+right)/2,(top + bottom)/2);    
            velocity = new Vector2(shiftX, shiftY);
                    
        }
    }
}
