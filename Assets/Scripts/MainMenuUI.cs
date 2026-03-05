using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {
        public GameObject loadoutMenuGameObject;
        public GameObject shopMenuGameObject;
        public GameObject leaderboardMenuGameObject;
        
        public Button LeaderboardButton;
        public Button LoadoutButton;
        public Button PlayButton;
        public Button ShopButton;

        void Awake() {
                LeaderboardButton.onClick.AddListener(() => {
                        leaderboardMenuGameObject.SetActive(true);
                        shopMenuGameObject.SetActive(false);
                        loadoutMenuGameObject.SetActive(false);
                });
                LoadoutButton.onClick.AddListener(() => {
                        loadoutMenuGameObject.SetActive(true);
                        shopMenuGameObject.SetActive(false);
                        leaderboardMenuGameObject.SetActive(false);
                });
                PlayButton.onClick.AddListener(() => {
                        GameManager.Instance.StartGameRun();
                });
                ShopButton.onClick.AddListener(() => {
                        shopMenuGameObject.SetActive(true);
                        leaderboardMenuGameObject.SetActive(false);
                        loadoutMenuGameObject.SetActive(false);
                });
        }

        public void HideLoadoutMenu() {
                loadoutMenuGameObject.SetActive(false);
        }

        public void HideShopMenu() {
                shopMenuGameObject.SetActive(false);
        }

        public void HideLeaderboardMenu() {
                leaderboardMenuGameObject.SetActive(false);
        }
}