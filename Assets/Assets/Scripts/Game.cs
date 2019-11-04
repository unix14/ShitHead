using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace GoFish
{
    public class Game : MonoBehaviour
    {
        public const int PLAYER_INITIAL_2_CARDS = 2;
        public const int PLAYER_INITIAL_CARDS = 6;

        public Text MessageText;

        protected CardAnimator cardAnimator;

        public GameDataManager gameDataManager;

        public List<Transform> PlayerPositions = new List<Transform>();
        public List<Transform> BookPositions = new List<Transform>();

        protected Player localPlayer;
        protected Player remotePlayer;

        protected Player currentTurnPlayer;
        protected Player currentTurnTargetPlayer;

        //protected Card selectedCard;
        protected Ranks selectedRank;
        protected byte selectedCardValue;
        public List<Card> selectedCards = new List<Card>();

        public enum GameState
        {
            Idel,
            GameStarted,
            TurnStarted,
            TurnSelectingNumber,
            TurnConfirmedSelectedNumber,
            //TurnWaitingForOpponentConfirmation,
            //TurnOpponentConfirmed,
            TurnGoFish,
            GameFinished
        };

        public GameState gameState = GameState.Idel;

        protected void Awake()
        {
            localPlayer = new Player(); 
            localPlayer.PlayerId = "offline-player";
            localPlayer.PlayerName = "Player";
            localPlayer.Position = PlayerPositions[0].position;
            localPlayer.BookPosition = BookPositions[0].position;

            remotePlayer = new Player();
            remotePlayer.PlayerId = "offline-bot";
            remotePlayer.PlayerName = "Bot";
            remotePlayer.Position = PlayerPositions[1].position;
            remotePlayer.BookPosition = BookPositions[1].position;
            remotePlayer.IsAI = true;

            cardAnimator = FindObjectOfType<CardAnimator>();
        }

        void Start()
        {
            gameState = GameState.GameStarted;
            GameFlow();
        }

        //****************** Game Flow *********************//
        public virtual void GameFlow()
        {
            if (gameState > GameState.GameStarted)
            {
                CheckPlayersBooks();
                ShowAndHidePlayersDisplayingCards();

                if (gameDataManager.GameFinished())
                {
                    gameState = GameState.GameFinished;
                }
            }

            switch (gameState)
            {
                case GameState.Idel:
                    {
                        Debug.Log("IDEL");
                        break;
                    }
                case GameState.GameStarted:
                    {
                        Debug.Log("GameStarted");
                        OnGameStarted();
                        break;
                    }
                case GameState.TurnStarted:
                    {
                        Debug.Log("TurnStarted");
                        OnTurnStarted();
                        break;
                    }
                case GameState.TurnSelectingNumber:
                    {
                        Debug.Log("TurnSelectingNumber");
                        OnTurnSelectingNumber();
                        break;
                    }
                case GameState.TurnConfirmedSelectedNumber:
                    {
                        Debug.Log("TurnComfirmedSelectedNumber");
                        OnTurnConfirmedSelectedNumber();
                        break;
                    }
                //case GameState.TurnWaitingForOpponentConfirmation:
                //    {
                //        Debug.Log("TurnWaitingForOpponentConfirmation");
                //        OnTurnWaitingForOpponentConfirmation();
                //        break;
                //    }
                //case GameState.TurnOpponentConfirmed:
                //    {
                //        Debug.Log("TurnOpponentConfirmed");
                //        OnTurnOpponentConfirmed();
                //        break;
                //    }
                case GameState.TurnGoFish:
                    {
                        Debug.Log("TurnGoFish");
                        OnTurnGoFish();
                        break;
                    }
                case GameState.GameFinished:
                    {
                        Debug.Log("GameFinished");
                        OnGameFinished();
                        break;
                    }
            }
        }

        protected virtual void OnGameStarted()
        {
            gameDataManager = new GameDataManager(localPlayer, remotePlayer);
            gameDataManager.Shuffle();


            for (int i = 0; i < 3; i++)
            {
                byte bookRankP1 = gameDataManager.DrawCardValue();
                byte bookRankP2 = gameDataManager.DrawCardValue();

                //cardAnimator.DealBooks(localPlayer, bookRankP1);
                //cardAnimator.DealBooks(remotePlayer, bookRankP2);

                remotePlayer.CreateBottomBook(bookRankP1, cardAnimator,i);
                localPlayer.CreateBottomBook(bookRankP2, cardAnimator, i);

                gameDataManager.AddBooksForPlayer(localPlayer, Card.GetRank(bookRankP1));
                gameDataManager.AddBooksForPlayer(remotePlayer, Card.GetRank(bookRankP2));
            }

            for (int i = 0; i < 3; i++)
            {
                byte bookRankP1 = gameDataManager.DrawCardValue();
                byte bookRankP2 = gameDataManager.DrawCardValue();

                remotePlayer.CreateTopBook(bookRankP1, cardAnimator,i);
                localPlayer.CreateTopBook(bookRankP2, cardAnimator,i);

                //localPlayer.ReceiveBook(bookRankP1, cardAnimator);

                gameDataManager.AddBooksForPlayer(localPlayer, Card.GetRank(bookRankP1));
                gameDataManager.AddBooksForPlayer(remotePlayer, Card.GetRank(bookRankP2));

            }

            for (int i=0; i<3; i++)
            {
                gameDataManager.DealCardValuesToPlayer(localPlayer, PLAYER_INITIAL_2_CARDS);
                gameDataManager.DealCardValuesToPlayer(remotePlayer, PLAYER_INITIAL_2_CARDS);

                cardAnimator.DealDisplayingCards(localPlayer, PLAYER_INITIAL_2_CARDS);
                cardAnimator.DealDisplayingCards(remotePlayer, PLAYER_INITIAL_2_CARDS);
            }

            gameState = GameState.TurnStarted;
        }

        protected virtual void OnTurnStarted()
        {
            SwitchTurn();
            gameState = GameState.TurnSelectingNumber;
            GameFlow();
        }

        public void OnTurnSelectingNumber()
        {
            ResetSelectedCard();

            if (currentTurnPlayer == localPlayer)
            {
                SetMessage($"Your turn. Pick a card from your hand.");
            }
            else
            {
                SetMessage($"{currentTurnPlayer.PlayerName}'s turn");
            }

            if (currentTurnPlayer.IsAI)
            {
                Card stackTopCard = cardAnimator.GetStackTopCard();
                Card stackPreviousTopCard = cardAnimator.GetStackPreviousTopCard();

                selectedCardValue = gameDataManager.AiDecideCardFromPlayer(currentTurnPlayer, stackTopCard, stackPreviousTopCard);
                if (Card.GetRank(selectedCardValue) == Ranks.Ten)
                {
                    gameState = GameState.TurnSelectingNumber;
                }
                else
                {
                    gameState = GameState.TurnConfirmedSelectedNumber;
                }
                GameFlow();
            }
        }

        protected virtual void OnTurnConfirmedSelectedNumber()
        {
            //gameDataManager.RemoveCardValueFromPlayer(currentTurnPlayer, (byte)selectedRank);
            //TakeCardFromPileIfNeeded();
            if (currentTurnPlayer == localPlayer)
            {
                SetMessage($"Asking {currentTurnTargetPlayer.PlayerName} for {selectedRank}s...");
            }
            else
            {
                SetMessage($"{currentTurnPlayer.PlayerName} is asking for {selectedRank}s...");
            }



            //byte cardValueFromTargetPlayer = gameDataManager.TakeOneCardValueWithRankFromPlayer(currentTurnPlayer, selectedCard.Rank);

            //selectedCardValue = selectedCards[0].Value;
            List<Byte> selectedCardValues = GetSelectedCardValues();

            if (selectedCardValue != Card.NO_VALUE)
            {
                //currentTurnPlayer.PutCardInStack(cardAnimator, selectedCard);
                bool senderIsLocalPlayer = currentTurnPlayer == localPlayer;

                Boolean actionResult = currentTurnPlayer.SendDisplayingCardToStack(cardAnimator, GetSelectedCardValues(), senderIsLocalPlayer);

                if (actionResult)
                {
                    gameDataManager.RemoveCardValuesFromPlayer(currentTurnPlayer, selectedCardValues);   //selectedCard
                    gameState = GameState.TurnStarted;
                    TakeCardFromPileIfNeeded();
                }
                else
                {
                    SetMessage("Incorrect choice");

                    gameState = GameState.TurnSelectingNumber;
                    //GameFlow();
                    ResetSelectedCard();
                }
                selectedCardValue = 0;  
            }
            else
            {
                SetMessage("Take the Pile");

                OnTakeStackCards();
                gameState = GameState.TurnSelectingNumber;
                GameFlow();
            }
        }

        private List<byte> GetSelectedCardValues()
        {
            List<byte> selectedCardValues = new List<byte>();
            foreach (Card card in selectedCards)
            {
                selectedCardValues.Add(card.Value);
            }
            return selectedCardValues;
        }

        private void TakeCardFromPile()
        {
            //List<byte> playerCardValues = gameDataManager.PlayerCards(remotePlayer);


            byte cardValue = gameDataManager.DrawCardValue();

            if (cardValue == Constants.POOL_IS_EMPTY)
            {
                Debug.LogError("Pool is empty");
                return;
            }

            //playerCardValues.Add(cardValue);
            //currentTurnPlayer.SetCardValues(playerCardValues);

            if (Card.GetRank(cardValue) == selectedRank)
            {
                cardAnimator.DrawDisplayingCard(currentTurnPlayer, cardValue);
            }
            else
            {
                cardAnimator.DrawDisplayingCard(currentTurnPlayer);

            }

            //gameDataManager.DealCardValuesToPlayer(currentTurnPlayer, 1);

            //cardAnimator.DealDisplayingCards(currentTurnPlayer, 1);

            gameState = GameState.TurnStarted;

            gameDataManager.AddCardValueToPlayer(currentTurnPlayer, cardValue);


            GameFlow();
        }

        private void TakeCardFromPileIfNeeded()
        {
            if(currentTurnPlayer.DisplayingCards.Count < PLAYER_INITIAL_CARDS)
            {
                TakeCardFromPile();
            }
        }

        public void OnTakeStackCards()
        {
            List<byte> stackCardsValues = cardAnimator.getStackValues();
            bool senderNotIsLocalPlayer = currentTurnPlayer != localPlayer;

            currentTurnPlayer.SendStackCardsToPlayer(currentTurnPlayer, cardAnimator, stackCardsValues, senderNotIsLocalPlayer);

            //cardAnimator.
            gameDataManager.AddCardValuesToPlayer(currentTurnPlayer, stackCardsValues);
        }

        //public void OnTurnWaitingForOpponentConfirmation()
        //{
        //    if (currentTurnTargetPlayer.IsAI)
        //    {
        //        gameState = GameState.TurnOpponentConfirmed;
        //        GameFlow();
        //    }
        //}

        //protected virtual void OnTurnOpponentConfirmed()
        //{
        //    byte cardValueFromTargetPlayer = gameDataManager.TakeOneCardValueWithRankFromPlayer(currentTurnPlayer, selectedRank);

        //    if (cardValueFromTargetPlayer > 0)
        //    {

        //        bool senderIsLocalPlayer = currentTurnPlayer == localPlayer;

        //        Boolean actionResult = currentTurnPlayer.SendDisplayingCardToStack(cardAnimator, cardValueFromTargetPlayer, senderIsLocalPlayer);

        //        if (actionResult)
        //        {
        //            gameDataManager.RemoveCardValueFromPlayer(currentTurnPlayer, (byte)selectedRank);
        //            gameState = GameState.TurnSelectingNumber;
        //            TakeCardFromPileIfNeeded();
        //        }
        //        else
        //        {
        //            SetMessage("Incorrect choice");

        //            gameState = GameState.TurnSelectingNumber;
        //            GameFlow();
        //        }
        //    }
        //    else
        //    {
        //        SetMessage("Incorrect choice");

        //        gameState = GameState.TurnSelectingNumber;
        //        GameFlow();
        //    }
        //}

        protected virtual void OnTurnGoFish()
        {
            SetMessage($"Go FUCK fish!");

            TakeCardFromPileIfNeeded();
        }

        public void OnGameFinished()
        {
            if (gameDataManager.Winner() == localPlayer)
            {
                SetMessage($"You WON!");
            }
            else if (gameDataManager.Winner() == remotePlayer)
            {
                SetMessage($"You LOST!");
            }
        }

        //****************** Helper Methods *********************//
        public void ResetSelectedCard()
        {
            if (selectedCards.Count > 0)
            {
                foreach(Card c in selectedCards)
                {
                    c.OnSelected(false);
                }
                selectedRank = 0;   //TODO REMOvE?
            }
            selectedCardValue = 0;
        }

        protected void SetMessage(string message)
        {
            MessageText.text = message;
        }

        public void SwitchTurn()
        {
            if (currentTurnPlayer == null)
            {
                currentTurnPlayer = localPlayer;
                currentTurnTargetPlayer = remotePlayer;
                return;
            }

            if (currentTurnPlayer == localPlayer)
            {
                currentTurnPlayer = remotePlayer;
                currentTurnTargetPlayer = localPlayer;
            }
            else
            {
                currentTurnPlayer = localPlayer;
                currentTurnTargetPlayer = remotePlayer;
            }
        }

        public void PlayerShowBooksIfNecessary(Player player)
        {
            Dictionary<Ranks, List<byte>> books = gameDataManager.GetBooks(player);

            if (books != null)
            {
                foreach (var book in books)
                {
                    player.ReceiveBook(book.Key, cardAnimator);

                    gameDataManager.RemoveCardValuesFromPlayer(player, book.Value);
                    gameDataManager.AddBooksForPlayer(player, book.Key);

                }
            }
        }

        public void CheckPlayersBooks()
        {
            List<byte> playerCardValues = gameDataManager.PlayerCards(localPlayer);
            localPlayer.SetCardValues(playerCardValues);
            PlayerShowBooksIfNecessary(localPlayer);

            playerCardValues = gameDataManager.PlayerCards(remotePlayer);
            remotePlayer.SetCardValues(playerCardValues);
            PlayerShowBooksIfNecessary(remotePlayer);
        }

        public void ShowAndHidePlayersDisplayingCards()
        {
            localPlayer.ShowCardValues();
            remotePlayer.HideCardValues();
            localPlayer.ShowBookValues();
            remotePlayer.ShowBookValues();
        }

        //****************** User Interaction *********************//
        public void OnCardSelected(Card card)
        {
            if (gameState == GameState.TurnSelectingNumber)
            {
                if (card.isInStack)
                {
                    //cardAnimator.openLast3CardsFromStack();
                    OnTakeStackCards();
                }
                else if (card.isTouchable)
                {
                    if (card.OwnerId == currentTurnPlayer.PlayerId && card.GetFaceUp())
                    {
                        List<Card> newSelectedCards = new List<Card>();
                        if (selectedCards.Count > 0)
                        {
                            newSelectedCards.AddRange(selectedCards);
                            if (card.Rank == selectedRank) //newSelectedCards[0].Rank 
                            {
                                newSelectedCards.Add(card);
                            }
                            else
                            {
                                newSelectedCards.Clear();
                                foreach (Card c in selectedCards)
                                {
                                    c.OnSelected(false);
                                }
                                selectedRank = 0;
                                selectedCardValue = Card.NO_VALUE;
                            }
                        }
                        else
                        {
                            newSelectedCards.Add(card);
                        }

                        selectedCards = newSelectedCards;

                        selectedRank = card.Rank;
                        selectedCardValue = card.GetCardValue();

                        if(selectedCards.Count >0)
                        {
                            foreach (Card c in selectedCards)
                            {
                                c.OnSelected(true);
                            }
                        }
                        else
                        {
                            card.OnSelected(true);
                        }

                        SetMessage($"Throw {card.Rank} for {currentTurnTargetPlayer.PlayerName}?");
                    }
                } 
            }
        }

        public virtual void OnOkSelected()
        {
            if (gameState == GameState.TurnSelectingNumber && localPlayer == currentTurnPlayer)
            {
                if (selectedCards.Count > 0)
                {
                    gameState = GameState.TurnConfirmedSelectedNumber;
                    GameFlow();
                }
            }
            else if (gameState == GameState.TurnConfirmedSelectedNumber && localPlayer == currentTurnTargetPlayer)
            {
                gameState = GameState.TurnStarted;
                GameFlow();
            }
            else if (gameState == GameState.GameFinished || gameDataManager.GameFinished())
            {
                SceneManager.LoadScene(0);
            } else if (gameDataManager.GameFinishedPoolOfCards())
            {
                OnGameFinishedPoolOfCards();
            } else if (gameDataManager.Player1FinishedPoolOfCards())
            {
                OnPlayer1FinishedPoolOfCards();
            }
            else if (gameDataManager.Player2FinishedPoolOfCards())
            {
                OnPlayer2FinishedPoolOfCards();
            }
        }

        private void OnPlayer1FinishedPoolOfCards()
        {
            localPlayer.SetTopBooksClickable(true);
        }

        private void OnPlayer2FinishedPoolOfCards()
        {
            remotePlayer.SetTopBooksClickable(true);
        }

        private void OnGameFinishedPoolOfCards()
        {

        }

        //****************** Animator Event *********************//
        public virtual void AllAnimationsFinished()
        {
            GameFlow();
        }
    }
}
