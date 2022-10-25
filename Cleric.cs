using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Cleric : PlayerFigure {
    [SerializeField] GameObject projectile;

    [SerializeField] AudioClip healSound;
    enum AttackType {BasicFire, Skull, Sunstrike, Midas, Fireball}
    [SerializeField] AttackType currentAttack = AttackType.BasicFire;

    public override bool CheckTarget(Figure target) {
        List<Node> inRange = map.AllPointsInRange(map.graph[tileX, tileY], range, true);
        foreach(Node n in inRange) {
            if(n.figure == target) return true;
        }
        return false;
    }

    public override void HandleWizardEffect(WizardEffects wizardEffect) {
        currentAttack = AttackType.BasicFire;
        targetAllies = false;
        ResetEffects();
        switch (wizardEffect) {
            case WizardEffects.Teleport:
                teleporting = true;
                break;
            case WizardEffects.Fireball:
                damage = 3;
                currentAttack = AttackType.Fireball;
                break;
            case WizardEffects.Sunstrike:
                range = 6;
                currentAttack = AttackType.Sunstrike;
                break;
            case WizardEffects.Midas:
                range = 5;
                targetAllies = true;
                currentAttack = AttackType.Midas;
                break;
            case WizardEffects.Skull:
                range = 6;
                currentAttack = AttackType.Skull;
                break;
        }
    }

    public async override Task<Figure> PlayAttackEffect(Figure target) {
        Figure returnTarget = target;
        animator.SetTrigger("Attack");
        Eman.lm.audioManager.PlaySound("Fireball");
        Vector3 targ = target.transform.position;
        targ.z = 0f;
        Vector3 objectPos = transform.position;
        targ.x = targ.x - objectPos.x;
        targ.y = targ.y - objectPos.y;
        float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
        GameObject effect = Instantiate(projectile, transform.position, Quaternion.identity);
        effect.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        LeanTween.move(effect, target.transform.position, .75f);
        if(currentAttack == AttackType.Skull) {
            effect.GetComponent<Animator>().SetBool("Skull", true);
            Vector3 directionalLocal = transform.InverseTransformPoint(target.transform.position);
            if (directionalLocal.x > 0) {
                effect.GetComponent<SpriteRenderer>().flipY = false;
            } else {
                effect.GetComponent<SpriteRenderer>().flipY = true;
            }
        }
        await Task.Delay(750);
        switch (currentAttack) {
            case AttackType.Skull:
                Eman.lm.audioManager.PlaySound("FireballHit");
                List<Figure> tempTargets = new List<Figure>();
                foreach(Figure f in Eman.enemyFigures) {
                    if (f != target) tempTargets.Add(f);
                }
                if(tempTargets.Count > 0) {
                    target.PlayHurtSound();
                    target.health -= damage;
                    Eman.UpdateHealthIndicator(target);
                    target.animator.SetTrigger("Hit");
                    target.CheckDeath();
                    Figure newTarget = tempTargets[Random.Range(0, tempTargets.Count)];
                    Vector3 pos = newTarget.transform.position;
                    pos.z = 0f;
                    Vector3 newPos = effect.transform.position;
                    pos.x = pos.x - newPos.x;
                    pos.y = pos.y - newPos.y;
                    float newAngle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
                    effect.transform.rotation = Quaternion.Euler(new Vector3(0, 0, newAngle));
                    LeanTween.move(effect, newTarget.transform.position, .75f);
                    await Task.Delay(750);
                    Eman.lm.audioManager.PlaySound("FireballHit");
                    returnTarget = newTarget;
                }
                effect.transform.localScale = effect.transform.localScale*2;
                effect.GetComponent<Animator>().SetTrigger("SkullHit");
                await Task.Delay(370);
                break;
            case AttackType.BasicFire:
                target.PlayHurtSound();
                Eman.lm.audioManager.PlaySound("FireballHit");
                effect.GetComponent<Animator>().SetTrigger("Fire");
                await Task.Delay(420);
                break;
            case AttackType.Sunstrike:
                target.PlayHurtSound();
                Eman.lm.audioManager.PlaySound("FireballHit");
                List<Figure> additionalTargets = new List<Figure>();
                foreach(Node n in map.AllPointsInRange(map.graph[target.tileX,target.tileY], 2, true)) {
                    if(n.figure != null) {
                        if(!n.figure.isPenguin && !n.figure.playerFigure) {
                            additionalTargets.Add(n.figure);
                        }
                    }
                }
                foreach(Figure f in additionalTargets) {
                    f.health -= damage;
                    Eman.UpdateHealthIndicator(f);
                    f.animator.SetTrigger("Hit");
                    f.CheckDeath();
                }
                effect.transform.localScale = effect.transform.localScale * 5;
                effect.GetComponent<Animator>().SetTrigger("Sun");
                await Task.Delay(800);
                returnTarget = null;
                break;
            case AttackType.Midas:
                Eman.lm.audioManager.PlaySound(healSound);
                target.health += 2;
                Eman.UpdateHealthIndicator(target);
                effect.GetComponent<Animator>().SetTrigger("Midas");
                await Task.Delay(460);
                returnTarget = null;
                break;
            case AttackType.Fireball:
                target.PlayHurtSound();
                Eman.lm.audioManager.PlaySound("FireballHit");
                effect.transform.localScale = effect.transform.localScale * 2;
                effect.GetComponent<Animator>().SetTrigger("Fire");
                await Task.Delay(420);
                break;
        }
        Destroy(effect);
        return returnTarget;
    }
}
