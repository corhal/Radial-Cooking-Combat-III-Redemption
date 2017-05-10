using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissionData : MonoBehaviour {

	public int MinPointsPerAction;
	public int MaxPointPerAction;
	public int TrashIngredientsCount;

	public float InitialSpawnTimer;
	//public float MinSpawnTimer;
	//public float MaxSpawnTimer;

	public int[] StarGoals;

	public int[] IngredientCounts;

	public DishData[] DishDatas;

	public List<Variation> Variations;
	public List<Booster> Boosters;
	public List<float> BoosterLifetimes;
	public List<int> BoosterCounts;

	public void InitializeFromMissionData(MissionData missionData) {
		TrashIngredientsCount = missionData.TrashIngredientsCount;
		MinPointsPerAction = missionData.MinPointsPerAction;
		MaxPointPerAction = missionData.MaxPointPerAction;
		InitialSpawnTimer = missionData.InitialSpawnTimer;
		//MinSpawnTimer = missionData.MinSpawnTimer;
		//MaxSpawnTimer = missionData.MaxSpawnTimer;
		StarGoals = new int[missionData.StarGoals.Length];
		missionData.StarGoals.CopyTo (StarGoals, 0);
		IngredientCounts = new int[missionData.IngredientCounts.Length];
		missionData.IngredientCounts.CopyTo (IngredientCounts, 0);
		DishDatas = new DishData[missionData.DishDatas.Length];
		missionData.DishDatas.CopyTo (DishDatas, 0);
		Variations = new List<Variation> (missionData.Variations);
		Boosters = new List<Booster> (missionData.Boosters);
		BoosterLifetimes = new List<float> (missionData.BoosterLifetimes);
		BoosterCounts = new List<int> (missionData.BoosterCounts);
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
	goldhunt,
	crookedMan
}

public enum Booster {
	none,
	candle,
	winter
}
