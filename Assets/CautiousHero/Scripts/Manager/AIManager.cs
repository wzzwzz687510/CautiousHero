using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public struct SkillInfo
    {
        public int index;
        public int skillID;
        public int cooldown;

        public void Update()
        {
            if (cooldown > 0)
                cooldown--;
        }
    }

    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance { get; private set; }

        public GameObject creaturePrefab;

        private GameObject creatureHolder;

        public PlayerController player;
        public CreatureController[] Creatures { get; private set; }
        public bool isCalculating { get; private set; }

        /// player max damage
        private int pMaxD;
        /// player max damage skill ID
        private int pMaxDID;

        private HashSet<Location> playerEffectZone;
        private HashSet<Location> damgerousZone;
        private HashSet<Location> healingZone;
        private SimplePriorityQueue<int> possibleAttackedCreature;
        private CastSkillAction currentCSA;


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
            Creatures = new CreatureController[config.creatureSets.Length];
            for (int i = 0; i < config.creatureSets.Length; i++) {
                if (!GridManager.Instance.IsValidLocation(config.creatureSets[i].location)) {
                    Debug.LogError("Creature " + i + " location over edge, please check battle config!");
                    continue;
                }
                Creatures[i] = Instantiate(creaturePrefab, creatureHolder.transform).GetComponent<CreatureController>();
                StartCoroutine(Creatures[i].InitCreature(config.creatureSets[i].tCreature,
                     GridManager.Instance.GetTileController(config.creatureSets[i].location)));
            }
        }

        public void OnBotTurnStart()
        {
            PrepareDecisionMaking();
            StartCoroutine( BotTurn());
        }

        public bool IsAllBotsDeath()
        {
            bool res = true;
            foreach (var creature in Creatures) {
                res &= creature.IsDeath;
            }
            return res;
        }

        // Avoid dangerous zone 
        private void CalculateHealingZone()
        {
            healingZone = new HashSet<Location>();

            for (int i = 0; i < Creatures.Length; i++) {
                for (int j = 0; j < Creatures[i].Skills.Length; j++) {
                    if (SkillCheck(Creatures[i].ActiveSkills[j], Label.Healing)) {
                        foreach (var loc in GridManager.Instance.CalculateEntityGivenSkillEffectZone(
                            Creatures[i], false, j, damgerousZone)) {
                            if (!healingZone.Contains(loc))
                                healingZone.Add(loc);
                        }
                    }
                }
            }
        }

        private void CalculatePlayerEffectZone()
        {
            damgerousZone = new HashSet<Location>();
            pMaxD = 0;
            pMaxDID = -1;

            for (int i = 0; i < player.ActiveSkills.Length; i++) {
                if (SkillCheck(player.ActiveSkills[i], Label.Damage)) {
                    int damage = (player.ActiveSkills[i].TSkill as BasicAttackSkill).CalculateValue(player.Hash, 1);
                    if (damage > pMaxD) {
                        pMaxD = damage;
                        pMaxDID = i;
                    }
                }
            }

            IEnumerable<Location> coll;
            if (pMaxDID != -1) {
                coll = GridManager.Instance.CalculateEntityGivenSkillEffectZone(player, true, pMaxDID);
                foreach (var loc in coll) {
                    damgerousZone.Add(loc);
                }
            }

            GridManager.Instance.CalculateEntityEffectZone(player, new HashSet<Label>() { Label.Damage }, true, out playerEffectZone);

            // Test
            //GridManager.Instance.ResetAllTiles();
            //foreach (var item in damgerousZone) {
            //    GridManager.Instance.ChangeTileState(item, TileState.PlaceSelected);
            //}
        }

        private void PrepareDecisionMaking()
        {
            currentCSA = new CastSkillAction();
            possibleAttackedCreature = new SimplePriorityQueue<int>();
            //var timer = System.DateTime.Now;
            CalculatePlayerEffectZone();
            //Debug.Log((System.DateTime.Now - timer).TotalSeconds.ToString());
            //timer = System.DateTime.Now;
            CalculateHealingZone();
            //Debug.Log((System.DateTime.Now - timer).TotalSeconds.ToString());

            foreach (var creature in Creatures) {
                creature.OnTurnStarted();
            }
        }

        private IEnumerator BotTurn()
        {
            isCalculating = true;
            AnimationManager.Instance.PlayAll();
            for (int i = 0; i < Creatures.Length; i++) {
                if (Creatures[i].IsDeath)
                    continue;

                if (player.IsDeath) break;
                AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(Creatures[i], Color.red));
                AnimationManager.Instance.AddAnimClip(new BaseAnimClip(AnimType.Delay, 0.5f));
                DecisionMaking(i);
                if (player.IsDeath) break;
                AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(Creatures[i], Color.black));
                yield return null;
            }
            isCalculating = false;
        }

        private void DecisionMaking(int index)
        {
            if (!KillPlayer(index)) {
                if (playerEffectZone.Contains(Creatures[index].Loc)) {
                    AvoidPlayerEffect(index);
                }
                else {
                    ApplyStrategy(index, true);
                }
            }
        }

        // cover alley in safe location
        private bool CoverAlley(int index)
        {
            if (!GridManager.Instance.TryGetTileOutsideZone(Creatures[index], player, playerEffectZone, out TileController tc))
                return false;

            var creature = Creatures[possibleAttackedCreature.Dequeue()];
            for (int i = 0; i < player.ActiveSkills.Length; i++) {
                if (SkillCheck(player.ActiveSkills[i], Label.Damage)) {
                    var ppas = GridManager.Instance.CalculateCastSkillTile(player, i, creature.Loc, true);

                    foreach (var pLoc in ppas) {
                        for (int j = 0; j < Creatures[index].Skills.Length; j++) {
                            if (SkillCheck(Creatures[index].ActiveSkills[j], Label.Damage)) {
                                var ccsa = GridManager.Instance.CalculateCastSkillTile(Creatures[index], j, pLoc.destination.Loc);
                                foreach (var csa in ccsa) {
                                    if (!playerEffectZone.Contains(csa.destination.Loc)) {
                                        Creatures[index].MoveToTile(csa.destination);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void ApplyStrategy(int index, bool isAvoidZone)
        {
            if (player.IsDeath) return;
            HealAlley(index, isAvoidZone);
            BuffAlley(index, isAvoidZone);

            // Costful
            //if (possibleAttackedCreature.Count != 0) {
            //    if (CoverAlley(index)) return;
            //}

            if (pMaxD >= Creatures[index].HealthPoints) {
                var coll = GridManager.Instance.TryGetLocationOutsideZone(Creatures[index], player, damgerousZone);
                var origin = Creatures[index].Loc;
                Location minLoc = new Location();
                int minDistance = 99, tmp;

                foreach (var loc in coll) {
                    tmp = loc.GetDistance(origin);
                    if (minDistance > tmp) {
                        minLoc = loc;
                        minDistance = tmp;
                    }
                }

                Creatures[index].MoveToTile(minLoc.GetTileController());
            }
            else if (TryCastGivenLabelSkill(index, Label.DefenseBuff, Creatures[index].Loc)) {
            }
            else {
                TryCastGivenLabelSkill(index, Label.Damage, player.Loc);
            }
            possibleAttackedCreature.Enqueue(index, Creatures[index].Loc.GetDistance(player.Loc));
        }

        private void AttackPlayer(int index)
        {
            if (Creatures[index].ActionPoints == 0 || !HasCastableGivenLabelSkill(index, Label.Damage))
                return;

            TryCastGivenLabelSkill(index, Label.Damage, player.Loc);
        }

        private void AttackPlayer(int index, HashSet<Location> avoidZone)
        {
            if (Creatures[index].ActionPoints == 0 || !HasCastableGivenLabelSkill(index, Label.Damage))
                return;

            TryCastGivenLabelSkill(index, Label.Damage, player.Loc, avoidZone);
        }

        private void BuffAlley(int index,bool isAvoidZone)
        {
            if (Creatures[index].ActionPoints == 0 || !HasCastableGivenLabelSkill(index, Label.StrengthenBuff))
                return;

            int alleyID = -1;
            for (int i = 0; i < Creatures.Length; i++) {
                if (playerEffectZone.Contains(Creatures[i].Loc))
                    continue;

                foreach (var skill in Creatures[i].ActiveSkills) {
                    if (!SkillCheck(skill, Label.Damage))
                        continue;
                    foreach (var el in skill.TSkill.GetEffectZone(Creatures[i].Loc)) {
                        if (el.Equals(player.Loc)) {
                            alleyID = i;
                            break;
                        }
                    }
                    if (alleyID != -1)
                        break;
                }
            }

            if (alleyID != -1) {
                if (isAvoidZone) {
                    if (!TryCastGivenLabelSkill(index, Label.StrengthenBuff, Creatures[alleyID].Loc, playerEffectZone))
                        TryCastGivenLabelSkill(index, Label.StrengthenBuff, Creatures[alleyID].Loc, playerEffectZone);
                }
                else {
                    TryCastGivenLabelSkill(index, Label.StrengthenBuff, Creatures[alleyID].Loc);
                }
            }
        }

        private void HealAlley(int index,bool isAvoidZone)
        {
            if (Creatures[index].ActionPoints == 0 || !HasCastableGivenLabelSkill(index, Label.Healing))
                return;

            int lowestHP = Creatures[0].HealthPoints;
            int lowestID = -1;
            for (int i = 0; i < Creatures.Length; i++) {
                if (!Creatures[i].IsDeath && Creatures[i].HealthPoints < pMaxD && 
                    Creatures[i].HealthPoints!= Creatures[i].MaxHealthPoints) {
                    if (lowestID == -1 || Creatures[i].HealthPoints < lowestHP) {
                        lowestID = i;
                        lowestHP = Creatures[i].HealthPoints;
                    }
                }
            }
            
            if (lowestID != -1) {
                if (isAvoidZone) {
                    if (!TryCastGivenLabelSkill(index, Label.Healing, Creatures[lowestID].Loc, playerEffectZone))
                        TryCastGivenLabelSkill(index, Label.Healing, Creatures[lowestID].Loc, damgerousZone);
                }
                else {
                    TryCastGivenLabelSkill(index, Label.Healing, Creatures[lowestID].Loc);
                }
            }               
        }

        private void AvoidPlayerEffect(int index)
        {
            if (!TryCastGivenLabelSkill(index, Label.HardControl, player.Loc)) {
                if (!TryCastGivenLabelSkill(index, Label.SoftControl, player.Loc)) {
                    if (!TryCastGivenLabelSkill(index, Label.Obstacle, player.Loc)) {
                        if (pMaxD >= Creatures[index].HealthPoints) {
                            // To do try to cast highest damage
                            if (!TryCastGivenLabelSkill(index, Label.Damage, player.Loc, damgerousZone)) {
                                ApplyStrategy(index, false);
                                return;
                            }
                        }
                        else {
                            if (!TryCastGivenLabelSkill(index, Label.Damage, player.Loc)) {
                                ApplyStrategy(index, false);
                                return;
                            }
                        }
                    }
                }
            }

            CalculatePlayerEffectZone();
        }

        private bool KillPlayer(int index)
        {
            var skills = Creatures[index].ActiveSkills;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], Label.Damage)) {
                    if (player.CalculateFinalDamage((skills[i].TSkill as BasicAttackSkill).baseValue) > player.HealthPoints)
                        return TryCastGivenIDSkill(index, i, player.Loc);
                }
            }

            return false;
        }

        private bool SkillCheck(InstanceSkill skill,Label label)
        {
            return skill.Castable && skill.TSkill.labels.Contains(label);
        }

        private bool HasCastableGivenLabelSkill(int index, Label label)
        {
            var skills = Creatures[index].ActiveSkills;
            bool hasSkill = false;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], label))
                    hasSkill = true;
            }

            return hasSkill;
        }

        private bool TryCastGivenIDSkill(int index,int skillID, Location targetLoc)
        {
            var skill = Creatures[index].ActiveSkills[skillID];
            foreach (var csa in GridManager.Instance.CalculateCastSkillTile(Creatures[index], skillID, targetLoc)) {
                currentCSA = csa;
                CastSkillActionAnimation(index, skillID);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to cast given label of skill, if caster don't have place to cast, then do nothing. (Have AP check already)
        /// </summary>
        /// <param name="index">Cast creature ID</param>
        /// <param name="label">Cast skill label</param>
        /// <param name="targetLoc">Skill target</param>
        /// <returns></returns>
        private bool TryCastGivenLabelSkill(int index, Label label, Location targetLoc)
        {
            var skills = Creatures[index].ActiveSkills;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], label) && !skills[i].TSkill.labels.Contains(Label.SuicideAttack)) {
                    return TryCastGivenIDSkill(index, i, targetLoc);
                }
            }
            return false;
        }

        /// <summary>
        /// Try to cast given label of skill, if caster don't have place to cast, then do nothing. (Have AP check already)
        /// </summary>
        /// <param name="index">Cast creature ID</param>
        /// <param name="label">Cast skill label</param>
        /// <param name="target">Skill target</param>
        /// <param name="avoidZone">Avoid move into this zone to cast skill</param>
        /// <returns></returns>
        private bool TryCastGivenLabelSkill(int index, Label label, Location targetLoc, HashSet<Location> avoidZone)
        {
            var skills = Creatures[index].ActiveSkills;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], label)) {
                    foreach (var csa in GridManager.Instance.CalculateCastSkillTile(Creatures[index], i, targetLoc)) {
                        if (!avoidZone.Contains(csa.destination.Loc)) {
                            currentCSA = csa;
                            CastSkillActionAnimation(index, i);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void CastSkillActionAnimation(int index,int skillID)
        {
            Creatures[index].MoveToTile(currentCSA.destination);
            Creatures[index].CastSkill(skillID, currentCSA.castLocation);

        }

    }
}
