using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wing.RPGSystem
{
    [CreateAssetMenu(fileName = "Tile", menuName = "Wing/Scriptable Patterns/TemplateTile", order = 5)]
    public class TemplateTile : ScriptableObject
    {
        public TileType type;
        public ElementMana mana;

        static Dictionary<TileType, ElementMana> cache;
        public static Dictionary<TileType, ElementMana> Dict {
            get {
                // load if not loaded yet
                return cache ?? (cache = Resources.LoadAll<TemplateTile>("Tiles").ToDictionary(
                    item => item.type, item => item.mana)
                );
            }
        }
    }
}

