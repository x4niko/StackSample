using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	public Text scoreText;

	void Start () {
		scoreText.text = PlayerPrefs.GetInt ("score").ToString ();
	}

	public void startGame () {
		SceneManager.LoadScene ("GameScene");
	}
}
