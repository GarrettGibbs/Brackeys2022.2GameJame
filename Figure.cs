using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Figure : MonoBehaviour
{
    public Animator animator;
	public int health = 3;
	public int movement = 4;
	public int damage = 1;
	public int tileX;
	public int tileY;
	public MapManager map;
	public EncounterManager Eman;

	bool moving = false;
	[SerializeField] float yAdjustment = 50.5f;
	[SerializeField] float xAdjustment = 0f;
	public bool playerFigure;
	public bool isPenguin = false;
	public Sprite portrait;
	private GameObject deathAnimation;
	[SerializeField] GameObject spiral;
	[SerializeField] AudioClip[] hitSounds;
	public AudioClip deathSound;
	public AudioClip attackSound;

	public void SetupFigure(int x, int y, MapManager mm, bool pf, Sprite p, EncounterManager em, float xa, float ya, GameObject d) {
		tileX = x;
		tileY = y;
		map = mm;
		map.AddFigureToTile(this, tileX, tileY);
		playerFigure = pf;
		portrait = p;
		Eman = em;
		xAdjustment = xa;
		yAdjustment = ya;
		deathAnimation = d;
	}

	public async Task MoveToTile(List<Node> path) {
		moving = true;
		animator.SetBool("Walk", true);
		foreach (Node n in path) {
			await MoveToPoint(n);
		}
		moving = false;
		animator.SetBool("Walk", false);
	}

	private async Task MoveToPoint(Node n) {
		map.RemoveFigureFromTile(tileX, tileY);
		float moveTime = .4f;
		Vector3 destination = new Vector3(n.x, n.y + yAdjustment, n.graphY + Random.Range(.1f, .3f));
		//await EMan.CheckIfPaused();
		Vector3 directionalLocal = transform.InverseTransformPoint(destination);
        if (directionalLocal.x > 0) {
            if (isPenguin) {
				GetComponent<SpriteRenderer>().flipX = false;
				destination.x += xAdjustment;
			} else {
				GetComponent<SpriteRenderer>().flipX = true;
				destination.x -= xAdjustment;
			}
        } else {
			if (isPenguin) {
				GetComponent<SpriteRenderer>().flipX = true;
				destination.x -= xAdjustment;
			} else {
				GetComponent<SpriteRenderer>().flipX = false;
				destination.x += xAdjustment;
			}
		}
		LeanTween.move(gameObject, destination, moveTime);
		//RotateToFace(destination);
		map.AddFigureToTile(this, n.graphX, n.graphY);
		tileX = n.graphX;
		tileY = n.graphY;
		await Task.Delay(400);
	}

	public async Task CheckDeath() {
		if (health > 0) return;
		GameObject effect = Instantiate(deathAnimation, transform.position, Quaternion.identity);
		await Eman.RemoveFigure(this);
		map.RemoveFigureFromTile(tileX, tileY);
		Eman.lm.audioManager.PlaySound("Death");
		await Task.Delay(400);
		Eman.lm.audioManager.PlaySound(deathSound);
		await Task.Delay(400);
		Destroy(effect);
		Destroy(gameObject);
    }

	public async Task Teleport(int x,int y) {
		map.RemoveFigureFromTile(tileX, tileY);
		Vector3 destination = new Vector3(map.graph[x,y].x, map.graph[x, y].y + yAdjustment, y);
		Vector3 directionalLocal = transform.InverseTransformPoint(destination);
		if (directionalLocal.x > 0) {
			GetComponent<SpriteRenderer>().flipX = true;
			destination.x -= xAdjustment;
		} else {
			GetComponent<SpriteRenderer>().flipX = false;
			destination.x += xAdjustment;
		}
		SpriteRenderer spiralColor = spiral.GetComponent<SpriteRenderer>();
		LeanTween.value(spiral, 0f, 1f, .5f).setOnUpdate((float val) => {
			Color c = spiralColor.color;
			c.a = val;
			spiralColor.color = c;
		});
		spiral.SetActive(true);
		Eman.lm.audioManager.PlaySound("Teleport");
		await Task.Delay(600);
		transform.position = destination;
		map.AddFigureToTile(this, x, y);
		tileX = x;
		tileY = y;
		LeanTween.value(spiral, 1f, 0f, .3f).setOnUpdate((float val) => {
			Color c = spiralColor.color;
			c.a = val;
			spiralColor.color = c;
		});
		await Task.Delay(300);
		spiral.SetActive(false);
	}

	public void FaceTarget(Figure f) {
		Vector3 directionalLocal = transform.InverseTransformPoint(f.transform.position);
		if (directionalLocal.x > 0) {
			if (GetComponent<SpriteRenderer>().flipX == true) return;
			GetComponent<SpriteRenderer>().flipX = true;
			transform.position = new Vector3(transform.position.x - (xAdjustment*2), transform.position.y, transform.position.z);
		} else {
			if (GetComponent<SpriteRenderer>().flipX == false) return;
			GetComponent<SpriteRenderer>().flipX = false;
			transform.position = new Vector3(transform.position.x + (xAdjustment*2), transform.position.y, transform.position.z);
		}
	}

	public void PlayHurtSound() {
		Eman.lm.audioManager.PlaySound(hitSounds[Random.Range(0, hitSounds.Length)]);
    }
}
