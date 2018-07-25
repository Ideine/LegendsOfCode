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

	[Flags]
	enum Bonus
	{
		None = 0,
		Breakthrough = 1,
		Charge = 2,
		Guard = 4,
		Drain = 8,
		Lethal = 16,
		Ward = 32
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
		public Bonus Abilities;
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
			Abilities = Bonus.None;
			if (inputs[7][0] == 'B') Abilities |= Bonus.Breakthrough;
			if (inputs[7][0] == 'C') Abilities |= Bonus.Charge;
			if (inputs[7][0] == 'D') Abilities |= Bonus.Drain;
			if (inputs[7][0] == 'G') Abilities |= Bonus.Guard;
			if (inputs[7][0] == 'L') Abilities |= Bonus.Lethal;
			if (inputs[7][0] == 'W') Abilities |= Bonus.Ward;
			
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

	private static IEnumerable<List<Card>> DoCombination(Card[] hand, int mana)
	{
		return ChooseRec(hand, new List<Card>(), 0, mana);
	}

	private static IEnumerable<List<Card>> ChooseRec(Card[] hand, List<Card> entry, int offset, int remainingMana)
	{
		if (offset >= hand.Length)
		{
			yield return entry;
		}
		else
		{
			Card[] local = entry.ToArray();
			bool found = false;
			for (var i = offset; i < hand.Length; i++)
			{
				Card other = hand[i];
				if (other.Cost > remainingMana)
				{
					continue;
				}

				List<Card> t = new List<Card>(local)
				{
					other
				};
				found = true;
				foreach (List<Card> cards in ChooseRec(hand, t, i + 1, remainingMana - other.Cost))
				{
					yield return cards;
				}
			}

			if (!found)
			{
				yield return entry;
			}
		}
	}

	private static string SummonAction(Player me, Card[] hand)
	{
		List<Tuple<int, List<Card>>> playableCombinations = DoCombination(hand, me.Mana)
			.Where(x => x.Count > 0)
			.Select(x => Tuple.Create(x.Sum(y => y.Cost), x))
			.OrderByDescending(x => x.Item1)
			.ThenBy(x => x.Item2.Count)
			.ToList();

		if (playableCombinations.Count == 0)
		{
			return null;
		}

		int firstMana = playableCombinations[0].Item1;
		int firstCount = playableCombinations[0].Item2.Count;

		List<Tuple<List<Card>,int>> filteredItems = playableCombinations.TakeWhile(x => x.Item1 == firstMana && x.Item2.Count == firstCount)
			.Select(x => Tuple.Create(x.Item2, Ecart(x.Item2)))
			.OrderBy(x => x.Item2)
			.ToList();

		int diff = filteredItems[0].Item2;

		List<Card> items = filteredItems.TakeWhile(x => x.Item2 == diff)
			.Select(x => Tuple.Create(x.Item1, x.Item1.Sum(y => y.Defense)))
			.OrderBy(x => x.Item2)
			.Select(x => x.Item1)
			.First();

		return string.Join(";", items.Select(x => $"SUMMON {x.InstanceId}"));
	}

	private static int Ecart(List<Card> cards)
	{
		double average = cards.Average(x => x.Cost);
		return (int) Math.Floor(cards.Sum(x => x.Cost - average));
	}

	private static string Play(Player me, Player opponent, int opponentHand, List<Card> cards)
	{
		Card[] board = cards.Where(x => x.Location == CardLocation.MyBoard).OrderBy(x => x.Attack).ThenBy(x => x.Defense).ToArray();
		Card[] hand = cards.Where(x => x.Location == CardLocation.MyHand && x.Cost <= me.Mana).OrderByDescending(x => x.Cost).ToArray();
		List<Card> opponentBoard = cards.Where(x => x.Location == CardLocation.OpponentBoard).ToList();
		if (IsLethal(opponent.Health, board))
		{
			return string.Join(";", board.Select(x => $"ATTACK {x.InstanceId} -1 Lethal mode"));
		}

		string summonAction = null;
		if (hand.Length > 0)
		{
			summonAction = SummonAction(me, hand);
			/*
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
			*/
		}

		string attackAction = null;
		if (board.Length > 0)
		{
			//attack

			// Trier nos cartes par attaque puis vie (done plus haut)

			//source / target
			List<Tuple<int, int>> result = new List<Tuple<int, int>>();
			foreach (Card myCard in board)
			{
				Card[] killable = opponentBoard.Where(x => x.Defense <= myCard.Attack).OrderBy(x => x.Attack).ToArray();

				if (killable.Length == 0)
				{
					result.Add(Tuple.Create(myCard.InstanceId, -1));
					continue;
				}

				Card current = killable[0];
				if (current.Attack > myCard.Defense)
				{
					result.Add(Tuple.Create(myCard.InstanceId, killable.Last().InstanceId));
					opponentBoard.Remove(killable.Last());
					continue;
				}

				int index = -1;
				int usedAttackPoint = 0;

				for (var i = 0; i < killable.Length; i++)
				{
					current = killable[i];
					if (current.Attack > myCard.Defense)
					{
						continue;
					}

					if (usedAttackPoint < current.Defense)
					{
						usedAttackPoint = current.Defense;
						index = i;
					}
				}

				if (index < 0)
				{
					index = 0;
				}

				result.Add(Tuple.Create(myCard.InstanceId, killable[index].InstanceId));
				opponentBoard.Remove(killable[index]);
			}
			
			//TODO: attach
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