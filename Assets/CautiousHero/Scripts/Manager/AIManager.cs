using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance { get; private set; }

        public GameObject creaturePrefab;

        private GameObject creatureHolder;

        public PlayerController player;
        public CreatureController[] Creatures { get; private set; }
        public bool isCalculating { get; private set; }

        private int playerMaxDamage;
        private HashSet<Location> playerEffectZone;
        private CastSkillAction currentCSA;
        //private bool isCasting;
        //private bool isAnimating;

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
            BotTurn();
        }

        public bool IsAllBotsDeath()
        {
            bool res = true;
            foreach (var creature in Creatures) {
                res &= creature.IsDeath;
            }
            return res;
        }

        public void StopBot()
        {
            StopAllCoroutines();
        }

        private void CalculatePlayerMaxDamage()
        {
            playerMaxDamage = 0;
            foreach (var skill in player.ActiveSkills) {
                if (SkillCheck(skill,Label.Damage)) {
                    int damage = (skill.TSkill as BasicAttackSkill).CalculateValue(player, 1);
                    playerMaxDamage = Mathf.Max(playerMaxDamage, damage);
                }
            }
        }

        private void PrepareDecisionMaking()
        {
            CalculatePlayerMaxDamage();
            playerEffectZone = GridManager.Instance.CalculateEntityEffectZone(player, true);
            currentCSA = new CastSkillAction();

            foreach (var creature in Creatures) {
                creature.OnEntityTurnStart();
            }
        }

        private void BotTurn()
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

            }
            isCalculating = false;
        }

        private void DecisionMaking(int index)
        {
            KillPlayer(index);

            // if player can effect agent
            if (playerEffectZone.Contains(Creatures[index].Loc)) {
                AvoidPlayerEffect(index);
            }
            else {
                ApplyStrategy(index,true);
            }
        }

        private void ApplyStrategy(int index,bool isAvoidZone)
        {
            if (player.IsDeath) return;
            HealAlley(index,isAvoidZone);
            BuffAlley(index,isAvoidZone);
            AttackPlayer(index,isAvoidZone);
        }

        private void AttackPlayer(int index,bool isAvoidZone)
        {
            if (Creatures[index].ActionPoints == 0 || !HasCastableGivenLabelSkill(index, Label.Damage))
                return;

            if (isAvoidZone) {
                TryCastGivenLabelSkill(index, Label.Damage, player, playerEffectZone);
            }   
            else {
                TryCastGivenLabelSkill(index, Label.Damage, player);
            }
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
                    TryCastGivenLabelSkill(index, Label.StrengthenBuff, Creatures[alleyID], playerEffectZone);
                }
                else {
                    TryCastGivenLabelSkill(index, Label.StrengthenBuff, Creatures[alleyID]);
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
                if (!Creatures[i].IsDeath && Creatures[i].HealthPoints < playerMaxDamage && 
                    Creatures[i].HealthPoints!= Creatures[i].MaxHealthPoints) {
                    if (lowestID == -1 || Creatures[i].HealthPoints < lowestHP) {
                        lowestID = i;
                        lowestHP = Creatures[i].HealthPoints;
                    }
                }
            }
            
            if (lowestID != -1) {
                if (isAvoidZone) {
                    TryCastGivenLabelSkill(index, Label.Healing, Creatures[lowestID], playerEffectZone);
                }
                else {
                    TryCastGivenLabelSkill(index, Label.Healing, Creatures[lowestID]);
                }
            }               
        }

        private void AvoidPlayerEffect(int index)
        {
            if (!TryCastGivenLabelSkill(index,Label.HardControl,player)) {
                if (!TryCastGivenLabelSkill(index, Label.SoftControl, player)) {
                    if (!TryCastGivenLabelSkill(index, Label.Obstacle, player)) {
                        TileController tc;
                        if (GridManager.Instance.TryGetSafeTile(Creatures[index], player, out tc)) {
                            Creatures[index].MoveToTile(tc);
                        }
                        else {
                            TryCastGivenLabelSkill(index, Label.DefenseBuff, Creatures[index]);
                        }
                    }
                }
            }

            ApplyStrategy(index, false);
        }

        private void KillPlayer(int index)
        {
            var skills = Creatures[index].ActiveSkills;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], Label.Damage)) {
                    if (player.CalculateFinalDamage((skills[i].TSkill as BasicAttackSkill).baseValue) > player.HealthPoints)
                        TryCastGivenIDSkill(index, i, player);
                }
            }
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

        private bool TryCastGivenIDSkill(int index,int skillID, Entity target)
        {
            var skill = Creatures[index].ActiveSkills[skillID];
            if (GridManager.Instance.CalculateCastSkillTile(Creatures[index], skillID, target.Loc, out currentCSA)) {
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
        /// <param name="target">Skill target</param>
        /// <returns></returns>
        private bool TryCastGivenLabelSkill(int index, Label label,Entity target)
        {
            var skills = Creatures[index].ActiveSkills;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], label) && !skills[i].TSkill.labels.Contains(Label.SuicideAttack)) {
                    if (GridManager.Instance.CalculateCastSkillTile(Creatures[index], i, target.Loc, out currentCSA)) {
                        CastSkillActionAnimation(index, i);
                        return true;
                    }
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
        private bool TryCastGivenLabelSkill(int index, Label label, Entity target, HashSet<Location> avoidZone)
        {
            var skills = Creatures[index].ActiveSkills;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], label)) {
                    if (GridManager.Instance.CalculateCastSkillTile(Creatures[index], i, target.Loc, out currentCSA) &&
                        !avoidZone.Contains(currentCSA.destination.Loc)) {
                        CastSkillActionAnimation(index, i);
                        return true;
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
