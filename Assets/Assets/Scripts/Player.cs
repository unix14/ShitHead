using System;
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
            Vector2 nextPos;
            if (NumberOfDisplayingCards < 10)
            {
                nextPos = Position + Vector2.right * Constants.PLAYER_CARD_POSITION_OFFSET * NumberOfDisplayingCards;
            }
            else
            {
                nextPos = Position - Vector2.left * Constants.PLAYER_CARD_POSITION_OFFSET * (10-NumberOfDisplayingCards);
            }
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
            Debug.Log("DisplayingCards.Add(card) " + card);
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
                    Debug.Log("DisplayingCards.RemoveAll( " + card);
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

        public void CreateBottomBook(byte cardValue, CardAnimator cardAnimator, int index)
        {
            Vector2 targetPosition = GetBookPositionByIndex(index);
            Card card = cardAnimator.TakeFirstDisplayingCard();

            card.SetCardValue(cardValue);
            card.SetDisplayingOrder(-1);
            card.OwnerId = PlayerId;
            card.SetFaceUp(false);
            card.isTouchable = false;

            float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
            Quaternion rotation = Quaternion.Euler(Vector3.forward * randomRotation);

            cardAnimator.AddCardAnimation(card, targetPosition, rotation);
            HiddenBooks.Add(card);
            NumberOfHiddenBooks++;
        }

        public void CreateTopBook(byte rank, CardAnimator cardAnimator, int index)
        {
            Vector2 targetPosition = GetBookPositionByIndex(index);

            Card card = cardAnimator.TakeFirstDisplayingCard();

            //card.Suit = (Suits)UnityEngine.Random.Range(0, 3);
            //card.Rank = rank;

            card.SetCardValue(rank);

            card.SetDisplayingOrder(0);
            card.OwnerId = PlayerId;
            //card.SetFaceUp(true);
            card.isTouchable = false;

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
                card.transform.rotation = Quaternion.identity;
                
                NumberOfDisplayingCards++;
                cardAnimator.AddCardAnimation(card, NextCardPosition());
            }
        }

        public void SendStackCardsToPlayer(Player receivingPlayer, CardAnimator cardAnimator, List<byte> cardValues, bool isLocalPlayer)
        {
            List<Card> StackCards = cardAnimator.getStack();
            int playerDisplayingCardsCount = StackCards.Count;

            if (playerDisplayingCardsCount < cardValues.Count)
            {
                Debug.LogError("Not enough stack cards");
                return;
            }

            for (int index = 0; index < cardValues.Count; index++)
            {

                Card card = null;
                byte cardValue = cardValues[index];

                if (isLocalPlayer)
                {
                    foreach (Card c in StackCards)
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
                    card = StackCards[playerDisplayingCardsCount - 1 - index];
                    card.SetCardValue(cardValue);
                    card.SetFaceUp(true);
                }

                if (card != null)
                {
                    card.isInStack = false;
                    card.isTouchable = true;

                    receivingPlayer.ReceiveDisplayingCard(card);
                    cardAnimator.AddCardAnimation(card, receivingPlayer.NextCardPosition());
                    if (isLocalPlayer)
                        NumberOfDisplayingCards++;
                    else
                        receivingPlayer.NumberOfDisplayingCards++;
                }
                else
                {
                    Debug.LogError("Unable to find stack card.");
                }
            }
            cardAnimator.clearStack();
            RepositionDisplayingCards(cardAnimator);
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
                    Debug.Log("DisplayingCards.Remove(card) " + card);

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

        public Boolean SendDisplayingCardToStack(CardAnimator cardAnimator, List<byte> cardValues, bool isLocalPlayer)
        {
            int playerDisplayingCardsCount = DisplayingCards.Count;

            if (playerDisplayingCardsCount == 0)
            {
                Debug.LogError("Not enough displaying cards420");
                return false;
            }

            bool result = true;

            for(int i=0; i < cardValues.Count; i++)
            {
                byte cardValue = cardValues[i];
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
                    card = DisplayingCards[i];           //[i] ??
                    card.SetCardValue(cardValue);
                    //result = false;
                }
                result = result && isCardOkToThrow(cardAnimator, card);
            }
            return result;
        }

        private bool isCardOkToThrow(CardAnimator cardAnimator, Card card)
        {
            if (card != null)
            {
                bool doIhaveAluckyCard = card.Rank == Ranks.Two || card.Rank == Ranks.Three || card.Rank == Ranks.Ten;
                Card topStackCard = cardAnimator.GetStackTopCard();
                Card previousStackCard = cardAnimator.GetStackPreviousTopCard();

                if (topStackCard != null)
                {
                    if (topStackCard.Rank == Ranks.Ace)
                    {
                        if (card.Rank == Ranks.Ace || doIhaveAluckyCard)
                        {
                            if (card.Rank == Ranks.Ten)
                            {
                                Debug.LogError("Implement :: put 10 card in stack");
                                Put10CardInStack(cardAnimator, card);
                                return true;
                            }
                            else
                            {
                                PutCardInStack(cardAnimator, card);
                                return true;
                            }
                        }
                        else
                        {
                            Debug.LogError("You encountred ACE");
                            return false;
                        }
                    }
                    else if (topStackCard.Rank == Ranks.Seven)
                    {
                        if (card.Rank <= Ranks.Seven && card.Rank != Ranks.Ace)
                        {
                            PutCardInStack(cardAnimator, card);
                            return true;
                        }
                        else if (doIhaveAluckyCard)
                        {
                            if (card.Rank == Ranks.Ten)
                            {
                                Debug.LogError("Implement :: put 10 card in stack");
                                Put10CardInStack(cardAnimator, card);
                                return true;
                            }
                            else
                            {
                                PutCardInStack(cardAnimator, card);
                                return true;
                            }
                        }
                        else
                        {
                            Debug.LogError("You encountred  Need bigger than 7");
                            return false;
                        }
                    }
                    else if (topStackCard.Rank == Ranks.Two || card.Rank == Ranks.Ace || doIhaveAluckyCard)
                    {
                        if (card.Rank == Ranks.Ten)
                        {
                            Debug.LogError("Implement :: put 10 card in stack");
                            Put10CardInStack(cardAnimator, card);
                            return true;
                        }
                        else
                        {
                            PutCardInStack(cardAnimator, card);
                            return true;
                        }
                    }
                    else if (topStackCard.Rank == Ranks.Three)
                    {
                        if (previousStackCard != null)
                        {
                            if (card.Rank >= previousStackCard.Rank && previousStackCard.Rank != Ranks.Ace)
                            {
                                PutCardInStack(cardAnimator, card);
                                return true;
                            }
                            else if (previousStackCard.Rank == Ranks.Ace && (card.Rank == Ranks.Ace || doIhaveAluckyCard))
                            {
                                if (card.Rank == Ranks.Ten)
                                {
                                    Debug.LogError("Implement :: put 10 card in stack");
                                    Put10CardInStack(cardAnimator, card);
                                    return true;
                                }
                                else
                                {
                                    PutCardInStack(cardAnimator, card);
                                    return true;
                                }

                            }
                            else
                            {
                                Debug.LogError("You encountred Need bigger Card Than " + previousStackCard.Rank);
                                return false;
                            }
                        }
                        else
                        {
                            if (card.Rank >= Ranks.Three)
                            {
                                PutCardInStack(cardAnimator, card);
                                return true;
                            }
                            else
                            {
                                Debug.LogError("You encountred Need bigger Card Than 3");
                                return false;
                            }
                        }
                    }
                    else if (card.Rank >= topStackCard.Rank)
                    {
                        PutCardInStack(cardAnimator, card);
                        return true;
                    }
                    else
                    {
                        Debug.LogError("You encountred Need bigger Card " + card.Rank + card.Value);
                        return false;
                    }
                }
                else
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
        }

        public void Put10CardInStack(CardAnimator cardAnimator, Card card)
        {
            Debug.Log("DisplayingCards.Remove(card10) " + card);
            card.isInStack = true;
            card.isTouchable = false;
            card.SetFaceUp(true);

            DisplayingCards.Remove(card);
            cardAnimator.Add10CardToStack(card);
            NumberOfDisplayingCards--;

            RepositionDisplayingCards(cardAnimator);
        }

        public void PutCardInStack(CardAnimator cardAnimator, Card card)
        {
            Debug.Log("DisplayingCards.Remove(card) " + card);
            card.isInStack = true;
            card.isTouchable = false;
            card.SetFaceUp(true);

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

        internal void SetTopBooksClickable(bool isClickable)
        {
            foreach (Card c in DisplayingBooks)
            {
                c.isTouchable = true;
            }
        }
    }
}
