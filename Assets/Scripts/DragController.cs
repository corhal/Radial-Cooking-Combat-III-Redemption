using UnityEngine;
using System.Collections;

public class DragController : MonoBehaviour {
	
	Vector3 offset;
	bool shouldDrag;
	public int layerMask;
	Ingredient myIngredient;
	Vector3 initialPosition;

	void Awake() {		
		layerMask = LayerMask.GetMask ("Ingredients");
		myIngredient = GetComponent<Ingredient> ();
	}

	void OnMouseDown () {
		if (myIngredient.Active && !GameController.instance.IsPaused) {
			shouldDrag = true;
			offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
			initialPosition = transform.position;
		}
	}

	void OnMouseUp () {
		shouldDrag = false;
		if (transform.position.x < initialPosition.x - 0.5f && myIngredient.Active && !GameController.instance.IsPaused) {
			myIngredient.IsBeingDestroyed = true;
			myIngredient.ForceNext ();
		}
	}

	void Update() {
		if (myIngredient.Active && shouldDrag && Input.GetMouseButton(0) && !GameController.instance.IsPaused) {
			Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
			transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offset;
		}
	}
}
