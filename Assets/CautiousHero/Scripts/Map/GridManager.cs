using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

public class GridManager : MonoBehaviour
{
    public GameObject prefab;
    public Sprite[] tileSprites;

    protected MapGenerator m_mapGenerator;

    private GameObject tileHolder;

    private void Awake()
    {
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
                    SpriteRenderer sr = Instantiate(prefab, new Vector3(0.524f * (x - y), -0.262f * (x + y), 0), Quaternion.identity, tileHolder.transform).GetComponent<SpriteRenderer>();
                    sr.sortingOrder = y * m_mapGenerator.width + x - m_mapGenerator.width * m_mapGenerator.height;
                    sr.sprite = tileSprites[m_mapGenerator.map[x, y] - 1];
                }
            }
        }
    }
}
