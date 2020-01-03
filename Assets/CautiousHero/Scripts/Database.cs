using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wing.RPGSystem
{
    [System.Serializable]
    public struct PlayerData
    {
        public string seed;
        public long randomCnt;
        public int mapFileCnt;
        public int worldConfigHash;

        public string worldName;
        public string playerName;
        public int spriteID;
        public EntityAttribute attribute;
        public long coins;
        public long gainedExp;
        public List<int> learnedSkills;
        public List<Location> worldMap;
    }

    [System.Serializable]
    public struct AreaData
    {
        public Dictionary<Location, AreaInfo> areaInfo;
    }

    public class Database : MonoBehaviour
    {
        public static Database Instance { get; private set; }

        [Header("Setting")]
        public int areaChunkSize = 16;

        [Header("Test")]
        public bool resetData;
        public string seed = "";
        public string worldName;
        public string playerName;
        public int spriteID;
        public EntityAttribute attribute;
        public BaseSkill[] skills;
        public AreaConfig config;

        private List<int> defaultSkillDeck;

        public PlayerData ActiveData { get { return m_activeData; } }
        private PlayerData m_activeData;
        public AreaData[] AreaChunks { get; private set; }
        public string saveName;
        private System.Random sr;

        private string nameKey = "LastSaveName";

        private void Awake()
        {
            if (!Instance)
                Instance = this;

            DontDestroyOnLoad(gameObject);

            defaultSkillDeck = new List<int>();
            for (int i = 0; i < skills.Length; i++) {
                defaultSkillDeck.Add(skills[i].Hash);
            }
            if (resetData) CreateNewSave(seed, worldName, playerName, spriteID, attribute, defaultSkillDeck);
            saveName = PlayerPrefs.GetString(nameKey);
            LoadData(saveName);
        }

        private void Start()
        {
            StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync("Main");
            while (!ao.isDone) {
                yield return null;
            }
        }

        public void SaveData(string saveName)
        {
            m_activeData.mapFileCnt = AreaChunks.Length;
            BinaryFormatter bf = new BinaryFormatter();
            string path = Application.persistentDataPath + "/" + saveName + ".sav";
            FileStream file = File.Create(path);
            bf.Serialize(file, m_activeData);
            file.Close();

            for (int i = 0; i < AreaChunks.Length; i++) {
                bf = new BinaryFormatter();
                path = Application.persistentDataPath + "/" + saveName + "-MapChunk" + i + ".sav";
                file = File.Create(path);
                bf.Serialize(file, AreaChunks[i]);
                file.Close();
            }

            PlayerPrefs.SetString(nameKey, saveName);
            Debug.Log("Game has saved to " + path);
        }

        public void LoadData(string saveName)
        {
            if (saveName == "" || !File.Exists(Application.persistentDataPath + "/" + saveName + ".sav")) {
                CreateNewSave(seed, worldName, playerName, spriteID, attribute, defaultSkillDeck);
                return;
            }

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + saveName + ".sav", FileMode.Open);
            m_activeData = (PlayerData)bf.Deserialize(file);
            file.Close();

            AreaChunks = new AreaData[m_activeData.mapFileCnt];
            for (int i = 0; i < m_activeData.mapFileCnt; i++) {
                bf = new BinaryFormatter();
                file = File.Open(Application.persistentDataPath + "/" + saveName + "-MapChunk" + i + ".sav", FileMode.Open);
                AreaChunks[i] = (AreaData)bf.Deserialize(file);
                file.Close();
            }

            sr = new System.Random(ActiveData.seed.GetStableHashCode());
            for (int i = 0; i < ActiveData.randomCnt; i++) {
                sr.Next();
            }

            Debug.Log("Game Loaded");
        }

        public void CreateNewSave(string seed, string worldName, string playerName, int spriteID, EntityAttribute attribute, List<int> skillDeck)
        {
            m_activeData = new PlayerData {
                seed = seed,
                worldName = worldName,
                playerName = playerName,
                spriteID = spriteID,
                attribute = attribute,
                learnedSkills = skillDeck
            };
            if (seed == "") {
                m_activeData.seed = System.DateTime.Now.ToString();
            }
            AreaChunks = new AreaData[0];

            sr = new System.Random(ActiveData.seed.GetStableHashCode());
            SaveData(worldName);
        }

        public void InitAreaChunk(int areaNumber)
        {
            int length = Mathf.CeilToInt(1.0f * areaNumber / areaChunkSize);
            AreaChunks = new AreaData[length];
            for (int i = 0; i < length; i++) {
                AreaChunks[i] = new AreaData();
            }
        }

        public int Random(int min, int max)
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
                if (!BaseSkill.Dict.TryGetValue(ActiveData.learnedSkills[i], out skills[i])) {
                    Debug.LogError("Skill does not exist, please check!");
                }
            }
            return skills;
        }

        public bool TryGetAreaInfo(Location key, out AreaInfo areaInfo)
        {
            foreach (var chunk in AreaChunks) {
                if (chunk.areaInfo.ContainsKey(key)) {
                    areaInfo = chunk.areaInfo[key];
                    return true;
                }
            }
            areaInfo = new AreaInfo();
            return false;
        }
    }
}

