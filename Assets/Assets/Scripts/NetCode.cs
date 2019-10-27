using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SWNetwork;
using UnityEngine.Events;
using System;

namespace GoFish
{
    [Serializable]
    public class GameDataEvent : UnityEvent<EncryptedData>
    {

    }

    [Serializable]
    public class RankSelectedEvent : UnityEvent<Ranks>
    {

    }
    public class NetCode : MonoBehaviour
    {
        public GameDataEvent OnGameDataReadyEvent = new GameDataEvent();
        public GameDataEvent OnGameDataChangedEvent = new GameDataEvent();

        public UnityEvent OnGameStateChangedEvent = new UnityEvent();

        public RankSelectedEvent OnRankSelectedEvent = new RankSelectedEvent();

        public UnityEvent OnOpponentConfirmed = new UnityEvent();
        public UnityEvent OnLeftRoom = new UnityEvent();

        RoomPropertyAgent roomPropertyAgent;
        public RoomRemoteEventAgent roomRemoteEventAgent;

        const string ENCRYPTED_DATA = "EncryptedData";
        const string GAME_STATE_CHANGED = "GameStateChanged";
        const string RANK_SELECTED = "RankSelected";
        const string OPPONENT_CONFIRMED = "OpponentConfirmed";


        public void ModifyGameData(EncryptedData encryptedData)
        {
            roomPropertyAgent.Modify(ENCRYPTED_DATA, encryptedData);
        }

        public void NotifyOtherPlayersGameStateChanged()
        {
            roomRemoteEventAgent.Invoke(GAME_STATE_CHANGED);
        }

        public void NotifyHostPlayerRankSelected(int selectedRank)
        {
            SWNetworkMessage message = new SWNetworkMessage();
            message.Push(selectedRank);
            roomRemoteEventAgent.Invoke(RANK_SELECTED,message);
        }
        
        public void Start()
        {
            roomRemoteEventAgent.AddListener(RANK_SELECTED, OnRankSelectedRemoteEvent);
            Debug.Log("Start:: AddListener is set");
        }

        private void Awake()
        {
            roomPropertyAgent = FindObjectOfType<RoomPropertyAgent>();
            roomRemoteEventAgent = FindObjectOfType<RoomRemoteEventAgent>();
        }

        // Room property events

        public void OnEncryptedDataReady()
        {
            EncryptedData encryptedData = roomPropertyAgent.GetPropertyWithName(ENCRYPTED_DATA).GetValue<EncryptedData>();
            OnGameDataReadyEvent.Invoke(encryptedData);
        }

        public void OnEncryptedDataChanged()
        {
            EncryptedData encryptedData = roomPropertyAgent.GetPropertyWithName(ENCRYPTED_DATA).GetValue<EncryptedData>();
            OnGameDataChangedEvent.Invoke(encryptedData);
        }

        public void NotifyHostPlayerOpponentConfirmed()
        {
            roomRemoteEventAgent.Invoke(OPPONENT_CONFIRMED);
        }
        public void EnableRoomPropertyAgent()
        {
            roomPropertyAgent.Initialize();
        }

        public void LeaveRoom()
        {
            NetworkClient.Instance.DisconnectFromRoom();
            NetworkClient.Lobby.LeaveRoom((successful, error) => {

                if (successful)
                {
                    Debug.Log("Left room");
                }
                else
                {
                    Debug.Log($"Failed to leave room {error}");
                }

                OnLeftRoom.Invoke();
            });
        }

        // Room remote Events
        public void OnGameStateChangedRemoteEvent()
        {
            OnGameStateChangedEvent.Invoke();
        }

        public void OnRankSelectedRemoteEvent(SWNetworkMessage message)
        {
            Debug.Log("OnRankSelectedRemoteEvent:: Message is " + message);
            int intRank = message.PopInt32();
            OnRankSelectedEvent.Invoke((Ranks)intRank);
        }

        public void OnOpponentConfirmedRemoteEvent()
        {
            OnOpponentConfirmed.Invoke();
        }
    }
}

