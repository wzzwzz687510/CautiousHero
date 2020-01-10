using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace Wing.RPGSystem
{
    public enum AnimType
    {
        MovePath,
        MoveInstant,
        Cast,
        HPChange,
        ArmourPChange,
        SkillShift,
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
            :base(AnimType.MovePath,duration)
        {
            this.entityHash = entityHash;
            this.path = path;
        }
    }

    public class MoveInstantAnimClip : BaseAnimClip
    {
        public int entityHash;
        public Location destination;

        public MoveInstantAnimClip(int entityHash, Location destination, float duration = 0.5f)
            : base(AnimType.MoveInstant, duration)
        {
            this.entityHash = entityHash;
            this.destination = destination;
        }
    }

    public class CastAnimClip : BaseAnimClip
    {
        public CastType castType;
        public int skillHash;
        public Location start;
        public Location end;

        public CastAnimClip(CastType castType, int skillHash,
            Location from, Location to, float duration = 0.5f)
            : base(AnimType.Cast, duration)
        {
            this.castType = castType;
            this.skillHash = skillHash;
            start = from;
            end = to;
        }
    }

    public class HPChangeAnimClip : BaseAnimClip
    {
        public int entityHash;
        public int hp;
        public int maxHP;

        public HPChangeAnimClip(int entityHash, int hp,int maxHP, float duration = 0.5f)
            : base(AnimType.HPChange, duration)
        {
            this.entityHash = entityHash;
            this.hp = hp;
            this.maxHP = maxHP;
        }
    }

    public class ArmourPChangeAnimClip : BaseAnimClip
    {
        public int entityHash;
        public bool isPhysical;
        public int remainedNumber;

        public ArmourPChangeAnimClip(int entityHash,bool isPhysical,int remainedNumber, float duration = 0.5f)
            : base(AnimType.ArmourPChange, duration)
        {
            this.entityHash = entityHash;
            this.isPhysical = isPhysical;
            this.remainedNumber = remainedNumber;
        }
    }

    public class SkillShiftAnimClip : BaseAnimClip
    {
        public SkillShiftAnimClip(float duration = 0.2f)
            : base(AnimType.SkillShift, duration)
        {
        }
    }

    public class OutlineEntityAnimClip : BaseAnimClip
    {
        public int entityHash;
        public Color color;

        public OutlineEntityAnimClip(int entityHash, Color color, float duration = 0.5f)
            : base(AnimType.OutlineEntity, duration)
        {
            this.entityHash = entityHash;
            this.color = color;
        }
    }

    public class AnimationManager : MonoBehaviour
    {
        public static AnimationManager Instance { get; private set; }

        [Header("Setting")]
        public float animRate = 1.0f;
        public Vector3 effectOffset;
        public GameObject skillPrefab;

        public bool IsPlaying { get; private set; }
        public int Count { get { return clips.Count; } }

        private Queue<BaseAnimClip> clips = new Queue<BaseAnimClip>();
        private Transform effectHolder;
        private bool isWorldView => WorldMapManager.Instance.IsWorldView;
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
            StopAllCoroutines();
            clips.Clear();
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
                    float duration = movePathClip.duration * entity.MovePath.Length * animRate;
                    entity.transform.DOPath(movePathClip.path, duration);
                    for (int i = 0; i < movePathClip.path.Length; i++) {
                        var pathLoc = isWorldView ? movePathClip.path[i].WorldViewToLocation() : movePathClip.path[i].AreaViewToLocation();
                        yield return new WaitForSeconds(clip.duration * animRate);
                        entity.OnSortingOrderChanged?.Invoke(pathLoc.x + pathLoc.y * 8);
                    }
                    break;
                case AnimType.MoveInstant:
                    var moveInstantClip = clip as MoveInstantAnimClip;
                    if (!EntityManager.Instance.TryGetEntity(moveInstantClip.entityHash, out entity))
                        break;

                    entity.transform.position = isWorldView ? 
                        moveInstantClip.destination.ToWorldView(): moveInstantClip.destination.ToAreaView();
                    entity.OnSortingOrderChanged?.Invoke(moveInstantClip.destination.x + moveInstantClip.destination.y * 8);
                    break;
                case AnimType.Cast:
                    var castClip = clip as CastAnimClip;
                    if (castClip.skillHash.GetBaseSkill().castEffect.effectName == null) break;
                    GameObject effect;
                    Vector3 endPosition = effectOffset + (isWorldView ? 
                        castClip.end.ToWorldView() : castClip.end.ToAreaView());
                    switch (castClip.castType) {
                        case CastType.Instant:                            
                            effect = Instantiate(skillPrefab, endPosition, Quaternion.identity, effectHolder);
                            if (effect.TryGetComponent(out Animator instantAnim))
                                instantAnim?.Play(castClip.skillHash.GetBaseSkill().castEffect.effectName, 0);

                            //yield return StartCoroutine(PlayAnimation(clips.Dequeue()));
                            StartCoroutine(DelayDestory(effect, castClip.duration * animRate));
                            break;
                        case CastType.Trajectory:
                            Vector3 startPosition = effectOffset + (isWorldView ? 
                                castClip.start.ToWorldView() : castClip.start.ToAreaView());
                            effect = Instantiate(skillPrefab, startPosition, Quaternion.identity, effectHolder);
                            if (effect.TryGetComponent(out Animator trajectoryAnim))
                                trajectoryAnim?.Play(castClip.skillHash.GetBaseSkill().castEffect.effectName, 0);
                            int distance = AStarSearch.Heuristic(castClip.start, castClip.end);
                            effect.transform.DOMove(endPosition, castClip.duration * distance * animRate)
                                  .OnComplete(() => Destroy(effect));
                            yield return new WaitForSeconds(castClip.duration * distance * animRate);
                            break;
                        default:
                            break;
                    }

                    break;
                case AnimType.HPChange:
                    var hpChangeClip = clip as HPChangeAnimClip;
                    if (!EntityManager.Instance.TryGetEntity(hpChangeClip.entityHash, out entity))
                        break;
                    entity.HPChangeAnimation?.Invoke(hpChangeClip.hp, hpChangeClip.maxHP, hpChangeClip.duration * animRate);
                    yield return new WaitForSeconds(hpChangeClip.duration * animRate);
                    break;
                case AnimType.ArmourPChange:
                    var apChangeClip = clip as ArmourPChangeAnimClip;
                    if (!EntityManager.Instance.TryGetEntity(apChangeClip.entityHash, out entity))
                        break;
                    if (apChangeClip.isPhysical)
                        entity.ArmourPointsChangeAnimation?.Invoke(true, apChangeClip.remainedNumber);
                    else
                        entity.ArmourPointsChangeAnimation?.Invoke(false, apChangeClip.remainedNumber);
                    yield return new WaitForSeconds(apChangeClip.duration * animRate);
                    break;
                case AnimType.SkillShift:
                    var ssClip = clip as SkillShiftAnimClip;

                    break;
                case AnimType.Delay:
                    yield return new WaitForSeconds(clip.duration);
                    break;
                case AnimType.OutlineEntity:
                    var outlineEntityClip = clip as OutlineEntityAnimClip;
                    outlineEntityClip.entityHash.GetEntity().ChangeOutlineColor(outlineEntityClip.color);
                    break;
                case AnimType.Gameover:
                    yield return new WaitForSeconds(clip.duration);
                    OnGameoverEvent?.Invoke();                   
                    break;
                default:
                    break;
            }

            if (!isPlayAll) {
                IsPlaying = false;
                OnAnimCompleted?.Invoke();
            }
        }

        private IEnumerator DelayDestory(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(go);
        }
    }
}


