using UnityEngine;
using System.Collections;
using UnityEngine.UI; //public Text timerText

public class myTimer : MonoBehaviour {
	
	public float myCoolTimer = 99;
	private Text timerText;
	
	string format;
	float minutes, seconds;

	// Use this for initialization
	void Start () {
		timerText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () { //https://msdn.microsoft.com/en-us/library/txafckwd.aspx
		myCoolTimer -= Time.deltaTime; //+= to count up
		//see if the game is over
		if(myCoolTimer <= 0){
			print("GAME OVER\n");
			timerText.text = "Timer: 0:00";
            Application.Quit();
			//SceneManager.LoadScene("Name of End Game Scene");
		}
        else if(Input.GetKeyDown("p")){
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
            }
        }

        else {
			minutes = Mathf.FloorToInt(myCoolTimer/60);
			seconds = myCoolTimer - minutes*60; 
			format = string.Format("{0:0}:{1:00}", minutes, seconds); //H:M:SSSS
			timerText.text = "Timer: " + format;			
		}
        

	}
}
