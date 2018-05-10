using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_zombyKenny : Unit, IReaction<GameObject> {

	//Атакуемые слои
	public LayerMask attackCollision;
	//Область агра
	DangerArea start;

	//Ссылка на игрока
	GameObject target;
	public float bornDelay = 0f;
	bool idle = true;
	bool attackable = false;

	//Сила толчка во время получения урона
	public float impulsePower = 3;

	//Местоположения относительно игрока
	float targetRange = 0f;
	float targetDirection =0f;

	void Awake() {
		start = GetComponentInParent<DangerArea> ();
		start.AddEnemie (this);
	}

	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
	}
	
	void Update () {
		if (!idle) {
			if (alive && !stunned) {
				
				//Определение местоположения игрока
				targetRange = Mathf.Abs (transform.position.x - target.transform.position.x);
				targetDirection = Mathf.Sign (transform.position.x - target.transform.position.x);
				flipParam = input;
				anim.SetFloat ("run", Mathf.Abs (input * moveSpeed));

				if (attackCheck) {
					if (targetRange < (attackRange - 0.5f) && ((targetDirection < 0f && direction > 0f) || (targetDirection > 0f && direction < 0f))) {
						input = 0f;
						GetDamage ();
					} else
						input = -targetDirection;
				}
			} else
				Impulse ();
		}

		rb.velocity = new Vector2 (input * moveSpeed, rb.velocity.y);
	}

	public override void GetDamage (){
		if (attackCheck) {
			attackCheck = false;
			attackable = false;
			anim.SetTrigger ("attack");
		}
	}

	public void SecondAttack() {
		StartCoroutine ("ResetAttackCheck");
		attackable = true;
		attackModify [1] = false;
		stunned = true;
		input = -targetDirection;
		flipParam = input;
	}

	//Построить луч атаки
	public void CreateAttackVector() {
		Vector2 targetVector = new Vector2 (direction, 0);
		Vector2 rayOrigin = new Vector2 (transform.position.x, transform.position.y + 0.7f);

		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, targetVector, attackRange, attackCollision);

		if (hit) {
			hit.transform.GetComponent<Unit> ().SetDamage (attack, direction, attackModify);
		}
	}

	public override void SetDamage (float damage, float impulseDirection, bool[] attackModify){
		ReduceHP (damage);
		if (attackable) {
			SetStun (impulseDirection);
			anim.SetTrigger ("attackable");
		}
	}

	public IEnumerator ResetAttackCheck () {
		yield return new WaitForSeconds (attackSpeed);
		attackCheck = true;
		attackModify [1] = true;
	}

	public override void SetStun (float direction){
		input = direction;
		stunned = true;
	}

	//Сбросить чек стана
	public void ResetStunCheck () {
		input = 0f;
		moveSpeed = 2f;
		stunned = false;
	}
	public override void Die (){
		anim.SetTrigger ("die");
		alive = false;
		gameObject.layer = 2;
		gameObject.tag = "Puddle";
	}

	//Начать преследование
	public void Chase (GameObject player) {
		target = player;
		StartCoroutine ("TimeToBorn");
	}

	//Задержка перед воскрешением
	IEnumerator TimeToBorn() {
		yield return new WaitForSeconds (bornDelay);
		anim.SetTrigger ("born");
	}

	public void StartChase() {
		gameObject.layer = 9;
		idle = false;
	}

	//Остановить преследование
	public void Idle () {}

	void Impulse () {
		moveSpeed = Mathf.Sqrt (Time.deltaTime) * impulsePower;
	}

	//Уменьшить ХП + проверка на "смерть"
	void ReduceHP (float damage) {
		if (health <= damage) {
			Die ();
		}
		health -= damage;
	}
}
