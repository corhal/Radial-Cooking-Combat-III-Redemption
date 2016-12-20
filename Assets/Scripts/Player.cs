using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public bool FirstTime = true;
	public bool HasWon;
	public int StarCount;
	public static Player instance;
	public MissionData MyMission;

	void Awake() {
		if (instance == null) {
			//if not, set instance to this
			instance = this;
		}

		//If instance already exists and it's not this:
		else if (instance != this) {

			//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
			Destroy (gameObject);  
		}

		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);
	}

	public void SetMission(MissionData missionData) {
		MyMission.InitializeFromMissionData (missionData);
	}
}
