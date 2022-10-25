using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum WizardEffects { Nothing, Teleport, Fireball, Skull, Midas, Sunstrike, EmpowerKnight, EmpowerArcher, Penguin };

public class Wizard : MonoBehaviour
{
    [SerializeField] float speed = 1.0f; //how fast it shakes
    [SerializeField] float amount = 1.0f; //how much it shakes
    [SerializeField] Transform[] travelPoints;
    [SerializeField] SpriteRenderer[] spriteRenderers;
    [SerializeField] GameObject magicEffect;
    float timeSinceShift = 0f;
    float timeSinceTravel = 0f;
    int rand = 0;
    bool traveling = false;
    float randInterval = 0f;
    Vector3 middlePoint = new Vector3(960, 540, 0);
    
    [SerializeField] Animator animator;
    [SerializeField] EncounterManager encounterManager;
    [SerializeField] MapManager map;
    [SerializeField] AudioClip effectStart;
    [SerializeField] AudioClip effectLand;

    //[SerializeField] WizardEffects testWizardEffect;

    List<Transform> workingPoints = new List<Transform>();

    [SerializeField] Image effectIcon;
    [SerializeField] TMP_Text effectText;
    [SerializeField] TMP_Text effectDescription;

    [SerializeField] GameObject LeftSpeech;
    [SerializeField] TMP_Text LeftSpeechText;
    [SerializeField] GameObject RightSpeech;
    [SerializeField] TMP_Text RightSpeechText;

    Transform knight;
    Transform cleric;
    Transform archer;

    public bool activated = false;
    public bool acting = false;
    public int interferred = 0;
    public bool oneLiner = false;

    private int ranChance = 10;
    private string[] randomDialogue = new string[] { 
        "Sometimes magic is more about the art than following a spellbook.",
        "Excuse me, I was under the impression that we need to kill some demons.",
        "I wanted to let you know that you are not my worst batch of students... you are the second worst.",
        "You know, being dead is pretty cool, no more taxes and fetch quests.",
        "You know, I was thinking is this my afterlife or my after death?",
        "By the way guys, what did you do with my body?",
        "So, Hocus-pocus was once a common term for a magician, juggler, or other similar entertainers. I'm on wizardpedia because you guys bore me.",
        "I never understood why after attacking our enemies we have to sit and wait for them to attack as well.",
        "The duties of a teacher are neither few nor small, because we have to deal with students like you.",
        "I wouldn't have done that, and if I would I definitely wouldn't have done it like that.",
        "I used to be an optimist before having pupils."
        };
    private string[] teleportDialogue = new string[] {
        "This seems like a better spot for you.",
        "I always tried to push you farther.",
        "Get over there!",
        "Always like to help out.",
        "Weeeeeee!"
    };
    private string[] apDialogue = new string[] { 
        "You seemed like you needed a boost.",
        "Always happy to lend a hand!",
        "I'll give you that burst you need.",
        "Low stamina I see..."
    };
    private string[] redirectDialogue = new string[] { 
        "This seems like a better target.",
        "Let's put our sites on the real threat.",
        "You should be focusing on this one.",
        "Do I have to do everything around here?"
    };
    private string[] deathDialogue = new string[] {
        "Are you dying on purpose just to annoy me?",
        "I really don't miss the company.",
        "Are you even trying?"
    };
    private string[] penguinDialogue = new string[] {
        "Whoops... well I guess they are cute.",
        "That was not intentional...",
        "I think I've grown fond of the little guys."
    };

    public void SetupWizard(List<Figure> figures) {
        knight = figures[0].transform;
        cleric = figures[1].transform;
        archer = figures[2].transform;
    }

    void Update() {
        if (!activated) return;
        if (!acting) {
            if (timeSinceTravel > randInterval) MoveToPoint();
        }
        timeSinceShift += Time.deltaTime;
        if (timeSinceShift > 1.6f) {
            rand = Random.Range(0, 2);
            timeSinceShift = 0;
        }
        if(!traveling) {
            timeSinceTravel += Time.deltaTime;
            if (rand == 0) {
                transform.position = new Vector3(transform.position.x + Mathf.Sin(Time.time * speed) * amount, transform.position.y + Mathf.Sin(Time.time * speed) * amount, transform.position.z);
            } else {
                transform.position = new Vector3(transform.position.x + Mathf.Sin(Time.time * speed) * amount, transform.position.y - Mathf.Sin(Time.time * speed) * amount, transform.position.z);
            }
        }
        var rotationVector = transform.rotation.eulerAngles;
        rotationVector.z = Mathf.Sin(Time.time * speed) * 5;
        transform.rotation = Quaternion.Euler(rotationVector);
    }

    private async void MoveToPoint() {
        timeSinceTravel = 0;
        traveling = true;
        randInterval = Random.Range(5f, 10f);
        if(workingPoints.Count == 0) {
            foreach(Transform t in travelPoints) {
                workingPoints.Add(t);
            }
        }
        Transform point = workingPoints[Random.Range(0,workingPoints.Count)];
        LeanTween.move(gameObject, point, 2.5f);
        Vector3 directionalLocal = transform.InverseTransformPoint(point.position);
        if (directionalLocal.x > 0) {
            FlipWizard(true);
        } else {
            FlipWizard(false);
        }
        await Task.Delay(2505);
        directionalLocal = transform.InverseTransformPoint(middlePoint);
        if (directionalLocal.x > 0) {
            FlipWizard(true);
        } else {
            FlipWizard(false);
        }
        workingPoints.Remove(point);
        traveling = false;
    }

    private void FlipWizard(bool facingRight) {
        foreach(SpriteRenderer sr in spriteRenderers) {
            if (facingRight) {
                sr.flipX = true;
            } else {
                sr.flipX = false;
            }
        }
    }

    public async Task<WizardEffects> NewWizardEffect() {
        acting = true;
        interferred = 0;
        WizardEffects newEffect;
        int rand = Random.Range(0,4);
        if(rand == 0) {
            rand = Random.Range(0, 2);
            if(rand == 0) {
                newEffect = WizardEffects.Nothing;
            } else {
                newEffect = WizardEffects.Penguin;
            }
        } else {
            newEffect = (WizardEffects)Random.Range(1, 8);
        }
        //WizardEffects newEffect = testWizardEffect;
        LeanTween.scale(gameObject, new Vector3(1.2f, 1.2f, 1.2f), .5f);
        animator.SetTrigger("Attack");
        switch (newEffect) {
            case WizardEffects.Nothing:
                effectIcon.sprite = Resources.Load<Sprite>("Sprites/UI/WizardIcons/Nothing");
                effectText.text = "Nothing";
                effectDescription.text = "Nothing...";
                break;
            case WizardEffects.Teleport:
                effectIcon.sprite = Resources.Load<Sprite>("Sprites/UI/WizardIcons/Teleport");
                effectText.text = "Teleport";
                effectDescription.text = "Your team can teleport, but lose their normal movement.";
                break;
            case WizardEffects.Fireball:
                effectIcon.sprite = Resources.Load<Sprite>("Sprites/UI/WizardIcons/Fireball");
                effectText.text = "Improved Fireball";
                effectDescription.text = "Clara's fireball packs an extra punch.";
                break;
            case WizardEffects.Skull:
                effectIcon.sprite = Resources.Load<Sprite>("Sprites/UI/WizardIcons/Skull");
                effectText.text = "Flame Skull";
                effectDescription.text = "The Skull has a mind of it's own...";
                break;
            case WizardEffects.Midas:
                effectIcon.sprite = Resources.Load<Sprite>("Sprites/UI/WizardIcons/Midas");
                effectText.text = "Healing Flame";
                effectDescription.text = "Clara can only target her allies with this one.";
                break;
            case WizardEffects.Sunstrike:
                effectIcon.sprite = Resources.Load<Sprite>("Sprites/UI/WizardIcons/Sunstrike");
                effectText.text = "Sunstrike";
                effectDescription.text = "Target clusters of enemies with this one.";
                break;
            case WizardEffects.EmpowerKnight:
                effectIcon.sprite = Resources.Load<Sprite>("Sprites/UI/WizardIcons/EmpowerKnight");
                effectText.text = "Empower Knight";
                effectDescription.text = "Knigel can move farther and hit harder.";
                break;
            case WizardEffects.EmpowerArcher:
                effectIcon.sprite = Resources.Load<Sprite>("Sprites/UI/WizardIcons/EmpowerArcher");
                effectText.text = "Empower Crossbowman";
                effectDescription.text = "Archie can reach farther and hit harder.";
                break;
            case WizardEffects.Penguin:
                effectIcon.sprite = Resources.Load<Sprite>("Sprites/UI/WizardIcons/Penguin");
                effectText.text = "Penguins!?!";
                effectDescription.text = "They're so cute!!!";
                break;
        }
        effectIcon.gameObject.SetActive(true);
        effectText.gameObject.SetActive(true);
        if(newEffect == WizardEffects.Fireball || newEffect == WizardEffects.Skull || newEffect == WizardEffects.Sunstrike || newEffect == WizardEffects.Midas) {
            if (!encounterManager.ClericDead) {
                CreateAndSendEffect(cleric.position, cleric);
            }
        } else if(newEffect == WizardEffects.EmpowerKnight) {
            if (!encounterManager.KnightDead) {
                CreateAndSendEffect(knight.position, knight);
            }
        } else if (newEffect == WizardEffects.EmpowerArcher) {
            if (!encounterManager.ArcherDead) {
                CreateAndSendEffect(archer.position, archer);
            }
        } else if(newEffect == WizardEffects.Teleport) {
            if (!encounterManager.KnightDead) {
                CreateAndSendEffect(knight.position, knight);
            }
            if (!encounterManager.ClericDead) {
                CreateAndSendEffect(cleric.position, cleric);
            }
            if (!encounterManager.ArcherDead) {
                CreateAndSendEffect(archer.position, archer);
            }
        } else if(newEffect == WizardEffects.Penguin) {
            PenguinDialogue();
            List<Vector3> points = await encounterManager.spawnManager.SpawnPenguins();
            foreach(Vector3 p in points) {
                CreateAndSendEffect(p, null);
            }
        }
        await Task.Delay(1500);
        acting = false;
        return newEffect;
    }

    private async void CreateAndSendEffect(Vector3 target, Transform spot) {
        Vector3 targ = target;
        targ.z = 0f;
        Vector3 objectPos = transform.position;
        targ.x = targ.x - objectPos.x;
        targ.y = targ.y - objectPos.y;
        float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
        GameObject effect = Instantiate(magicEffect, transform.position, Quaternion.identity);
        effect.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
        LeanTween.move(effect, target, .75f);
        encounterManager.lm.audioManager.PlaySound(effectStart);
        await Task.Delay(500);
        LeanTween.scale(gameObject, new Vector3(1f, 1f, 1f), .2f);
        await Task.Delay(250);
        effect.GetComponent<Animator>().SetTrigger("Hit");
        encounterManager.lm.audioManager.PlaySound(effectLand);
        //Debug.Break();
        await Task.Delay(750);
        if(spot != null) {
            spot.GetComponent<PlayerFigure>().UpgradeAnimation();
        }
        Destroy(effect);
    }

    public async Task TeleportRandomly(Figure f) {
        Node target = map.graph[Random.Range(0, map.mapSizeX), Random.Range(0, map.mapSizeY)];
        if (target.figure != null) {
            bool valid = false;
            foreach (Node n in target.neighbours) {
                if (n.figure == null) {
                    target = n;
                    valid = true;
                    break;
                }
            }
            if (!valid) return;
        }
        TeleportDialogue();
        interferred++;
        CreateAndSendEffect(f.transform.position, null);
        await Task.Delay(1500);
        
        await f.Teleport(target.graphX, target.graphY);
    }

    public async Task RechargeAP(Figure f) {
        interferred++;
        CreateAndSendEffect(f.transform.position, null);
        ApDialogue();
        await Task.Delay(1500);
    }

    public async Task<Figure> RedirectAttack(Figure f) {
        int ran = Random.Range(0, 2);
        if (ran > 0) return f;
        Figure tempFigure = encounterManager.enemyFigures[Random.Range(0, encounterManager.enemyFigures.Count)];
        if(tempFigure == f) return f;
        interferred++;
        CreateAndSendEffect(tempFigure.transform.position, null);
        RedirectDialogue();
        await Task.Delay(1500);
        return tempFigure;
    }

    public async void OneLiner(string line) {
        oneLiner = true;
        Vector3 directionalLocal = transform.InverseTransformPoint(middlePoint);
        if (directionalLocal.x < 0) {
            LeftSpeech.SetActive(true);
            LeftSpeechText.text = line;
            CanvasGroup cg = LeftSpeech.GetComponent<CanvasGroup>();
            LeanTween.value(LeftSpeech, 0f, 1f, .3f).setOnUpdate((float val) => {
                cg.alpha = val;
            });
            await Task.Delay(6000);
            LeanTween.value(LeftSpeech, 1f, 0f, 1f).setOnUpdate((float val) => {
                cg.alpha = val;
            });
            await Task.Delay(1000);
            LeftSpeech.SetActive(false);
        } else {
            RightSpeech.SetActive(true);
            RightSpeechText.text = line;
            CanvasGroup cg = RightSpeech.GetComponent<CanvasGroup>();
            LeanTween.value(RightSpeech, 0f, 1f, .3f).setOnUpdate((float val) => {
                cg.alpha = val;
            });
            await Task.Delay(6000);
            LeanTween.value(RightSpeech, 1f, 0f, 1f).setOnUpdate((float val) => {
                cg.alpha = val;
            });
            await Task.Delay(1000);
            RightSpeech.SetActive(false);
        }
        oneLiner = false;
    }

    public void CheckRandomDialogue() {
        if (oneLiner || encounterManager.lm.progressManager.ranDialogueIndex >= randomDialogue.Length) return;
        if(Random.Range(0,ranChance) == 0) {
            ranChance = 10;
            OneLiner(randomDialogue[encounterManager.lm.progressManager.ranDialogueIndex]);
            encounterManager.lm.progressManager.ranDialogueIndex++;
        } else {
            ranChance = Mathf.Clamp(ranChance - 1, 2, 10);
        }
    }

    private void TeleportDialogue() {
        if (oneLiner || encounterManager.lm.progressManager.teleportDialogueIndex >= teleportDialogue.Length) return;
        OneLiner(teleportDialogue[encounterManager.lm.progressManager.teleportDialogueIndex]);
        encounterManager.lm.progressManager.teleportDialogueIndex++;
    }

    private void ApDialogue() {
        if (oneLiner || encounterManager.lm.progressManager.apDialogueIndex >= apDialogue.Length) return;
        OneLiner(apDialogue[encounterManager.lm.progressManager.apDialogueIndex]);
        encounterManager.lm.progressManager.apDialogueIndex++;
    }

    private void RedirectDialogue() {
        if (oneLiner || encounterManager.lm.progressManager.redirectDialogueIndex >= redirectDialogue.Length) return;
        OneLiner(redirectDialogue[encounterManager.lm.progressManager.redirectDialogueIndex]);
        encounterManager.lm.progressManager.redirectDialogueIndex++;
    }

    public void DeathDialogue() {
        if (oneLiner || encounterManager.lm.progressManager.deathDialogueIndex >= deathDialogue.Length) return;
        OneLiner(deathDialogue[encounterManager.lm.progressManager.deathDialogueIndex]);
        encounterManager.lm.progressManager.deathDialogueIndex++;
    }

    private void PenguinDialogue() {
        if (oneLiner || encounterManager.lm.progressManager.penguinDialogueIndex >= penguinDialogue.Length) return;
        OneLiner(penguinDialogue[encounterManager.lm.progressManager.penguinDialogueIndex]);
        encounterManager.lm.progressManager.penguinDialogueIndex++;
    }
}
