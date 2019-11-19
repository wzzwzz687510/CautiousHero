﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Wing.TileUtils;
using DG.Tweening;

namespace Wing.RPGSystem
{
    public enum AnimType
    {
        MovePath,
        MoveInstant,
        Cast,
        HPChange,
        Delay,
        OutlineEntity,
        Gameover
    }

    public class BaseAnimClip
    {
        public AnimType type;
        public float duration;

        public BaseAnimClip() { }

        public BaseAnimClip(AnimType type, float duration)
        {
            this.type = type;
            this.duration = duration;
        }
    }

    public class MovePathAnimClip : BaseAnimClip
    {
        public int entityHash;
        public Vector3[] path;

        public MovePathAnimClip(int entityHash, Vector3[] path, float duration = 0.5f)
        {
            this.type = AnimType.MovePath;
            this.duration = duration;
            this.entityHash = entityHash;
            this.path = path;
        }
    }

    public class MoveInstantAnimClip : BaseAnimClip
    {
        public int entityHash;
        public Location destination;

        public MoveInstantAnimClip(int entityHash, Location destination, float duration = 0.5f)
        {
            this.type = AnimType.MoveInstant;
            this.duration = duration;
            this.entityHash = entityHash;
            this.destination = destination;
        }
    }

    public class CastAnimClip : BaseAnimClip
    {
        public CastType castType;
        public GameObject castEffect;
        public Location start;
        public Location end;

        public CastAnimClip(CastType castType, GameObject effectPrefab,
            Location from, Location to, float duration = 0.5f)
        {
            this.type = AnimType.Cast;
            this.duration = duration;
            this.castType = castType;
            castEffect = effectPrefab;
            start = from;
            end = to;
        }
    }

    public class HPChangeAnimClip : BaseAnimClip
    {
        public int entityHash;
        public float ratio;

        public HPChangeAnimClip(int entityHash, float ratio, float duration = 0.5f)
        {
            this.type = AnimType.HPChange;
            this.duration = duration;
            this.entityHash = entityHash;
            this.ratio = ratio;
        }
    }

    public class OutlineEntityAnimClip : BaseAnimClip
    {
        public Entity entity;
        public Color color;

        public OutlineEntityAnimClip(Entity entity, Color color, float duration = 0.5f)
        {
            this.type = AnimType.OutlineEntity;
            this.duration = duration;
            this.entity = entity;
            this.color = color;
        }
    }

    public class AnimationManager : MonoBehaviour
    {
        public static AnimationManager Instance { get; private set; }
        public bool IsPlaying { get; private set; }
        public int Count { get { return clips.Count; } }

        private Queue<BaseAnimClip> clips = new Queue<BaseAnimClip>();
        private Transform effectHolder;

        private bool isPlayAll;
        private Coroutine currentCoroutine;

        [HideInInspector] public UnityEvent OnAnimCompleted;
        [HideInInspector] public UnityEvent OnGameoverEvent;

        private void Awake()
        {
            if (!Instance)
                Instance = this;

            effectHolder = new GameObject("Effect Holder").transform;
        }

        public void AddDelay(float duration)
        {
            AddAnimClip(new BaseAnimClip(AnimType.Delay, duration));
        }

        public void AddAnimClip(BaseAnimClip clip)
        {
            clips.Enqueue(clip);
        }

        public void PlayOnce(bool isInstant = true)
        {
            IsPlaying = true;
            if (isInstant) {
                PlayAnimClip(clips.Dequeue());
            }
            else {
                if (!isPlayAll)
                    StartCoroutine(WaitForPlayOnce());
            }
        }

        public void PlayAll()
        {
            IsPlaying = true;
            currentCoroutine = StartCoroutine(PlayAllAnim());
        }

        public void PlayAnimClip(BaseAnimClip clip)
        {
            currentCoroutine = StartCoroutine(PlayAnimation(clip));
        }

        public void Clear()
        {
            clips.Clear();
        }

        public void Stop()
        {
            StopAllCoroutines();
        }

        private IEnumerator WaitForPlayOnce()
        {         
            isPlayAll = true;
            yield return currentCoroutine;
            while (clips.Count != 0) {
                yield return StartCoroutine(PlayAnimation(clips.Dequeue()));
            }
            isPlayAll = false;
            IsPlaying = false;
            OnAnimCompleted?.Invoke();
        }

        private IEnumerator PlayAllAnim()
        {
            isPlayAll = true;
            while (clips.Count != 0 || AIManager.Instance.isCalculating) {
                if (clips.Count == 0) {
                    yield return null;
                    continue;
                }

                yield return StartCoroutine(PlayAnimation(clips.Dequeue()));
            }
            isPlayAll = false;
            IsPlaying = false;
            OnAnimCompleted?.Invoke();
        }

        private IEnumerator PlayAnimation(BaseAnimClip clip)
        {
            //Debug.Log(clip.type);
            Entity entity;
            switch (clip.type) {
                case AnimType.MovePath:
                    var movePathClip = clip as MovePathAnimClip;
                    if (!EntityManager.Instance.TryGetEntity(movePathClip.entityHash, out entity))
                        break;

                    //var pathLoc = (Location)movePathClip.path[movePathClip.path.Length - 1];
                    entity.transform.DOPath(entity.movePath, movePathClip.duration * entity.movePath.Length);
                    for (int i = 0; i < movePathClip.path.Length; i++) {
                        var pathLoc = (Location)movePathClip.path[i];
                        yield return new WaitForSeconds(clip.duration);
                        entity.OnSortingOrderChanged?.Invoke(pathLoc.x + pathLoc.y * 8);
                    }
                    break;
                case AnimType.MoveInstant:
                    var moveInstantClip = clip as MoveInstantAnimClip;
                    if (!EntityManager.Instance.TryGetEntity(moveInstantClip.entityHash, out entity))
                        break;

                    entity.transform.position = moveInstantClip.destination;
                    entity.OnSortingOrderChanged?.Invoke(moveInstantClip.destination.x + moveInstantClip.destination.y * 8);
                    break;
                case AnimType.Cast:
                    var castClip = clip as CastAnimClip;
                    GameObject effect;
                    Vector3 fix = new Vector3(0, 0.4f, 0);
                    switch (castClip.castType) {
                        case CastType.Instant:
                            effect = Instantiate(castClip.castEffect, castClip.end + fix, Quaternion.identity, effectHolder);
                            yield return new WaitForSeconds(castClip.duration);
                            Destroy(effect);
                            break;
                        case CastType.Trajectory:
                            effect = Instantiate(castClip.castEffect, castClip.start + fix, Quaternion.identity, effectHolder);
                            int distance = AStarSearch.Heuristic(castClip.start, castClip.end);
                            effect.transform.DOMove(castClip.end + fix, castClip.duration * distance).OnComplete(() => Destroy(effect));
                            yield return new WaitForSeconds(castClip.duration * distance);
                            break;
                        default:
                            break;
                    }

                    break;
                case AnimType.HPChange:
                    var hpChangeClip = clip as HPChangeAnimClip;
                    if (!EntityManager.Instance.TryGetEntity(hpChangeClip.entityHash, out entity))
                        break;
                    entity.OnHPChanged?.Invoke(hpChangeClip.ratio, hpChangeClip.duration);
                    yield return new WaitForSeconds(hpChangeClip.duration);
                    break;
                case AnimType.Delay:
                    yield return new WaitForSeconds(clip.duration);
                    break;
                case AnimType.OutlineEntity:
                    var outlineEntityClip = clip as OutlineEntityAnimClip;
                    outlineEntityClip.entity.ChangeOutlineColor(outlineEntityClip.color);
                    break;
                case AnimType.Gameover:
                    OnGameoverEvent?.Invoke();
                    yield return new WaitForSeconds(clip.duration);
                    break;
                default:
                    break;
            }

            if (!isPlayAll) {
                IsPlaying = false;
                OnAnimCompleted?.Invoke();
            }
        }

    }
}

