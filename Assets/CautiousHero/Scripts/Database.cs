using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;
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
                    defaultClass.skillSet[1].Hash , defaultRace.skill.Hash };
            unlockedBuffs = new List<int>() { defaultRace.relic.Hash, defaultRace.relic.Hash };
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
        public int selectClassID;
        public int selectRaceID;

        public EntityAttribute attribute;
        public int HealthPoints;
        public long coins;
        public long exp;
        public List<int> gainedRelicHashes;
        public List<int> learnedSkillHashes;
        public List<Location> worldMap;
        public List<Location> stageLocations;
        public int currentStage;
        public Location characterLocation;
        public Location enterAreaDirection;
        public Location worldBound;

        public static WorldData ActiveData => Database.Instance.ActiveWorldData;
        public static bool ReachLastStage => ActiveData.currentStage == ActiveData.stageLocations.Count - 1;
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
        public const int AreaChunkSize = 16;

        [Header("Test")]
        public bool reset;
        public TRace defaultRace;
        public TClass defaultClass;
        public TRace completeRace;
        public TClass completeClass;
        public WorldConfig defaultWorldConfig;
        public WorldConfig hardConfig;
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

        public UnityEvent WorldDataChangedEvent;
        public UnityEvent AreaDataChangedEvent;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
            DontDestroyOnLoad(gameObject);

            SelectSlot = -1;
            m_playerData = new PlayerData[3];
            if (!reset) LoadAll();
            else PlayerPrefs.DeleteAll();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K)) {
                AreaManager.Instance.PrepareChooseSkill();
            }

            //if (Input.GetKeyDown(KeyCode.B)) {
            //    Entity character = AreaManager.Instance.character;
            //    character.BuffManager.AddBuff(new BuffHandler(character.Hash, character.Hash, defaultRace.relic.Hash));
            //}
        }

        private void Start()
        {
            StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync("Game");
            while (!ao.isDone) {
                yield return null;
            }
            
        }

        public void SaveAll()
        {
            SavePlayerData();
            SaveWorldData();
            SaveAreaChunks();
        }

        public void SaveAreaInfo(int chunkID, AreaInfo info)
        {
            AreaChunks[chunkID].areaInfo[info.loc] = info;
            SaveData("GameData_" + ActivePlayerData.name + "_MapChunk" + chunkID, AreaChunks[chunkID]);
            AreaDataChangedEvent?.Invoke();
        }

        public void SaveAreaChunks()
        {
            for (int i = 0; i < AreaChunks.Length; i++) {
                SaveData("GameData_"+ ActivePlayerData.name+"_MapChunk" + i, AreaChunks[i]);
            }
        }

        public void SaveWorldData()
        {
            m_activeWorldData.mapFileCnt = AreaChunks.Length;
            SaveData("GameData_" + ActivePlayerData.name, ActiveWorldData);
        }

        public void SavePlayerData()
        {
            PlayerPrefs.SetInt(selectSlotKey, SelectSlot);
            for (int i = 0; i < 3; i++) {
                if (m_playerData[i].name != null) PlayerPrefs.SetString(saveKeys[i], m_playerData[i].name);
            }
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
            if (SelectSlot == -1) {
                PlayerPrefs.SetInt(selectSlotKey, -1);
                return;
            }
            if (m_playerData[SelectSlot].name == null) {
                SelectSlot = -1;
                for (int i = 0; i < 3; i++) {
                    if(m_playerData[i].name != null) SelectSlot = i;
                }
                if (SelectSlot == -1) {
                    PlayerPrefs.SetInt(selectSlotKey, -1);
                    return;
                }
            }

            if (ActivePlayerData.isNewGame) {
                Debug.Log("Game Loaded");
                return;
            }

            filePath = GetFilePath("GameData_" + ActivePlayerData.name);
            LoadData(filePath, ref m_activeWorldData);
            AreaChunks = new AreaData[m_activeWorldData.mapFileCnt];
            for (int i = 0; i < m_activeWorldData.mapFileCnt; i++) {
                filePath = GetFilePath("GameData_" + ActivePlayerData.name + "_MapChunk" + i);
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
            skillDeck.Add(tRace.skill.Hash);
            m_activeWorldData = new WorldData() {
                seed = System.DateTime.Now.ToString(),
                selectClassID = classID,
                selectRaceID = raceID,
                attribute = (tRace.attribute + tClass.attribute) / 2,
                HealthPoints = attribute.maxHealth,
                gainedRelicHashes = new List<int>() { classID.GetTClassFromID().relic.Hash,
                    raceID.GetTRaceFromID().relic.Hash },
                learnedSkillHashes = skillDeck,
                worldMap = new List<Location>(),
                stageLocations = new List<Location>(),
                currentStage = 0,
                characterLocation = new Location(4, 0),
                enterAreaDirection = Location.Up,
                worldBound = new Location(8, -1)
            };
            AreaChunks = new AreaData[0];

            sr = new System.Random(ActiveWorldData.seed.GetStableHashCode());
            //SaveAll();
        }

        public void InitAreaChunk(int areaNumber)
        {
            int length = Mathf.CeilToInt(1.0f * areaNumber / AreaChunkSize);
            AreaChunks = new AreaData[length];
            for (int i = 0; i < length; i++) {
                AreaChunks[i] = new AreaData {
                    areaInfo = new Dictionary<Location, AreaInfo>()
                };
            }
        }

        public PlayerData GetPlayerData(int slotID) => m_playerData[slotID];

        public int Random(int min, int max)
        {
            m_activeWorldData.randomCnt++;
            return sr.Next(min, max);
        }

        public List<Location> GetDiscoveredAreaLocs()
        {
            List<Location> locs = new List<Location>();
            foreach (var chunk in AreaChunks) {
                foreach (var ai in chunk.areaInfo.Values) {
                    if (ai.discovered) locs.Add(ai.loc);
                }
            }

            return locs;
        }

        public void SetAreaDiscovered(Location loc)
        {
            foreach (var chunk in AreaChunks) {
                if (chunk.areaInfo.ContainsKey(loc)) {
                    AreaInfo ai = chunk.areaInfo[loc];
                    ai.discovered = true;
                    chunk.areaInfo[loc] = ai;
                    break;
                }
            }
        }

        public void CompleteAnArea(Location loc)
        {
            m_activeWorldData.characterLocation = loc;
            SaveWorldData();
            SaveAreaChunks();
        }

        public void EnterAnArea(Location loc,Location dir)
        {           
            m_activeWorldData.characterLocation = loc;
            m_activeWorldData.enterAreaDirection = dir;
            SaveWorldData();
            SaveAreaChunks();
        }

        public bool TryGetAreaInfo(Location key, out AreaInfo areaInfo)
        {
            foreach (var chunk in AreaChunks) {
                if (chunk.areaInfo.ContainsKey(key)) {
                    chunk.areaInfo.TryGetValue(key,out areaInfo);
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

        public void SetCurrentStage(int id)
        {
            m_activeWorldData.currentStage = id;
        }

        public void SetNewGame()
        {
            m_playerData[SelectSlot].isNewGame = true;
            SavePlayerData();
        }

        public void SetPlaytime(float time)
        {
            m_playerData[SelectSlot].totalPlayTime += time;
        }

        public void UnlockClass(int hash)
        {
            if (!m_playerData[SelectSlot].unlockedClasses.Contains(hash))
                m_playerData[SelectSlot].unlockedClasses.Add(hash);
        }

        public void UnlockRace(int hash)
        {
            if (!m_playerData[SelectSlot].unlockedRaces.Contains(hash))
                m_playerData[SelectSlot].unlockedRaces.Add(hash);
        }

        public void CompleteTutorial()
        {
            UnlockClass(completeClass.Hash);
            UnlockRace(completeRace.Hash);
        }

        public void SetCharacterData(int hp)
        {
            m_activeWorldData.HealthPoints = hp;
            WorldDataChangedEvent?.Invoke();
        }

        public void SetCharacterData(EntityAttribute attribute)
        {
            m_activeWorldData.attribute = attribute;
            WorldDataChangedEvent?.Invoke();
        }

        public void ApplyResourceChange(int coin, int exp,bool isIncrease)
        {
            if (isIncrease) {
                m_activeWorldData.coins += coin;
                m_activeWorldData.exp += exp;
            }
            else {
                m_activeWorldData.coins -= coin;
                m_activeWorldData.exp -= exp;
            }
            WorldDataChangedEvent?.Invoke();
        }

        public void LearnASkill(int skillHash)
        {
            m_activeWorldData.learnedSkillHashes.Add(skillHash);
            WorldDataChangedEvent?.Invoke();
        }
    }
}

