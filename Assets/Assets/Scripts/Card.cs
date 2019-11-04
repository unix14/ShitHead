using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.U2D;

namespace GoFish
{
    /// <summary>
    /// SetFaceUp(false) clears card's face value
    /// To display a card's value, call SetCardValue(byte) to assign the Rank and the Suit to the card, then call SetFaceUp(true)
    /// </summary>
    /// 
    public class Card : MonoBehaviour
    {

        public const byte NO_VALUE = 255;

        public static Ranks GetRank(byte value)
        {
            return (Ranks)(value / 4 + 1);
        }

        public static Suits GetSuit(byte value)
        {
            return (Suits)(value % 4);
        }

        public SpriteAtlas Atlas;

        public Suits Suit = Suits.NoSuits;
        public Ranks Rank = Ranks.NoRanks;
        public byte Value = NO_VALUE;

        public bool isTouchable = true;
        public bool isInStack = false;

        public string OwnerId;

        SpriteRenderer spriteRenderer;

        bool faceUp = false;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            UpdateSprite();
        }

        public bool GetFaceUp()
        {
            return faceUp;
        }

        public void SetFaceUp(bool value)
        {
            faceUp = value;
            UpdateSprite();

            // Setting faceup to false also resets card's value.
            if (value == false)
            {
                Rank = Ranks.NoRanks;
                Suit = Suits.NoSuits;
            }
        }

        public void SetCardValue(byte value)
        {
            // 0-3 are 1's
            // 4-7 are 2's
            // ...
            // 48-51 are kings's
            Rank = (Ranks)(value / 4 + 1);

            // 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48 are Spades(0)
            Suit = (Suits)(value % 4);

            Value = value;
        }

        public byte GetCardValue()
        {
            return Value;
        }

        void UpdateSprite()
        {
            if (faceUp)
            {
                spriteRenderer.sprite = Atlas.GetSprite(SpriteName());
            }
            else
            {
                spriteRenderer.sprite = Atlas.GetSprite(Constants.CARD_BACK_SPRITE);
            }
        }

        string GetRankDescription()
        {
            FieldInfo fieldInfo = Rank.GetType().GetField(Rank.ToString());
            DescriptionAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes[0].Description;
        }

        string SpriteName()
        {
            string testName = $"card{Suit}{GetRankDescription()}";
            return testName;
        }

        public void SetDisplayingOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }

        public void OnSelected(bool selected)
        {
            if (selected)
            {
                transform.position = (Vector2)transform.position + Vector2.up * Constants.CARD_SELECTED_OFFSET;
            }
            else
            {
                transform.position = (Vector2)transform.position - Vector2.up * Constants.CARD_SELECTED_OFFSET;
            }
        }

        //public void OnStackCardSelected(bool selected)
        //{
        //    if (selected)
        //    {
        //        transform.position = (Vector2)transform.position + Vector2.right * Constants.CARD_SELECTED_OFFSET * 5;
        //    }
        //    else
        //    {
        //        transform.position = (Vector2)transform.position - Vector2.right * Constants.CARD_SELECTED_OFFSET * 5;
        //    }
        //}
    }
}

