using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "TTile", menuName = "Wing/Configs/TTile", order = 5)]
    public class TTile : ScriptableObject
    {
        public string templateName;
        public string description;
        public int Hash => templateName.GetStableHashCode();
        public TileType type;
        public Sprite fSprite;
        public Sprite bSprite;
        public ElementMana mana;

        static Dictionary<int, TTile> cache;
        public static Dictionary<int, TTile> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<TTile>("Tiles").ToDictionary(
                    item => item.Hash, item => item)
                );
            }
        }
    }
}

