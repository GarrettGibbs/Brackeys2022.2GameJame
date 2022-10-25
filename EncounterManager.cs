using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public Figure selectedFigure;
    [SerializeField] MapManager map;
    public LevelManager lm;
    public SpawnManager spawnManager;
    [SerializeField] TurnOrder[] turnOrders;
    [SerializeField] TurnOrder[] enemyTurnOrders;
    [SerializeField] Wizard wizard;

    public List<Figure> allFigures = new List<Figure>();
    public List<Figure> playerFigures = new List<Figure>();
    public List<Figure> enemyFigures = new List<Figure>();

    public bool playersAction = false;

    public WizardEffects wizardEffect;

    [SerializeField] int requiredKills;
    [SerializeField] int levelIndex;
    private int kills = 0;

    public bool KnightDead = false;
    public bool ClericDead = false;
    public bool ArcherDead = false;

    private bool levelEnded = false;
    private bool levelStarted = false;
    private bool won = false;
    private bool loss = false;

    public async void StartEncounter() {
        await spawnManager.InitializeEncounter();
        wizard.SetupWizard(playerFigures);
        selectedFigure = playerFigures[0];
        lm.audioManager.TransitionMusic(MusicType.Battle);
        await Task.Delay(1000);
        lm.progressManager.currentLevel = levelIndex;
        if (!lm.progressManager.levelsStared[levelIndex]) {
            lm.dialogueManager.startOfLevel.TriggerDialogue();
            lm.progressManager.levelsStared[levelIndex] = true;
            await Task.Delay(500);
        } else {
            levelStarted = true;
            wizard.activated = true;
            await Task.Delay(1000);
            NewPlayerRound();
        }
        lm.circleTransition.OpenBlackScreen();
    }

    public async void OutOfDialogue() {
        if (!levelStarted) {
            levelStarted = true;
            wizard.activated = true;
            await Task.Delay(1000);
            NewPlayerRound();
        } else if (won) {
            lm.NextLevel();
        } else if (loss) {
            lm.RestartLevel();
        }
    }

    public async void CheckClick(int x, int y) {
        lm.audioManager.PlaySound("Click");
        if (!playersAction || levelEnded) return;
        PlayerFigure pf = (PlayerFigure)selectedFigure;
        playersAction = false;
        pf.RemoveHighlight();
        if (map.graph[x, y].figure == null) {
            if (pf.GetMoveNodes().Contains(map.graph[x, y])) {
                turnOrders[playerFigures.IndexOf(selectedFigure)].UpdateActionPoints(-1);
                if (pf.teleporting) {
                    await selectedFigure.Teleport(x, y);
                } else {
                    List<Node> path = map.GeneratePathTo(map.graph[x, y], map.graph[selectedFigure.tileX, selectedFigure.tileY], true);
                    await selectedFigure.MoveToTile(path);
                }
            }
        } else {
            if (pf.targetAllies) {
                if ((map.graph[x, y].figure.playerFigure) && !map.graph[x, y].figure.isPenguin) {
                    if (pf.CheckTarget(map.graph[x, y].figure)) {
                        turnOrders[playerFigures.IndexOf(selectedFigure)].UpdateActionPoints(-1);
                        await attackFigure(map.graph[x, y].figure);
                    }
                }
            } else {
                if ((!map.graph[x, y].figure.playerFigure) && !map.graph[x, y].figure.isPenguin) {
                    if (pf.CheckTarget(map.graph[x, y].figure)) {
                        turnOrders[playerFigures.IndexOf(selectedFigure)].UpdateActionPoints(-1);
                        Figure tempTarget = map.graph[x, y].figure;
                        if (pf.isCleric && wizard.interferred < 2) {
                            tempTarget = await wizard.RedirectAttack(tempTarget);
                        }
                        await attackFigure(tempTarget);
                    }
                }
            }
        }
        if (turnOrders[playerFigures.IndexOf(selectedFigure)].actionPoints <= 0) {
            if (wizard.interferred < 2) {
                int rand = Random.Range(0, 3);
                if (rand == 0){
                    await wizard.RechargeAP(selectedFigure);
                    turnOrders[playerFigures.IndexOf(selectedFigure)].UpdateActionPoints(1);
                    pf.HightlightTiles();
                    playersAction = true;
                    return;
                }
            }
            NextTurn();
        } else {
            pf.HightlightTiles();
            playersAction = true;
        }
    }

    public async Task attackFigure(Figure target) {
        selectedFigure.FaceTarget(target);
        Figure newTarget = target;
        selectedFigure.animator.SetTrigger("Attack");
        if (selectedFigure.playerFigure) {
            PlayerFigure pf = (PlayerFigure)selectedFigure;
            newTarget = await pf.PlayAttackEffect(target);
            if(newTarget == null) {
                await Task.Delay(750);
                return;
            }
        } else {
            await Task.Delay(250);
        }
        newTarget.PlayHurtSound();
        newTarget.health -= selectedFigure.damage;
        UpdateHealthIndicator(newTarget);
        newTarget.animator.SetTrigger("Hit");
        await newTarget.CheckDeath();
        await Task.Delay(750);
    }

    public void AddFigure(Figure f) {
        allFigures.Add(f);
        if (f.playerFigure) {
            playerFigures.Add(f);
            foreach(TurnOrder to in turnOrders) {
                if (!to.gameObject.activeSelf) {
                    to.gameObject.SetActive(true);
                    to.SetupIndicator(f);
                    break;
                }
            }
        } else {
            enemyFigures.Add(f);
            foreach (TurnOrder to in enemyTurnOrders) {
                if (!to.gameObject.activeSelf) {
                    to.gameObject.SetActive(true);
                    to.SetupIndicator(f);
                    break;
                }
            }
        }
    }

    private async void NewPlayerRound() {
        if (levelEnded) return;
        wizardEffect = await wizard.NewWizardEffect();
        foreach(Figure f in playerFigures) {
            PlayerFigure pf = (PlayerFigure)f;
            pf.HandleWizardEffect(wizardEffect);
        }
        foreach(TurnOrder to in turnOrders) {
            to.actionPoints = 3;
            to.UpdateActionPoints(0);
        }
        NextTurn();
    }

    public async void NextTurn() {
        if (levelEnded) return;
        bool canStillGo = false;
        for (int i = 0; i < playerFigures.Count; i++) {
            if(turnOrders[i].actionPoints > 0) {
                canStillGo = true;
                selectedFigure = playerFigures[i];
                break;
            }
        }
        UpdateTurnOrders();
        if (!canStillGo) {
            playersAction = false;
            EnemyTurn();
        } else {
            if (wizard.interferred < 2) {
                int rand = Random.Range(0, 6);
                if (rand == 0) {
                    await wizard.TeleportRandomly(selectedFigure);
                }
            } else {
                wizard.CheckRandomDialogue();
            }
            if (levelEnded) return;
            PlayerFigure pf = (PlayerFigure)selectedFigure;
            lm.audioManager.PlaySound(pf.selectSound);
            pf.HightlightTiles();
            playersAction = true;
        }
    }

    private async void EnemyTurn() {
        if (levelEnded) return;
        selectedFigure = null;
        UpdateTurnOrders();
        foreach(Figure f in enemyFigures) {
            if (levelEnded) return;
            wizard.CheckRandomDialogue();
            selectedFigure = f;
            UpdateTurnOrders();
            EnemyFigure ef = (EnemyFigure)f;
            await ef.TakeAction();
        }
        spawnManager.CheckEnemySpawn();
        await spawnManager.MovePenguins();
        NewPlayerRound();
    }

    private void UpdateTurnOrders() {
        for (int i = 0; i < playerFigures.Count; i++) {
            if (playerFigures[i] == selectedFigure) {
                turnOrders[i].ToggleIndicator(true);
            } else {
                turnOrders[i].ToggleIndicator(false);
            }
        }
        for (int i = 0; i < enemyTurnOrders.Length; i++) {
            if (enemyTurnOrders[i].gameObject.activeSelf) {
                if (enemyTurnOrders[i].figure == selectedFigure) {
                    enemyTurnOrders[i].ToggleIndicator(true);
                } else {
                    enemyTurnOrders[i].ToggleIndicator(false);
                }
            }
        }
    }

    public async Task RemoveFigure(Figure f) {
        allFigures.Remove(f);
        switch (f.playerFigure) {
            case true:
                foreach (TurnOrder to in turnOrders) {
                    if (to.figure == f) {
                        to.gameObject.SetActive(false);
                        break;
                    }
                }
                PlayerFigure pf = (PlayerFigure)f;
                if(pf.isCleric) ClericDead = true;
                else if(pf.isKnight) KnightDead = true;
                else if(pf.isArcher) ArcherDead = true;
                playerFigures.Remove(f);
                CheckLoss();
                await Task.Delay(1000);
                break;
            case false:
                foreach(TurnOrder to in enemyTurnOrders) {
                    if(to.figure == f) {
                        to.gameObject.SetActive(false);
                        break;
                    }
                }
                enemyFigures.Remove(f);
                kills++;
                CheckWin();
                break;
        }
    }

    public async void SelectFigure(Figure f) {
        if (levelEnded) return;
        wizard.CheckRandomDialogue();
        PlayerFigure pf = (PlayerFigure)selectedFigure;
        pf.RemoveHighlight();
        if (wizard.interferred < 2) {
            int rand = Random.Range(0, 6);
            //int rand = 0;
            if (rand == 0) {
                await wizard.TeleportRandomly(f);
            }
        }
        pf = (PlayerFigure)f;
        pf.HightlightTiles();
        lm.audioManager.PlaySound(pf.selectSound);
        selectedFigure = f;
        UpdateTurnOrders();
    }

    public void SkipTurn() {
        if (!playersAction || levelEnded) return;
        playersAction = false;
        PlayerFigure pf = (PlayerFigure)selectedFigure;
        pf.RemoveHighlight();
        foreach(TurnOrder to in turnOrders) {
            to.actionPoints = 0;
            to.UpdateActionPoints(0);
        }
        EnemyTurn();
    }

    public void UpdateHealthIndicator(Figure f) {
        if(f.health < 0) f.health = 0;
        if (f.playerFigure) {
            turnOrders[playerFigures.IndexOf(f)].UpdateHealth(f.health);
        } else {
            foreach(TurnOrder to in enemyTurnOrders) {
                if (!to.gameObject.activeSelf) continue;
                if(to.figure == f) {
                    if(f.health == 0) {
                        to.gameObject.SetActive(false);
                    } else {
                        to.UpdateHealth(f.health);
                    }
                    break;
                }
            }
        }
    }

    private void CheckWin() {
        if(kills >= requiredKills) {
            lm.audioManager.PlaySound("Victory");
            won = true;
            levelEnded = true;
            playersAction = false;
            lm.dialogueManager.endOfLevel.TriggerDialogue();
        }
    }

    private void CheckLoss() {
        if(playerFigures.Count == 0) {
            lm.audioManager.PlaySound("Loss");
            loss = true;
            levelEnded = true;
            playersAction = false;
            lm.dialogueManager.restartLevel.TriggerDialogue();
        } else {
            wizard.DeathDialogue();
        }
    }
}
