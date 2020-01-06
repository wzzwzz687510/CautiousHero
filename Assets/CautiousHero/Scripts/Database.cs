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
        public string name;
        public float totalPlayTime;
        public bool isNewGame;
        public List<int> unlockedRaces;
        public List<int> unlockedClasses;
        public List<int> unlockedSkills;
        public List<int> unlockedBuffs;
        public List<int> unlockedEquipments;
        public List<int> unlockedCreatures;

        public PlayerData(string name)
        {
            TRace defaultRace = Database.Instance.defaultRace;
            TClass defaultClass = Database.Instance.defaultClass;
            this.name = name;
            isNewGame = true;
            totalPlayTime = 0;
            unlockedRaces = new List<int>() { defaultRace.Hash };
            unlockedClasses = new List<int>() { defaultClass.Hash };
            unlockedSkills = new List<int>() { defaultClass.skillSet[0].Hash,
                    defaultClass.skillSet[1].Hash , defaultClass.skillSet[2].Hash };
            unlockedBuffs = new List<int>() { defaultRace.buffSet[0].Hash, defaultRace.buffSet[1].Hash };
            unlockedCreatures = new List<int>();
            unlockedEquipments = new List<int>();
        }
    }

    [System.Serializable]
    public struct WorldData
    {
        public string seed;
        public long randomCnt;
        public int mapFileCnt;
        public int worldConfigHash;
        public string worldName;
        public float playTime;

        public int spriteID;
        public EntityAttribute attribute;
        public int HealthPoints;
        public long coins;
        public long gainedExp;
        public List<int> learnedSkills;
        public List<Location> worldMap;
        public Location worldBound;
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
        public bool reset;
        public TRace defaultRace;
        public TClass defaultClass;
        public WorldConfig defaultWorldConfig;
        public EntityAttribute attribute;

        public WorldData ActiveWorldData { get { return m_activeWorldData; } }
        private WorldData m_activeWorldData;
        public PlayerData ActivePlayerData { get { return m_playerData[SelectSlot]; } }
        private PlayerData[] m_playerData;
        public AreaData[] AreaChunks { get; private set; }        
        private System.Random sr;

        public int SelectSlot { get; private set; }
        private readonly string selectSlotKey = "SelectSlot";
        private readonly string[] saveKeys = { "Slot0", "Slot1", "Slot2" };        

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            DontDestroyOnLoad(gameObject);

            SelectSlot = -1;
            m_playerData = new PlayerData[3];
            if (!reset) LoadAll();
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

        public void SaveAll()
        {
            SaveData(saveKeys[SelectSlot], ActivePlayerData);
            m_activeWorldData.mapFileCnt = AreaChunks.Length;
            SaveData("GameData_" + ActivePlayerData.name, ActiveWorldData);

            for (int i = 0; i < AreaChunks.Length; i++) {
                SaveData("GameData_MapChunk" + i, AreaChunks[i]);
            }

            for (int i = 0; i < 3; i++) {
                if (m_playerData[i].name != null) PlayerPrefs.SetString(saveKeys[i], m_playerData[i].name);
            }            
        }

        public void SavePlayerData()
        {
            PlayerPrefs.SetInt(selectSlotKey, SelectSlot);
            PlayerPrefs.SetString(saveKeys[SelectSlot], ActivePlayerData.name);
            SaveData(saveKeys[SelectSlot], ActivePlayerData);
        }

        private void SaveData(string fileName, object target)
        {
            BinaryFormatter bf = new BinaryFormatter();
            string path = GetFilePath(fileName);
            FileStream file = File.Create(path);
            bf.Serialize(file, target);
            file.Close();

            Debug.Log("Data has saved to " + path);
        }

        public void LoadAll()
        {
            string filePath;
            SelectSlot = PlayerPrefs.GetInt(selectSlotKey, -1);
            
            for (int i = 0; i < 3; i++) {
                filePath = GetFilePath(saveKeys[i]);
                if (File.Exists(filePath))
                    LoadData(filePath, ref m_playerData[i]);
                if (SelectSlot == -1 && m_playerData[i].name != null) SelectSlot = i;
            }
            if(m_playerData[SelectSlot].name == null) {
                SelectSlot = -1;
                for (int i = 0; i < 3; i++) {
                    if(m_playerData[i].name != null) SelectSlot = i;
                }
            }
            if (SelectSlot == -1) {
                PlayerPrefs.SetInt(selectSlotKey, -1);
                return;
            }
            if (ActivePlayerData.isNewGame) {
                Debug.Log("Game Loaded");
                return;
            }

            filePath = GetFilePath("GameData_" + ActivePlayerData.name);
            LoadData(filePath, ref m_activeWorldData);
            AreaChunks = new AreaData[m_activeWorldData.mapFileCnt];
            for (int i = 0; i < m_activeWorldData.mapFileCnt; i++) {
                filePath = GetFilePath("GameData_MapChunk" + i);
                LoadData(filePath, ref AreaChunks[i]);
            }

            sr = new System.Random(ActiveWorldData.seed.GetStableHashCode());
            for (int i = 0; i < ActiveWorldData.randomCnt; i++) {
                sr.Next();
            }

            Debug.Log("Game Loaded");
        }

        private string GetFilePath(string fileName)
        {
            return Application.persistentDataPath + "/" + fileName + ".sav";
        }

        private void LoadData<T>(string filePath,ref T target)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            target = (T)bf.Deserialize(file);
            file.Close();
        }

        public void ChangeSelectSaveSlot(int slotID) => SelectSlot = slotID;

        public void CreateNewPlayer(string name)
        {
            m_playerData[SelectSlot] = new PlayerData(name);
            SavePlayerData();
        }

        public void CreateNewGame(int raceID, int classID)
        {
            m_playerData[SelectSlot].isNewGame = false;
            TRace tRace = ActivePlayerData.unlockedRaces[raceID].GetTRace();
            TClass tClass = ActivePlayerData.unlockedClasses[classID].GetTClass();
            List<int> skillDeck = new List<int>();
            for (int i = 0; i < 5; i++) {
                skillDeck.Add(tClass.skillSet[0].Hash);
            }
            for (int i = 0; i < 4; i++) {
                skillDeck.Add(tClass.skillSet[1].Hash);
            }
            skillDeck.Add(tClass.skillSet[2].Hash);
            m_activeWorldData = new WorldData() {
                seed = System.DateTime.Now.ToString(),
                attribute = attribute,
                HealthPoints = attribute.maxHealth,
                learnedSkills = skillDeck,
                worldMap = new List<Location>()
            };
            AreaChunks = new AreaData[0];

            sr = new System.Random(ActiveWorldData.seed.GetStableHashCode());
            //SaveAll();
        }

        public void InitAreaChunk(int areaNumber)
        {
            int length = Mathf.CeilToInt(1.0f * areaNumber / areaChunkSize);
            AreaChunks = new AreaData[length];
            for (int i = 0; i < length; i++) {
                AreaChunks[i] = new AreaData();
                AreaChunks[i].areaInfo = new Dictionary<Location, AreaInfo>();
            }
        }

        public PlayerData GetPlayerData(int slotID) => m_playerData[slotID];

        public int Random(int min, int max)
        {
            m_activeWorldData.randomCnt++;
            return sr.Next(min, max);
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

        public void SetWorldBound(int x,int y)
        {
            m_activeWorldData.worldBound = new Location(x, y);
        }
    }
}

