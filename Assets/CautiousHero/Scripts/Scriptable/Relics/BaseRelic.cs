using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Relic", menuName = "Wing/Scriptable Relics/BaseRelic", order = 50)]
    public class BaseRelic : ScriptableObject
    {
        [Header("Basic Parameters")]
        public string relicName;
        public string description;
        public Sprite sprite;
        public int Hash { get { return relicName.GetStableHashCode(); } }
        public EntityAttribute attributeEffect;
        public BaseBuff[] buffs;

        [Header("Labels")]
        public Rarity rarity;

        public void ApplyEffect(int entityHash)
        {
            foreach (var buff in buffs) {
                entityHash.GetEntity().EntityBuffManager.AddBuff(new BuffHandler(entityHash,entityHash,buff.Hash));
            }            
        }

        static Dictionary<int, BaseRelic> cache;
        public static Dictionary<int, BaseRelic> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<BaseRelic>("Relics").ToDictionary(
                    item => item.Hash, item => item)
                );
            }
        }
    }
}

