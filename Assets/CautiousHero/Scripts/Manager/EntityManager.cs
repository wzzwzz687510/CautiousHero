using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class EntityManager : MonoBehaviour
    {
        public static EntityManager Instance { get; private set; }

        public Dictionary<int, Entity> EntityDic { get; private set; }

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            EntityDic = new Dictionary<int, Entity>();
        }

        public void ResetEntityDicionary()
        {
            EntityDic.Clear();
            EntityDic.Add(WorldMapManager.Instance.character.Hash, WorldMapManager.Instance.character);
        }

        public int AddEntity(Entity entity)
        {
            var hash = (entity.EntityName+ EntityDic.Count).GetStableHashCode();
            if (!EntityDic.ContainsKey(hash))
                EntityDic.Add(hash, entity);
            return hash;
        }

        public bool TryGetEntity(int hash,out Entity entity)
        {
            if(!EntityDic.ContainsKey(hash)) Debug.LogError("hash don not exist!");
            return EntityDic.TryGetValue(hash, out entity);
        }
    }
}

