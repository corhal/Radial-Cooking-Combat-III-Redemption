using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public GameObject BonusScorePrefab;
	public GameObject FlyingStarsPrefab;
	public List<int> AllowedIngredients;

	public GameObject RedTint;

	public int[] StarGoals;
	public Image[] StarImages;
	public ParticleSystem[] StarParticles;

	public bool IsFirst = true;
	public bool IsSecond = false;

	public float GoalScore;
	public float Score;

	public int Mistakes;
	public bool IsPaused;

	int starCount;

	public int dishCount;

	public float FailFadeTime;
	public float FailTimer;
	public Text ComboLabel;
	public string[] ComboKeys;
	public List<string> ComboKeysToNuke;
	public int ComboCount;
	public int CorrectCount;
	public float  ComboFadeTimer;
	public float ComboFadeTime;
	public float ComboTimer;

	public Text DishCountLabel;
	public Text ScoreLabel;
	public Image[] Hearts;
	public ParticleSystem[] HeartParticles;
	public Text HelperLabel;
	public GameObject HelperImage;
	public Image Shading;
	Animation helperAnim;
	public float LerpSpeed;
	List<GameObject> ingredientsToMove;
	List<Vector2> initialPositions;
	List<Vector2> destinationPositions;
	List<float> interpolators;

	public Button RestartButton;
	public GameObject DishPrefab;
	public GameObject IngredientPrefab;
	public GameObject BlastParticlesPrefab;

	public LerpScore Lerper;

	List<Ingredient> spawnedIngredients;

	public Transform LeftSpawn;
	public Transform CenterSpawn;
	public Transform RightSpawn;

	public Transform DishSpawn;
	public Transform[] DishSpritesSpawns;
	public GameObject Cook;

	public float SpawnTime;
	public float timer;

	Storage storage;

	bool lastIngredient;

	public static GameController instance;

	Collider2D mouseCollider;

	Color comboDefaultColor;

	public Text MemoryTimerLabel;

	bool readyToEnd;
	public float EndTimer;
	public bool NextComplex;

	public Text Advisor;
	public Image Arrow;

	void Awake() {
		if (Player.instance.MyMission.MyVariation != Variation.classic) {
			IsFirst = false;
			IsSecond = false;
		}
		Player.instance.FirstTime = false;
		instance = this;
		Lerper = GetComponent<LerpScore> ();
		mouseCollider = GetComponent<Collider2D> ();
		helperAnim = HelperImage.GetComponent<Animation> ();
		//IngredientsCounts = new Dictionary<int, int> ();	
		storage = GameObject.FindGameObjectWithTag ("Player").GetComponent<Storage> ();
		Dish.OnDishReady += Dish_OnDishReady;
		Ingredient.OnIngredientDestroyed += Ingredient_OnIngredientDestroyed;
		Dish.OnDishInitialized += Dish_OnDishInitialized;
		Flyer.OnFlyerArrived += Flyer_OnFlyerArrived;
		spawnedIngredients = new List<Ingredient> ();
		ingredientsToMove = new List<GameObject> ();
		initialPositions = new List<Vector2> ();
		destinationPositions = new List<Vector2> ();
		interpolators = new List<float> ();
		comboDefaultColor = ComboLabel.color;
	}

	void Flyer_OnFlyerArrived (GameObject myObj) {
		//if (myObj.GetComponentInChildren<Text>() != null) {
		    ComboCount += myObj.GetComponent<Flyer> ().TapCount;
			AddScore (myObj.GetComponent<Flyer> ().SavedScore);
		//}
		Destroy (myObj);
	}

	public GameObject GhostDishPrefab;
	void Dish_OnDishInitialized (Dish dish)	{		
		ResetDish ();
	}

	void ResetDish() {		
		AllowedIngredients = new List<int> (Dish.instance.Ingredients);

		List<Ingredient> ingredientsToRemove = new List<Ingredient> (spawnedIngredients);

		for (int i = 0; i < ingredientsToRemove.Count; i++) {
			Destroy (spawnedIngredients [i].gameObject);
			spawnedIngredients.Remove (ingredientsToRemove [i]);
		}

		for (int i = 0; i < 1; i++) {
			int ingredientToAdd;
			do {
				ingredientToAdd = Random.Range (0, storage.IngredientSprites.Length);
			} while (AllowedIngredients.Contains(ingredientToAdd));

			AllowedIngredients.Add (ingredientToAdd);
		}

		//SpawnTime = 1.75f;
		//timer = 0.0f;
	}

	public GameObject[] preGhostDishes;
	void Dish_OnDishReady (Dish dish) {
		if (dishCount + 1 < preGhostDishes.Length && preGhostDishes[dishCount + 1] != null) {
			preGhostDishes [dishCount + 1].SetActive (false);
		}
		GameObject ghostDish = Instantiate (GhostDishPrefab, DishSpritesSpawns [dishCount].transform.position, DishSpritesSpawns [dishCount].transform.rotation) as GameObject;
		ghostDish.GetComponentInChildren<SpriteRenderer> ().sprite = dish.DishSprite.sprite;
		ghostDish.GetComponentInChildren<SpriteRenderer> ().color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		lastIngredient = true;
		Debug.Log (lastIngredient);
	}

	void Start() {	
		StarGoals = new int[Player.instance.MyMission.StarGoals.Length];
		Player.instance.MyMission.StarGoals.CopyTo (StarGoals, 0);
		Player.instance.StarCount = 0;
		dishCount = 0;	
		DishCountLabel.text = "Dish: " + dishCount + "/3";
		GameObject newDish = Instantiate (DishPrefab, DishSpawn.transform.position, DishSpawn.transform.rotation) as GameObject;
		newDish.GetComponent<Dish> ().minIngredients = dishCount + 1;
		DishCountLabel.text = "Dish: " + dishCount + "/3";

		for (int i = 0; i < StarImages.Length; i++) {	
			StarImages [i].rectTransform.anchoredPosition = new Vector2 (155.0f * (float) StarGoals[i] / (float) StarGoals[2] - 172.0f * (1 - (float) StarGoals[i] / (float) StarGoals[2]) - 15.0f, 460.0f);
			StarParticles [i].transform.position = StarImages [i].transform.position;
		}
	}

	public void FirstStep(Ingredient ingredient) {		
		AddScore (ingredient.CurrentScore);
		SpawnTime = Random.Range (Player.instance.MyMission.MinSpawnTimer + 0.5f, Player.instance.MyMission.MaxSpawnTimer + 0.5f); //3.0f;
		ingredient.MaxScore = Random.Range (Player.instance.MyMission.MinPointsPerAction, Player.instance.MyMission.MaxPointPerAction);// SpawnTime * 10.0f;
		ingredient.CurrentScore = ingredient.MaxScore;
		timer = 0.0f;
		ingredient.mySlider.gameObject.SetActive (false);
		//ingredient.mySprite.gameObject.transform.position = LeftSpawn.position;
		//ingredient.mySlider.maxValue = SpawnTime;
		//ingredient.Fill.color = Color.blue;
		int count = (Player.instance.MyMission.MyVariation != Variation.sixChoices && Player.instance.MyMission.MyVariation != Variation.sixChoicesRotating) ? 1 : 2;
		for (int i = 0; i < count; i++) {
			ingredient.ItemsContainers[i].SetActive (true);
		}
		foreach (var slider in ingredient.ItemSliders) {
			//slider.gameObject.SetActive (true);
			slider.maxValue = SpawnTime;
		}

		if (Player.instance.MyMission.MyVariation == Variation.rotatingHints || Player.instance.MyMission.MyVariation == Variation.sixChoicesRotating) {
			ingredient.SpinHelpers ();
		}
	}

	void AddScore(float score) {
		Lerper.AddPoints(Score, score * ComboCount);
		Score += score * ComboCount;
		for (int i = 0; i < StarGoals.Length; i++) {
			if (StarImages[i].sprite != storage.StarSprites[1] && Score >= StarGoals[i]) {
				StarImages [i].sprite = storage.StarSprites [1];
				StarParticles [i].Play ();
				starCount = i + 1;
			}
		}
	}

	public void PlayAnimation(string anim) {		
		HelperImage.GetComponent<SpriteRenderer>().color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		HelperImage.transform.rotation = Quaternion.Euler (Vector3.zero);
		HelperImage.transform.position = new Vector2 (206.0f, -94.0f);
		HelperImage.transform.localScale = new Vector3 (50.0f, 50.0f, 50.0f);
		if (anim == "ChopHelp" || anim == "FirstHelp" || anim == "SecondHelp") {
			HelperImage.GetComponent<SpriteRenderer> ().sprite = storage.HandSprites [0];
		} else {
			HelperImage.GetComponent<SpriteRenderer> ().sprite = storage.HandSprites [1];
		}
		helperAnim.Play (anim);
	}

	public void ShowActionHelp(Ingredient ingredient) {
		Shading.gameObject.SetActive (true);
		NextComplex = false;
		AddScore (ingredient.CurrentScore);
		SpawnTime = Random.Range (Player.instance.MyMission.MinSpawnTimer + 0.5f, Player.instance.MyMission.MaxSpawnTimer + 0.5f);
		ingredient.MaxScore = Random.Range (Player.instance.MyMission.MinPointsPerAction, Player.instance.MyMission.MaxPointPerAction);// SpawnTime * 2 * 10.0f;
		ingredient.CurrentScore = 0.0f;
		timer = 0.0f;
		//ingredient.mySlider.gameObject.SetActive (false);
		ingredient.mySlider.maxValue = SpawnTime;
		ingredient.Fill.color = Color.green;
		string animationName = UppercaseFirst (ingredient.Interaction.ToString()) + "Help";
		HelperLabel.gameObject.SetActive (true);
		HelperLabel.text = UppercaseFirst (ingredient.Interaction.ToString()) + "!";
		PlayAnimation (animationName);
	}

	string UppercaseFirst(string s)	{
		// Check for empty string.
		if (string.IsNullOrEmpty(s)) {
			return string.Empty;
		}
		// Return char and concat substring.
		return char.ToUpper(s[0]) + s.Substring(1);
	}

	void MoveIngredient(GameObject ingredient) {
		HelperLabel.gameObject.SetActive (false);
		Shading.gameObject.SetActive (false);
		IsPaused = false;
		Destroy(ingredient.GetComponent<Collider2D>());
		ingredient.transform.localScale *= 0.5f;

		ingredientsToMove.Add (ingredient);
		initialPositions.Add (ingredient.transform.position);
		destinationPositions.Add (Dish.instance.IngredientSprites [Dish.instance.Ingredients.IndexOf (ingredient.GetComponent<Ingredient> ().IngredientType)].transform.position);
		interpolators.Add (0.0f);
	}

	void ArriveIngredient(GameObject ingredient) {		
		Dish.instance.IngredientSprites [Dish.instance.Ingredients.IndexOf (ingredient.GetComponent<Ingredient> ().IngredientType)].gameObject.GetComponent<Animation> ().Play ();
		Dish.instance.AddCorrect (ingredient.GetComponent<Ingredient>());

		if (dishCount == 1 && Dish.instance.collectedIngredients == 1) {			
			NextComplex = true;
		}

		if (dishCount == 2 && Dish.instance.collectedIngredients == 2) {
			NextComplex = true;
		}

		int index = ingredientsToMove.IndexOf (ingredient);
		ingredientsToMove.Remove (ingredient);
		initialPositions.RemoveAt (index);
		destinationPositions.RemoveAt (index);
		interpolators.RemoveAt (index);
		if (Dish.instance.IngredientsCounts.ContainsKey(ingredient.GetComponent<Ingredient>().IngredientType) && Dish.instance.IngredientsCounts[ingredient.GetComponent<Ingredient>().IngredientType] == Dish.instance.IngredientConditionsDict[ingredient.GetComponent<Ingredient>().IngredientType]) {
			AllowedIngredients.Remove (ingredient.GetComponent<Ingredient>().IngredientType);
		}
		Destroy (ingredient);

	}

	void UseCombo() {
		ComboLabel.text = "x0";
		CorrectCount = 0;
		ComboCount = 1;
		//NextComplex = false;
	}

	public void AddIncorrect(Ingredient ingredient) {
		HelperLabel.gameObject.SetActive (false);
		IsPaused = false;
		Dish.instance.AddIncorrect ();
		RedTint.GetComponent<Animation> ().Play();
		Mistakes--;
		Player.instance.HasWon = false;

		UseCombo();

		for (int i = 0; i < Hearts.Length; i++) {
			if (Hearts[i].sprite != storage.HeartSprites[0]) {				
				HeartParticles [i].Play ();
				Hearts [i].sprite = storage.HeartSprites [0];
				break;
			}
		}

		Destroy (ingredient.gameObject);
		Instantiate (BlastParticlesPrefab, ingredient.transform.position, BlastParticlesPrefab.transform.rotation);
		if (Mistakes <= 0) {
			readyToEnd = true;
		}
		spawnedIngredients.Remove (ingredient);
		if (spawnedIngredients.Count == 0) {
			SpawnTime = 0.6f;
			timer = 0.0f;
		}
	}

	void Ingredient_OnIngredientDestroyed (Ingredient ingredient, bool terminated) {		
		if (IsSecond) {			
			HelperImage.GetComponent<Animation> ().Stop();
			HelperLabel.gameObject.SetActive (false);
			Advisor.gameObject.SetActive (false);
			IsSecond = false;
		} 
		if (IsFirst) {			
			HelperImage.GetComponent<Animation> ().Stop();
			HelperLabel.gameObject.SetActive (false);
			Advisor.gameObject.SetActive (false);
			IsFirst = false;
			IsSecond = true;
		} 
		if ((!terminated && !Dish.instance.Ingredients.Contains(ingredient.IngredientType) 
			|| (Dish.instance.Ingredients.Contains(ingredient.IngredientType) 
				&& Dish.instance.IngredientsCounts[ingredient.IngredientType] == Dish.instance.IngredientConditionsDict[ingredient.IngredientType]) )
			|| (terminated && Dish.instance.Ingredients.Contains(ingredient.IngredientType))) {
			AddIncorrect(ingredient);
			spawnedIngredients.Remove (ingredient);
			if (spawnedIngredients.Count == 0) {
				SpawnTime = 0.1f;
				timer = 0.0f;
			}
		} else if (!terminated && Dish.instance.Ingredients.Contains(ingredient.IngredientType)) {	
			if (ingredient.Complex && ingredient.Action) {
				ingredient.BonusGameLabel.gameObject.SetActive (false);
				GameObject bonusScoreObj = Instantiate (BonusScorePrefab, ingredient.AdditionalScoreLabel.transform.position, BonusScorePrefab.transform.rotation) as GameObject;
				Flyer flyer = bonusScoreObj.GetComponent<Flyer> ();
				flyer.DestinationPosition = ScoreLabel.gameObject.transform.position;
				flyer.SavedScore = 0; // ingredient.CurrentScore;
				flyer.TapCount = ingredient.comboCount;
			} else {
				//AddScore (ingredient.CurrentScore);
			}
			MoveIngredient (ingredient.gameObject);
			GameObject newStars = Instantiate (FlyingStarsPrefab, ingredient.transform.position, ingredient.transform.rotation) as GameObject;
			newStars.GetComponent<Flyer> ().ShowScore = true;
			newStars.GetComponent<Flyer> ().DestinationPosition = Lerper.StarTarget.transform.position;
			newStars.GetComponent<Flyer> ().SavedScore = ingredient.CurrentScore;
			CorrectCount++;
			//ComboCount++;
			ingredient.mySlider.gameObject.SetActive (false);

			spawnedIngredients.Remove (ingredient);
			if (spawnedIngredients.Count == 0) {
				SpawnTime = 0.65f;
				timer = 0.0f;
			}
		} else {
			Destroy(ingredient.gameObject);
			if (ingredient.Clicked) {
				//AddScore (ingredient.CurrentScore );
				GameObject newStars = Instantiate (FlyingStarsPrefab, ingredient.transform.position, ingredient.transform.rotation) as GameObject;
				newStars.GetComponent<Flyer> ().ShowScore = true;
				newStars.GetComponent<Flyer> ().DestinationPosition = Lerper.StarTarget.transform.position;
				newStars.GetComponent<Flyer> ().SavedScore = ingredient.CurrentScore;
				CorrectCount++;
				//ComboCount++;
			}
			spawnedIngredients.Remove (ingredient);
			if (spawnedIngredients.Count == 0) {
				SpawnTime = 0.1f;
				timer = 0.0f;
			}
		}
	}

	void Update() {		
		if (lastIngredient) {			
			Destroy (Dish.instance.gameObject);
			GameObject blast = Instantiate (BlastParticlesPrefab, DishSpawn.transform.position, DishSpawn.transform.rotation) as GameObject;
			blast.GetComponentInChildren<ParticleSystem> ().startColor = Color.blue;
			lastIngredient = false;
			dishCount++;
			if (dishCount < 3) {
				GameObject newDish = Instantiate (DishPrefab, DishSpawn.transform.position, DishSpawn.transform.rotation) as GameObject;
				newDish.GetComponent<Dish> ().minIngredients = dishCount + 1;
				DishCountLabel.text = "Dish: " + dishCount + "/3";
			}
		}
		if (dishCount >= 3) {
			DishCountLabel.text = "You won!";
			Player.instance.StarCount = starCount;
			Player.instance.HasWon = true;
			readyToEnd = true;			
		}
		if (readyToEnd) {
			EndTimer -= Time.deltaTime;
			if (EndTimer <= 0.0f) {
				Restart ();
			}
		} else {
			if (Input.GetMouseButton (0)) {
				mouseCollider.transform.position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			}	
			timer += Time.deltaTime;
			foreach (var ingredient in spawnedIngredients) {
				ingredient.mySlider.value = timer;
				foreach (var slider in ingredient.ItemSliders) {
					slider.value = timer;
				}
			}
			if (timer >= SpawnTime) {
				//timer = 0.0f;
				for (int i = 0; i < spawnedIngredients.Count; i++) {
					if (spawnedIngredients [i].Active) {
						//spawnedIngredients [i].Active = false;
						if (!spawnedIngredients [i].Action && !IsPaused) {							
							spawnedIngredients [i].DestroyIngredient (true);
						} else if (spawnedIngredients [i].Action && IsPaused) {
							spawnedIngredients [i].DestroyIngredient (false);
						} else {
							spawnedIngredients [i].DestroyIngredient (true);
						}

					}
				}
				spawnedIngredients.Clear ();
				if (Dish.instance.Ingredients.Count > 0 && timer >= SpawnTime + 0.6f) {
					timer = 0.0f;
					SpawnIngredient ();
				}
			}

			for (int i = 0; i < ingredientsToMove.Count; i++) {
				ingredientsToMove [i].transform.position = Vector2.Lerp (initialPositions [i], destinationPositions [i], interpolators [i]);
				interpolators [i] += LerpSpeed * Time.deltaTime;

				if (interpolators [i] >= 1.0f) {
					ArriveIngredient (ingredientsToMove [i]);
				}
			}
		}
	}

	void SpawnIngredient () {		
		int count = (Player.instance.MyMission.MyVariation != Variation.doubleTrouble) ? 1 : 2;

		Transform[] SpawnTransforms = new Transform[count];
		if (count == 1) {
			SpawnTransforms [0] = CenterSpawn;
		} else {
			SpawnTransforms [0] = LeftSpawn;
			SpawnTransforms [1] = RightSpawn;
		}

		for (int i = 0; i < count; i++) {
			SpawnTime = Random.Range (Player.instance.MyMission.MinSpawnTimer, Player.instance.MyMission.MaxSpawnTimer);
			Random.InitState (Random.Range(0, 40000));

			float x = Random.Range (SpawnTransforms [i].position.x, SpawnTransforms [i].position.x);
			float y = Random.Range (SpawnTransforms [i].position.y, SpawnTransforms [i].position.y);

			Vector2 spawnPoint = new Vector2 (x, y);

			GameObject newIngredient = Instantiate (IngredientPrefab, spawnPoint, IngredientPrefab.transform.rotation) as GameObject;
			Ingredient ingredient = newIngredient.GetComponent<Ingredient> ();
			int index;

			index = Random.Range (0, AllowedIngredients.Count);

			ingredient.IngredientType = AllowedIngredients[index];

			ingredient.mySprite.sprite = storage.IngredientSprites[ingredient.IngredientType];

			ingredient.mySlider.maxValue = SpawnTime;

			spawnedIngredients.Add (ingredient);
		}
	}

	public void Restart() {
		Dish.OnDishReady -= Dish_OnDishReady;
		Ingredient.OnIngredientDestroyed -= Ingredient_OnIngredientDestroyed;
		Dish.OnDishInitialized -= Dish_OnDishInitialized;
		Flyer.OnFlyerArrived -= Flyer_OnFlyerArrived;
		SceneManager.LoadScene (0);
	}
}
