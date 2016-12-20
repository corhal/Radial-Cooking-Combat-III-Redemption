using UnityEngine;
using System.Collections;

public class Blast : MonoBehaviour {

	ParticleSystem particles;

	void Awake() {
		particles = GetComponentInChildren<ParticleSystem> ();
	}

	void Update() {
		if (!particles.isPlaying) {
			Destroy (gameObject);
		}
	}
}
