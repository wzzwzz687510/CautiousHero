﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Range(0,0.1f)]   public float animationDuration = 0.01f;

    public GameObject prefab;
    public Sprite[] tileSprites;

    private MapGenerator m_mg;
    public Vector2 MapBoundingBox { get { return new Vector2(m_mg.width, m_mg.height); } }
    private GameObject tileHolder;
    private Dictionary<Location, TileController> tileDic 
        = new Dictionary<Location, TileController>();

    public delegate void OnCompleteMapRendering(MapGenerator generator);
    public event OnCompleteMapRendering onCompleteMapRenderEvent;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        m_mg = GetComponent<MapGenerator>();
    }

    private IEnumerator Start()
    {
        RenderMap();
        yield return new WaitForSeconds(2);
        onCompleteMapRenderEvent(m_mg);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.R)) {
        //    RenderMap();
        //}
    }

    private void RenderMap()
    {
        m_mg.GenerateMap(tileSprites.Length);

        if (tileHolder)
            Destroy(tileHolder);

        tileHolder = new GameObject("Tile Holder");
        tileDic = new Dictionary<Location, TileController>();

        for (int x = 0; x < m_mg.width; x++) {
            for (int y = 0; y < m_mg.height; y++) {
                if (m_mg.map[x, y] != 0) {
                    var pos = new Location(x, y);
                    TileController tc = Instantiate(prefab, new Vector3(0.524f * (x - y), -0.262f * (x + y), 0), 
                        Quaternion.identity, tileHolder.transform).GetComponent<TileController>();
                    tc.Init_SpriteRenderer(pos, y * m_mg.width + x - m_mg.width * m_mg.height, 
                        m_mg.map[x, y] - 1, UnityEngine.Random.Range(0.01f, 1f));
                    tileDic.Add(pos, tc);
                }
            }
        }
        //Debug.Log("tile cnt:" + tileHolder.transform.childCount);
    }

    public void ResetAllTiles()
    {
        foreach (var tile in tileDic.Values) {
            tile.ChangeTileState(TileState.Normal);
            tile.DebindCastLocation();
        } 
    }

    public bool IsLocationValid(Location id)
    {
        return tileDic.ContainsKey(id);
    }

    public TileController GetTileController(Location id)
    {
        TileController tc;
        if (tileDic.TryGetValue(id, out tc)) {
            return tc;
        }
        return null;
    }

    public bool ChangeTileState(Location id, TileState state)
    {
        if(!IsLocationValid(id))
            return false;

        GetTileController(id).ChangeTileState(state);
        return true;
    }

    public TileController GetRandomTile(bool isValid = true)
    {
        int cnt = 0;
        if (isValid) {
            var tilesEnumerator = ValidTiles().GetEnumerator();
            int random = Random.Range(0, tileDic.Count);
            for (int i = 0; i < random; i++) {
                if (--random < 0)
                    return tilesEnumerator.Current;
                if (!tilesEnumerator.MoveNext()) {
                    random = Random.Range(0, cnt);
                    for (int j = 0; j < random; j++) {
                        if (--random < 0)
                            return tilesEnumerator.Current;
                    }
                }
                cnt++;
            }

        }

        cnt = Random.Range(0, tileDic.Count);
        foreach (var tile in tileDic.Values) {
            if (--cnt < 0)
                return tile;
        }

        return null;
    }

    public IEnumerable<TileController> GetTrajectoryHitTile(Location id, Location dir, bool highlight = false)
    {
        TileController tc;
        if (!tileDic.TryGetValue(id, out tc))
            yield break;
        Location tmp = id;

        // Safe count for exit from while.
        int cnt = 0;
        while (tc.isEmpty) {
            if (highlight)
                tc.ChangeTileState(TileState.CastZone);
            yield return tc;
            tmp += dir;
            if (!tileDic.TryGetValue(tmp, out tc) || cnt++ > 8)
                yield break;
        }

        if (highlight)
            tc.ChangeTileState(TileState.CastSelected);
        yield return tc;
    }



    IEnumerable<TileController> ValidTiles()
    {
        foreach (var tile in tileDic.Values) {
            if (tile.isEmpty)
                yield return tile;
        }
    }

}
