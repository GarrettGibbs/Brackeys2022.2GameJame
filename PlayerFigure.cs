using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class PlayerFigure : Figure
{
    public bool teleporting = false;
    public int range;
    private List<ClickableTile> highlightedTiles = new List<ClickableTile>();
    Color moveColor = new Color(203f/ 255f, 203f/ 255f, 203f/ 255f);
    Color targetColor = new Color(231f/ 255f, 76f/ 255f, 77f/ 255f);
    Color buffColor = new Color(38f/ 255f, 243f/ 255f, 0f/ 255f);
    Color selectedColor = new Color(148f/ 255f, 0f/ 255f, 255f/ 255f);

    int saveMovement;
    int saveDamage;
    int saveRange;

    public bool targetAllies = false;
    public bool isCleric = false;
    public bool isKnight = false;
    public bool isArcher = false;

    public string selectSound;

    [SerializeField] GameObject upgradeImage;

    private void Start() {
        saveDamage = damage;
        saveMovement = movement;
        saveRange = range;
    }

    public abstract bool CheckTarget(Figure target);
    public abstract void HandleWizardEffect(WizardEffects wizardEffect);
    public abstract Task<Figure> PlayAttackEffect(Figure target);

    public void HightlightTiles() {
        highlightedTiles.Clear();
        highlightedTiles.Add(map.graph[tileX,tileY].tile);
        map.graph[tileX, tileY].tile.HighlightTile(selectedColor);
        List<Node> walkTiles = GetMoveNodes();
        foreach (Node node in walkTiles) {
            if(node.figure == null) {
                highlightedTiles.Add(node.tile);
                node.tile.HighlightTile(moveColor);
            }
        }
        List<Node> attackTiles = map.AllPointsInRange(map.graph[tileX, tileY], range, true);
        foreach (Node node in attackTiles) {
            if (node.figure != null) {
                if (targetAllies) {
                    if (node.figure.playerFigure && CheckTarget(node.figure)) {
                        highlightedTiles.Add(node.tile);
                        node.tile.HighlightTile(buffColor);
                    } 
                } else if(!node.figure.isPenguin) {
                    if (!node.figure.playerFigure && CheckTarget(node.figure)) {
                        highlightedTiles.Add(node.tile);
                        node.tile.HighlightTile(targetColor);
                    }
                }
            }
        }
    }

    public List<Node> GetMoveNodes() {
        if (teleporting) {
            List<Node> teleportNodes = new List<Node>();
            for (int i = 5; i < 8; i++) {
                foreach(Node n in map.AllPointsInRange(map.graph[tileX,tileY], i, false)) {
                    if(!teleportNodes.Contains(n)) teleportNodes.Add(n);
                }
            }
            return teleportNodes;
        } else {
            return map.AllPointsInRange(map.graph[tileX, tileY], movement, true);
        }
    }

    public void RemoveHighlight() {
        foreach(ClickableTile tile in highlightedTiles) {
            tile.DeHighlightTile();
        }
    }

    public void ResetEffects() {
        teleporting = false;
        damage = saveDamage;
        movement = saveMovement;
        range = saveRange;
    }

    public async void UpgradeAnimation() {
        upgradeImage.GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;
        upgradeImage.SetActive(true);
        LeanTween.value(upgradeImage, 1f, 1.6f, .2f).setOnUpdate((float val) => {
            upgradeImage.transform.localScale = new Vector3(val, val, val);
        });
        await Task.Delay(200);
        LeanTween.value(upgradeImage, 1.6f, 1.4f, .3f).setOnUpdate((float val) => {
            upgradeImage.transform.localScale = new Vector3(val, val, val);
        });
        await Task.Delay(800);
        LeanTween.value(upgradeImage, 1.4f, 1f, .5f).setOnUpdate((float val) => {
            upgradeImage.transform.localScale = new Vector3(val, val, val);
        });
        await Task.Delay(500);
        upgradeImage.SetActive(false);
    }
}
