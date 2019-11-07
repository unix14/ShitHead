using SWNetwork;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GoFish
{
    public class Lobby : MonoBehaviour
    {
        public enum LobbyState
        {
            Default,
            JoinedRoom,
        }
        public LobbyState State = LobbyState.Default;
        public bool Debugging = false;

        public GameObject PopoverBackground;
        public GameObject EnterNicknamePopover;
        public GameObject WaitForOpponentPopover;
        public GameObject StartRoomButton;
        public InputField NicknameInputField;

        public GameObject Player1Portrait;
        public GameObject Player2Portrait;

        public ExplainCanvasScript explainPanel;

        string nickname;

        private void Start()
        {
            // disable all online UI elements
            HideAllPopover();
            NetworkClient.Lobby.OnLobbyConnectedEvent += OnLoadConnected;
            NetworkClient.Lobby.OnNewPlayerJoinRoomEvent += OnNewPlayerJoinRoomEvent;
            NetworkClient.Lobby.OnRoomReadyEvent += OnRoomReadyEvent;
            NetworkClient.Lobby.OnPlayerLeaveRoomEvent += OnPlayerLeaveRoomEvent;
        }

        private void OnPlayerLeaveRoomEvent(SWLeaveRoomEventData eventData)
        {
            if (State == LobbyState.JoinedRoom)
            {
                SceneManager.LoadScene("LobbyScene");
            }
            Debug.Log("OnPlayerLeaveRoomEvent:: eventData:" + eventData);
        }

        private void OnDestroy()
        {
            NetworkClient.Lobby.OnLobbyConnectedEvent -= OnLoadConnected;
            NetworkClient.Lobby.OnNewPlayerJoinRoomEvent -= OnNewPlayerJoinRoomEvent;
            NetworkClient.Lobby.OnRoomReadyEvent -= OnRoomReadyEvent;
            NetworkClient.Lobby.OnPlayerLeaveRoomEvent -= OnPlayerLeaveRoomEvent;
        }

        void ShowEnterNicknamePopover()
        {
            PopoverBackground.SetActive(true);
            EnterNicknamePopover.SetActive(true);
        }

        void ShowJoinedRoomPopover()
        {
            EnterNicknamePopover.SetActive(false);
            WaitForOpponentPopover.SetActive(true);
            StartRoomButton.SetActive(false);
            Player1Portrait.SetActive(false);
            Player2Portrait.SetActive(false);
        }

        void ShowReadyToStartUI()
        {
            StartRoomButton.SetActive(true);
            Player1Portrait.SetActive(true);
            Player2Portrait.SetActive(true);
        }

        void HideAllPopover()
        {
            PopoverBackground.SetActive(false);
            EnterNicknamePopover.SetActive(false);
            WaitForOpponentPopover.SetActive(false);
            StartRoomButton.SetActive(false);
            Player1Portrait.SetActive(false);
            Player2Portrait.SetActive(false);
        }

        public void onExplainPanel()
        {
            explainPanel.gameObject.active = !explainPanel.gameObject.active;

            if (PlayerPrefs.GetString("user_lang") == "Hebrew")
            {
                explainPanel.isHebrew = true;
            }
            else
            {
                explainPanel.isHebrew = false;
            }
        }

        //****************** UI event handlers *********************//
        /// <summary>
        /// Practice button was clicked.
        /// </summary>
        public void OnPracticeClicked()
        {
            Debug.Log("OnPracticeClicked");
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// Online button was clicked.
        /// </summary>
        public void OnOnlineClicked()
        {
            Debug.Log("OnOnlineClicked");
            ShowEnterNicknamePopover();
        }

        /// <summary>
        /// Cancel button in the popover was clicked.
        /// </summary>
        public void OnCancelClicked()
        {
            Debug.Log("OnCancelClicked");

            if (State == LobbyState.JoinedRoom)
            {
                LeaveRoom();
            }

            HideAllPopover();
        }

        void LeaveRoom()
        {
            NetworkClient.Lobby.LeaveRoom((successful, error) =>
            {
                if (successful)
                {
                    Debug.Log("Left room");
                    State = LobbyState.Default;
                }
                else
                {
                    Debug.Log("Failed to leave room" + error);
                }
            });
        }

        /// <summary>
        /// Start button in the WaitForOpponentPopover was clicked.
        /// </summary>
        public void OnStartRoomClicked()
        {
            Debug.Log("OnStartRoomClicked");
            // players are ready to player now.
            if (Debugging)
            {
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                // TODO: Start room
                //SceneManager.LoadScene("MultiPlayerGameScene");
                StartRoom();
            }
        }

        void StartRoom() 
        {
            NetworkClient.Lobby.StartRoom((successful, error) =>
            {
                if (successful)
                {
                    Debug.Log("Started room");
                    State = LobbyState.Default;
                }
                else
                {
                    Debug.Log("Failed to start room" + error);
                }
            });

/*            NetworkClient.Lobby.ChangeRoomSettings(5, false, (bool successful, SWLobbyError error) => {
                if (successful)
                {
                    Debug.Log("Changed room settings.");
                }
                else
                {
                    Debug.Log("Failed to change room settings " + error);
                }
            });*/
        }

        void CheckIn()
        {
            NetworkClient.Instance.CheckIn(nickname, (bool successful, string error) =>
            {
                if (!successful)
                {
                    Debug.LogError(error);
                }
            });
        }

        void JoinOrCreateRoom()
        { 
            NetworkClient.Lobby.JoinOrCreateRoom(false, 2, 7, (successful, reply, error) =>
            {
                if (successful)
                {
                    Debug.Log("Failed to join or create room" + error);
                    State = LobbyState.JoinedRoom;
                    ShowJoinedRoomPopover();
                    GetPlayersInTheRoom();
                }
                else
                {
                    Debug.Log("Failed to join or create room" + error);
                }
            });
        }

        void RegisterToTheLobbyServer()
        {
            NetworkClient.Lobby.Register(nickname, (successful , reply, error) => {
                if (successful)
                {
                    Debug.Log("Lobby registered "+ reply);

                    if (string.IsNullOrEmpty(reply.roomId))
                    {
                        JoinOrCreateRoom();
                    } 
                    else if (reply.started)
                    {
                        State = LobbyState.JoinedRoom;
                        ConnectToRoom();
                    } else
                    {
                        State = LobbyState.JoinedRoom;
                        ShowJoinedRoomPopover();
                        GetPlayersInTheRoom();
                    }
                }
                else
                {
                    Debug.Log("Lobby failed to registered " + reply);
                }
            });
        }

        void ConnectToRoom()
        {
            NetworkClient.Instance.ConnectToRoom((connected) => {
                if (connected)
                {
                    Debug.Log("Connected to room");

                    SceneManager.LoadScene("MultiPlayerScene");
                   
                }
                else
                {
                    Debug.Log("Failed to connect to the game server");
                }
            });
        }

        void OnLoadConnected()
        {
            RegisterToTheLobbyServer();
        }

        //Lobby events
        private void OnNewPlayerJoinRoomEvent(SWJoinRoomEventData eventData)
        {
            if (NetworkClient.Lobby.IsOwner)
            {
                ShowReadyToStartUI();
            }
        }

        void OnRoomReadyEvent(SWRoomReadyEventData eventData)
        {
            ConnectToRoom();
        }


        void GetPlayersInTheRoom() {
            NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) => {
                if (successful)
                {
                    Debug.Log("Got Players " + reply);

                    if(reply.players.Count == 1)
                    {
                        Player1Portrait.SetActive(true);
                    } else {
                        Player1Portrait.SetActive(true);
                        Player2Portrait.SetActive(true);

                        if (NetworkClient.Lobby.IsOwner)
                        {
                            ShowReadyToStartUI();
                        }
                    }
                    foreach (SWPlayer player in reply.players)
                    {
                        Debug.Log("Player custom data" + player.GetCustomDataString());
                    }
                }
                else
                {
                    Debug.Log("Lobby failed to registered " + reply);
                }
            });
        }


        /// <summary>
        /// Ok button in the EnterNicknamePopover was clicked.
        /// </summary>
        public void OnConfirmNicknameClicked()
        {
            PlayerData playerData = new PlayerData(NicknameInputField.text);
            nickname = playerData.DecodeName();
            Debug.Log($"OnConfirmNicknameClicked: {nickname}");

            if (Debugging)
            {
                ShowJoinedRoomPopover();
                ShowReadyToStartUI();
            }
            else
            {
                //TODO: Use nickname as player custom id to check into SocketWeaver.
                CheckIn();
            }
        }
    }

    [Serializable]
    public class PlayerData
    {
        public byte[] encodedName;

        public PlayerData(string name)
        {
            encodedName = Encoding.UTF8.GetBytes(name);
        }

        public string DecodeName()
        {
            return Encoding.UTF8.GetString(encodedName);
        }
    }
}
