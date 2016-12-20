using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Flyer : MonoBehaviour {

	public bool ShowScore;
	public Vector2 InitialPosition;
	public Vector2 DestinationPosition;
	public Text myText;
	public float SavedScore; // =\
	public float Speed;
	public int TapCount;
	float interpolator;

	public delegate void FlyerArrivedEventHandler (GameObject myObj);
	public static event FlyerArrivedEventHandler OnFlyerArrived;

	void Awake() {
		InitialPosition = transform.position;
		myText = GetComponentInChildren<Text> ();
	}

	void Start() {
		interpolator = 0.0f;
		if (myText != null) {
			if (!ShowScore) {
				myText.text = "x" + TapCount;
			} else {
				myText.text = "+" + SavedScore * GameController.instance.ComboCount;
			}
		}
	}

	void Update() {		
		transform.position = Vector2.Lerp (InitialPosition, DestinationPosition, interpolator);
		interpolator += Speed * Time.deltaTime;

		if (interpolator >= 1.0f) {
			OnFlyerArrived (gameObject);
		}
	}
}
