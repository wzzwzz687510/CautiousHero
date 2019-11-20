using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class EntityManager : MonoBehaviour
    {
        public static EntityManager Instance { get; private set; }

        private Dictionary<int, Entity> entityDic = new Dictionary<int, Entity>();

        private void Awake()
        {
            if (!Instance)
                Instance = this;
        }

        public int AddEntity(Entity entity)
        {
            var hash = (entity.EntityName+ entityDic.Count).GetStableHashCode();
            if (!entityDic.ContainsKey(hash))
                entityDic.Add(hash, entity);
            return hash;
        }

        public bool TryGetEntity(int hash,out Entity entity)
        {
            return entityDic.TryGetValue(hash, out entity);
        }
    }
}

