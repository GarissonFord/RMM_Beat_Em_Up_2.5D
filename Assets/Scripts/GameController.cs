using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour 
{
	public Text scoreText;
	public int score;

	//Create an array of enemy spawn points
	public Transform[] spawnPoints;
	public GameObject enemy;

	public int timeBetweenSpawns;

	// Use this for initialization
	void Start () 
	{
		score = 0;
		scoreText.text = "Score: " + score;
		Spawn ();
	}

	// Update is called once per frame
	void Update () 
	{
		DisplayScore ();
	}

	void Spawn()
	{
		int i = Random.Range (0, spawnPoints.Length);
		Instantiate (enemy, spawnPoints [i].position, spawnPoints [i].rotation);

		Invoke ("Spawn", timeBetweenSpawns);
	}

	public void DisplayScore()
	{
		scoreText.text = "Score: " + score;
	}

	public void UpdateScore(int s)
	{
		score += s;
	}

	public void RestartLevel()
	{
		SceneManager.LoadScene ("Main");
	}
}
