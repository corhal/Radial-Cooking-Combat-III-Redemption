using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuController : MonoBehaviour {

	public Text wonLabel;

	void Awake() {
		if (Player.instance.FirstTime) {
			wonLabel.gameObject.SetActive (false);
		}
		if (Player.instance.HasWon) {
			wonLabel.text = "You won and got " + Player.instance.StarCount + " stars!";
		} else {
			wonLabel.text = "You lost";
		}
	}

	public void ChangeScene(int scene) {
		SceneManager.LoadScene (1);
	}

	public void SetMission(MissionData missionData) {
		Player.instance.MyMission.InitializeFromMissionData (missionData);
	}
}
