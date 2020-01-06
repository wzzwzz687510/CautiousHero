using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wing.RPGSystem
{
    public class SaveSlot : MonoBehaviour
    {
        public int slotID;
        public Text playerName;
        public Text playtime;
        public Text progress;
        public Text emptyText;
        public GameObject playerData;

        public void UpdateUI()
        {
            if (Database.Instance.GetPlayerData(slotID).name != null) {
                PlayerData data = Database.Instance.GetPlayerData(slotID);
                emptyText.enabled = false;
                playerName.text = data.name;
                playtime.text = "Playtime " + System.TimeSpan.FromSeconds(data.totalPlayTime).ToString(@"hh\:mm\:ss");
                int unlockedCnt = data.unlockedBuffs.Count + data.unlockedClasses.Count + data.unlockedCreatures.Count +
                    data.unlockedEquipments.Count + data.unlockedRaces.Count + data.unlockedSkills.Count;
                int totalCnt = BaseBuff.Dict.Count + TClass.Dict.Count + BaseCreature.Dict.Count + BaseEquipment.Dict.Count +
                    TRace.Dict.Count + BaseSkill.Dict.Count;
                //Debug.Log("unlocked: " + unlockedCnt + ", total: " + totalCnt);
                progress.text = 100 * unlockedCnt / totalCnt + "%";

                playerData.SetActive(true);
            }
        }
    }
}

