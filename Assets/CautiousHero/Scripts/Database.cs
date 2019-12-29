using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct PlayerData
    {
        public string worldName;
        public string playerName;
        public long randomCnt;
        public int spriteID;
        public EntityAttribute attribute;
        public long coins;
        public long gainedExp;
        public List<int> learnedSkills;
        public Location mapLoc;
    }

    public class Database : MonoBehaviour
    {
        public static Database Instance { get; private set; }

        [Header("Test")]
        public bool resetData;
        public string worldName;
        public string playerName;
        public int spriteID;
        public EntityAttribute attribute;
        public BaseSkill[] skills;
        public BattleConfig config;

        private List<int> skillDeck;

        public PlayerData ActiveData { get { return m_activeData; } }
        private PlayerData m_activeData;
        private System.Random sr;

        private string nameKey = "LastSaveName";

        private void Awake()
        {
            if (!Instance)
                Instance = this;

            skillDeck = new List<int>();
            for (int i = 0; i < skills.Length; i++) {
                skillDeck.Add(skills[i].Hash);
            }
            LoadData(worldName);
            if (resetData) CreateNewSave(worldName, playerName, spriteID, attribute, skillDeck);
        }

        public void SaveData(string saveName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            var path = Application.persistentDataPath + "/" + saveName + ".sav";
            FileStream file = File.Create(path);
            bf.Serialize(file, m_activeData);
            file.Close();

            PlayerPrefs.SetString(nameKey, saveName);
            Debug.Log("Game has saved to "+ path);
        }

        public void LoadData(string saveName)
        {
            if (!File.Exists(Application.persistentDataPath + "/"+ saveName + ".sav")){
                CreateNewSave(worldName, playerName, spriteID, attribute, skillDeck);
                return;
            }

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + saveName + ".sav", FileMode.Open);
            m_activeData = (PlayerData)bf.Deserialize(file);
            file.Close();

            sr = new System.Random(ActiveData.playerName.GetStableHashCode());
            for (int i = 0; i < ActiveData.randomCnt; i++) {
                sr.Next();
            }

            Debug.Log("Game Loaded");
        }

        public void CreateNewSave(string worldName,string playerName,int spriteID, EntityAttribute attribute,List<int> skillDeck)
        {
            m_activeData = new PlayerData();
            m_activeData.worldName = worldName;
            m_activeData.playerName = playerName;
            m_activeData.spriteID = spriteID;
            m_activeData.attribute = attribute;
            m_activeData.learnedSkills = skillDeck;

            sr = new System.Random(ActiveData.playerName.GetStableHashCode());
            SaveData(worldName);
        }

        public int Random(int min,int max)
        {
            m_activeData.randomCnt++;
            return sr.Next(min, max);
        }

        // Obsolute
        public BaseSkill[] GetEquippedSkills()
        {
            //foreach (var key in BaseSkill.Dict.Keys) {
            //    Debug.Log(key);
            //}
            
            var skills = new BaseSkill[ActiveData.learnedSkills.Count];
            for (int i = 0; i < skills.Length; i++) {
                if(!BaseSkill.Dict.TryGetValue(ActiveData.learnedSkills[i], out skills[i])) {
                    Debug.LogError("Skill does not exist, please check!");
                }
            }
            return skills;
        }

    }
}

