using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    public enum EquipmentType
    {
        Helmet,
        Chest,
        Leg,
        Boot,
        Ring,
        Amulet
    }

    // Obsolute
    //[CreateAssetMenu(fileName = "Equipment", menuName = "Wing/Scriptable Equipments/BaseEquipment", order = 50)]
    public class BaseEquipment : ScriptableObject
    {
        [Header("Basic Parameters")]
        public string equipmentName;
        public string description;
        public Sprite sprite;
        public int Hash { get { return equipmentName.GetStableHashCode(); } }
        public EntityAttribute attributeEffect;
        public BaseBuff[] buffs;

        static Dictionary<int, BaseEquipment> cache;
        public static Dictionary<int, BaseEquipment> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<BaseEquipment>("Equipments").ToDictionary(
                    item => item.equipmentName.GetStableHashCode(), item => item)
                );
            }
        }
    }
}
