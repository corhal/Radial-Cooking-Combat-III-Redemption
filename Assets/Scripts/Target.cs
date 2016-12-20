using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour {

	public Ingredient MyIngredient;

	void OnMouseDown() {
		MyIngredient.TakeDamage (2);
		Destroy (gameObject);
		MyIngredient.TargetsCount--;
	}

	void Start() {
		float scaleFactor = Random.Range (1.0f, 1.5f);
		gameObject.transform.localScale *= scaleFactor;
	}

	void Update() {
		if (MyIngredient.gameObject.GetComponent<Collider2D>() == null || gameObject.transform.localScale.x <= 0.0f) {
			Destroy (gameObject);
			MyIngredient.TargetsCount--;
		}
		gameObject.transform.localScale -= new Vector3 (0.01f, 0.01f);
	}
}
