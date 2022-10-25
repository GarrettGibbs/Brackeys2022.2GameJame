using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject toSpawnPrefab;
    public Vector2 pos;
    public Sprite portrait;
    public bool playerFigure;
    public float xAdjustment = 0f;
    public float yAdjustment = 50.5f;
}
