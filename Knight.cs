using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Knight : PlayerFigure
{
    public override bool CheckTarget(Figure target) {
        foreach (Node n in map.graph[tileX,tileY].neighbours) {
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
            case WizardEffects.EmpowerKnight:
                movement = 6;
                damage = 3;
                break;
        }
    }

    public override async Task<Figure> PlayAttackEffect(Figure target) {
        Eman.lm.audioManager.PlaySound("MeleeHeavy");
        await Task.Delay(500);
        return target;
    }
}
