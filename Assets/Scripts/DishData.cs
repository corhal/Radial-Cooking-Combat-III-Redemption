using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DishData {
	public int[] IngredientsConditions;
	public int DishSpriteIndex;
	public int[] IngredientSpriteIndexes;

	public int[] HelperItemIndexes;

	public Minigame[] Minigames;

	public List<Variation> Variations;
}
