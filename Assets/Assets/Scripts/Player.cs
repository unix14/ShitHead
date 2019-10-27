﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoFish
{
    /// <summary>
    /// Manages the positions of the player's cards
    /// </summary>
    [Serializable]
    public class Player : IEquatable<Player>
    {
        public string PlayerId;
        public string PlayerName;
        public bool IsAI;
        public Vector2 Position;
        public Vector2 BookPosition;

        int NumberOfDisplayingCards;
        int NumberOfBooks;
        int NumberOfHiddenBooks;

        public List<Card> DisplayingCards = new List<Card>();
        public List<Card> DisplayingBooks = new List<Card>();
        public List<Card> HiddenBooks = new List<Card>();

        public Vector2 NextCardPosition()
        {
            Vector2 nextPos = Position + Vector2.right * Constants.PLAYER_CARD_POSITION_OFFSET * NumberOfDisplayingCards;
            return nextPos;
        }

        public Vector2 NextBookPosition()
        {
            Vector2 nextPos = BookPosition + Vector2.right * Constants.PLAYER_BOOK_POSITION_OFFSET * NumberOfBooks;
            return nextPos;
        }

        public Vector2 GetBookPositionByIndex(int index)
        {
            Vector2 nextPos = BookPosition + Vector2.right * Constants.PLAYER_BOOK_POSITION_OFFSET * index;
            return nextPos;
        }

        public void SetCardValues(List<byte> values)
        {
            if (DisplayingCards.Count != values.Count)
            {
                Debug.LogError($"Displaying cards count {DisplayingCards.Count} is not equal to card values count {values.Count} for {PlayerId}");
                return;
            }

            for (int index = 0; index < values.Count; index++)
            {
                Card card = DisplayingCards[index];
                card.SetCardValue(values[index]);
                card.SetDisplayingOrder(index);
            }
        }

        public void HideCardValues()
        {
            foreach (Card card in DisplayingCards)
            {
                card.SetFaceUp(false);
            }
        }

        public void ShowCardValues()
        {
            foreach (Card card in DisplayingCards)
            {
                card.SetFaceUp(true);
            }
        }

        public void HideBookValues()
        {
            foreach (Card card in DisplayingBooks)
            {
                card.SetFaceUp(false);
            }
        }

        public void ShowBookValues()
        {
            foreach (Card card in DisplayingBooks)
            {
                card.SetFaceUp(true);
            }
        }

        public void ReceiveDisplayingCard(Card card)
        {
            DisplayingCards.Add(card);
            card.OwnerId = PlayerId;
            NumberOfDisplayingCards++;
        }

        public void ReceiveBook(Ranks rank, CardAnimator cardAnimator)
        {
            Vector2 targetPosition = NextBookPosition();
            List<Card> displayingCardsToRemove = new List<Card>();

            foreach (Card card in DisplayingCards)
            {
                if (card.Rank == rank)
                {
                    card.SetFaceUp(true);
                    float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
                    cardAnimator.AddCardAnimation(card, targetPosition, Quaternion.Euler(Vector3.forward * randomRotation));
                    displayingCardsToRemove.Add(card);

                    DisplayingBooks.Add(card);
                }
            }

            DisplayingCards.RemoveAll(card => displayingCardsToRemove.Contains(card));
            RepositionDisplayingCards(cardAnimator);
            NumberOfBooks++;
        }

        public void RestoreBook(Ranks rank, CardAnimator cardAnimator)
        {
            Vector2 targetPosition = NextBookPosition();

            for (int i = 0; i < 4; i++)
            {
                Card card = cardAnimator.TakeFirstDisplayingCard();

                int intRankValue = (int)rank;
                int cardValue = (intRankValue - 1) * 4 + i;

                card.SetCardValue((byte)cardValue);
                card.SetFaceUp(true);

                float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
                card.transform.position = targetPosition;
                card.transform.rotation = Quaternion.Euler(Vector3.forward * randomRotation);
                DisplayingBooks.Add(card);
            }

            NumberOfBooks++;
        }

        public void CreateBottomBook(Ranks rank, CardAnimator cardAnimator, int index)
        {
            Vector2 targetPosition = GetBookPositionByIndex(index);
            Card card = cardAnimator.TakeFirstDisplayingCard();

            int intRankValue = (int)rank;
            int cardValue = (intRankValue - 1);

            card.Rank = rank;
            card.Suit = (Suits) UnityEngine.Random.Range(0,3);
            //card.SetCardValue((byte)cardValue);
            card.SetDisplayingOrder(-1);
            card.OwnerId = PlayerId;
            card.SetFaceUp(false);

            float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
            Quaternion rotation = Quaternion.Euler(Vector3.forward * randomRotation);

            cardAnimator.AddCardAnimation(card, targetPosition, rotation);
            HiddenBooks.Add(card);
            NumberOfHiddenBooks++;
        }

        public void CreateTopBook(Ranks rank, CardAnimator cardAnimator, int index)
        {
            Vector2 targetPosition = GetBookPositionByIndex(index);

            Card card = cardAnimator.TakeFirstDisplayingCard();

            //card.Suit = (Suits)UnityEngine.Random.Range(0, 3);
            //card.Rank = rank;

            card.SetCardValue((byte)rank);

            card.SetDisplayingOrder(0);
            card.OwnerId = PlayerId;
            //card.SetFaceUp(true);


            float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
            Quaternion rotation = Quaternion.Euler(Vector3.forward * randomRotation);

            cardAnimator.AddCardAnimation(card, targetPosition, rotation);
            DisplayingBooks.Add(card);

            NumberOfBooks++;
        }

        public void RepositionDisplayingCards(CardAnimator cardAnimator)
        {
            NumberOfDisplayingCards = 0;
            foreach (Card card in DisplayingCards)
            {
                NumberOfDisplayingCards++;
                cardAnimator.AddCardAnimation(card, NextCardPosition());
            }
        }

        public void SendDisplayingCardToPlayer(Player receivingPlayer, CardAnimator cardAnimator, List<byte> cardValues, bool isLocalPlayer)
        {
            int playerDisplayingCardsCount = DisplayingCards.Count;

            if (playerDisplayingCardsCount < cardValues.Count)
            {
                Debug.LogError("Not enough displaying cards");
                return;
            }

            for (int index = 0; index < cardValues.Count; index++)
            {

                Card card = null;
                byte cardValue = cardValues[index];

                if (isLocalPlayer)
                {
                    foreach (Card c in DisplayingCards)
                    {
                        if (c.Rank == Card.GetRank(cardValue) && c.Suit == Card.GetSuit(cardValue))
                        {
                            card = c;
                            break;
                        }
                    }
                }
                else
                {
                    card = DisplayingCards[playerDisplayingCardsCount - 1 - index];
                    card.SetCardValue(cardValue);
                    card.SetFaceUp(true);
                }

                if(card != null)
                {
                    DisplayingCards.Remove(card);
                    receivingPlayer.ReceiveDisplayingCard(card);
                    cardAnimator.AddCardAnimation(card, receivingPlayer.NextCardPosition());
                    NumberOfDisplayingCards--;
                }
                else
                {
                    Debug.LogError("Unable to find displaying card.");
                }
            }

            RepositionDisplayingCards(cardAnimator);
        }


        public Boolean SendDisplayingCardToStack(CardAnimator cardAnimator, byte cardValue, bool isLocalPlayer)
        {
            int playerDisplayingCardsCount = DisplayingCards.Count;

            if (playerDisplayingCardsCount == 0)
            {
                Debug.LogError("Not enough displaying cards420");
                return false;
            }


                Card card = null;

                if (isLocalPlayer)
                {
                    foreach (Card c in DisplayingCards)
                    {
                        if (c.Rank == Card.GetRank(cardValue) && c.Suit == Card.GetSuit(cardValue))
                        {
                            card = c;
                            break;
                        }
                    }
                }
                else
                {
                    card = DisplayingCards[playerDisplayingCardsCount - 1];
                    card.SetCardValue(cardValue);
                    card.SetFaceUp(true);
                }

                if (card != null)
                {
                    Card topStackCard = cardAnimator.GetStackTopCard();

                    if(topStackCard != null)
                    {
                        if(card.Rank >= topStackCard.Rank)
                        {
                            PutCardInStack(cardAnimator, card);
                            return true;
                        }
                        else if(topStackCard.Rank == Ranks.Seven)
                        {
                            if (card.Rank <= Ranks.Seven)
                            {
                                PutCardInStack(cardAnimator, card);
                                return true;
                            }
                            else
                            {
                                Debug.LogError("You encountred 7");
                                return false;
                            }
                        }
                        else
                        {
                            Debug.LogError("You encountred Need bigger Card");

                            return false;
                        }
                    }else
                    {
                        PutCardInStack(cardAnimator, card);
                        return true;
                    }
                    
                }
                else
                {
                    Debug.LogError("Unable to find displaying card.");
                    return false;
                }
            return false;
        }

        private void PutCardInStack(CardAnimator cardAnimator, Card card)
        {
            DisplayingCards.Remove(card);
            cardAnimator.AddCardToStack(card);
            NumberOfDisplayingCards--;
            
            RepositionDisplayingCards(cardAnimator);
        }

        public bool Equals(Player other)
        {
            if (PlayerId.Equals(other.PlayerId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
