using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	//Bounds for the game view
	public float xMin, xMax, zMin, zMax;

	//Player hitpoints
	private float hitPoint = 100;
	private float maxHitPoint = 100;
	public Image currentHealthBar;
	public Text ratioText;

	//Used for the Flip() function
	public bool facingRight;

	//Variables for movement speed
	public float speed;

	public float moveHorizontal;
	public float moveVertical;

	//Rigidbody reference
	private Rigidbody rb;

	//Reference to the punch and kick colliders
	public Collider[] attackHitboxes;

	//Damage given when an attack lands
	public float damage;

	//time of last attack and cooldown time for an attack
	public float lastAtk;
	public float atkCooldown;

	//Hurt audio
	private AudioSource hurt;

	//Taken from YouTube tutorial
	public float attackMovementSpeed;
	public float walkMovementSpeed;

	public GameObject kickHitBox;

	//Reference to GameController
	public GameObject gc;
	public GameController controller;

	Animator anim;
	AnimatorStateInfo currentStateInfo;

	//Current animation state
	static int currentState;
	//Numerical representations of the idle and walk animation states
	static int idleState = Animator.StringToHash ("Base Layer.PlayerIdle");
	static int walkState = Animator.StringToHash ("Base Layer.PlayerMove");

	//The sprite currently being rendered
	SpriteRenderer currentSprite;
	public Sprite kickSpriteHitFrame;

	public bool tempInvincible;

	// Use this for initialization
	void Awake () 
	{
		facingRight = true;
		rb = GetComponent<Rigidbody> ();
		hurt = GetComponent<AudioSource> ();
		gc = GameObject.Find("GameController");
		controller = gc.GetComponent<GameController> ();
		anim = GetComponent<Animator> ();
		currentSprite = GetComponent<SpriteRenderer> ();
		tempInvincible = false;
	}

	// Update is called once per frame
	void Update()
	{
		//0th index is the base layer
		currentStateInfo = anim.GetCurrentAnimatorStateInfo (0);
		currentState = currentStateInfo.fullPathHash;

		//For testing, just to check our current state
		/*
		if (currentState == idleState)
			Debug.Log ("Idle State");

		if (currentState == walkState)
			Debug.Log ("Walk State");
		*/

		//Control speed based on commands
		if (currentState == idleState || currentState == walkState) 
			speed = walkMovementSpeed;
		else
			speed = attackMovementSpeed;
	}

	void FixedUpdate () 
	{ 
		if (Input.GetButton ("Kick") /*&& ((Time.time - lastAtk) > atkCooldown)*/) 
		{
			anim.SetBool ("Attack", true);
			if (kickSpriteHitFrame == currentSprite.sprite) 
			{
				kickHitBox.gameObject.SetActive (true);
				Collider col = kickHitBox.GetComponent<Collider> ();
				Attack (col);
			}
			else
				kickHitBox.gameObject.SetActive (false);
		} 
		else 
		{
			anim.SetBool ("Attack", false);
		}

		//Test code to play our knockback animation
		if (Input.GetKeyDown (KeyCode.Q))
			StartCoroutine (KnockedBack());

		//Gets the keyboard input for movement
		moveHorizontal = Input.GetAxis("Horizontal");
		moveVertical = Input.GetAxis ("Vertical");

		//Changes position based on moveHorizontal and moveVertical
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		rb.velocity = movement * speed;
		rb.position = new Vector3 
						(
							Mathf.Clamp(rb.position.x, xMin, xMax),
							transform.position.y,
							Mathf.Clamp(rb.position.z, zMin, zMax)
						);

		//Animator updates
		anim.SetFloat("Speed", rb.velocity.sqrMagnitude);

		//Flips when hitting 'right' and facing left
		if (moveHorizontal > 0 && !facingRight)
			Flip ();
		//Flips when hitting 'left' and facing right
		else if (moveHorizontal < 0 && facingRight)
			Flip ();
	}

	private void Attack (Collider col)
	{
		//Debug.Log (col.gameObject.name);
		//Checks if players attack colliders have in fact, collided with something
		Collider [] collision = Physics.OverlapBox(col.bounds.center, col.bounds.extents, col.transform.rotation, LayerMask.GetMask("Enemy"));
		//if null, continue on
		for (int i = 0; i < collision.Length; i++) 
		{
			if (collision [i] == null) {
				Debug.Log ("No collision");
			} 
			else if(collision[i].gameObject.CompareTag("Enemy"))
			{
				GameObject enemy = collision [i].gameObject;
				//EnemyController ec = enemy.GetComponent<EnemyController> ();
				collision [i].SendMessageUpwards ("TakeDamage", damage);
				Debug.Log (collision[i].gameObject);
			}
		}
	}

	//Changes rotation of the player
	void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	IEnumerator KnockedBack()
	{/*
		anim.Play ("PlayerHurt");

		if (facingRight == false)
			rb.AddForce (transform.right * knockedBackForce);
		else if (facingRight == true)
			rb.AddForce (transform.right * -(knockedBackForce));
	*/
		yield return new WaitForSeconds (3.0f);

		anim.Play ("PlayerIdle");
	}

	public void TakeDamage(float damage)
	{
		//Player dies in one hit still, so we'll have to figure this out 
		if (!tempInvincible) 
		{
			hurt.Play ();
			hitPoint -= damage;
			//Gives the player a short burst of invincibility to get themselves
			//out of overwhelming situations
			StartCoroutine(TempInvincibility());
			UpdateHealthBar ();

			if (hitPoint <= 0) {
				hitPoint = 0;
				UpdateHealthBar ();
				Destroy (gameObject);
				controller.RestartLevel ();
			}
		}
	}

	IEnumerator TempInvincibility()
	{
		tempInvincible = true;
		Debug.Log ("Temporarily invincible");
		yield return new WaitForSeconds (3.0f);
		tempInvincible = false;
		Debug.Log ("No longer invincible");
	}
		
	private void UpdateHealthBar()
	{
		float ratio = hitPoint / maxHitPoint;
		currentHealthBar.rectTransform.localScale = new Vector2 (ratio, 1);
		ratioText.text = (ratio * 100).ToString () + "%";
	}
}
