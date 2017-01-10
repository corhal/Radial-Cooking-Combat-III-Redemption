using UnityEngine;
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

	public Dictionary<int, Minigame> Minigames;

	public int ingredientsCount;

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

	public List<int> UnallowedIngredients;
	public List<Variation> Variations;

	public DishData dishData;

	public void Animate() {
		MyAnimation.Play ();
	}

	void Awake() {
		storage = GameObject.FindGameObjectWithTag ("Player").GetComponent<Storage> ();

		MyAnimation = GetComponentInChildren<Animation> ();

		IngredientConditionsDict = new Dictionary<int, int> ();
		IngredientsCounts = new Dictionary<int, int> ();
		IngredientItems = new Dictionary<int, int> ();
		Ingredients = new List<int> ();
		GameController.instance.OnBoosterStatusChanged += GameController_instance_OnBoosterStatusChanged;
	}

	void GameController_instance_OnBoosterStatusChanged (Booster booster, bool active) {
		if (Variations.Contains(Variation.shadowplay) && booster == Booster.candle) {
			if (active) {
				for (int i = 0; i < IngredientSprites.Count; i++) {
					IngredientSprites[i].color = Color.white;
					HelperSprites[i].color = Color.white;
				}
			} else {
				for (int i = 0; i < IngredientSprites.Count; i++) {
					IngredientSprites[i].color = Color.black;
					HelperSprites[i].color = Color.black;
				}
			}
		}
	}

	void InitializeDish () {
		if (dishData != null) {
			Variations = new List<Variation> (dishData.Variations);
		} else {
			Variations = new List<Variation> (Player.instance.MyMission.Variations);
		}

		DishIndex = GameController.instance.dishCount;
		ingredientsCount = (dishData == null) ? 4 : dishData.IngredientSpriteIndexes.Length;

		DishSprite.sprite = (dishData == null) ? storage.DishSprites [DishIndex] : storage.DishSprites [dishData.DishSpriteIndex];
		DishSprite.transform.position = GameController.instance.DishSpritesSpawns [DishIndex].position;

		totalIngredients = 0;

		Minigames = new Dictionary<int, Minigame> ();

		List<Sprite> AllowedIngredientSprites = new List<Sprite> (storage.IngredientSmallSprites);
		List<Sprite> UnallowedIngredientSprites = new List<Sprite> ();
		if (Variations.Contains(Variation.shadowplay)) {
			UnallowedIngredientSprites.Add (AllowedIngredientSprites[9]);
			UnallowedIngredientSprites.Add (AllowedIngredientSprites[7]);
			UnallowedIngredientSprites.Add (AllowedIngredientSprites[2]);
			UnallowedIngredients.Add (9);
			UnallowedIngredients.Add (7);
			UnallowedIngredients.Add (2);
		}
		for (int i = 0; i < ingredientsCount; i++) {
			int uniqueIngredient = 0;
			int ingredientCondition = 2;
			if (dishData == null) {
				uniqueIngredient = Random.Range(0, AllowedIngredientSprites.Count);
				while (IngredientConditionsDict.ContainsKey(uniqueIngredient) || UnallowedIngredientSprites.Contains(AllowedIngredientSprites[uniqueIngredient])) {
					uniqueIngredient = Random.Range(0, AllowedIngredientSprites.Count);
				}
			} else {
				uniqueIngredient = dishData.IngredientSpriteIndexes [i];
				ingredientCondition = dishData.IngredientsConditions [i];
			}

			if (dishData != null) {
				Minigames.Add (uniqueIngredient, dishData.Minigames [i]);
				Debug.Log (uniqueIngredient + ", " + dishData.Minigames [i]);
			} else {
				float chance = Random.Range (0.0f, 1.0f);
				if (chance <= 0.3f) {
					int minigameIndex = Random.Range (0, 4);
					Minigame minigame = (Minigame)minigameIndex;
					Minigames.Add (uniqueIngredient, minigame);
					ingredientCondition = 1;
				} else {
					Minigames.Add (uniqueIngredient, Minigame.None);
				}
			}

			IngredientConditionsDict.Add (uniqueIngredient, ingredientCondition);
			IngredientsCounts.Add (uniqueIngredient, 0);
			Ingredients.Add(uniqueIngredient);
			IngredientSprites [i].gameObject.SetActive (true);
			IngredientSprites [i].sprite = AllowedIngredientSprites [uniqueIngredient];

			if (Variations.Contains(Variation.crookedMan)) {
				IngredientSprites [i].gameObject.transform.Rotate (0.0f, 0.0f, Random.Range(0.0f, 360.0f));
			}
		}
		for (int i = 0; i < ingredientsCount; i++) {
			int uniqueItem = 0;
			if (dishData == null) {
				uniqueItem = Random.Range(0, storage.ItemSprites.Length);
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
			} else {
				uniqueItem = dishData.HelperItemIndexes [i];
			}

			IngredientItems.Add (Ingredients [i], uniqueItem);
			if (uniqueItem != -1) {
				HelperSprites [i].sprite = storage.ItemSprites [uniqueItem];
			} else {
				HelperSprites [i].sprite = null;
				HelperSprites [i].gameObject.SetActive (false);
			}

			Debug.Log (Ingredients [i] + ", " + uniqueItem);
		}
		foreach (var condition in IngredientConditionsDict) {
			totalIngredients += condition.Value;
		}
	}

	void Start () {		
		InitializeDish ();
		RefreshLabels ();

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

		if (Variations.Contains(Variation.shadowplay)) {
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
	public bool startInitialize;
	public float initTimer;

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
				if (Variations.Contains(Variation.memory)) {
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
			}
		}
	}

	void IngredientsSuckIn(int index) {	
		shouldMoves [index] = true;
	}

	void RefreshLabels() {
		for (int i = 0; i < Ingredients.Count; i++) {	
			ingredientCountLabels [i].gameObject.SetActive (true);
			ingredientCountLabels [i].text = IngredientsCounts [Ingredients [i]] + "/" + IngredientConditionsDict [Ingredients [i]];
		}
	}

	void OnDestroy () {
		GameController.instance.OnBoosterStatusChanged -= GameController_instance_OnBoosterStatusChanged;
	}
}
