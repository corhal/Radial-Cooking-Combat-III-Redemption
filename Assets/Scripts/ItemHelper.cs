using UnityEngine;
using System.Collections;

public class ItemHelper : MonoBehaviour {

	public Ingredient myIngredient;

	void Awake() {
		//Debug.Log ("Helper spawned");
	}

	void OnMouseDown() {	
		//Debug.Log ("Item clicked");
		myIngredient.ClickHelper (this);
	}
}
