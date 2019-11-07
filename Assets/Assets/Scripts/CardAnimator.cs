using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GoFish
{
    public class CardAnimation
    {
        public Card card;
        public Vector2 destination;
        public Quaternion rotation;
        public float animationSpeed;


        public CardAnimation(Card c, Vector2 pos)
        {
            card = c;
            destination = pos;
            rotation = Quaternion.identity;
            animationSpeed = Constants.CARD_MOVEMENT_SPEED;
        }

        public CardAnimation(Card c, Vector2 pos, Quaternion rot)
        {
            card = c;
            destination = pos;
            rotation = rot;
            animationSpeed = Constants.CARD_MOVEMENT_SPEED;
        }

        public CardAnimation(Card c, Vector2 pos, Quaternion rot,float animSpeed)
        {
            card = c;
            destination = pos;
            rotation = rot;
            animationSpeed = animSpeed;
        }

        public bool Play()
        {
            bool finished = false;

            if (Vector2.Distance(card.transform.position, destination) < Constants.CARD_SNAP_DISTANCE)
            {
                card.transform.position = destination;
                finished = true;
            }
            else
            {
                card.transform.position = Vector2.MoveTowards(card.transform.position, destination, animationSpeed * Time.deltaTime);
                card.transform.rotation = Quaternion.Lerp(card.transform.rotation, rotation, Constants.CARD_ROTATION_SPEED * Time.deltaTime);
            }

            return finished;
        }
    }

    /// <summary>
    /// Controls all card animations in the game
    /// </summary>
    public class CardAnimator : MonoBehaviour
    {
        public GameObject CardPrefab;

        public List<Card> DisplayingCards;

        public List<Card> BooksCards;

        public List<Card> StackCards;

        public Queue<CardAnimation> cardAnimations;

        CardAnimation currentCardAnimation;

        Vector2 startPosition = new Vector2(-15f, -1f);
        Vector2 stackPosition = new Vector2(0f, 0f);

        // invoked when all queued card animations have been played
        public UnityEvent OnAllAnimationsFinished = new UnityEvent();

        bool working = false;

        public int stackSize { get => StackCards.Count; }
        public int dumpStackSize = 0;

        public Card GetStackTopCard(){
            return GetCardAtPosition(1);
        }

        public Card GetStackPreviousTopCard()
        {
            return GetCardAtPosition(2);
        }

        public Card GetCardAtPosition(int indexFromTop)
        {
            if (StackCards != null && stackSize > indexFromTop-1)
            {
                return StackCards[stackSize - indexFromTop];
            }
            return null;
        }

        public bool isTop4CardsAreSameRank()
        {
            if(StackCards != null && stackSize > 3)
            {
                Card firstTopCard = GetStackTopCard();
                Card secondTopCard = GetStackPreviousTopCard();
                Card thirdTopCard = GetCardAtPosition(3);
                Card forthTopCard = GetCardAtPosition(4);

                Ranks rank = firstTopCard.Rank;

                if (firstTopCard != null && secondTopCard != null && thirdTopCard != null && forthTopCard != null
                    && rank == secondTopCard.Rank && rank == thirdTopCard.Rank && rank == forthTopCard.Rank )
                {
                    return true;
                }
            }
            return false;
        }
        void Start()
        {
            cardAnimations = new Queue<CardAnimation>();
            InitializeDeck();
        }

        void InitializeDeck()
        {
            DisplayingCards = new List<Card>();
            BooksCards = new List<Card>();
            StackCards = new List<Card>();

            for (byte value = 0; value < 52; value++)
            {
                Vector2 newPosition = startPosition + Vector2.up * Constants.DECK_CARD_POSITION_OFFSET * value;
                GameObject newGameObject = Instantiate(CardPrefab, newPosition, Quaternion.identity);
                newGameObject.transform.parent = transform;
                Card card = newGameObject.GetComponent<Card>();
                card.SetDisplayingOrder(-1);
                card.transform.position = newPosition;
                DisplayingCards.Add(card);
            }
        }

        public Card TakeFirstDisplayingCard()
        {
            int numberOfDisplayingCard = DisplayingCards.Count;

            if (numberOfDisplayingCard > 0)
            {
                Card card = DisplayingCards[numberOfDisplayingCard - 1];
                DisplayingCards.Remove(card);

                return card;
            }
            return null;
        }

        public void DealDisplayingCards(Player player, int numberOfCard, bool animated = true)
        {
            int start = DisplayingCards.Count - 1;
            int finish = DisplayingCards.Count - 1 - numberOfCard;

            List<Card> cardsToRemoveFromDeck = new List<Card>();

            for (int i = start; i > finish; i--)
            {
                Card card = DisplayingCards[i];
                player.ReceiveDisplayingCard(card);
                cardsToRemoveFromDeck.Add(card);
                if (animated)
                {
                    AddCardAnimation(card, player.NextCardPosition());
                }
                else
                {
                    card.transform.position = player.NextCardPosition();
                }
                
            }

            foreach (Card card in cardsToRemoveFromDeck)
            {
                DisplayingCards.Remove(card);
            }
        }

        public void DealBooks(Player player, Ranks rank)
        {
            //Vector2 newPosition = startPosition  * value;
            Vector2 newPosition = player.NextBookPosition();
            GameObject newGameObject = Instantiate(CardPrefab, newPosition, Quaternion.identity);
            newGameObject.transform.parent = transform;
            Card card = newGameObject.GetComponent<Card>();
            //card.SetDisplayingOrder(-1);
            card.Rank = rank;
            card.transform.position = newPosition;
            BooksCards.Add(card);
           
            //if (animated)
             //{
                 AddCardAnimation(card, newPosition);
                player.ReceiveBook(rank, this);
            //}
            //else
            //{
            //    card.transform.position = newPosition;
            //}
        }

        public void DrawDisplayingCard(Player player)
        {
            int numberOfDisplayingCard = DisplayingCards.Count;

            if (numberOfDisplayingCard > 0)
            {
                Card card = DisplayingCards[numberOfDisplayingCard - 1];
                player.ReceiveDisplayingCard(card);
                AddCardAnimation(card, player.NextCardPosition());

                DisplayingCards.Remove(card);
            }
        }

        public void DrawDisplayingCard(Player player, byte value)
        {
            int numberOfDisplayingCard = DisplayingCards.Count;

            if (numberOfDisplayingCard > 0)
            {
                Card card = DisplayingCards[numberOfDisplayingCard - 1];
                card.SetCardValue(value);
                card.SetFaceUp(true);
                player.ReceiveDisplayingCard(card);
                AddCardAnimation(card, player.NextCardPosition());

                DisplayingCards.Remove(card);
            }
        }

        public void AddCardAnimation(Card card, Vector2 position)
        {
            CardAnimation ca = new CardAnimation(card, position);
            cardAnimations.Enqueue(ca);
            working = true;
        }

        public void AddCardAnimation(Card card, Vector2 position, int index)
        {
            CardAnimation ca = new CardAnimation(card, position, Quaternion.identity, Constants.STACK_OF_CARDS_MOVEMENT_SPEED + (index * 5));

            cardAnimations.Enqueue(ca);
            working = true;
        }

        public void AddCardToStack(Card card)
        {
            float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
            Quaternion rotation = Quaternion.Euler(Vector3.forward * randomRotation);

            CardAnimation ca = new CardAnimation(card, stackPosition, rotation);
            
            StackCards.Add(card);
            card.SetDisplayingOrder(stackSize);

            cardAnimations.Enqueue(ca);
            working = true;
        }

        public void Add10CardToStack(Card card)
        {
            float randomRotation = UnityEngine.Random.Range(-1 * Constants.BOOK_MAX_RANDOM_ROTATION, Constants.BOOK_MAX_RANDOM_ROTATION);
            Quaternion rotation = Quaternion.Euler(Vector3.forward * randomRotation);


            CardAnimation ca = new CardAnimation(card, stackPosition, rotation);

            StackCards.Add(card);
            card.SetDisplayingOrder(stackSize);

            //card.SetFaceUp(true);
            cardAnimations.Enqueue(ca);
            
            burnStackToDeathPile();

            working = true;
        }

        public void burnStackToDeathPile()
        {
            for (int i = 0; i < StackCards.Count; i++)
            {
                Card c = StackCards[i];
                CardAnimation cleanStackAniamtion = new CardAnimation(c, startPosition * -1, c.transform.rotation, Constants.STACK_OF_CARDS_MOVEMENT_SPEED + (i * 5));
                cardAnimations.Enqueue(cleanStackAniamtion);

                c.isTouchable = false;
                c.isInStack = false;
                dumpStackSize++;
                c.SetDisplayingOrder(dumpStackSize);
            }
            working = true;

            StackCards.Clear();
        }

        public List<Card> getStack()
        {
            return StackCards;
        }

        public List<byte> getStackValues()
        {
            List<byte> returnedList = new List<byte>();

            foreach(Card c in StackCards)
            {
                returnedList.Add(c.Value);
            }
            return returnedList;
        }

        public void AddCardAnimation(Card card, Vector2 position, Quaternion rotation)
        {
            CardAnimation ca = new CardAnimation(card, position, rotation);
            cardAnimations.Enqueue(ca);
            working = true;
        }

        public void openLast3CardsFromStack(bool isOpen)
        {
            int allStackCardsCount = StackCards.Count - 1;

            if (allStackCardsCount > 3 && isOpen)
            {
                for (int i = 0; i <= 4; i++)
                {
                        StackCards[allStackCardsCount - i].transform.position = (Vector2)transform.position + Vector2.right *Constants.CARD_SELECTED_STACK_OFFSET* (i* 3.2f);
                }
            } else if (isOpen)
            {
                for (int i = 0; i <= allStackCardsCount; i++)
                {
                    StackCards[allStackCardsCount - i].transform.position = (Vector2)transform.position + Vector2.right * Constants.CARD_SELECTED_STACK_OFFSET * (i * 3.2f);
                }
            }
            else
            {
                closeStackCards();
            }
        }

        public void closeStackCards()
        {
            int allStackCardsCount = StackCards.Count - 1;

            for (int i = 0; i <= allStackCardsCount; i++)
            {
                Quaternion oldRotation = StackCards[i].transform.rotation;
                StackCards[i].transform.position = stackPosition;
                StackCards[i].transform.rotation = oldRotation;
            }
        }

        private void Update()
        {
            if (currentCardAnimation == null)
            {
                NextAnimation();
            }
            else
            {
                if (currentCardAnimation.Play())
                {
                    NextAnimation();
                }
            }
        }

        internal void clearStack()
        {
            StackCards.Clear();
        }

        void NextAnimation()
        {
            if(cardAnimations == null)
            {
                cardAnimations = new Queue<CardAnimation>();
            }
            currentCardAnimation = null;

            if (cardAnimations.Count > 0)
            {
                CardAnimation ca = cardAnimations.Dequeue();
                currentCardAnimation = ca;
            }
            else
            {
                if (working)
                {
                    working = false;
                    OnAllAnimationsFinished.Invoke();
                }
            }
        }
    }
}
