using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public struct CastableSkill
    {
        public int creatureID;
        public CastSkillAction action;

        public CastableSkill(int creatureID, CastSkillAction action)
        {
            this.creatureID = creatureID;
            this.action = action;
        }
    }

    public struct CastSkillAction
    {
        public Location destination;
        public Location castLocation;

        public CastSkillAction(Location moveto, Location castLoc)
        {
            destination = moveto;
            castLocation = castLoc;
        }
    }

    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance { get; private set; }

        public GameObject creaturePrefab;

        private GameObject creatureHolder;

        public PlayerController player;
        public List<CreatureController> Creatures { get; private set; }
        public bool isCalculating { get; private set; }
        
        ///// player max damage
        //private int pMaxD;
        ///// player max damage skill ID
        //private int pMaxDID;
        //private HashSet<Location> playerEffectZone;
        //private HashSet<Location> damgerousZone;
        //private HashSet<Location> healingZone;

        private SimplePriorityQueue<int> possibleAttackedCreature;
        private CastSkillAction currentCSA;
        private List<CastableSkill> castableSkills;


        private void Awake()
        {
            if (!Instance)
                Instance = this;
        }

        public void Init(BattleConfig config)
        {
            if (creatureHolder)
                Destroy(creatureHolder);
            creatureHolder = new GameObject("Creature Holder");
            Creatures = new List<CreatureController>();
            for (int i = 0; i < config.creatureSets.Length; i++) {
                if (!config.creatureSets[i].location.IsValid()) {
                    Debug.LogError("Creature " + i + " location over edge, please check battle config!");
                    Creatures.Add(null);
                    continue;
                }
                Creatures.Add(Instantiate(creaturePrefab, creatureHolder.transform).GetComponent<CreatureController>());
                StartCoroutine(Creatures[i].InitCreature(config.creatureSets[i].tCreature,config.creatureSets[i].location));
            }

            castableSkills = new List<CastableSkill>();
        }

        public void OnBotTurnStart()
        {
            PrepareDecisionMaking();
            StartCoroutine(BotTurn());
        }

        public bool IsAllBotsDeath()
        {
            bool res = true;
            foreach (var creature in Creatures) {
                res &= creature.IsDeath;
            }
            return res;
        }

        #region Obsolute
        //private void CalculateHealingZone()
        //{
        //    healingZone = new HashSet<Location>();

        //    for (int creatureID = 0; creatureID < Creatures.Length; creatureID++) {
        //        for (int skillID = 0; skillID < Creatures[creatureID].Skills.Count; skillID++) {
        //            if (SkillCheck(Creatures[creatureID].Hash, skillID, Label.Healing)) {
        //                foreach (var loc in GridManager.Instance.CalculateEntityGivenSkillEffectZone(
        //                    Creatures[creatureID], false, skillID, damgerousZone)) {
        //                    if (!healingZone.Contains(loc))
        //                        healingZone.Add(loc);
        //                }
        //            }
        //        }
        //    }
        //}
        //private void CalculatePlayerEffectZone()
        //{
        //    damgerousZone = new HashSet<Location>();
        //    pMaxD = 0;
        //    pMaxDID = -1;

        //    for (int i = 0; i < player.Skills.Count; i++) {
        //        if (SkillCheck(player.Hash,i, Label.Damage)) {
        //            int damage = (player.Skills[i].GetBaseSkill() as BasicAttackSkill).CalculateValue(player.Hash, 1);
        //            if (damage > pMaxD) {
        //                pMaxD = damage;
        //                pMaxDID = i;
        //            }
        //        }
        //    }

        //    IEnumerable<Location> coll;
        //    if (pMaxDID != -1) {
        //        coll = GridManager.Instance.CalculateEntityGivenSkillEffectZone(player, true, pMaxDID);
        //        foreach (var loc in coll) {
        //            damgerousZone.Add(loc);
        //        }
        //    }

        //    GridManager.Instance.CalculateEntityEffectZone(player, new HashSet<Label>() { Label.Damage }, true, out playerEffectZone);

        //    // Test
        //    //GridManager.Instance.ResetAllTiles();
        //    //foreach (var item in damgerousZone) {
        //    //    GridManager.Instance.ChangeTileState(item, TileState.PlaceSelected);
        //    //}
        //}
        #endregion

        private void PrepareDecisionMaking()
        {
            currentCSA = new CastSkillAction();
            possibleAttackedCreature = new SimplePriorityQueue<int>();

            foreach (var creature in Creatures) {
                creature.OnTurnStarted();
            }
            CalculateCastableSkills();
        }

        private IEnumerator BotTurn()
        {
            isCalculating = true;
            AnimationManager.Instance.PlayAll();
            yield return StartCoroutine(DecisionMaking());
            isCalculating = false;
        }

        private IEnumerator DecisionMaking()
        {
            yield return StartCoroutine(CastGivenLabelSkill(Label.Combo1st));
            yield return StartCoroutine(CastGivenLabelSkill(Label.Combo2rd));
            yield return StartCoroutine(CastGivenLabelSkill(Label.Combo3th));

            // Cast left skills
            foreach (var skill in castableSkills) {
                CreatureController bot = Creatures[skill.creatureID];
                if (player.IsDeath) break;
                AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.red));
                AnimationManager.Instance.AddAnimClip(new BaseAnimClip(AnimType.Delay, 0.5f));
                bot.MoveToTile(skill.action.destination);
                bot.CastSkill(bot.NextCastSkillID, skill.action.castLocation);
                if (player.IsDeath) break;
                AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.black));
                castableSkills.Remove(skill);
                yield return null;
            }

            // Consume spare action points
            foreach (var bot in Creatures) {
                int sparePoints = bot.ActionPoints + bot.ActionPointsPerTurn - bot.MaxActionPoints;
                if (sparePoints > 0) {
                    int minDistance = bot.Loc.Distance(bot.NextSkillTarget.Loc);
                    Location nearestEL = bot.Loc;
                    foreach (var el in bot.NextSkill.GetEffectZone(bot.Loc)) {
                        int distance = el.Distance(bot.NextSkillTarget.Loc);
                        if (minDistance > distance) {
                            minDistance = distance;
                            nearestEL = el;
                            if (minDistance == 0) break;
                        }
                    }
                    if (minDistance != 0) {
                        Location deltaLoc = bot.NextSkillTarget.Loc - nearestEL;
                        int deltaDistance = minDistance - sparePoints;
                        if (deltaDistance > 0) {
                            int ddd = deltaDistance - deltaLoc.x;
                            if (ddd > 0) {
                                deltaLoc = new Location(0, deltaLoc.y - ddd);
                            }
                            else {
                                deltaLoc = new Location(deltaLoc.x - deltaDistance, deltaLoc.y);
                            }
                        }

                        Location destination = bot.Loc + deltaLoc;
                        if (bot.Loc.HasPath(destination)) {
                            AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.red));
                            bot.MoveToTile(destination);
                            AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.black));
                        }
                    }
                }
            }
        }

        private IEnumerator CastGivenLabelSkill(Label label)
        {
            foreach (var skill in castableSkills) {
                CreatureController bot = Creatures[skill.creatureID];
                if (bot.NextSkill.labels.Contains(label)) {
                    if (player.IsDeath) break;
                    AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.red));
                    AnimationManager.Instance.AddAnimClip(new BaseAnimClip(AnimType.Delay, 0.5f));
                    bot.MoveToTile(skill.action.destination);
                    bot.CastSkill(bot.NextCastSkillID, skill.action.castLocation);
                    SetNextSkillTarget(skill.creatureID);
                    if (player.IsDeath) break;
                    AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.black));
                    castableSkills.Remove(skill);
                    yield return null;
                }
            }
        }

        private void SetNextSkillTarget(int creatureID)
        {
            CreatureController bot = Creatures[creatureID];
            CreatureController nextBot = Creatures[(creatureID == Creatures.Count - 1 ? 0 : creatureID + 1)];

            int targetHash = bot.NextSkill.labels.Contains(Label.Ally) ? nextBot.Hash : player.Hash;
            bot.SetNextSkillTarget(targetHash);
        }

        private bool SkillCheck(int entityHash,int skillID, Label label)
        {
            BaseSkill skill = entityHash.GetEntity().SkillHashes[skillID].GetBaseSkill();
            return skill.labels.Contains(label) && entityHash.GetEntity().ActionPoints >= skill.actionPointsCost;
        }

        private void CalculateCastableSkills()
        {
            castableSkills.Clear();
            bool findAction;
            for (int creatureID = 0; creatureID < Creatures.Count; creatureID++) {
                CreatureController bot = Creatures[creatureID];               
                findAction = false;
                int moveSteps = (bot.ActionPoints - bot.NextSkill.actionPointsCost) / bot.MoveCost;
                if (moveSteps<0) continue;

                // Ally skill cast to next bot to simplify the AI complexity. Increase the skill design and config set complexity.
                Location targetLoc = bot.NextSkillTarget.Loc;
                foreach (var cp in bot.NextSkill.CastPattern) {
                    foreach (var loc in bot.NextSkill.GetSubEffectZone(bot.Loc,cp)) {
                        if (loc.Distance(player.Loc) <= moveSteps) {
                            Location moveto = bot.Loc + targetLoc - loc;
                            if (bot.Loc.HasPath(moveto)) {
                                castableSkills.Add(new CastableSkill(creatureID, new CastSkillAction(moveto, moveto + cp)));
                                findAction = true;
                            }
                        }
                        if (findAction) break;
                    }
                    if (findAction) break;
                }
            }
        }

        //private bool HasCastableGivenLabelSkill(int index, Label label)
        //{
        //    var skills = Creatures[index].ActiveSkills;
        //    bool hasSkill = false;
        //    for (int i = 0; i < skills.Length; i++) {
        //        if (SkillCheck(skills[i], label))
        //            hasSkill = true;
        //    }

        //    return hasSkill;
        //}

        //private bool TryCastGivenIDSkill(int index,int skillID, Location targetLoc)
        //{
        //    var skill = Creatures[index].ActiveSkills[skillID];
        //    foreach (var csa in GridManager.Instance.CalculateCastSkillTile(Creatures[index], skillID, targetLoc)) {
        //        currentCSA = csa;
        //        CastSkillActionAnimation(index, skillID);
        //        return true;
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// Try to cast given label of skill, if caster don't have place to cast, then do nothing. (Have AP check already)
        ///// </summary>
        ///// <param name="index">Cast creature ID</param>
        ///// <param name="label">Cast skill label</param>
        ///// <param name="targetLoc">Skill target</param>
        ///// <returns></returns>
        //private bool TryCastGivenLabelSkill(int index, Label label, Location targetLoc)
        //{
        //    var skills = Creatures[index].ActiveSkills;
        //    for (int i = 0; i < skills.Length; i++) {
        //        if (SkillCheck(skills[i], label) && !skills[i].TSkill.labels.Contains(Label.SuicideAttack)) {
        //            return TryCastGivenIDSkill(index, i, targetLoc);
        //        }
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// Try to cast given label of skill, if caster don't have place to cast, then do nothing. (Have AP check already)
        ///// </summary>
        ///// <param name="index">Cast creature ID</param>
        ///// <param name="label">Cast skill label</param>
        ///// <param name="target">Skill target</param>
        ///// <param name="avoidZone">Avoid move into this zone to cast skill</param>
        ///// <returns></returns>
        //private bool TryCastGivenLabelSkill(int index, Label label, Location targetLoc, HashSet<Location> avoidZone)
        //{
        //    var skills = Creatures[index].ActiveSkills;
        //    for (int i = 0; i < skills.Length; i++) {
        //        if (SkillCheck(skills[i], label)) {
        //            foreach (var csa in GridManager.Instance.CalculateCastSkillTile(Creatures[index], i, targetLoc)) {
        //                if (!avoidZone.Contains(csa.destination.Loc)) {
        //                    currentCSA = csa;
        //                    CastSkillActionAnimation(index, i);
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}

        private void CastSkillActionAnimation(int index,int skillID)
        {
            Creatures[index].MoveToTile(currentCSA.destination);
            Creatures[index].CastSkill(skillID, currentCSA.castLocation);
        }

    }
}
