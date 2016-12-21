﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Dish : MonoBehaviour {

	public int DishIndex;
	public bool IsActive;
	public GameObject ingredientsObj;
	public SpriteRenderer DishSprite;
	public List<SpriteRenderer> IngredientSprites;
	public List<SpriteRenderer> ingredientSprites;

	public int totalIngredients;
	public int collectedIngredients;

	public int MyDish;
	public List<int> Ingredients;
	public List<int> ingredients;

	public List<Text> ingredientCountLabels;
	public List<SpriteRenderer> HelperSprites;

	public Text ScoreLabel;

	public Dictionary<int, int> IngredientConditionsDict;

	public Dictionary<int, int> IngredientItems;

	public Dictionary<int, int> IngredientsCounts;

	//public static Dish instance;
	public int minIngredients;

	Animation MyAnimation;
	public Animation IngredientsAnimation;
	public float waitTime;
	float timer;
	Storage storage;
	bool allArrived;

	public bool IsReady;

	public delegate void DishReadyEventHandler (Dish dish);
	public static event DishReadyEventHandler OnDishReady;

	public delegate void DishInitializedEventHandler (Dish dish);
	public static event DishInitializedEventHandler OnDishInitialized;

	/*public delegate void DishSwitchEventHandler (Dish dish);
	public static event DishSwitchEventHandler OnDishSwitch;*/

	public void Animate() {
		MyAnimation.Play ();
	}

	void Awake() {
		storage = GameObject.FindGameObjectWithTag ("Player").GetComponent<Storage> ();
		//instance = this;

		MyAnimation = GetComponentInChildren<Animation> ();

		IngredientConditionsDict = new Dictionary<int, int> ();
		IngredientsCounts = new Dictionary<int, int> ();
		IngredientItems = new Dictionary<int, int> ();
		Ingredients = new List<int> ();
	}

	void Start() {			
		DishIndex = GameController.instance.dishCount;
		minIngredients = Player.instance.MyMission.IngredientCounts[DishIndex];
		DishSprite.sprite = storage.DishSprites [DishIndex];
		DishSprite.transform.position = GameController.instance.DishSpritesSpawns [DishIndex].position;
		if (DishIndex >= 3) {
			gameObject.SetActive (false);
		}

		int maxIngredients = Player.instance.MyMission.IngredientCounts[DishIndex];

		int length = Random.Range (minIngredients, maxIngredients);
		totalIngredients = 0;
		List<Sprite> AllowedIngredientSprites = new List<Sprite> (storage.IngredientSmallSprites);
		List<Sprite> UnallowedIngredientSprites = new List<Sprite> ();
		if (Player.instance.MyMission.MyVariation == Variation.shadowplay) {
			UnallowedIngredientSprites.Add (AllowedIngredientSprites[9]);
			UnallowedIngredientSprites.Add (AllowedIngredientSprites[7]);
			UnallowedIngredientSprites.Add (AllowedIngredientSprites[2]);
		}
		for (int i = 0; i < length; i++) {
			int uniqueIngredient = Random.Range(0, AllowedIngredientSprites.Count);
			while (IngredientConditionsDict.ContainsKey(uniqueIngredient) || UnallowedIngredientSprites.Contains(AllowedIngredientSprites[uniqueIngredient])) {
				uniqueIngredient = Random.Range(0, AllowedIngredientSprites.Count);
			}
			IngredientConditionsDict.Add (uniqueIngredient, Random.Range(2, 2));
			IngredientsCounts.Add (uniqueIngredient, 0);
			Ingredients.Add(uniqueIngredient);
			IngredientSprites [i].gameObject.SetActive (true);
			IngredientSprites [i].sprite = AllowedIngredientSprites [uniqueIngredient];
		}
		for (int i = 0; i < length; i++) {
			int uniqueItem = Random.Range(0, storage.ItemSprites.Length);
			while (IngredientItems.ContainsValue(uniqueItem)) {
				uniqueItem = Random.Range(0, storage.ItemSprites.Length);
			}

			float noChance;
			if (DishIndex == 1) {
				noChance = 0.5f;
			} else {
				noChance = 0.4f;
			}

			if (Random.Range (0.0f, 1.0f) <= noChance) {
				uniqueItem = -1;
			}
			IngredientItems.Add (Ingredients [i], uniqueItem);

			if (uniqueItem != -1) {
				HelperSprites [i].sprite = storage.ItemSprites [uniqueItem];
			} else {
				HelperSprites [i].sprite = null;
				HelperSprites [i].gameObject.SetActive (false);
			}
		}
		foreach (var condition in IngredientConditionsDict) {
			totalIngredients += condition.Value;
		}
		RefreshLabels ();
		if (Player.instance.MyMission.MyVariation == Variation.memory) {
			initTimer = 6.0f;
			GameController.instance.timer = 0.0f;
			GameController.instance.SpawnTime = initTimer;
		} else {
			GameController.instance.timer = 0.0f;
			GameController.instance.SpawnTime = 1.75f;
		}

		startInitialize = true;

		ingredients = new List<int>(Ingredients);
		ingredientSprites = new List<SpriteRenderer>(IngredientSprites);
		interpolators = new List<float> ();
		shouldMoves = new List<bool> ();
		for (int i = 0; i < ingredients.Count; i++) {
			shouldMoves.Add (false);
			interpolators.Add (0.0f);
			initialPositions.Add(ingredientSprites [i].transform.position);
			destinationPositions.Add(DishSprite.gameObject.transform.position);
		}

		InitialCookPosition = GameController.instance.Cook.transform.position;
		DestinationCookPosition = new Vector3 (DishSprite.gameObject.transform.position.x, DishSprite.gameObject.transform.position.y, DishSprite.gameObject.transform.position.z + 0.5f);
		interpolator = 0.0f;
		moveCook = true;

		GameController.instance.dishCount++;
		if (GameController.instance.dishCount == 3) {
			GameController.instance.dishCount = 0;
		}

		if (Player.instance.MyMission.MyVariation == Variation.shadowplay) {
			for (int i = 0; i < IngredientSprites.Count; i++) {
				IngredientSprites[i].color = Color.black;
				HelperSprites[i].color = Color.black;
			}
		}
	}

	public List<Vector2> initialPositions;
	public List<Vector2> destinationPositions;
	public List<float> interpolators;
	List<bool> shouldMoves;
	public bool ending;
	public float Speed;

	Vector2 InitialCookPosition;
	Vector2 DestinationCookPosition;
	float interpolator;
	bool moveCook;
	bool startInitialize;
	float initTimer;

	public void SetActive(bool active) {
		IsActive = active;
		if (active) {
			DishSprite.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
			InitialCookPosition = GameController.instance.Cook.transform.position;
			interpolator = 0.0f;
			moveCook = true;
		} else if (!IsReady) {
			DishSprite.color = new Color (1.0f, 1.0f, 1.0f, 0.5f);
		}
		if (allArrived) {
			IngredientsAnimation.gameObject.SetActive (active);
		}
	}

	void Update() {
		if (!IsActive) {
			return;
		}
		timer += Time.deltaTime;
		if (!allArrived && timer >= waitTime) {
			IngredientsAnimation.gameObject.SetActive (true);
			allArrived = true;
			IngredientsAnimation.gameObject.SetActive (true);
			IngredientsAnimation.Play ();
		}
		if (moveCook) {
			GameController.instance.Cook.transform.position = Vector2.Lerp (InitialCookPosition, DestinationCookPosition, interpolator);
			interpolator += Speed * Time.deltaTime;

			if (interpolator >= 1.0f) {
				moveCook = false;
			}
		}
		int readyCount = 0;
		for (int i = 0; i < shouldMoves.Count; i++) {
			if (shouldMoves[i]) {					
				ingredientSprites[i].gameObject.transform.position = Vector2.Lerp (initialPositions[i], destinationPositions[i], interpolators[i]);
				interpolators[i] += Speed * Time.deltaTime;

				if (interpolators[i] >= 1.0f) {
					ingredientSprites [i].gameObject.SetActive (false);
					readyCount++;
				}

				if (readyCount == ingredients.Count) {		
					IsReady = true;
					OnDishReady (this);
				}
			}
		}

		if (startInitialize) {
			initTimer -= Time.deltaTime;
			GameController.instance.MemoryTimerLabel.text = (int)initTimer + "...";
			if (initTimer <= 4.0f) {
				GameController.instance.MemoryTimerLabel.gameObject.SetActive (true);
			}
			if (initTimer <= 0.0f) {
				GameController.instance.MemoryTimerLabel.gameObject.SetActive (false);
				if (Player.instance.MyMission.MyVariation == Variation.memory) {
					IngredientsAnimation.gameObject.SetActive (false);
				}
				startInitialize = false;
				OnDishInitialized (this);
			}
		}
	}

	public void AddCorrect(Ingredient ingredient) {
		if (IngredientsCounts.ContainsKey(ingredient.IngredientType)) {
			IngredientsCounts [ingredient.IngredientType]++;
			collectedIngredients++;
			RefreshLabels ();
			if (IngredientsCounts [ingredient.IngredientType] == IngredientConditionsDict [ingredient.IngredientType]) {
				HelperSprites [Ingredients.IndexOf (ingredient.IngredientType)].color = new Color (1.0f, 1.0f, 1.0f, 0.0f);
				IngredientsSuckIn (ingredients.IndexOf (ingredient.IngredientType));
				ingredientCountLabels [Ingredients.IndexOf (ingredient.IngredientType)].color = new Color (1.0f, 1.0f, 1.0f, 0.0f);
				ingredientCountLabels.RemoveAt (Ingredients.IndexOf (ingredient.IngredientType));
				IngredientSprites.RemoveAt (Ingredients.IndexOf (ingredient.IngredientType));
				HelperSprites.RemoveAt (Ingredients.IndexOf (ingredient.IngredientType));
				Ingredients.Remove (ingredient.IngredientType);
				if (Ingredients.Count == 0) {					

				}
			}

			if (Player.instance.MyMission.MyVariation == Variation.switcheroo && Ingredients.Count > 0) {
				// GameController.instance.SwitchDish ();
			}
		}
	}

	void IngredientsSuckIn(int index) {	
		shouldMoves [index] = true;
	}

	public void AddIncorrect() {
	}

	void RefreshLabels() {
		for (int i = 0; i < Ingredients.Count; i++) {	
			ingredientCountLabels [i].gameObject.SetActive (true);
			ingredientCountLabels [i].text = IngredientsCounts [Ingredients [i]] + "/" + IngredientConditionsDict [Ingredients [i]];
		}
	}
}
