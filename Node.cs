using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    public List<Node> neighbours;
    public float x;
    public float y;
    public int graphX;
    public int graphY;
    public Figure figure;
    public ClickableTile tile;
    public enum TileTypes { Standard, NonWalkable, NonTargetable };
    public TileTypes tileType;
    //public List<Reaction> reactions;

    public Node() {
        neighbours = new List<Node>();
        //reactions = new List<Reaction>();
    }

    public float DistanceTo(Node n) {
        if (n == null) {
            Debug.LogError("WTF");
        }
        return Vector2.Distance(new Vector2(x, y), new Vector2(n.x, n.y));
    }
}
