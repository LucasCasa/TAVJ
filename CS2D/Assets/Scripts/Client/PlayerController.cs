using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public PlayerInput playerInput;

	// Use this for initialization
	void Start () {
		playerInput = new PlayerInput ();
	}
	
	// Update is called once per frame
	void Update () {
		playerInput.shoot = false;
		if (Input.GetKeyDown (KeyCode.W)) {
			playerInput.up = true;
		} else if (Input.GetKeyUp (KeyCode.W)) {
			playerInput.up = false;
		}

		if (Input.GetKeyDown (KeyCode.A)) {
			playerInput.left = true;
		} else if (Input.GetKeyUp (KeyCode.A)) {
			playerInput.left = false;
		}

		if (Input.GetKeyDown (KeyCode.S)) {
			playerInput.down = true;
		} else if (Input.GetKeyUp (KeyCode.S)) {
			playerInput.down = false;
		}

		if (Input.GetKeyDown (KeyCode.D)) {
			playerInput.right = true;
		} else if (Input.GetKeyUp (KeyCode.D)) {
			playerInput.right = false;
		}

		if (Input.GetKeyDown (KeyCode.Q)) {
			playerInput.shoot = true;
		}
	}
}
