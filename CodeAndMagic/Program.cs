using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Program
{
    class Player
    {
        public int Health;
        public int Mana;
        public int Deck;
        public int Rune;

        public void Read()
        {
            string[] inputs = Console.ReadLine().Split(' ');
            Health = int.Parse(inputs[0]);
            Mana = int.Parse(inputs[1]);
            Deck = int.Parse(inputs[2]);
            Rune = int.Parse(inputs[3]);
        }
    }

    class Card
    {
        public int CardNumber;
        public int InstanceId;
        public CardLocation Location;
        public int CardType;
        public int Cost;
        public int Attack;
        public int Defense;
        public string Abilities;
        public int MyHealthChange;
        public int OpponentHealthChange;
        public int CardDraw;

        public void Read()
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int location = int.Parse(inputs[2]);
            CardNumber = int.Parse(inputs[0]);
            InstanceId = int.Parse(inputs[1]);
            Location = location == 0 ? CardLocation.MyHand : location == 1 ? CardLocation.MyBoard : CardLocation.OpponentBoard;
            CardType = int.Parse(inputs[3]);
            Cost = int.Parse(inputs[4]);
            Attack = int.Parse(inputs[5]);
            Defense = int.Parse(inputs[6]);
            Abilities = inputs[7];
            MyHealthChange = int.Parse(inputs[8]);
            OpponentHealthChange = int.Parse(inputs[9]);
            CardDraw = int.Parse(inputs[10]);
        }
    }

    enum CardLocation
    {
        MyHand,
        MyBoard,
        OpponentBoard,
    }
    
    static void Main(string[] args)
    {
        string[] inputs;
        // game loop
        while (true)
        {
            Player me = new Player();
            Player opponent = new Player();
            
            me.Read();
            opponent.Read();
            
            int opponentHand = int.Parse(Console.ReadLine());
            int cardCount = int.Parse(Console.ReadLine());
            
            List<Card> cards = new List<Card>(cardCount);
            for (int i = 0; i < cardCount; i++)
            {
                Card card = new Card();
                card.Read();
                cards.Add(card);
            }

            if (me.Mana == 0)
            {
                Console.WriteLine(Draft(cards));
            }
            else
            {
                Console.WriteLine(Play(me, opponent, opponentHand, cards));
            }
            
            //Console.WriteLine("PASS");
        }
    }

    private static string Play(Player me, Player opponent, int opponentHand, List<Card> cards)
    {
        Card[] board = cards.Where(x => x.Location == CardLocation.MyBoard).ToArray();
        Card[] hand = cards.Where(x => x.Location == CardLocation.MyHand && x.Cost <= me.Mana).OrderByDescending(x => x.Cost).ToArray();
        if (IsLethal(opponent.Health, board))
        {
            return string.Join(";", board.Select(x => $"ATTACK {x.InstanceId} -1 Lethal mode"));
        }

        string summonAction = null;
        if (hand.Length > 0)
        {
            //trouver la carte à jouer
            List<Card> cardsToPlay = new List<Card>(2);
            int mana = me.Mana;

            for (int i = 0; i < hand.Length && mana > 0; ++i)
            {
                Card c = hand[i];
                if (c.Cost < mana)
                {
                    cardsToPlay.Add(c);
                    mana -= c.Cost;
                }
            }

            if (cardsToPlay.Count > 0)
            {
                summonAction = string.Join(";", cardsToPlay.Select(x => $"SUMMON {x.InstanceId}"));
            }
        }

        string attackAction = null;
        if (board.Length > 0)
        {
            //attack
            
            attackAction = string.Join(";", board.Select(x => $"ATTACK {x.InstanceId} -1 yolo"));
        }

        if (summonAction != null || attackAction != null)
        {
            if (summonAction == null)
            {
                return attackAction;
            }

            if (attackAction == null)
            {
                return summonAction;
            }

            return $"{summonAction};{attackAction}";
        }
        
        return "PASS";
    }

    private static bool IsLethal(int opponentHealthPoint, Card[] cards)
    {
        int attackPoints = cards.Sum(x => x.Attack);
        return attackPoints >= opponentHealthPoint;
    }

    private static int[] _draftCardsPerManaCount = new int[13];
    private static double _minValueToTake = 2.95;

    private static int[] _draftMaximalCards = 
    {
        // 0 - 5
        1, 2, 2, 3, 4, 4,
        // 6 - 10
        3, 2, 2, 1, 0,
        // 11 - 12
        0, 1,
    };
    
    private static string Draft(List<Card> cards)
    {
        double[] cardValues = new double[3];
        int bestCardIndex = -1;
        double bestValue = -100;
        
        for (var i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];

            int currentManaCount = _draftCardsPerManaCount[card.Cost];
            int expectedManaCount = _draftMaximalCards[card.Cost];

            double divider = card.Cost;
            if (card.Cost == 0)
            {
                divider = 0.8;
            }

            double value = (card.Attack + card.Defense * 1.25) / divider;
            
            //ligue bois only
            if (card.Attack == 0)
            {
                value = -10;
            }
            
            cardValues[i] = value;
            
            if (currentManaCount >= expectedManaCount && value < _minValueToTake)
            {
                continue;
            }

            if (bestValue < value)
            {
                bestValue = value;
                bestCardIndex = i;
            }
        }

        if (bestCardIndex < 0)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                if (bestValue < cardValues[i])
                {
                    bestValue = cardValues[i];
                    bestCardIndex = i;
                }
            }
        }

        return $"PICK {bestCardIndex}";
    }
}