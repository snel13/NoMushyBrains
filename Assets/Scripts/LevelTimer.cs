using UnityEngine;
using System.Collections;
using UnityEngine.UI; //public Text timerText

public class LevelTimer : MonoBehaviour {
	
	public float levelOneTimer = 99;
	private Text timerText;
	
	string format;
	float minutes, seconds;

	// Use this for initialization
	void Start () {
		timerText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () { //https://msdn.microsoft.com/en-us/library/txafckwd.aspx
		levelOneTimer -= Time.deltaTime; //+= to count up
		//see if the game is over
		if(levelOneTimer <= 0){
			print("GAME OVER\n");
			timerText.text = "Timer: 0:00";
            // new WaitForSeconds(10f);
            Application.Quit();
            // Invoke("Application.Quit", .5f);
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
			minutes = Mathf.FloorToInt(levelOneTimer/60);
			seconds = levelOneTimer - minutes*60; 
			format = string.Format("{0:0}:{1:00}", minutes, seconds); //H:M:SSSS
			timerText.text = "Timer: " + format;			
		}
        

	}
}
