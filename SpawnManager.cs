using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] EncounterManager encounterManager;
    [SerializeField] MapManager map;
    [SerializeField] SpawnPoint[] playerFigures;
    [SerializeField] SpawnPoint[] startingEnemies;
    [SerializeField] List<SpawnPoint> remainingEnemies;
    [SerializeField] GameObject deathAnimation;
    [SerializeField] GameObject spawnAnimation;
    [SerializeField] GameObject penguin;

    List<Figure> penguins = new List<Figure>();

    public Task InitializeEncounter() {
        foreach(SpawnPoint sp in playerFigures) {
            SpawnFigure(sp, false);
        }
        foreach (SpawnPoint sp in startingEnemies) {
            SpawnFigure(sp, false);
        }
        return Task.CompletedTask;
    }

    public async void CheckEnemySpawn() {
        if (remainingEnemies.Count == 0 || encounterManager.enemyFigures.Count == 8) return;
        await SpawnFigure(remainingEnemies[0]);
        remainingEnemies.RemoveAt(0);
    }

    private async Task SpawnFigure(SpawnPoint sp, int pen = 0) {
        if(map.graph[(int)sp.pos.x, (int)sp.pos.y].figure != null){
            bool valid = false;
            foreach(Node n in map.graph[(int)sp.pos.x, (int)sp.pos.y].neighbours) {
                if(n.figure == null) {
                    sp.pos.x = n.graphX;
                    sp.pos.y = n.graphY;
                    valid = true;
                    break;
                }
            }
            if (!valid) return;
        }
        Vector3 spawnPoint = new Vector3(map.graph[(int)sp.pos.x, (int)sp.pos.y].x + sp.xAdjustment, map.graph[(int)sp.pos.x, (int)sp.pos.y].y + sp.yAdjustment, sp.pos.y);
        GameObject sA = (GameObject)Instantiate(spawnAnimation, spawnPoint, Quaternion.identity, map.transform);
        if(pen == 0) {
            encounterManager.lm.audioManager.PlaySound("Spawn");
            sA.GetComponent<SpriteRenderer>().color = Color.red;
        }
        await Task.Delay(1100);
        GameObject f = (GameObject)Instantiate(sp.toSpawnPrefab, spawnPoint, Quaternion.identity, map.transform);
        Figure figure = f.GetComponent<Figure>();
        map.graph[(int)sp.pos.x, (int)sp.pos.y].figure = figure;
        figure.SetupFigure((int)sp.pos.x, (int)sp.pos.y, map, sp.playerFigure, sp.portrait, encounterManager, sp.xAdjustment, sp.yAdjustment, deathAnimation);
        if (figure.isPenguin) {
            penguins.Add(figure);
            f.GetComponent<SpriteRenderer>().flipX = (Random.value > 0.5f);
        } else {
            encounterManager.AddFigure(figure);
        }
        await Task.Delay(250);
        Destroy(sA);
    }

    private void SpawnFigure(SpawnPoint sp, bool anim) {
        if (map.graph[(int)sp.pos.x, (int)sp.pos.y].figure != null) {
            bool valid = false;
            foreach (Node n in map.graph[(int)sp.pos.x, (int)sp.pos.y].neighbours) {
                if (n.figure == null) {
                    sp.pos.x = n.graphX;
                    sp.pos.y = n.graphY;
                    valid = true;
                    break;
                }
            }
            if (!valid) return;
        }
        Vector3 spawnPoint = new Vector3(map.graph[(int)sp.pos.x, (int)sp.pos.y].x + sp.xAdjustment, map.graph[(int)sp.pos.x, (int)sp.pos.y].y + sp.yAdjustment, sp.pos.y + Random.Range(.1f, .3f));
        GameObject f = (GameObject)Instantiate(sp.toSpawnPrefab, spawnPoint, Quaternion.identity, map.transform);
        Figure figure = f.GetComponent<Figure>();
        map.graph[(int)sp.pos.x, (int)sp.pos.y].figure = figure;
        figure.SetupFigure((int)sp.pos.x, (int)sp.pos.y, map, sp.playerFigure, sp.portrait, encounterManager, sp.xAdjustment, sp.yAdjustment, deathAnimation);
        if (figure.isPenguin) {
            penguins.Add(figure);
        } else {
            encounterManager.AddFigure(figure);
        }
    } 

    public async Task<List<Vector3>> SpawnPenguins() {
        List<Vector3> spawnPoints = new List<Vector3>();
        List<SpawnPoint> allSpawns = new List<SpawnPoint>();
        encounterManager.lm.audioManager.PlaySound("PenguinSpawn");
        for (int i = 0; i < 3; i++) {
            SpawnPoint sp = gameObject.AddComponent<SpawnPoint>();
            sp.toSpawnPrefab = penguin;
            sp.pos.x = Random.Range(0, map.mapSizeX);
            sp.pos.y = Random.Range(0, map.mapSizeY);
            if (map.graph[(int)sp.pos.x, (int)sp.pos.y].figure != null) {
                bool valid = false;
                foreach (Node n in map.graph[(int)sp.pos.x, (int)sp.pos.y].neighbours) {
                    if (n.figure == null) {
                        sp.pos.x = n.graphX;
                        sp.pos.y = n.graphY;
                        valid = true;
                        break;
                    }
                }
                if (!valid) continue;
            }
            sp.yAdjustment = 25f;
            DelayedSpawn(sp);
            spawnPoints.Add(new Vector3(map.graph[(int)sp.pos.x, (int)sp.pos.y].x + sp.xAdjustment, map.graph[(int)sp.pos.x, (int)sp.pos.y].y + sp.yAdjustment, sp.pos.y));
        }

        return spawnPoints;
    }

    private async void DelayedSpawn(SpawnPoint sp) {
        await Task.Delay(1200);
        SpawnFigure(sp, 1);
        encounterManager.lm.audioManager.PlayPenguinAmbiance();
    }

    public async Task MovePenguins() {
        if (penguins.Count == 0) return;
        Figure p = penguins[Random.Range(0, penguins.Count)];
        List<Node> potential = map.AllPointsInRange(map.graph[p.tileX, p.tileY], 3, true);
        Node target = potential[Random.Range(0, potential.Count)];
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
        await p.MoveToTile(map.GeneratePathTo(target, map.graph[p.tileX, p.tileY], true));
    }
}
