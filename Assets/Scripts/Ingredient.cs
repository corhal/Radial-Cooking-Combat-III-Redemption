using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Ingredient : MonoBehaviour {
	
	public int HP;
	public bool Complex;

	public bool Action;
	public bool Item;

	public Minigame Interaction;
	public GameObject TargetPrefab;

	public bool Correct;
	public bool Active = true;
	public int IngredientType;
	public SpriteRenderer mySprite;
	public Image Fill;
	public Image ComboImage;
	Storage storage;

	public ParticleSystem MyParticles;

	public GameObject[] ItemsContainers;

	public List<int> Items;
	public List<SpriteRenderer> ItemSprites;

	public Slider mySlider;
	public Slider[] ItemSliders;
	Rigidbody2D myBody;

	public float MaxScore;
	public float CurrentScore;

	public delegate void DestroyedEventHandler (Ingredient ingredient, bool terminated);
	public static event DestroyedEventHandler OnIngredientDestroyed;

	public GameObject MultiplierBlock;
	public Text AdditionalScoreLabel;
	public Text BonusGameLabel;
	public Text TapCountLabel;

	public float RotationSpeed;
	public bool ShouldRotate;

	public bool Clicked;

	void Awake() {		
		mySprite = GetComponentInChildren<SpriteRenderer> ();
		mySlider = GetComponentInChildren<Slider> ();
		myBody = GetComponent<Rigidbody2D> ();
		storage = GameObject.FindGameObjectWithTag ("Player").GetComponent<Storage> ();
	}

	void Start() {
		MaxScore = Random.Range (Player.instance.MyMission.MinPointsPerAction, Player.instance.MyMission.MaxPointPerAction);
		CurrentScore = MaxScore;
		Items = new List<int> ();
		if (GameController.instance.dishCount == 0 && GameController.instance.IsFirst == true) {
			GameController.instance.SpawnTime = 20.0f;
			mySlider.maxValue = GameController.instance.SpawnTime;
			GameController.instance.HelperLabel.gameObject.SetActive (true);
			GameController.instance.Advisor.text = "Tap matching ingredients";
			GameController.instance.Advisor.gameObject.SetActive (true);
			GameController.instance.HelperLabel.text = "";
			GameController.instance.PlayAnimation ("FirstHelp");
			int index = Random.Range (0, GameController.instance.CurrentDish.Ingredients.Count);
			IngredientType = GameController.instance.CurrentDish.Ingredients[index];
			mySprite.sprite = storage.IngredientSprites [IngredientType];
		} else if (GameController.instance.dishCount == 0 && GameController.instance.IsSecond == true) {
			GameController.instance.SpawnTime = 20.0f;
			mySlider.maxValue = GameController.instance.SpawnTime;
			GameController.instance.HelperLabel.gameObject.SetActive (true);
			GameController.instance.HelperLabel.text = "";
			GameController.instance.Advisor.text = "Discard wrong ingredients";
			GameController.instance.Advisor.gameObject.SetActive (true);
			GameController.instance.Arrow.sprite = storage.ArrowSprites [1];
			GameController.instance.PlayAnimation ("SecondHelp");
			int index = Random.Range (0, GameController.instance.AllowedIngredients.Count);
			while (GameController.instance.CurrentDish.Ingredients.Contains(GameController.instance.AllowedIngredients[index])) {
				index = Random.Range (0, GameController.instance.AllowedIngredients.Count);
			}
			IngredientType = GameController.instance.AllowedIngredients [index];
			mySprite.sprite = storage.IngredientSprites [IngredientType];
		} else if (GameController.instance.NextComplex) {
			int index = Random.Range (0, GameController.instance.CurrentDish.Ingredients.Count);
			IngredientType = GameController.instance.CurrentDish.Ingredients[index];
			mySprite.sprite = storage.IngredientSprites [IngredientType];

			Complex = true;
			Action = true;
			float rand = Random.Range (0.0f, 1.0f);
			if (rand <= 0.25f) {
				Interaction = Minigame.Slice;
			} else if (rand <= 0.5f) {
				Interaction = Minigame.Chop;
			} else if (rand <= 0.75f) {
				Interaction = Minigame.Grate;
			} else {
				Interaction = Minigame.Targets;
			}
		} else if (GameController.instance.CurrentDish.IngredientItems.ContainsKey(IngredientType) && GameController.instance.CurrentDish.IngredientItems[IngredientType] != -1) {
			Complex = true;
			Item = true;

			int targetItem = GameController.instance.CurrentDish.IngredientItems [IngredientType];
			bool hasRelevant = false;
			int count = (Player.instance.MyMission.MyVariation != Variation.sixChoices && Player.instance.MyMission.MyVariation != Variation.sixChoicesRotating) ? 3 : 6;
			float probability = 1.0f / (float) count;
			for (int i = 0; i < count; i++) {
				int uniqueItem = Random.Range(0, storage.ItemSprites.Length);
				if (Random.Range(0.0f, 1.0f) <= probability) {
					uniqueItem = targetItem;
				}
				while (Items.Contains(uniqueItem)) {
					uniqueItem = Random.Range(0, storage.ItemSprites.Length);
				}
				if (uniqueItem == targetItem) {
					hasRelevant = true;
				} else if (!hasRelevant && i == count - 1) {
					uniqueItem = targetItem;
					hasRelevant = true;
				}
				Items.Add (uniqueItem);
				ItemSprites [i].sprite = storage.ItemSprites [uniqueItem];
			}
		}
		/*if (Player.instance.MyMission.MyVariation == Variation.shadowplay) {
			mySprite.color = Color.black;
			foreach (var itemSprite in ItemSprites) {
				itemSprite.color = Color.black;
			}
		}*/
	}

	public void DestroyIngredient(bool terminated) {
		if (MultiplierBlock.gameObject.activeSelf) {
			MultiplierBlock.gameObject.SetActive(false);
			ComboImage.gameObject.SetActive (false);
		}

		Active = false;
		OnIngredientDestroyed (this, terminated);
	}

	void OnTriggerEnter2D(Collider2D other) {
		
	}

	void OnMouseDown() {
		if (GetComponentInChildren<Button> () != null) {
			GetComponentInChildren<Button> ().gameObject.SetActive (false);	
		}
		if (!Complex) {
			Active = false;
			DestroyIngredient (false);
		} else if (Item) {
			if (!GameController.instance.IsPaused) {
				GameController.instance.IsPaused = true;
				GameController.instance.FirstStep (this);
			}
		} else if (Action) {
			if (!GameController.instance.IsPaused && Interaction == Minigame.Targets) {
				SpawnTargets ();
			}

			StartAction ();

			if (GameController.instance.IsPaused && Interaction == Minigame.Chop) {
				TakeDamage(1);
			}
		}
	}

	public void StartAction() {
		if (!GameController.instance.IsPaused) {
			GameController.instance.NextComplex = false;
			MultiplierBlock.gameObject.SetActive (true);
			AdditionalScoreLabel.text = "x" + 0;
			BonusGameLabel.gameObject.SetActive (true);
			GameController.instance.IsPaused = true;
			GameController.instance.ShowActionHelp (this);
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (Action && GameController.instance.IsPaused && other.gameObject.GetComponent<GameController>() != null) {
			if (Interaction == Minigame.Slice) {
				TakeDamage (1);
			} else if (Interaction == Minigame.Grate && other.transform.position.y >= (GetComponent<Collider2D>().bounds.center + GetComponent<Collider2D>().bounds.extents).y) {
				TakeDamage (1);
			}
		}
	}

	public void ForceNext() {		
		Active = false;
		Clicked = true;
		DestroyIngredient (true);
	}

	public void ClickHelper(ItemHelper helper) {
		if (ItemSprites.Contains(helper.gameObject.GetComponent<SpriteRenderer>())) {
			int index = ItemSprites.IndexOf(helper.gameObject.GetComponent<SpriteRenderer> ());
			int item = Items [index];
			int targetItem = GameController.instance.CurrentDish.IngredientItems [IngredientType];
			GameController.instance.IsPaused = false;
			if (item == targetItem) {
				Active = false;
				foreach (var itemContainer in ItemsContainers) {
					itemContainer.SetActive (false);
				}
				DestroyIngredient (false);
			} else {
				foreach (var itemContainer in ItemsContainers) {
					itemContainer.SetActive (false);
				}
				GameController.instance.AddIncorrect(this);
			}
		}
	}

	void Update() {
		if (Interaction == Minigame.Targets) {			
			if (TargetsCount <= 0) {
				GameController.instance.timer = GameController.instance.SpawnTime;
			}
		}
		if (ShouldRotate) {
			for (int i = 0; i < ItemsContainers.Length; i++) {
				ItemsContainers [i].transform.RotateAround (transform.position, transform.forward, RotationSpeed * Time.deltaTime);
			}
			for (int i = 0; i < ItemSprites.Count; i++) {
				ItemSprites [i].transform.Rotate (ItemSprites [i].transform.forward, -RotationSpeed * Time.deltaTime);
				ItemSliders [i].transform.Rotate (ItemSliders [i].transform.forward, -RotationSpeed * Time.deltaTime);
			}
		}
	}

	public int TargetsCount = 10;
	public void SpawnTargets() {
		for (int i = 0; i < TargetsCount; i++) {
			GameObject newTargetObj = Instantiate (TargetPrefab) as GameObject;
			float x = Random.Range (-2.5f, 2.5f);
			float y = Random.Range (-4.5f, 4.5f);
			newTargetObj.transform.position = new Vector3 (x, y, 0.0f);
			Target newTarget = newTargetObj.GetComponent<Target> ();
			newTarget.MyIngredient = this;
		}
	}


	public void SpinHelpers() {
		ShouldRotate = true;
	}

	public int comboCount;
	int tapCount;
	//string combo;
	public void TakeDamage(int damage) {		
		MyParticles.Play ();
		//CurrentScore += 10.0f;
		//GameController.instance.ComboCount++;
		tapCount += damage;
		TapCountLabel.text = tapCount.ToString ();
		if (tapCount > 0) {
			ComboImage.gameObject.SetActive (true);
			//ComboImage.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f);
		}
		switch (tapCount) {
		case 1:
			comboCount = 0;
			ComboImage.color = new Color (1.0f, 1.0f, 1.0f, 0.0f);
			//combo = " ";
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);
			break;
		case 4:
			//combo = "Good!";
			comboCount = 1;
			//ComboImage.gameObject.SetActive (true);
			ComboImage.sprite = storage.ComboSprites [0];
			ComboImage.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.1f, 1.1f, 1.1f);
			break;
		case 8:
			//combo = "Great!";
			comboCount = 2;
			ComboImage.sprite = storage.ComboSprites [1];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.35f, 0.35f, 0.35f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.2f, 1.2f, 1.2f);
			break;
		case 12:
			//combo = "Perfect!";
			comboCount = 3;
			ComboImage.sprite = storage.ComboSprites [2];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.4f, 0.4f, 0.4f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
			break;
		case 16:
			//combo = "Amazing!";
			comboCount = 4;
			ComboImage.sprite = storage.ComboSprites [3];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.45f, 0.45f, 0.45f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.4f, 1.4f, 1.4f);
			break;
		case 20:
			comboCount = 5;
			//combo = "Legendary!";
			ComboImage.sprite = storage.ComboSprites [4];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.45f, 0.45f, 0.45f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);
			break;
		default:
			break;
		}
		AdditionalScoreLabel.text = "x" + comboCount;
		//GameController.instance.ShowCombo (this);
		HP--;
	}
}

public enum Minigame {
	Slice,
	Chop,
	Grate,
	Targets
}
