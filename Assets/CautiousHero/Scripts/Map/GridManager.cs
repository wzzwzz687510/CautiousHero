using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Range(0,0.1f)]   public float animationDuration = 0.01f;

    public GameObject prefab;
    public Sprite[] tileSprites;

    protected MapGenerator m_mapGenerator;

    private GameObject tileHolder;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        m_mapGenerator = GetComponent<MapGenerator>();
    }

    private void Start()
    {
       RenderMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            RenderMap();
        }
    }

    private void RenderMap()
    {
        m_mapGenerator.GenerateMap(tileSprites.Length);

        if (tileHolder)
            Destroy(tileHolder);

        tileHolder = new GameObject("Tile Holder");

        for (int x = 0; x < m_mapGenerator.width; x++) {
            for (int y = 0; y < m_mapGenerator.height; y++) {
                if (m_mapGenerator.map[x, y] != 0) {
                    TileController tc = Instantiate(prefab, new Vector3(0.524f * (x - y), -0.262f * (x + y), 0), Quaternion.identity, tileHolder.transform).GetComponent<TileController>();
                    tc.Init_SpriteRenderer(y * m_mapGenerator.width + x - m_mapGenerator.width * m_mapGenerator.height, m_mapGenerator.map[x, y] - 1, UnityEngine.Random.Range(0.01f, 1f));
                }
            }
        }

        //Debug.Log("tile cnt:" + tileHolder.transform.childCount);
    }
}
