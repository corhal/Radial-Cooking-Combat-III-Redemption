using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Ingredient : MonoBehaviour {
	
	public int HP;
	public bool Complex;

	// public SpriteRenderer BlurSprite;

	public string Side;

	public bool Focused;
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
	public List<BoxCollider2D> ItemColliders;

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

	float lifeTime;
	float timer;

	public float ColliderScaleFactor;

	public bool Clicked;

	Vector3 initialPosition;

	void Awake() {		
		ItemColliders = new List<BoxCollider2D> ();
		mySprite = GetComponentInChildren<SpriteRenderer> ();
		mySlider = GetComponentInChildren<Slider> ();
		myBody = GetComponent<Rigidbody2D> ();
		storage = GameObject.FindGameObjectWithTag ("Player").GetComponent<Storage> ();
		foreach (var itemSprite in ItemSprites) {
			ItemColliders.Add(itemSprite.gameObject.GetComponent<BoxCollider2D>());
		}
	}

	void Start() {
		MaxScore = Random.Range (Player.instance.MyMission.MinPointsPerAction, Player.instance.MyMission.MaxPointPerAction);
		CurrentScore = MaxScore;
		Items = new List<int> ();
		if (GameController.instance.dishCount == 0 && (GameController.instance.IsFirst || GameController.instance.IsSecond)) {
			GameController.instance.SpawnTime = 20.0f;
			mySlider.maxValue = GameController.instance.SpawnTime;
			GameController.instance.HelperLabel.gameObject.SetActive (true);
			GameController.instance.HelperLabel.text = "";
			GameController.instance.Advisor.gameObject.SetActive (true);
			string advisorText = "";
			string animationName = "";

			int index = Random.Range (0, GameController.instance.CurrentDish.Ingredients.Count);

			if (GameController.instance.IsFirst) {
				advisorText = "Tap matching ingredients";
				animationName = "FirstHelp";
			} else if (GameController.instance.IsSecond) {
				advisorText = "Discard wrong ingredients";
				animationName = "SecondHelp";
				while (GameController.instance.CurrentDish.Ingredients.Contains(GameController.instance.AllowedIngredients[index])) {
					index = Random.Range (0, GameController.instance.AllowedIngredients.Count);
				}
				GameController.instance.Arrow.sprite = storage.ArrowSprites [1];
			}
			IngredientType = GameController.instance.AllowedIngredients[index];
			mySprite.sprite = storage.IngredientSprites [IngredientType];

			GameController.instance.Advisor.text = advisorText;
			GameController.instance.PlayAnimation (animationName);

		} else if (Complex && Action) {
			//int index = Random.Range (0, GameController.instance.CurrentDish.Ingredients.Count);
			//IngredientType = GameController.instance.CurrentDish.Ingredients[index];
			//mySprite.sprite = storage.IngredientSprites [IngredientType];

			//Complex = true;
			//Action = true;
			if (Interaction == null) {
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
			}

		} else if (Complex && Item) {
			int targetItem = GameController.instance.CurrentDish.IngredientItems [IngredientType];
			bool hasRelevant = false;
			bool hasGold = false;
			int count = (!GameController.instance.CurrentDish.Variations.Contains(Variation.revolver)) ? 3 : 6;
			float probability = 1.0f / (float) count;
			float goldProbability = 2.0f / (float) count;
			for (int i = 0; i < count; i++) {
				int uniqueItem = Random.Range(0, storage.ItemSprites.Length);
				if (Random.Range(0.0f, 1.0f) <= probability) {
					uniqueItem = targetItem;
				}
				if (uniqueItem != targetItem && GameController.instance.CurrentDish.Variations.Contains(Variation.goldhunt) && Random.Range(0.0f, 1.0f) <= probability) {
					uniqueItem = -1;
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
				if (uniqueItem == -1) {
					ItemSprites [i].sprite = storage.MoneySprite;
					ItemSprites [i].gameObject.transform.localScale = Vector3.one;
				} else {
					ItemSprites [i].sprite = storage.ItemSprites [uniqueItem];
				}
			}
		}

		if (GameController.instance.CurrentDish.Variations.Contains(Variation.doubleTrouble)) {
			transform.localScale *= 0.8f;
		}

		initialPosition = transform.position;
		RotationSpeed = GameController.instance.RotationSpeed;
		ColliderScaleFactor = GameController.instance.ColliderScaleFactor;

		for (int i = 0; i < Items.Count; i++) {
			int index = ItemSprites.IndexOf(ItemColliders[i].gameObject.GetComponent<SpriteRenderer> ());
			Debug.Log ("Index: " + index);
			int item = Items [index];
			int targetItem = GameController.instance.CurrentDish.IngredientItems [IngredientType];

			if (item == targetItem) {
				ItemColliders[i].size *= ColliderScaleFactor;
			} else {
				ItemColliders[i].size /= ColliderScaleFactor;
			}
		}
	}

	public void DestroyIngredient(bool terminated) {
		IsBeingDestroyed = true;
		if (MultiplierBlock.gameObject.activeSelf) {
			MultiplierBlock.gameObject.SetActive(false);
			ComboImage.gameObject.SetActive (false);
		}

		Active = false;
		OnIngredientDestroyed (this, terminated);
	}

	public void Click () {
		if (transform.position.x < initialPosition.x - 0.5f) {
			return;
		} else {
			transform.position = initialPosition;
		}
		if (GameController.instance.FullPause && !Focused) {
			return;
		}
		Focused = true;
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

	void OnTriggerEnter2D(Collider2D other) {
		
	}

	void OnMouseDown () {
		
	}

	public bool IsBeingDestroyed;
	void OnMouseUp () {
		if (!IsBeingDestroyed) {
			Click ();
		}
	}

	public void StartAction () {
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

			if (GameController.instance.CurrentDish.Variations.Contains(Variation.goldhunt) && helper.gameObject.GetComponent<SpriteRenderer>().sprite == storage.MoneySprite) {
				GameController.instance.GoldParticles.gameObject.transform.position = helper.gameObject.transform.position;
				Destroy (helper.gameObject);
				Destroy (ItemSliders [index].gameObject);
				GameController.instance.GoldParticles.Stop ();
				GameController.instance.GoldParticles.Play ();
				return;
			}
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
		if (GameController.instance.FullPause && !Focused) {
			return;
		}
		if (Interaction == Minigame.Targets) {			
			if (TargetsCount <= 0) {
				GameController.instance.timer = GameController.instance.SpawnTime;
			}
		}
		if (ShouldRotate) {
			for (int i = 0; i < ItemsContainers.Length; i++) {
				ItemsContainers [i].transform.RotateAround (mySprite.transform.position, mySprite.transform.forward, RotationSpeed * GameController.instance.TimeStep);
			}
			for (int i = 0; i < ItemSprites.Count; i++) {
				ItemSprites [i].transform.Rotate (ItemSprites [i].transform.forward, -RotationSpeed * GameController.instance.TimeStep);
				ItemSliders [i].transform.Rotate (ItemSliders [i].transform.forward, -RotationSpeed * GameController.instance.TimeStep);
			}
		}

		timer += GameController.instance.TimeStep;

		mySlider.value = timer;
		foreach (var slider in ItemSliders) {
			slider.value = timer;
		}

		if (timer >= lifeTime) {			
			if (Active) {
				if (!Action && !GameController.instance.IsPaused) {							
					DestroyIngredient (true);
				} else if (Action && GameController.instance.IsPaused) {
					DestroyIngredient (false);
				} else {
					DestroyIngredient (true);
				}
			}
		}
	}

	public void SetLifeTime(float time) {
		lifeTime = time;
		timer = 0.0f;
		mySlider.maxValue = lifeTime;
		mySlider.value = timer;
		foreach (var slider in ItemSliders) {
			slider.maxValue = lifeTime;
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
		CurrentScore += 5.0f;
		//GameController.instance.ComboCount++;
		tapCount += damage;
		TapCountLabel.text = tapCount.ToString ();
		if (tapCount > 0) {
			ComboImage.gameObject.SetActive (true);
			//ComboImage.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f);
		}
		switch (tapCount) {
		case 1:
			//comboCount = 0;
			ComboImage.color = new Color (1.0f, 1.0f, 1.0f, 0.0f);
			//combo = " ";
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);
			break;
		case 4:
			//combo = "Good!";
			//comboCount = 1;
			//ComboImage.gameObject.SetActive (true);
			ComboImage.sprite = storage.ComboSprites [0];
			ComboImage.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.1f, 1.1f, 1.1f);
			break;
		case 8:
			//combo = "Great!";
			//comboCount = 2;
			ComboImage.sprite = storage.ComboSprites [1];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.35f, 0.35f, 0.35f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.2f, 1.2f, 1.2f);
			break;
		case 12:
			//combo = "Perfect!";
			//comboCount = 3;
			ComboImage.sprite = storage.ComboSprites [2];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.4f, 0.4f, 0.4f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
			break;
		case 16:
			//combo = "Amazing!";
			//comboCount = 4;
			ComboImage.sprite = storage.ComboSprites [3];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.45f, 0.45f, 0.45f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.4f, 1.4f, 1.4f);
			break;
		case 20:
			//comboCount = 5;
			//combo = "Legendary!";
			ComboImage.sprite = storage.ComboSprites [4];
			ComboImage.SetNativeSize ();
			ComboImage.gameObject.transform.localScale = new Vector3 (0.45f, 0.45f, 0.45f);
			//AdditionalScoreLabel.gameObject.transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);
			break;
		default:
			break;
		}
		AdditionalScoreLabel.text = "" + CurrentScore;
		//GameController.instance.ShowCombo (this);
		HP--;
	}
}

public enum Minigame {
	None,
	Slice,
	Chop,
	Grate,
	Targets
}
