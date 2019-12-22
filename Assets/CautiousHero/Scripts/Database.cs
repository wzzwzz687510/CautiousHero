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
        public int[] learnedSkills;
        public int[] equippedSkills;
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

        private int[] equippedSkills;

        public PlayerData ActiveData { get { return m_activeData; } }
        private PlayerData m_activeData;
        private System.Random sr;

        private string nameKey = "LastSaveName";

        private void Awake()
        {
            if (!Instance)
                Instance = this;

            equippedSkills = new int[4];
            for (int i = 0; i < 4; i++) {
                equippedSkills[i] = skills[i].Hash;
            }
            LoadData(worldName);
            if (resetData) CreateNewSave(worldName, playerName, spriteID, attribute, equippedSkills);
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
                CreateNewSave(worldName, playerName, spriteID, attribute, equippedSkills);
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

        public void CreateNewSave(string worldName,string playerName,int spriteID, EntityAttribute attribute,int[] equippedSkills)
        {
            m_activeData = new PlayerData();
            m_activeData.worldName = worldName;
            m_activeData.playerName = playerName;
            m_activeData.spriteID = spriteID;
            m_activeData.attribute = attribute;
            m_activeData.equippedSkills = equippedSkills;
            sr = new System.Random(ActiveData.playerName.GetStableHashCode());
            SaveData(worldName);
        }

        public int Random(int min,int max)
        {
            m_activeData.randomCnt++;
            return sr.Next(min, max);
        }

        public BaseSkill[] GetEquippedSkills()
        {
            //foreach (var key in BaseSkill.Dict.Keys) {
            //    Debug.Log(key);
            //}
            
            var skills = new BaseSkill[4];
            for (int i = 0; i < 4; i++) {
                if(!BaseSkill.Dict.TryGetValue(ActiveData.equippedSkills[i], out skills[i])) {
                    Debug.LogError("Skill does not exist, please check!");
                }
            }
            return skills;
        }

    }
}

