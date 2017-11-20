using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {

	//i.e. Speed
	public float speed;
	public float currentSpeed;

	//Damage done against the player
	public float damage;

	//Enemy hit points
	private float hitpoint = 200;

	//score value
	public int scoreValue;

	//Reference to player
	public GameObject player;

	//Reference to the animator
	public Animator anim;

	//Audio
	public AudioClip hurt;
	public AudioClip groan;
	public AudioSource audio;

	//Lifted from YouTube tutorial
	public bool playerInSight;

	public float targetDistance;

	private NavMeshAgent navMeshAgent;

	public GameObject spriteObject;

	public bool playerOnRight;
	public Vector3 playerRelativePosition;

	public bool facingRight;

	//for attacks
	public GameObject attackBox;
	public Sprite attackSpriteHitFrame;
	public Sprite currentSprite;

	//Reference to the game controllers
	public GameObject gameController;
	public GameController gameControllerScript;

	// Use this for initialization
	void Awake () 
	{
		audio = GetComponent<AudioSource> ();
		player = GameObject.Find ("Player");
		anim = spriteObject.GetComponent<Animator> ();
		anim.SetBool ("Dead", false);
		navMeshAgent = GetComponent<NavMeshAgent> ();
		navMeshAgent.speed = speed;
		gameController = GameObject.Find ("GameController");
		gameControllerScript = gameController.GetComponent<GameController> ();
	}
	
	void Update () 
	{
		currentSprite = spriteObject.GetComponent<SpriteRenderer> ().sprite;

		if (currentSprite == attackSpriteHitFrame) 
		{
			attackBox.gameObject.SetActive (true);
			Collider col = attackBox.GetComponent<Collider> ();
			Attack (col);
		} else 
		{
			attackBox.gameObject.SetActive (false);
		}

		targetDistance = Vector3.Distance(player.transform.position, gameObject.transform.position);
		playerRelativePosition = player.transform.position - gameObject.transform.position;

		if (playerRelativePosition.x > 0.0f) 
		{
			playerOnRight = false;
		} 
		else if (playerRelativePosition.x < 0.0f)
		{
			playerOnRight = true;
		}

		if (targetDistance < 7.0f) 
		{
			navMeshAgent.speed = 0.0f;
			anim.SetBool ("Attacking", true);
		} 
		else
		{
			navMeshAgent.speed = speed;
			currentSpeed = navMeshAgent.velocity.sqrMagnitude;
			navMeshAgent.SetDestination (player.transform.position);
			navMeshAgent.updateRotation = false;
			anim.SetBool ("Attacking", false);
		}

		if (playerOnRight && facingRight)
			Flip ();
		else if (!playerOnRight && !facingRight)
			Flip ();
	}

	private void TakeDamage(float damage)
	{
		audio.PlayOneShot(hurt, 0.7f);
		hitpoint -= damage;
		Debug.Log (hitpoint);
		if (hitpoint <= 0) 
		{
			hitpoint = 0;
			StartCoroutine (playDeathAnim ());
		}
	}

	IEnumerator playDeathAnim()
	{
		anim.SetBool ("Dead", true);
		//Stops the movement of the navmesh
		navMeshAgent.isStopped = true;
		gameControllerScript.UpdateScore (scoreValue);
		yield return new WaitForSeconds (3.0f);
		Destroy (gameObject);
	}

	void Attack(Collider col)
	{
		//Checks if players attack colliders have in fact, collided with something
		Collider [] collision = Physics.OverlapBox(col.bounds.center, col.bounds.extents, col.transform.rotation, LayerMask.GetMask("Player"));

		for (int i = 0; i < collision.Length; i++) 
		{


			if (collision [i].CompareTag ("Enemy")) 
			{
				Debug.Log ("No collision");
			}
			else if (collision [i].CompareTag ("Player")) 
			{
				Debug.Log ("Player hit");
				PlayerController pc = player.GetComponent<PlayerController> ();
				pc.SendMessageUpwards ("TakeDamage", damage);
			}
		}
		/*
		if (collision[0] == null) 
		{
			Debug.Log ("No collision");
			return;
		}
		else if(collision[0].CompareTag("Player"))
		{
			GameObject player = collision[0].gameObject;
			if(player.CompareTag("Player"))
			{
				PlayerController pc = player.GetComponent<PlayerController> ();
				pc.SendMessageUpwards ("TakeDamage", damage);
				Debug.Log (collision);
			}
		}
		*/
	}

	/*
	IEnumerator playDeathAnim()
	{
		anim.SetBool("Dead", true);
		yield return new WaitForSeconds (2f);
	}

	IEnumerator dmgFlash ()
	{ 
		Color normalColor = sr.color;
		Color dmgColor = Color.red;

		//flash of color
		for (int i = 0; i < 2; i++)
		{
			sr.color = dmgColor;
			yield return new WaitForSeconds(.1f);
			sr.color = normalColor; 
			yield return new WaitForSeconds(.1f);
		}
	}
	*/

	void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
