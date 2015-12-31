using UnityEngine;
using System.Collections;

public class ShakyCam : MonoBehaviour {

	private Vector3 initialPos;
	private Vector3 initialRot;
	// use a singleton so we can easily access this one instance from everywhere
	public static ShakyCam singleton;

	private float strength;

	void Start () {
		initialPos = transform.position;
		initialRot = transform.eulerAngles;
		singleton = this;
	}

	// shakes the camera as long as the strength hasn't dwindled away
	void Update () {
		if (strength > 0) {
			transform.position = initialPos + strength * Random.insideUnitSphere;
			transform.eulerAngles = initialRot + strength * Random.insideUnitSphere * 2.5f;

			strength -= Time.deltaTime/1.5f;
			if (strength <= 0) {
				transform.position = initialPos;
				transform.eulerAngles = initialRot;
			}
		}
	}

	// A shake of 1f is a decent shake, probably go less than that
	public void shake (float strength) {
		this.strength = Mathf.Max (strength, this.strength);
	}
}
