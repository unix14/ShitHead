using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GoFish
{
    [Serializable]
    public class EncryptedData
    {
        public byte[] data;
    }

    [Serializable]
    public class GameDataManager
    {
        Player localPlayer;
        Player remotePlayer;

        [SerializeField]
        ProtectedData protectedData;

        public GameDataManager(Player local, Player remote, string roomId = "1234567890123456")
        {
            localPlayer = local;
            remotePlayer = remote;
            protectedData = new ProtectedData(localPlayer.PlayerId, remotePlayer.PlayerId, roomId);
        }

        public void Shuffle()
        {
            List<byte> cardValues = new List<byte>();

            for (byte value = 0; value < 52; value++)
            {
                cardValues.Add(value);
            }

            List<byte> poolOfCards = new List<byte>();

            for (int index = 0; index < 52; index++)
            {
                int valueIndexToAdd = UnityEngine.Random.Range(0, cardValues.Count);

                byte valueToAdd = cardValues[valueIndexToAdd];
                poolOfCards.Add(valueToAdd);
                cardValues.Remove(valueToAdd);
            }

            protectedData.SetPoolOfCards(poolOfCards);
        }

        public void DealCardValuesToPlayer(Player player, int numberOfCards)
        {
            List<byte> poolOfCards = protectedData.GetPoolOfCards();

            int numberOfCardsInThePool = poolOfCards.Count;
            int start = Math.Max(0, numberOfCardsInThePool - 1 - numberOfCards);

            List<byte> cardValues = poolOfCards.GetRange(start, numberOfCards);
            poolOfCards.RemoveRange(start, numberOfCards);

            protectedData.AddCardValuesToPlayer(player, cardValues);
            protectedData.SetPoolOfCards(poolOfCards);
        }

        public byte DrawCardValue()
        {
            List<byte> poolOfCards = protectedData.GetPoolOfCards();

            int numberOfCardsInThePool = poolOfCards.Count;

            if (numberOfCardsInThePool > 0)
            {
                byte cardValue = poolOfCards[numberOfCardsInThePool - 1];
                poolOfCards.Remove(cardValue);
                protectedData.SetPoolOfCards(poolOfCards);
                return cardValue;
            }

            return Constants.POOL_IS_EMPTY;
        }

        public List<byte> PlayerCards(Player player)
        {
            return protectedData.PlayerCards(player);
        }

        public List<byte> PlayerBooks(Player player)
        {
            return protectedData.PlayerBooks(player);
        }

        public List<byte> PlayerHiddenBooks(Player player)
        {
            return protectedData.PlayerHiddenBooks(player);
        }

        public void AddCardValuesToPlayer(Player player, List<byte> cardValues)
        {
            protectedData.AddCardValuesToPlayer(player, cardValues);
        }

        public void AddCardValueToPlayer(Player player, byte cardValue)
        {
            protectedData.AddCardValueToPlayer(player, cardValue);
        }

        public void RemoveCardValuesFromPlayer(Player player, List<byte> cardValuesToRemove)
        {
            protectedData.RemoveCardValuesFromPlayer(player, cardValuesToRemove);
        }

        public void RemoveCardValueFromPlayer(Player player, byte cardValueToRemove)
        {
            protectedData.RemoveCardFromPlayer(player, cardValueToRemove);
        }



        public void AddBottomBooksForPlayer(Player player, Ranks ranks)
        {
            protectedData.AddBottomBooksForPlayer(player, ranks);
        }

        public void AddBooksForPlayer(Player player, Ranks ranks)
        {
            protectedData.AddBooksForPlayer(player, ranks);
        }

        public Player Winner()
        {
            string winnerPlayerId = protectedData.WinnerPlayerId();
            if (winnerPlayerId.Equals(localPlayer.PlayerId))
            {
                return localPlayer;
            }
            else if (winnerPlayerId.Equals(remotePlayer.PlayerId))
            {
                return remotePlayer;
            }
            else return null;
        }

        public bool GameFinished()
        {
            return protectedData.GameFinished();
        }

        public bool GameFinishedPoolOfCards()
        {
            return protectedData.GameFinishedPoolOfCards();
        }

        public bool Player1FinishedPoolOfCards()
        {
            return protectedData.Player1FinishedPoolOfCards();
        }

        public bool Player2FinishedPoolOfCards()
        {
            return protectedData.Player1FinishedPoolOfCards();
        }


        public List<byte> TakeCardValuesWithRankFromPlayer(Player player, Ranks ranks)
        {
            List<byte> playerCards = protectedData.PlayerCards(player);

            List<byte> result = new List<byte>();

            foreach (byte cv in playerCards)
            {
                if (Card.GetRank(cv) == ranks)
                {
                    result.Add(cv);
                }
            }

            protectedData.RemoveCardValuesFromPlayer(player, result);

            return result;
        }

        public byte TakeOneCardValueWithRankFromPlayer(Player player, Ranks ranks)
        {
            List<byte> playerCards = protectedData.PlayerCards(player);

            foreach (byte cv in playerCards)
            {
                if (Card.GetRank(cv) == ranks)
                {
                    //protectedData.RemoveCardFromPlayer(player, cv);

                    return cv;
                }
            }

            return 0;
        }

        public Dictionary<Ranks, List<byte>> GetBooks(Player player)
        {
            List<byte> playerCards = protectedData.PlayerCards(player);

            var groups = playerCards.GroupBy(Card.GetRank).Where(g => g.Count() == 4);

            if (groups.Count() > 0)
            {
                Dictionary<Ranks, List<byte>> setOfFourDictionary = new Dictionary<Ranks, List<byte>>();

                foreach (var group in groups)
                {
                    List<byte> cardValues = new List<byte>();

                    foreach (var value in group)
                    {
                        cardValues.Add(value);
                    }

                    setOfFourDictionary[group.Key] = cardValues;
                }

                return setOfFourDictionary;
            }

            return null;
        }

        public List<byte> AiDecideCardFromPlayer(Player player,Card topCard, Card previousTopCard)
        {
            List<byte> cardsToThrow = new List<byte>();
            List<byte> playerCards = protectedData.PlayerCards(player);

            if(playerCards.Count == 0 && player.isFinishedHandCards())
            {
                playerCards.AddRange(PlayerBooks(player));
            }
            if (playerCards.Count == 0 && player.isFinishedDisplayingBooks())
            {
                playerCards.AddRange(PlayerHiddenBooks(player));
            }

            foreach (byte cardValue in playerCards)
            {
                bool doIhaveLuckyCardValue = (Card.GetRank(cardValue) == Ranks.Two || Card.GetRank(cardValue) == Ranks.Three || Card.GetRank(cardValue) == Ranks.Ten);
                bool isCardsToThrowEmpty = cardsToThrow.Count == 0;
                bool isCardValueFitsCardsToThrow = (cardsToThrow.Count > 0 && Card.GetRank(cardsToThrow[0]) == Card.GetRank(cardValue));

                if (topCard != null && topCard.Value != Card.NO_VALUE && Card.GetRank(cardValue) <= Ranks.Seven && topCard.Rank == Ranks.Seven && Card.GetRank(cardValue) != Ranks.Ace)
                {
                    if(isCardsToThrowEmpty || isCardValueFitsCardsToThrow)
                    {
                        cardsToThrow.Add(cardValue);
                    }
                } else if (topCard == null || topCard.Rank == Ranks.Two)
                {
                    if (isCardsToThrowEmpty || isCardValueFitsCardsToThrow)
                    {
                        cardsToThrow.Add(cardValue);
                    }
                }
                else if(topCard != null && topCard.Rank == Ranks.Three)
                {
                    if(previousTopCard != null)
                    {
                        if (Card.GetRank(cardValue) >= previousTopCard.Rank && previousTopCard.Rank != Ranks.Ace)
                        {
                            if (isCardsToThrowEmpty || isCardValueFitsCardsToThrow)
                            {
                                cardsToThrow.Add(cardValue);
                            }
                        }
                        else if(previousTopCard.Rank == Ranks.Ace && doIhaveLuckyCardValue)
                        {
                            if (isCardsToThrowEmpty || isCardValueFitsCardsToThrow)
                            {
                                cardsToThrow.Add(cardValue);
                            }
                        }
                    }
                }
                else if (topCard != null && topCard.Rank == Ranks.Ace && (Card.GetRank(cardValue) == Ranks.Ace || doIhaveLuckyCardValue))
                {
                    if (isCardsToThrowEmpty || isCardValueFitsCardsToThrow)
                    {
                        cardsToThrow.Add(cardValue);
                    }
                }
                else if (topCard != null && topCard.Value != Card.NO_VALUE && Card.GetRank(cardValue) >= topCard.Rank && topCard.Rank != Ranks.Ace)
                {
                    if (isCardsToThrowEmpty || isCardValueFitsCardsToThrow)
                    {
                        cardsToThrow.Add(cardValue);
                    }
                }
            }
            return cardsToThrow;
        }

        public void SetCurrentTurnPlayer(Player player)
        {
            protectedData.SetCurrentTurnPlayerId(player.PlayerId);
        }

        public Player GetCurrentTurnPlayer()
        {
            string playerId = protectedData.GetCurrentTurnPlayerId();
            if (localPlayer.PlayerId.Equals(playerId))
            {
                return localPlayer;
            }
            else
            {
                return remotePlayer;
            }
        }

        public Player GetCurrentTurnTargetPlayer()
        {
            string playerId = protectedData.GetCurrentTurnPlayerId();
            if (localPlayer.PlayerId.Equals(playerId))
            {
                return remotePlayer;
            }
            else
            {
                return localPlayer;
            }
        }

        public void SetGameState(Game.GameState gameState)
        {
            protectedData.SetGameState((int)gameState);
        }

        public Game.GameState GetGameState()
        {
            return (Game.GameState)protectedData.GetGameState();
        }

        public void SetSelectedRank(Ranks rank)
        {
            protectedData.SetSelectedRank((int)rank);
        }

        public Ranks GetSelectedRank()
        {
            return (Ranks)protectedData.GetSelectedRank();
        }

        public EncryptedData EncryptedData()
        {
            Byte[] data = protectedData.ToArray();

            EncryptedData encryptedData = new EncryptedData();
            encryptedData.data = data;

            return encryptedData;
        }

        public void ApplyEncrptedData(EncryptedData encryptedData)
        {
            if (encryptedData == null)
            {
                return;
            }

            protectedData.ApplyByteArray(encryptedData.data);
        }
    }
}
