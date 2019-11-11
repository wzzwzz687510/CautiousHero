using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    public Location Loc { get; set; }

    private int movementPoint;
    private List<Location[]> paths = new List<Location[]>();

    private void Awake()
    {
    }

    public void MoveToTile(TileController tile, Stack<Location> path, bool anim = false)
    {
        Loc = tile.Loc;
        if (anim) {
            Location[] sortedPath = new Location[path.Count];
            for (int i = 0; i < sortedPath.Length; i++) {
                sortedPath[i] = path.Pop();
            }
            paths.Add(sortedPath);
           MoveAnimation();
        }
        else {
            transform.GetChild(0).localPosition = new Vector3(0, 5, 0);
            transform.GetChild(0).DOLocalMoveY(0.2f, 0.5f);
            transform.position = tile.transform.position;
        }
    }

    private void MoveAnimation()
    {
        int end = paths.Count - 1;
        Vector3[] points = new Vector3[paths[end].Length];
        for (int i = 0; i < points.Length; i++) {
            points[i] = paths[end][i];
        }

        transform.DOPath(points, 0.3f);
    }
}
