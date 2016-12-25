using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public ParticleSystem GoldParticles;
	public GameObject BonusScorePrefab;
	public GameObject FlyingStarsPrefab;
	public List<int> AllowedIngredients;

	public Dish CurrentDish;
	public Dish[] Dishes;

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

	public List<Ingredient> spawnedIngredients;

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
	bool switchDish;

	public static GameController instance;

	Collider2D mouseCollider;

	Color comboDefaultColor;

	public Text MemoryTimerLabel;

	bool readyToEnd;
	public float EndTimer;
	public bool NextComplex;

	public Text Advisor;
	public Image Arrow;

	public bool FullPause;

	void Awake() {
		if (!Player.instance.MyMission.Variations.Contains(Variation.classic)) {
			IsFirst = false;
			IsSecond = false;
		}
		Player.instance.FirstTime = false;
		instance = this;
		Lerper = GetComponent<LerpScore> ();
		mouseCollider = GetComponent<Collider2D> ();
		helperAnim = HelperImage.GetComponent<Animation> ();
		storage = GameObject.FindGameObjectWithTag ("Player").GetComponent<Storage> ();
		Dish.OnDishReady += Dish_OnDishReady;
		//Dish.OnDishSwitch += Dish_OnDishSwitch;
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

	void Dish_OnDishInitialized (Dish dish)	{		
		ResetDish ();
	}

	void ResetDish() {		
		AllowedIngredients = new List<int> (CurrentDish.Ingredients);

		List<Ingredient> ingredientsToRemove = new List<Ingredient> (spawnedIngredients);

		for (int i = 0; i < ingredientsToRemove.Count; i++) {
			//spawnedIngredients.Remove (ingredientsToRemove [i]);
			RemoveIngredient (ingredientsToRemove [i], true);
			Destroy (ingredientsToRemove [i].gameObject);
		}

		for (int i = 0; i < Player.instance.MyMission.TrashIngredientsCount; i++) {
			int ingredientToAdd;
			do {
				ingredientToAdd = Random.Range (0, storage.IngredientSprites.Length);
			} while (AllowedIngredients.Contains(ingredientToAdd) || CurrentDish.UnallowedIngredients.Contains(ingredientToAdd));

			AllowedIngredients.Add (ingredientToAdd);
		}
	}

	void Dish_OnDishReady (Dish dish) {
		SpawnTime = 0.0f;
		lastIngredient = true; // ?
	}

	void Start() {			
		StarGoals = new int[Player.instance.MyMission.StarGoals.Length];
		Player.instance.MyMission.StarGoals.CopyTo (StarGoals, 0);
		Player.instance.StarCount = 0;
		DishCountLabel.text = "Dish: " + dishCount + "/3";
		Dishes = new Dish[3];
		for (int i = 0; i < Dishes.Length; i++) {
			GameObject newDish = Instantiate (DishPrefab, DishSpawn.transform.position, DishSpawn.transform.rotation) as GameObject;
			newDish.GetComponent<Dish> ().minIngredients = i;
			Dishes [i] = newDish.GetComponent<Dish> ();
		}
		CurrentDish = Dishes [0];
		CurrentDish.SetActive (true);
		CurrentDish.IngredientsAnimation.gameObject.SetActive (false);
		DishCountLabel.text = "Dish: " + dishCount + "/3";

		for (int i = 0; i < StarImages.Length; i++) {	
			StarImages [i].rectTransform.anchoredPosition = new Vector2 (155.0f * (float) StarGoals[i] / (float) StarGoals[2] - 172.0f * (1 - (float) StarGoals[i] / (float) StarGoals[2]) - 15.0f, 460.0f);
			StarParticles [i].transform.position = StarImages [i].transform.position;
		}
		SpawnTime = 1.75f;
		if (Player.instance.MyMission.Variations.Contains(Variation.memory)) {
			CurrentDish.initTimer = 6.0f;
			timer = 0.0f;
			SpawnTime = CurrentDish.initTimer;
		}
	}

	public void FirstStep(Ingredient ingredient) {	
		if (Player.instance.MyMission.Variations.Contains(Variation.doubleTrouble)) {
			if (ingredient.Side == "left") {
				ingredient.gameObject.transform.position = new Vector2 (-1.01f, ingredient.gameObject.transform.position.y);
			} else {
				ingredient.gameObject.transform.position = new Vector2 (0.87f, ingredient.gameObject.transform.position.y);
			}
		}

		AddScore (ingredient.CurrentScore);
		SpawnTime = Random.Range (Player.instance.MyMission.MinSpawnTimer + 0.5f, Player.instance.MyMission.MaxSpawnTimer + 0.5f); 
		ingredient.SetLifeTime(SpawnTime);
		ingredient.MaxScore = Random.Range (Player.instance.MyMission.MinPointsPerAction, Player.instance.MyMission.MaxPointPerAction);
		ingredient.CurrentScore = ingredient.MaxScore;
		timer = 0.0f;
		ingredient.mySlider.gameObject.SetActive (false);
		int count = (!Player.instance.MyMission.Variations.Contains(Variation.revolver) /*&& !Player.instance.MyMission.Variations.Contains(Variation.sixChoicesRotating)*/) ? 1 : 2;
		for (int i = 0; i < count; i++) {
			ingredient.ItemsContainers[i].SetActive (true);
		}

		if (Player.instance.MyMission.Variations.Contains(Variation.carousel) /*|| Player.instance.MyMission.Variations.Contains(Variation.sixChoicesRotating)*/) {
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

		if (Player.instance.MyMission.Variations.Contains(Variation.doubleTrouble)) {
			Debug.Log ("Should align");
			int index = 0;
			for (int i = 0; i < spawnedIngredients.Count; i++) {
				if (spawnedIngredients[i].Focused) {
					index = i;
				}
			}
			if (index == 0) {
				HelperLabel.transform.localPosition = new Vector2 (-336.0f, HelperLabel.transform.localPosition.y);
			} else {
				HelperLabel.transform.localPosition = new Vector2 (-59.0f, HelperLabel.transform.localPosition.y);
			}
		}

		helperAnim.Play (anim);
	}

	public void ShowActionHelp(Ingredient ingredient) {
		FullPause = true;
		Shading.gameObject.SetActive (true);
		NextComplex = false;
		AddScore (ingredient.CurrentScore);
		SpawnTime = Random.Range (Player.instance.MyMission.MinSpawnTimer + 0.5f, Player.instance.MyMission.MaxSpawnTimer + 0.5f);
		ingredient.SetLifeTime(SpawnTime);
		ingredient.MaxScore = Random.Range (Player.instance.MyMission.MinPointsPerAction, Player.instance.MyMission.MaxPointPerAction);
		ingredient.CurrentScore = 0.0f;
		timer = 0.0f;
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
		destinationPositions.Add (CurrentDish.IngredientSprites [CurrentDish.Ingredients.IndexOf (ingredient.GetComponent<Ingredient> ().IngredientType)].transform.position);
		interpolators.Add (0.0f);
	}

	void ArriveIngredient(GameObject ingredient) {			
		CurrentDish.IngredientSprites [CurrentDish.Ingredients.IndexOf (ingredient.GetComponent<Ingredient> ().IngredientType)].gameObject.GetComponent<Animation> ().Play ();
		CurrentDish.AddCorrect (ingredient.GetComponent<Ingredient>());

		if (CurrentDish.DishIndex == 1 && CurrentDish.collectedIngredients == 1) {			
			NextComplex = true;
		}

		if (CurrentDish.DishIndex == 2 && CurrentDish.collectedIngredients == 2) {
			NextComplex = true;
		}

		int index = ingredientsToMove.IndexOf (ingredient);
		ingredientsToMove.Remove (ingredient);
		initialPositions.RemoveAt (index);
		destinationPositions.RemoveAt (index);
		interpolators.RemoveAt (index);
		if (CurrentDish.IngredientsCounts.ContainsKey(ingredient.GetComponent<Ingredient>().IngredientType) && CurrentDish.IngredientsCounts[ingredient.GetComponent<Ingredient>().IngredientType] == CurrentDish.IngredientConditionsDict[ingredient.GetComponent<Ingredient>().IngredientType]) {
			AllowedIngredients.Remove (ingredient.GetComponent<Ingredient>().IngredientType);
		}
		Destroy (ingredient);

		if (Player.instance.MyMission.Variations.Contains(Variation.switcheroo) && CurrentDish.Ingredients.Count != 0) {
			StartCoroutine (SwitchDish());
		}
	}

	IEnumerator SwitchDish() {	
		yield return new WaitForSeconds(0.3f);	

		SpawnTime = 0.0f;
		switchDish = true;
	}


	void UseCombo() {
		ComboLabel.text = "x0";
		CorrectCount = 0;
		ComboCount = 1;
	}

	public void AddIncorrect(Ingredient ingredient) {
		HelperLabel.gameObject.SetActive (false);
		IsPaused = false;
		CurrentDish.AddIncorrect ();
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
		if (spawnedIngredients.Count == 0 && !lastIngredient) { 
			SpawnTime = 0.6f;
			timer = 0.0f;
		}
	}

	void ShowTutorial() {
		if (IsSecond || IsFirst) {			
			HelperImage.GetComponent<Animation> ().Stop();
			HelperLabel.gameObject.SetActive (false);
			Advisor.gameObject.SetActive (false);
			IsFirst = false;
			IsSecond = !IsSecond;
		} 
	}

	void RemoveIngredient(Ingredient ingredient, bool longDelay) {
		float delay = (longDelay) ? 0.5f : 0.1f;
		delay = (lastIngredient) ? 0.1f : delay;
		spawnedIngredients.Remove (ingredient);
		if (spawnedIngredients.Count == 0 && !lastIngredient) {
			SpawnTime = delay;
			timer = 0.0f;
		} 
	}

	void SendScore(Ingredient ingredient) {
		GameObject newStars = Instantiate (FlyingStarsPrefab, ingredient.transform.position, ingredient.transform.rotation) as GameObject;
		newStars.GetComponent<Flyer> ().ShowScore = true;
		newStars.GetComponent<Flyer> ().DestinationPosition = Lerper.StarTarget.transform.position;
		newStars.GetComponent<Flyer> ().SavedScore = ingredient.CurrentScore;
		CorrectCount++;
	}

	void Ingredient_OnIngredientDestroyed (Ingredient ingredient, bool terminated) {		
		ShowTutorial ();
		bool longDelay = false;
		if ((!terminated && !CurrentDish.Ingredients.Contains(ingredient.IngredientType) // Не таймаут, кликнули по неправильному ингредиенту или таймаут и не кликнули по правильному
			|| (CurrentDish.Ingredients.Contains(ingredient.IngredientType) 
				&& CurrentDish.IngredientsCounts[ingredient.IngredientType] == CurrentDish.IngredientConditionsDict[ingredient.IngredientType]) )
			|| (terminated && CurrentDish.Ingredients.Contains(ingredient.IngredientType))) {
			AddIncorrect(ingredient);
			longDelay = false;
		} else if (!terminated && CurrentDish.Ingredients.Contains(ingredient.IngredientType)) {	// Не таймаут, кликнули по правильному
			if (ingredient.Complex && ingredient.Action) {
				ingredient.BonusGameLabel.gameObject.SetActive (false);
				GameObject bonusScoreObj = Instantiate (BonusScorePrefab, ingredient.AdditionalScoreLabel.transform.position, 
					BonusScorePrefab.transform.rotation) as GameObject;
				Flyer flyer = bonusScoreObj.GetComponent<Flyer> ();
				flyer.DestinationPosition = ScoreLabel.gameObject.transform.position;
				flyer.SavedScore = 0; 
				flyer.TapCount = ingredient.comboCount;
			}
			MoveIngredient (ingredient.gameObject);
			SendScore (ingredient);
			ingredient.mySlider.gameObject.SetActive (false);
			longDelay = true;

		} else { // Не могу понять, что это - скипнули неверный ингредиент?..
			Destroy(ingredient.gameObject);
			if (ingredient.Clicked) {
				SendScore (ingredient);
			}
		}
		RemoveIngredient (ingredient, longDelay);
		if (FullPause) {
			FullPause = false;
		}
	}

	void Update() {		
		if (Input.GetMouseButton (0)) {
			mouseCollider.transform.position = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		}	
		if (FullPause) {
			return;
		}
		if (switchDish && !lastIngredient) {
			switchDish = false;
			CurrentDish.SetActive (false);
			if (CurrentDish.DishIndex == 2) {				
				CurrentDish = Dishes [0];
				if (CurrentDish.IsReady) {
					CurrentDish = Dishes [1];
				}
				if (CurrentDish.IsReady) {
					CurrentDish = Dishes [2];
				}
			} else {
				CurrentDish = Dishes [CurrentDish.DishIndex + 1];
			}
			CurrentDish.SetActive (true);
			ResetDish ();
		}
		if (lastIngredient) {	
			if (!Player.instance.MyMission.Variations.Contains(Variation.memory)) {				
				if (!Player.instance.MyMission.Variations.Contains(Variation.switcheroo)) {
					timer = 0.0f;
					SpawnTime = 1.75f;
				}
			}
			GameObject blast = Instantiate (BlastParticlesPrefab, DishSpawn.transform.position, DishSpawn.transform.rotation) as GameObject;
			blast.GetComponentInChildren<ParticleSystem> ().startColor = Color.blue;
			lastIngredient = false;
			dishCount++;
			if (dishCount < 3) {
				CurrentDish = Dishes [dishCount];
				CurrentDish.SetActive (true);				
				Dishes [dishCount - 1].SetActive (false);								
			} else {
				CurrentDish.SetActive (false);
			}
			ResetDish ();
			if (Player.instance.MyMission.Variations.Contains(Variation.memory)) {
				CurrentDish.startInitialize = true;
				CurrentDish.initTimer = 6.0f;
				timer = 0.0f;
				SpawnTime = CurrentDish.initTimer;
			}

			if (dishCount >= 3) {
				DishCountLabel.text = "You won!";
				Player.instance.StarCount = starCount;
				Player.instance.HasWon = true;
				readyToEnd = true;			
			}
		}
		if (readyToEnd) {
			EndTimer -= Time.deltaTime;
			if (EndTimer <= 0.0f) {
				Restart ();
			}
		} else {			
			timer += Time.deltaTime;
			if (timer >= SpawnTime) {
				//spawnedIngredients.Clear ();
				if (CurrentDish.Ingredients.Count > 0 && timer >= SpawnTime + 0.6f) { // 0.6 - это задержка между уничтожением и спавном!
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
		int count = (!Player.instance.MyMission.Variations.Contains(Variation.doubleTrouble)) ? 1 : 2;

		Transform[] SpawnTransforms = new Transform[count];
		if (count == 1) {
			SpawnTransforms [0] = CenterSpawn;
		} else {
			SpawnTransforms [0] = LeftSpawn;
			SpawnTransforms [1] = RightSpawn;
		}

		for (int i = 0; i < count; i++) {
			SpawnTime = Random.Range (Player.instance.MyMission.MinSpawnTimer, Player.instance.MyMission.MaxSpawnTimer);
			timer = 0.0f;
			Random.InitState (Random.Range(0, 40000));

			float x = Random.Range (SpawnTransforms [i].position.x, SpawnTransforms [i].position.x);
			float y = Random.Range (SpawnTransforms [i].position.y, SpawnTransforms [i].position.y);

			Vector2 spawnPoint = new Vector2 (x, y);

			GameObject newIngredient = Instantiate (IngredientPrefab, spawnPoint, IngredientPrefab.transform.rotation) as GameObject;
			Ingredient ingredient = newIngredient.GetComponent<Ingredient> ();

			ingredient.SetLifeTime(SpawnTime);

			int index;

			List<int> spawnedIngredientsTypes = new List<int> ();
			foreach (var spawnedIngredient in spawnedIngredients) {
				spawnedIngredientsTypes.Add (spawnedIngredient.IngredientType);
			}

			do {
				index = Random.Range (0, AllowedIngredients.Count);
			}
			while (spawnedIngredientsTypes.Contains (AllowedIngredients [index]));

			ingredient.IngredientType = AllowedIngredients[index];

			ingredient.mySprite.sprite = storage.IngredientSprites[ingredient.IngredientType];

			spawnedIngredients.Add (ingredient);
			if (NextComplex) {
				ingredient.Complex = true;
				ingredient.Action = true;
				NextComplex = false;
			} else if (CurrentDish.IngredientItems.ContainsKey(ingredient.IngredientType) && CurrentDish.IngredientItems[ingredient.IngredientType] != -1) {
				ingredient.Complex = true;
				ingredient.Item = true;
			}

			if (Player.instance.MyMission.Variations.Contains(Variation.doubleTrouble)) {
				if (i == 0) {
					ingredient.Side = "left";
				} else {
					ingredient.Side = "right";
				}
				for (int j = 0; j < ingredient.ItemsContainers.Length; j++) {
					ingredient.ItemsContainers [j].transform.RotateAround (ingredient.transform.position, ingredient.transform.forward, 90.0f * (1 - 2 * i)); // costyll
				}
				for (int j = 0; j < ingredient.ItemSprites.Count; j++) {
					ingredient.ItemSprites [j].transform.Rotate (ingredient.ItemSprites [j].transform.forward, -90.0f * (1 - 2 * i));
					ingredient.ItemSliders [j].transform.Rotate (ingredient.ItemSliders [j].transform.forward, -90.0f * (1 - 2 * i));
				}
			}
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
