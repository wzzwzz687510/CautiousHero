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

        public PlayerController player;
        public List<CreatureController> Creatures { get; private set; }
        public bool isCalculating { get; private set; }

        private SimplePriorityQueue<int> possibleAttackedCreature;
        private List<CastableSkill> castableSkills;


        private void Awake()
        {
            if (!Instance)
                Instance = this;
        }

        public void Init(List<int> battleSet)
        {
            Creatures = new List<CreatureController>();

            for (int i = 0; i < battleSet.Count; i++) {
                Creatures.Add(battleSet[i].GetEntity() as CreatureController);
            }

            for (int i = 0; i < Creatures.Count; i++) {
                SetNextSkillTarget(i);
            }

            castableSkills = new List<CastableSkill>();
        }

        public void OnBotTurnStart()
        {
            PrepareDecisionMaking();
            StartCoroutine(BotTurn());
        }

        public void OnBotDeath(CreatureController cc)
        {            
            Creatures.Remove(cc);
        }

        public bool IsAllBotsDeath()
        {
            bool res = true;
            foreach (var creature in Creatures) {
                res &= creature.IsDeath;
            }
            return res;
        }

        private void PrepareDecisionMaking()
        {
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
                bot.MoveToTile(skill.action.destination,bot.MoveCost);
                bot.CastSkill(bot.NextCastSkillID, skill.action.castLocation);
                if (player.IsDeath) break;
                AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.black));
                yield return null;
            }

            // Consume spare action points
            foreach (var bot in Creatures) {
                int sparePoints = bot.ActionPoints + bot.ActionPointsPerTurn - bot.MaxActionPoints;
                if (sparePoints > 0) {
                    bool shouldMove = true;
                    foreach (var el in bot.NextSkill.GetEffectZone(bot.Loc)) {
                        if (el == bot.NextSkillTarget.Loc) {
                            shouldMove = false;
                            break;
                        }
                    }
                    if (shouldMove) {
                        bool finished = false;
                        foreach (var cp in bot.NextSkill.CastPattern) {
                            foreach (var ep in bot.NextSkill.EffectPattern) {
                                Location destination = bot.NextSkillTarget.Loc - cp - ep.loc;
                                if (destination.IsEmpty()) {
                                    destination = bot.Loc.GetLocationWithGivenStep(destination, sparePoints);
                                    //Debug.Log("creature id: " + Creatures.IndexOf(bot) + "destination loc: " + destination.ToString() + ", spare points: " + sparePoints);
                                    AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.red));
                                    bot.MoveToTile(destination,bot.MoveCost);
                                    AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.black));
                                    finished = true;
                                    break;
                                }
                            }
                            if (finished) break;
                        }
                    }
                }
            }
        }

        private IEnumerator CastGivenLabelSkill(Label label)
        {
            var newCastableSkills = new List<CastableSkill>();
            foreach (var skill in castableSkills) {
                CreatureController bot = Creatures[skill.creatureID];
                if (bot.NextSkill.labels.Contains(label)) {
                    if (player.IsDeath) break;
                    AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.red));
                    AnimationManager.Instance.AddAnimClip(new BaseAnimClip(AnimType.Delay, 0.5f));
                    bot.MoveToTile(skill.action.destination,bot.MoveCost);
                    bot.CastSkill(bot.NextCastSkillID, skill.action.castLocation);
                    SetNextSkillTarget(skill.creatureID);
                    if (player.IsDeath) break;
                    AnimationManager.Instance.AddAnimClip(new OutlineEntityAnimClip(bot.Hash, Color.black));
                    yield return null;
                }
                else {
                    newCastableSkills.Add(skill);
                }
            }
            castableSkills = newCastableSkills;
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
            HashSet<Location> destinations = new HashSet<Location>();
            for (int creatureID = 0; creatureID < Creatures.Count; creatureID++) {
                CreatureController bot = Creatures[creatureID];
                int moveSteps = (bot.ActionPoints - bot.NextSkill.actionPointsCost) / bot.MoveCost;
                if (moveSteps < 0) continue;

                // Ally skill cast to next bot to simplify the AI complexity. Increase the skill design and config set complexity.
                Location targetLoc = bot.NextSkillTarget.Loc;

                bool shouldMove = true;
                foreach (var cp in bot.NextSkill.CastPattern) {
                    foreach (var el in bot.NextSkill.GetSubEffectZone(bot.Loc,cp)) {
                        if (el == bot.NextSkillTarget.Loc) {
                            shouldMove = false;
                            castableSkills.Add(new CastableSkill(creatureID, new CastSkillAction(bot.Loc, bot.Loc + cp)));
                            break;
                        }
                    }
                }
                if (shouldMove) {
                    bool finished = false;
                    foreach (var cp in bot.NextSkill.CastPattern) {
                        foreach (var ep in bot.NextSkill.EffectPattern) {
                            Location destination = bot.NextSkillTarget.Loc - ep.loc - cp;
                            if (destination.IsEmpty() && !destinations.Contains(destination)) {
                                Location tmp = bot.Loc.GetLocationWithGivenStep(destination, moveSteps);
                                if (tmp == destination) {
                                    castableSkills.Add(new CastableSkill(creatureID, new CastSkillAction(destination, destination + cp)));
                                    destinations.Add(destination);
                                    // Set map temporary
                                    GridManager.Instance.Nav.SetTileWeight(bot.Loc, 1);
                                    GridManager.Instance.Nav.SetTileWeight(destination, 0);
                                    finished = true;
                                    break;
                                }
                            }
                        }
                        if (finished) break;
                    }
                }

            }
            // Resume map status
            foreach (var bot in Creatures) {
                GridManager.Instance.Nav.SetTileWeight(bot.Loc, 0);
            }
            foreach (var loc in destinations) {
                GridManager.Instance.Nav.SetTileWeight(loc, 1);
            }
        }

    }
}
