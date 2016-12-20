using UnityEngine;
using System.Collections;

public class MissionData : MonoBehaviour {

	public int MinPointsPerAction;
	public int MaxPointPerAction;

	public float MinSpawnTimer;
	public float MaxSpawnTimer;

	public int[] StarGoals;

	public int[] IngredientCounts;

	public Variation MyVariation;

	public void InitializeFromMissionData(MissionData missionData) {
		MinPointsPerAction = missionData.MinPointsPerAction;
		MaxPointPerAction = missionData.MaxPointPerAction;
		MinSpawnTimer = missionData.MinSpawnTimer;
		MaxSpawnTimer = missionData.MaxSpawnTimer;
		StarGoals = new int[missionData.StarGoals.Length];
		missionData.StarGoals.CopyTo (StarGoals, 0);
		IngredientCounts = new int[missionData.IngredientCounts.Length];
		missionData.IngredientCounts.CopyTo (IngredientCounts, 0);
		MyVariation = missionData.MyVariation;
	}
}

public enum Variation {
	classic,
	memory,
	doubleTrouble,
	sixChoices,
	rotatingHints,
	sixChoicesRotating,
	shadowplay
}
