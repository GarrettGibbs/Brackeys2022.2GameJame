using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Archer : PlayerFigure
{
    public override bool CheckTarget(Figure target) {
        List<Node> inRange = map.AllPointsInRange(map.graph[tileX, tileY], range, true);
        foreach (Node n in inRange) {
            if (map.graph[tileX, tileY].neighbours.Contains(n)) continue;
            if (n.figure == target) return true;
        }
        return false;
    }

    public override void HandleWizardEffect(WizardEffects wizardEffect) {
        ResetEffects();
        switch (wizardEffect) {
            case WizardEffects.Teleport:
                teleporting = true;
                break;
            case WizardEffects.EmpowerArcher:
                range = 7;
                damage = 2;
                break;
        }
    }

    public override async Task<Figure> PlayAttackEffect(Figure target) {
        Eman.lm.audioManager.PlaySound("ArrowAttack");
        await Task.Delay(500);
        Eman.lm.audioManager.PlaySound("ArrowHit");
        return target;
    }
}
