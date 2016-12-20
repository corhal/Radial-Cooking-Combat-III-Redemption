using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LerpScore : MonoBehaviour {

	public float animDurationSec;
	float animTimer = 0f;

	float currentScore;

	float savedDisplayedScore = 0;

	float displayedScore = 0;

	float maxScore;
	public Text ScoreLabel;

	Storage storage;
	public Slider ScoreSlider;
	public RectTransform StarTarget;

	void Awake() {
		storage = GameObject.FindGameObjectWithTag ("Player").GetComponent<Storage> ();
	}

	void Start() {
		maxScore = GameController.instance.GoalScore;
		ScoreSlider.maxValue = GameController.instance.StarGoals [2];
		ScoreSlider.value = 0;
	}

	public void AddPoints(float score, float points) {
		currentScore = score;

		savedDisplayedScore = displayedScore;

		currentScore += points;

		animTimer = 0f;

		//ScoreLabel.gameObject.GetComponent<Animation> ().Play ();
	}

	void Update () {
		animTimer += Time.deltaTime;
		float prcComplete = animTimer / animDurationSec;

		displayedScore = Mathf.Lerp(savedDisplayedScore, currentScore, prcComplete);
		ScoreLabel.text = "x" + GameController.instance.ComboCount;
		ScoreSlider.value = (int)displayedScore;
		StarTarget.anchoredPosition = new Vector2 (155.0f * (float) ScoreSlider.value / (float) ScoreSlider.maxValue - 172.0f * (1 - (float) ScoreSlider.value / (float) ScoreSlider.maxValue), 460.0f);
	}
}
