using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SWNetwork;
using UnityEngine.SceneManagement;

namespace GoFish
{
    public class MultiplayerGame : Game
    {
        NetCode netCode;

        protected new void Awake()
        {
            base.Awake();
            remotePlayer.IsAI = false;

            netCode = FindObjectOfType<NetCode>();

            NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) =>
            {
                if (successful)
                {
                    foreach (SWPlayer swPlayer in reply.players)
                    {
                        string playerName = swPlayer.GetCustomDataString();
                        string playerId = swPlayer.id;

                        if (playerId.Equals(NetworkClient.Instance.PlayerId))
                        {
                            localPlayer.PlayerId = playerId;
                            localPlayer.PlayerName = playerName;
                        }
                        else
                        {
                            remotePlayer.PlayerId = playerId;
                            remotePlayer.PlayerName = playerName;
                        }
                    }

                    gameDataManager = new GameDataManager(localPlayer, remotePlayer, NetworkClient.Lobby.RoomId);
                    netCode.EnableRoomPropertyAgent();
                }
                else
                {
                    Debug.Log("Failed to get players in room.");
                }

            });
        }

        protected new void Start()
        {
            Debug.Log("Multiplayer Game Start");
        }

        //Game flow
        public override void GameFlow()
        {
            Debug.Log("Should never be here");
        }

        protected override void OnGameStarted()
        {
            if (NetworkClient.Instance.IsHost)
            {
                gameDataManager.Shuffle();
                gameDataManager.DealCardValuesToPlayer(localPlayer, Constants.PLAYER_INITIAL_CARDS);
                gameDataManager.DealCardValuesToPlayer(remotePlayer, Constants.PLAYER_INITIAL_CARDS);

                gameState = GameState.TurnStarted;
                gameDataManager.SetGameState(gameState);
                netCode.ModifyGameData(gameDataManager.EncryptedData());
            }

            cardAnimator.DealDisplayingCards(localPlayer, Constants.PLAYER_INITIAL_CARDS);
            cardAnimator.DealDisplayingCards(remotePlayer, Constants.PLAYER_INITIAL_CARDS);

        }

        protected override void OnTurnStarted()
        {
            if (NetworkClient.Instance.IsHost)
            {
                SwitchTurn();
                gameState = GameState.TurnSelectingNumber;

                gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayersGameStateChanged();
            }

        }

        protected override void OnTurnConfirmedSelectedNumber()
        {
            if (currentTurnPlayer == localPlayer)
            {
                SetMessage($"Asking {currentTurnTargetPlayer.PlayerName} for {selectedRank}s...");
            }
            else
            {
                SetMessage($"{currentTurnPlayer.PlayerName} is asking for {selectedRank}s...");
            }

            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.TurnStarted;
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayersGameStateChanged();
            }
            gameState = GameState.TurnStarted;
            GameFlow();
        }

        //protected override void OnTurnOpponentConfirmed()
        //{
        //    List<byte> cardValuesFromTargetPlayer = gameDataManager.TakeCardValuesWithRankFromPlayer(currentTurnTargetPlayer, selectedRank);

        //    if (cardValuesFromTargetPlayer.Count > 0)
        //    {
        //        gameDataManager.AddCardValuesToPlayer(currentTurnPlayer, cardValuesFromTargetPlayer);

        //        bool senderIsLocalPlayer = currentTurnTargetPlayer == localPlayer;
        //        currentTurnTargetPlayer.SendDisplayingCardToPlayer(currentTurnPlayer, cardAnimator, cardValuesFromTargetPlayer, senderIsLocalPlayer);

        //        if (NetworkClient.Instance.IsHost)
        //        {
        //            gameState = GameState.TurnSelectingNumber;

        //            gameDataManager.SetGameState(gameState);
        //            netCode.ModifyGameData(gameDataManager.EncryptedData());
        //        }
        //    }
        //    else
        //    {
        //        if (NetworkClient.Instance.IsHost)
        //        {
        //            gameState = GameState.TurnGoFish;

        //            gameDataManager.SetGameState(gameState);
        //            netCode.ModifyGameData(gameDataManager.EncryptedData());
        //            netCode.NotifyOtherPlayersGameStateChanged();
        //        }
        //        gameState = GameState.TurnGoFish;
        //        GameFlow();
        //    }
        //}

        protected override void OnTurnGoFish()
        {
            SetMessage($"Go fish!");

            byte cardValue = gameDataManager.DrawCardValue();

            if (cardValue == Constants.POOL_IS_EMPTY)
            {
                Debug.LogError("Pool is empty");
                return;
            }

            if (Card.GetRank(cardValue) == selectedRank)
            {
                cardAnimator.DrawDisplayingCard(currentTurnPlayer, cardValue);
            }
            else
            {
                cardAnimator.DrawDisplayingCard(currentTurnPlayer);

                if (NetworkClient.Instance.IsHost)
                {
                    gameState = GameState.TurnStarted;

                }
            }

            gameDataManager.AddCardValueToPlayer(currentTurnPlayer, cardValue);
            if (NetworkClient.Instance.IsHost)
            {
                gameDataManager.SetGameState(gameState);
            netCode.ModifyGameData(gameDataManager.EncryptedData());
            }
        }

        public override void AllAnimationsFinished()
        {
            if (NetworkClient.Instance.IsHost)
            {
                netCode.NotifyOtherPlayersGameStateChanged();
            }
        }

        // User Interactions
        public override void OnOkSelected()
        {
            if (gameState == GameState.TurnSelectingNumber && localPlayer == currentTurnPlayer)
            {
                if (selectedCards.Count >0)
                {
                    netCode.NotifyHostPlayerRankSelected((int)selectedCards[0].Rank);   //TODO fix == not correct way

                }
            }
            else if (gameState == GameState.TurnConfirmedSelectedNumber && localPlayer == currentTurnTargetPlayer)
            {
                netCode.NotifyHostPlayerOpponentConfirmed();
            }
            else if (gameState == GameState.GameFinished)
            {
                netCode.LeaveRoom();
            }
        }

        // Net Code Events
        public void OnGameDataReady(EncryptedData encryptedData)
        {
            if(encryptedData == null)
            {
                Debug.Log("New Game");
                if (NetworkClient.Instance.IsHost)
                {
                    gameState = GameState.GameStarted;
                    gameDataManager.SetGameState(gameState);

                    netCode.ModifyGameData(gameDataManager.EncryptedData());

                    netCode.NotifyOtherPlayersGameStateChanged();
                }
            } else {
                OnGameDataChanged(encryptedData);
                gameState = gameDataManager.GetGameState();

                if(gameState > GameState.GameStarted)
                {
                    Debug.Log("Restore the game state");

                    //restore players cards
                    int restoredLocalCardsCount = gameDataManager.PlayerCards(localPlayer).Count;
                    int restoredRemoteCardsCount = gameDataManager.PlayerCards(remotePlayer).Count;
                    cardAnimator.DealDisplayingCards(localPlayer, restoredLocalCardsCount , false);
                    cardAnimator.DealDisplayingCards(remotePlayer, restoredRemoteCardsCount, false);

                    //retore player 1 Books
                    List<byte> booksForLocalPlayer = gameDataManager.PlayerBooks(localPlayer);
                    foreach(byte rank in booksForLocalPlayer)
                    {
                        localPlayer.RestoreBook((Ranks)rank, cardAnimator);

                        //TODO:: also restore Top Book

                        // use gameDataManager.PlayerHiddenBooks to get hidden books
                    }

                    //retore player 2 Books
                    List<byte> booksForRemotePlayer = gameDataManager.PlayerBooks(remotePlayer);
                    foreach (byte rank in booksForRemotePlayer)
                    {
                        remotePlayer.RestoreBook((Ranks)rank, cardAnimator);
                        
                        
                        //TODO:: also restore Top Book
                    }

                    base.GameFlow();
                }
            }
        }

        public void OnLeftRoom()
        {
            SceneManager.LoadScene("LobbySceene");
        }

        public void OnGameDataChanged(EncryptedData encryptedData)
        {
            gameDataManager.ApplyEncrptedData(encryptedData);
            gameState = gameDataManager.GetGameState();
            currentTurnPlayer = gameDataManager.GetCurrentTurnPlayer();
            currentTurnTargetPlayer = gameDataManager.GetCurrentTurnTargetPlayer();
            selectedRank = gameDataManager.GetSelectedRank();
        }
        public void OnGameStateChanged()
        {
            base.GameFlow();
        }

        public void OnRankSelected(Ranks rank)
        {
            selectedRank = rank;
            gameState = GameState.TurnConfirmedSelectedNumber;

            gameDataManager.SetSelectedRank(selectedRank);
            gameDataManager.SetGameState(gameState);

            netCode.ModifyGameData(gameDataManager.EncryptedData());
            netCode.NotifyOtherPlayersGameStateChanged();
        }

        public void OnOpponentConfirmed()
        {
            gameState = GameState.TurnStarted;

            gameDataManager.SetGameState(gameState);

            netCode.ModifyGameData(gameDataManager.EncryptedData());
            netCode.NotifyOtherPlayersGameStateChanged();
        }
    }
}
