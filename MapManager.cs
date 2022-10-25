using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Figure SelectedUnit;
    public GameObject tilePrefab;
    [SerializeField] Sprite defaultTile;
    [SerializeField] Sprite[] alternateTiles;
    public EncounterManager EMan;

    public Node[,] graph;

    [SerializeField]
    private Vector3[] SpecialTiles; //z=tiletype

    public int mapSizeX = 10;
    public int mapSizeY = 10;

    float oddRowXOffset = 51f;
    float zOffset = 88f;
    float xOffset = 100f;

    int xStartOffset = 95;
    int yStartOffset = 240;

    void Start() {
        GeneratePathFindingGraph();
        GenerateMapVisual();
        //SpawnFigures();
        EMan.StartEncounter();
    }

    void GeneratePathFindingGraph() {
        graph = new Node[mapSizeX, mapSizeY];

        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                graph[x, y] = new Node();

                float xPos = x * xOffset;
                //odd row?
                if (y % 2 == 1) {
                    xPos += oddRowXOffset;
                }
                graph[x, y].x = xPos + xStartOffset;
                graph[x, y].y = (y * zOffset) + yStartOffset;

                graph[x, y].graphX = x;
                graph[x, y].graphY = y;
            }
        }
        for (int x = 0; x < mapSizeX; x++) {
            for (int y = 0; y < mapSizeY; y++) {
                if (x > 0) {
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                }
                if (x < mapSizeX - 1) {
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                }
                if (y > 0) {
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                }
                if (y < mapSizeY - 1) {
                    graph[x, y].neighbours.Add(graph[x, y + 1]);
                }
                if (y % 2 != 1) {
                    if (x > 0 && y > 0) {
                        graph[x, y].neighbours.Add(graph[x - 1, y - 1]);
                    }
                    if (y < mapSizeY - 1 && x > 0) {
                        graph[x, y].neighbours.Add(graph[x - 1, y + 1]);
                    }
                } else {
                    if (x < mapSizeX - 1 && y > 0) {
                        graph[x, y].neighbours.Add(graph[x + 1, y - 1]);
                    }
                    if (y < mapSizeY - 1 && x < mapSizeX - 1) {
                        graph[x, y].neighbours.Add(graph[x + 1, y + 1]);
                    }
                }
            }
        }
        //set special tiles
        foreach (Vector3 tile in SpecialTiles) {
            Node.TileTypes tileType = new Node.TileTypes();
            switch (tile.z) {
                case 0:
                    tileType = Node.TileTypes.Standard;
                    break;
                case 1:
                    tileType = Node.TileTypes.NonWalkable;
                    break;
                case 2:
                    tileType = Node.TileTypes.NonTargetable;
                    break;
            }
            graph[(int)tile.x, (int)tile.y].tileType = tileType;
        }
    }

    void GenerateMapVisual() {
        for (int y = mapSizeY-1; y > -1; y--) {
            for (int x = mapSizeX-1; x > -1; x--) {
                float xPos = x * xOffset;

                //odd row?
                if (y % 2 == 1) {
                    xPos += oddRowXOffset;
                }
                GameObject hex_go = (GameObject)Instantiate(tilePrefab, new Vector3(xPos + xStartOffset, (y * zOffset) + yStartOffset, 0), Quaternion.identity, this.transform);
                hex_go.name = $"Hex_{x}_{y}";
                //hex_go.transform.SetParent(this.transform);
                ClickableTile ct = hex_go.GetComponent<ClickableTile>();
                graph[x, y].tile = ct;
                ct.tileX = x;
                ct.tileY = y;
                ct.transform.localScale = Vector3.one;
                if(Random.Range(0, 3) != 0) {
                    ct.tileSprite.sprite = defaultTile;
                } else {
                    ct.tileSprite.sprite = alternateTiles[Random.Range(0,alternateTiles.Length)];
                }
                ct.EMan = EMan;
            }
        }
    }

    public List<Node> GeneratePathTo(Node target, Node startPoint, bool movement) {

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        List<Node> unvisited = new List<Node>();

        Node source = graph[startPoint.graphX, startPoint.graphY];
        //Node target = graph[x, y];

        dist[source] = 0;
        prev[source] = null;

        foreach (Node v in graph) {
            if (v != source) {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }
            unvisited.Add(v);
        }
        while (unvisited.Count > 0) {
            Node u = null;

            foreach (Node possibleU in unvisited) {
                if (u == null || dist[possibleU] < dist[u]) {
                    u = possibleU;
                }
            }

            unvisited.Remove(u);

            foreach (Node v in u.neighbours) {
                float alt = dist[u] + u.DistanceTo(v);
                if (movement) {
                    if (alt < dist[v] && graph[v.graphX, v.graphY].figure == null && graph[v.graphX, v.graphY].tileType == Node.TileTypes.Standard) {
                        dist[v] = alt;
                        prev[v] = u;
                    } else if (alt < dist[v] && v == target) {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                } else { //ranged targeting path allows through figures and over non-movable tiles
                    if (alt < dist[v] && graph[v.graphX, v.graphY].tileType != Node.TileTypes.NonTargetable) {
                        dist[v] = alt;
                        prev[v] = u;
                    } else if (alt < dist[v] && v == target) {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                }

            }
        }
        if (prev[target] == null) {
            //no route to target from source
            return new List<Node>();
        }

        List<Node> currentPath = new List<Node>();
        Node curr = target;
        while (curr != null) {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        currentPath.RemoveAt(currentPath.Count - 1);
        currentPath.Reverse();
        return currentPath;
    }

    public List<Node> FindRangedAttackTargetPath(Node startPoint, Node targetPoint, int attackRange) {
        List<Node> PathToTarget = new List<Node>();
        List<Node> rangedPath = GeneratePathTo(targetPoint, startPoint, false);
        if (rangedPath.Count == attackRange) {
            return PathToTarget;
        } else {
            List<Node> validTargetFromPoints = AllPointsInRange(targetPoint, attackRange, false);
            List<List<Node>> possiblePaths = new List<List<Node>>();
            foreach (Node n in validTargetFromPoints) {
                if (n.tileType != Node.TileTypes.Standard) continue;
                possiblePaths.Add(GeneratePathTo(n, startPoint, true));
            }
            PathToTarget = possiblePaths.OrderBy(w => w.Count).ToList()[0];
            return PathToTarget;
        }
    }

    public List<Node> AllPointsInRange(Node startingPoint, int range, bool includingInside) {
        List<Node> pointsInRange = new List<Node>();
        List<List<Node>> distances = new List<List<Node>>();
        for (int i = 0; i < range; i++) {
            List<Node> tempNodes = new List<Node>();
            if (i == 0) {
                foreach (Node n in startingPoint.neighbours) {
                    tempNodes.Add(n);
                }
            } else {
                foreach (Node n in distances[i - 1]) {
                    foreach (Node n2 in n.neighbours) {
                        tempNodes.Add(n2);
                    }
                }
            }
            distances.Add(tempNodes);
        }
        foreach (List<Node> list in distances) {
            foreach (Node n in list) {
                if (!pointsInRange.Contains(n)) {
                    pointsInRange.Add(n);
                }
            }
        }
        if (!includingInside) { //removes nodes that are not the exact range
            //pointsInRange = pointsInRange.Where(n => (GeneratePathTo(startingPoint, n, false).Count == range)).ToList();
            List<Node> tempList = new List<Node>();
            foreach (Node n in pointsInRange) {
                List<Node> distance = GeneratePathTo(startingPoint, n, false);
                if (distance == null) continue;
                if (distance.Count == range) {
                    tempList.Add(n);
                }
            }
            pointsInRange = tempList;
        }
        return pointsInRange;
    }

    public void AddFigureToTile(Figure figure, int tileX, int tileY) {
        graph[tileX, tileY].figure = figure;
    }

    public void RemoveFigureFromTile(int tileX, int tileY) {
        graph[tileX, tileY].figure = null;
    }
}
