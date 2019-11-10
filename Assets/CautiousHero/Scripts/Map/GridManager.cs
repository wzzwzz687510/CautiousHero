using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Range(0,0.1f)]   public float animationDuration = 0.01f;

    public LayerMask tileLayer;
    public GameObject prefab;
    public Sprite[] tileSprites;

    protected MapGenerator m_mg;

    private GameObject tileHolder;
    private Dictionary<Location, TileController> tileDic 
        = new Dictionary<Location, TileController>();
    private TileNavigation m_astar;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        m_mg = GetComponent<MapGenerator>();
    }

    private void Start()
    {
       RenderMap();
       m_astar = new TileNavigation(m_mg.width, m_mg.height, m_mg.map);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            RenderMap();
        }

        if (Input.GetMouseButtonDown(0)) {
            var ray = Camera.main.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width,
                Input.mousePosition.y / Screen.height, Input.mousePosition.z));
            //Debug.DrawRay(ray.origin,10* ray.direction,Color.red,10);
            var hit = Physics2D.Raycast(ray.origin, ray.direction, 20, tileLayer);
            if (hit) {
                Debug.Log(hit.transform.parent.GetComponent<TileController>().Pos.ToString());
            }
        }
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
                    TileController tc = Instantiate(prefab, new Vector3(0.524f * (x - y), -0.262f * (x + y), 0), Quaternion.identity, tileHolder.transform).GetComponent<TileController>();
                    tc.Init_SpriteRenderer(pos, y * m_mg.width + x - m_mg.width * m_mg.height, m_mg.map[x, y] - 1, UnityEngine.Random.Range(0.01f, 1f));
                    tileDic.Add(pos, tc);
                }
            }
        }

        //Debug.Log("tile cnt:" + tileHolder.transform.childCount);
    }
}
