using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;
using DG.Tweening;
using SpriteGlow;

public class PlayerController : MonoBehaviour
{
    public Location Loc { get; set; }

    public int MovementPoint { get; private set; }
    private List<Location[]> paths = new List<Location[]>();

    protected SpriteRenderer m_sprite;
    protected SpriteGlowEffect m_playerOutline;
    protected BoxCollider2D m_collider;
    public SpriteRenderer Sprite { get { return m_sprite; } }

    private void Awake()
    {
        MovementPoint = 3;
        m_sprite = GetComponentInChildren<SpriteRenderer>();
        m_playerOutline = GetComponentInChildren<SpriteGlowEffect>();
        m_collider = GetComponentInChildren<BoxCollider2D>();
    }

    public void MoveToTile(TileController tile, Stack<Location> path, bool anim = false)
    {
        if (anim) {
            if (path.Count > MovementPoint)
                return;

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

        Loc = tile.Loc;
    }

    public void ChangeOutlineColor(Color c)
    {
        m_playerOutline.GlowColor = c;
    }

    public void SetActiveCollider(bool bl)
    {
        m_collider.enabled = bl;
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
