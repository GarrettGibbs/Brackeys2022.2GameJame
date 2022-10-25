using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MeleeEnemy : EnemyFigure
{
    Figure target = null;

    public async override Task TakeAction() {
        int distance = 50;
        foreach(Figure f in Eman.playerFigures) {
            int check = (map.GeneratePathTo(map.graph[f.tileX, f.tileY], map.graph[tileX, tileY], true).Count);
            if(check < distance && check != 0) {
                target = f;
                distance = check;
            }
        }
        List<Node> path = map.GeneratePathTo(map.graph[target.tileX, target.tileY], map.graph[tileX, tileY], true);
        if(path.Count > 0) {
            path.Remove(path[path.Count - 1]);
            if (path.Count - 1 > movement) {
                int different = path.Count - movement;
                path.RemoveRange(path.Count - different, different);
            }
            await MoveToTile(path);
        }
        foreach (Node n in map.graph[tileX, tileY].neighbours) {
            if (n.figure == target && target != null) {
                Eman.lm.audioManager.PlaySound(attackSound);
                await Eman.attackFigure(target);
            }
        }
    }
}
