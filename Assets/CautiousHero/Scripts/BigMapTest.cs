using UnityEngine;
using Wing.RPGSystem;
using Cinemachine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

public class BigMapTest : MonoBehaviour
{
    [Header("Turn Switch Visual")]
    public Image turnBG;
    public Text turnText;

    public LayerMask tileLayer;
    public PlayerController player;
    public CinemachineVirtualCamera mainCamera;

    private TileController selectTile;
    private bool isMoving;

    private void Start()
    {
        GridManager.Instance.OnCompleteMapRenderEvent += OnCompleteMapRenderEvent;
        AnimationManager.Instance.OnAnimCompleted.AddListener(OnAnimComplete);
        StartCoroutine(BattleStart());
    }

    private void Update()
    {
        var ray = Camera.main.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height, Input.mousePosition.z));
        var hit = Physics2D.Raycast(ray.origin, ray.direction, 20, tileLayer);
        if (hit) {
            var tile = hit.transform.parent.GetComponent<TileController>();
            if (!tile.IsEmpty)
                return;
            if (!selectTile) {
                selectTile = tile;
                tile.ChangeTileState(TileState.MoveSelected);
            }

            if (!tile.Equals(selectTile)) {
                tile.ChangeTileState(TileState.MoveSelected);
                selectTile.ChangeTileState(TileState.Normal);
                selectTile = tile;
            }
            else {
                if (!isMoving && Input.GetMouseButtonDown(0)) {
                    player.SetActionPoints(9999);
                    player.MoveToTile(selectTile);
                    AnimationManager.Instance.PlayOnce();
                    selectTile.ChangeTileState(TileState.Normal);
                    selectTile = null;
                }
            }

        }
    }

    private void OnAnimComplete()
    {
        isMoving = false;
    }

    private void OnCompleteMapRenderEvent()
    {
        
        player.InitPlayer(Database.Instance.ActiveData.attribute, Database.Instance.GetEquippedSkills());
        player.MoveToTile(GridManager.Instance.GetRandomTile(true), true);
        player.EntitySprite.color = Color.white;
        mainCamera.enabled = true;
        DOTween.To(() => mainCamera.m_Lens.OrthographicSize, value => mainCamera.m_Lens.OrthographicSize = value,1.5f, 1);
    }

    public IEnumerator BattleStart()
    {
        while (!GridManager.Instance.isRendered) {
            yield return null;
        }
        //yield return new WaitForSeconds(1f);
        turnText.text = "地  图  生  成  中";
        turnText.color = Color.white;
        turnBG.fillAmount = 0;
        turnBG.color = Color.white;
        DOTween.To(() => turnBG.fillAmount, ratio => turnBG.fillAmount = ratio, 1f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        DOTween.ToAlpha(() => turnText.color, color => turnText.color = color, 1f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        DOTween.ToAlpha(() => turnBG.color, color => turnBG.color = color, 0f, 0.5f);
        DOTween.ToAlpha(() => turnText.color, color => turnText.color = color, 0f, 0.5f);
        yield return new WaitForSeconds(0.5f);
    }
}
