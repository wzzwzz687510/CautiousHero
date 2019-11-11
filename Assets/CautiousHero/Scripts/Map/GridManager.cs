using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

    private void Start()
    {
        RenderMap();
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

    public void ResetTiles()
    {
        foreach (var tile in tileDic.Values) {
            tile.ChangeTileState(TileState.Normal);
        } 
    }

    public void ChangeTileState(Location id, TileState state)
    {
        TileController tc;
        if (tileDic.TryGetValue(id, out tc))
            tc.ChangeTileState(state);
    }
}
