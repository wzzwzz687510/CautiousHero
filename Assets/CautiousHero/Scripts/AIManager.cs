using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wing.TileUtils;

namespace Wing.RPGSystem
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance { get; private set; }

        public GameObject creaturePrefab;

        private GameObject creatureHolder;

        public PlayerController player;
        public CreatureController[] Creatures { get; private set; }

        private HashSet<Location> playerEffectZone;
        private CastSkillAction currentCSA;
        private bool isCasting;
        private bool isAnimating;

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

            BindAnimationEvents();
        }

        public IEnumerator OnBotTurnStart()
        {
            PrepareDecisionMaking();
            yield return StartCoroutine(BotTurn());
            BattleManager.Instance.CompleteBotTurn();
        }

        public bool IsAllBotsDeath()
        {
            bool res = true;
            foreach (var creature in Creatures) {
                res &= creature.isDeath;
            }
            return res;
        }

        public void StopBot()
        {
            StopAllCoroutines();
        }

        private void BindAnimationEvents()
        {
            foreach (var creature in Creatures) {
                creature.OnAnimationCompleted.AddListener(OnAnimationComplete);
            }
        }

        private void DebindAnimationEvents()
        {
            foreach (var creature in Creatures) {
                creature.OnAnimationCompleted.RemoveListener(OnAnimationComplete);
            }
        }

        private void PrepareDecisionMaking()
        {
            playerEffectZone = GridManager.Instance.CalculateEntityEffectZone(player, true);
            currentCSA = new CastSkillAction();
            isCasting = false;
            isAnimating = false;

            foreach (var creature in Creatures) {
                creature.OnEntityTurnStart();
            }
        }

        private IEnumerator BotTurn()
        {
            for (int i = 0; i < Creatures.Length; i++) {
                if (Creatures[i].isDeath)
                    continue;
                Creatures[i].ChangeOutlineColor(Color.red);
                yield return StartCoroutine(DecisionMaking(i));
                Creatures[i].ChangeOutlineColor(Color.black);
            }
        }

        private IEnumerator DecisionMaking(int index)
        {
            yield return StartCoroutine(KillPlayer(index));

            // if player can effect agent
            if (playerEffectZone.Contains(Creatures[index].Loc)) {
                yield return StartCoroutine(AvoidPlayerEffect(index));
            }
            else {
                yield return StartCoroutine(ApplyStrategy(index,true));
            }
        }

        private IEnumerator ApplyStrategy(int index,bool isAvoidZone)
        {
            HealAlley(index,isAvoidZone);
            while (isCasting) {
                yield return null;
            }

            BuffAlley(index,isAvoidZone);
            while (isCasting) {
                yield return null;
            }

            AttackPlayer(index,isAvoidZone);
            while (isCasting) {
                yield return null;
            }
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
                if (Creatures[i].HealthPoints < Creatures[i].MaxHealthPoints / 2) {
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

        private IEnumerator AvoidPlayerEffect(int index)
        {
            if (!TryCastGivenLabelSkill(index,Label.HardControl,player)) {
                if (!TryCastGivenLabelSkill(index, Label.SoftControl, player)) {
                    if (!TryCastGivenLabelSkill(index, Label.Obstacle, player)) {
                        TileController tc;
                        if (GridManager.Instance.TryGetSafeTile(Creatures[index], player, out tc)) {
                            yield return StartCoroutine(MoveToTile(index, tc));
                        }
                        else {
                            TryCastGivenLabelSkill(index, Label.DefenseBuff, Creatures[index]);
                        }
                    }
                }
            }

            while (isCasting) {
                yield return null;
            }
            yield return StartCoroutine(ApplyStrategy(index, false));
        }

        private IEnumerator KillPlayer(int index)
        {
            var skills = Creatures[index].ActiveSkills;
            for (int i = 0; i < skills.Length; i++) {
                if (SkillCheck(skills[i], Label.Damage)) {
                    if (player.GetDefendReducedValue((skills[i].TSkill as BasicAttackSkill).baseValue) > player.HealthPoints)
                        TryCastGivenIDSkill(index, i, player);
                }
            }

            while (isCasting) {
                yield return null;
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
                StartCoroutine(CastSkillActionAnimation(index, skillID));
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
                if (SkillCheck(skills[i],label)) {
                    if (GridManager.Instance.CalculateCastSkillTile(Creatures[index], i, target.Loc, out currentCSA)) {
                        StartCoroutine(CastSkillActionAnimation(index,i));
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
                        StartCoroutine(CastSkillActionAnimation(index, i));
                        return true;
                    }
                }
            }
            return false;
        }

        // On animation complete callback
        private void OnAnimationComplete()
        {
            isAnimating = false;
        }

        IEnumerator MoveToTile(int index, TileController moveto)
        {
            if (Creatures[index].Loc.Equals(moveto.Loc))
                yield break;

            isAnimating = true;
            Creatures[index].MoveToTile(moveto);
            while (isAnimating) {
                yield return null;
            }
        }

        IEnumerator CastSkill(int index, int skillID)
        {
            isAnimating = true;
            Creatures[index].CastSkill(skillID, currentCSA.castLocation);
            while (isAnimating) {
                yield return null;
            }
        }

        IEnumerator CastSkillActionAnimation(int index,int skillID)
        {
            isCasting = true;

            yield return StartCoroutine(MoveToTile(index,currentCSA.destination));
            yield return StartCoroutine(CastSkill(index, skillID));

            isCasting = false;
        }

    }
}
