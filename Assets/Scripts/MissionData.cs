using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissionData : MonoBehaviour {

	public int MinPointsPerAction;
	public int MaxPointPerAction;
	public int TrashIngredientsCount;

	public float MinSpawnTimer;
	public float MaxSpawnTimer;

	public int[] StarGoals;

	public int[] IngredientCounts;

	//public Variation MyVariation;
	public List<Variation> Variations;

	public void InitializeFromMissionData(MissionData missionData) {
		TrashIngredientsCount = missionData.TrashIngredientsCount;
		MinPointsPerAction = missionData.MinPointsPerAction;
		MaxPointPerAction = missionData.MaxPointPerAction;
		MinSpawnTimer = missionData.MinSpawnTimer;
		MaxSpawnTimer = missionData.MaxSpawnTimer;
		StarGoals = new int[missionData.StarGoals.Length];
		missionData.StarGoals.CopyTo (StarGoals, 0);
		IngredientCounts = new int[missionData.IngredientCounts.Length];
		missionData.IngredientCounts.CopyTo (IngredientCounts, 0);
		//MyVariation = missionData.MyVariation;
		Variations = new List<Variation>(missionData.Variations);
	}
}

public enum Variation {
	classic,
	memory,
	doubleTrouble,
	revolver,
	carousel,
	shadowplay,
	switcheroo,
	goldhunt
}
