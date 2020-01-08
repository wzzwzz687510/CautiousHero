using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wing.RPGSystem
{
    public class RewardUIController : MonoBehaviour
    {
        public Transform contentHolder;
        public GameObject skillPrefab;
        public GameObject coinPrefab;
        public GameObject expPrefab;
        public GameObject relicPrefab;

        private int selectChestID;

        public void AddContent(LootType type, int number)
        {
            if (number == 0) return;
            switch (type) {
                case LootType.Skill:
                    GameObject skill;
                    for (int i = 0; i < number; i++) {
                        skill = Instantiate(skillPrefab, contentHolder);
                        skill.GetComponentInChildren<Button>().onClick.AddListener(() => {
                            AreaManager.Instance.PrepareChooseSkill();
                        });
                    }
                    break;
                case LootType.Coin:
                    GameObject coin = Instantiate(coinPrefab, contentHolder);
                    coin.GetComponentInChildren<Text>().text = string.Format("{0} coin", number);
                    coin.GetComponentInChildren<Button>().onClick.AddListener(() => {
                        Database.Instance.ApplyResourceChange(number, 0, true);
                        if (selectChestID != -1) AreaManager.Instance.RemoveChestCoin(selectChestID);
                        CloseCheck();
                        Destroy(coin);
                    });
                    break;
                case LootType.Exp:
                    GameObject exp = Instantiate(expPrefab, contentHolder);
                    exp.GetComponentInChildren<Text>().text = string.Format("{0} exp", number);
                    exp.GetComponentInChildren<Button>().onClick.AddListener(() => {
                        Database.Instance.ApplyResourceChange(0, number, true);
                        CloseCheck();
                        Destroy(exp);
                    });
                    break;
                case LootType.Relic:
                    GameObject relic = Instantiate(relicPrefab, contentHolder);
                    relic.GetComponentInChildren<Text>().text = string.Format("{0} exp", number.GetRelic().relicName);
                    relic.GetComponentInChildren<Button>().onClick.AddListener(() => {
                        Database.Instance.ApplyResourceChange(0, number, true);
                        if (selectChestID != -1) AreaManager.Instance.RemoveChestRelic(selectChestID,number);
                        CloseCheck();
                        Destroy(relic);
                    });
                    break;
                default:
                    break;
            }
        }

        public void SetChestID(int id)
        {
            selectChestID = id;
        }

        public void CloseCheck()
        {
            if (contentHolder.childCount == 1) {
                gameObject.SetActive(false);
                AreaManager.Instance.SetMoveCheck(true);
            }
                
        }

        private void OnDisable()
        {
            selectChestID = -1;
            int cnt = contentHolder.childCount;
            for (int i = 0; i < cnt; i++) {
                Destroy(contentHolder.GetChild(i).gameObject);
            }
            Database.Instance.SaveWorldData();
        }
    }
}

