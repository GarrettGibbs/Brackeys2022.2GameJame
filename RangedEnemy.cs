using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RangedEnemy : EnemyFigure
{
    Figure target = null;
    [SerializeField] int range = 4;

    [SerializeField] GameObject projectile;
    [SerializeField] AudioClip throwAcid;
    [SerializeField] AudioClip acidLand;

    public async override Task TakeAction() {
        int distance = 50;
        foreach (Figure f in Eman.playerFigures) {
            int check = (map.GeneratePathTo(map.graph[f.tileX, f.tileY], map.graph[tileX, tileY], true).Count);
            if (check < distance && check != 0) {
                target = f;
                distance = check;
            }
        }
        List<Node> path = map.FindRangedAttackTargetPath(map.graph[tileX, tileY], map.graph[target.tileX, target.tileY], range);
        if (path.Count - 1 > movement) {
            int different = path.Count - movement;
            path.RemoveRange(path.Count - different, different);
        }
        await MoveToTile(path);
        if(map.GeneratePathTo(map.graph[tileX,tileY], map.graph[target.tileX, target.tileY], false).Count <= range) {
            FaceTarget(target);
            Eman.lm.audioManager.PlaySound(throwAcid);
            animator.SetTrigger("Attack");
            Vector3 targ = target.transform.position;
            targ.z = 0f;
            Vector3 objectPos = transform.position;
            targ.x = targ.x - objectPos.x;
            targ.y = targ.y - objectPos.y;
            float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
            GameObject effect = Instantiate(projectile, transform.position, Quaternion.identity);
            effect.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            LeanTween.move(effect, target.transform.position, .75f);
            await Task.Delay(750);
            Eman.lm.audioManager.PlaySound(acidLand);
            effect.GetComponent<Animator>().SetTrigger("Hit");
            target.animator.SetTrigger("Hit");
            target.PlayHurtSound();
            await Task.Delay(400);
            Destroy(effect);
            target.health -= damage;
            Eman.UpdateHealthIndicator(target);
            await target.CheckDeath();
            await Task.Delay(750);
        }
    }
}
