using UnityEngine;
using System.Collections;
using UnityEngine.UI; //public Text timerText

public class myTimer : MonoBehaviour {
	
	public float myCoolTimer = 99;
	private Text timerText;

	float minutes;
	float seconds;
	string format;

	// Use this for initialization
	void Start () {
		timerText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		myCoolTimer -= Time.deltaTime; //+= to count up
		minutes = Mathf.FloorToInt(myCoolTimer/60);
		seconds = myCoolTimer - minutes*60;
		format = string.Format("{0:0}:{1:00}", minutes, seconds); //H:M:SSSS
		timerText.text = "Timer: " + format;
	}
}
