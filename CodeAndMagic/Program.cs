using System;
using System.Linq;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Program
{
	enum CardType
	{
		Creature = 0,
		GreenItem = 1,
		RedItem = 2,
		BlueItem = 3
	}

	enum CardLocation
	{
		MyHand,
		MyBoard,
		OpponentBoard,
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
		public CardType CardType;
		public int Cost;
		public int Attack;
		public int Defense;
		public Bonus Abilities;
		public int MyHealthChange;
		public int OpponentHealthChange;
		public int CardDraw;

		public bool IsItem => CardType != CardType.Creature;
		public bool Guard => Abilities.HasFlag(Bonus.Guard);
		public bool Charge => Abilities.HasFlag(Bonus.Charge);
		public bool Breakthrough => Abilities.HasFlag(Bonus.Breakthrough);
		public bool Lethal => Abilities.HasFlag(Bonus.Lethal);
		public bool Drain => Abilities.HasFlag(Bonus.Drain);
		public bool Ward => Abilities.HasFlag(Bonus.Ward);

		public void Read()
		{
			string[] inputs = Console.ReadLine().Split(' ');
			CardNumber = int.Parse(inputs[0]);
			InstanceId = int.Parse(inputs[1]);

			int location = int.Parse(inputs[2]);
			Location = location == 0 ? CardLocation.MyHand :
				location == 1 ? CardLocation.MyBoard : CardLocation.OpponentBoard;

			CardType = (CardType) int.Parse(inputs[3]);
			Cost = int.Parse(inputs[4]);
			Attack = int.Parse(inputs[5]);
			Defense = int.Parse(inputs[6]);
			Abilities = Bonus.None;
			if (inputs[7][0] == 'B') Abilities |= Bonus.Breakthrough;
			if (inputs[7][1] == 'C') Abilities |= Bonus.Charge;
			if (inputs[7][2] == 'D') Abilities |= Bonus.Drain;
			if (inputs[7][3] == 'G') Abilities |= Bonus.Guard;
			if (inputs[7][4] == 'L') Abilities |= Bonus.Lethal;
			if (inputs[7][5] == 'W') Abilities |= Bonus.Ward;

			MyHealthChange = int.Parse(inputs[8]);
			OpponentHealthChange = int.Parse(inputs[9]);
			CardDraw = int.Parse(inputs[10]);
		}

		public double CalculerValeur()
		{
			switch (CardType)
			{
				case CardType.Creature: return InternalValueOfCreature();
				case CardType.BlueItem: return -90;
				case CardType.GreenItem: return -90;
				case CardType.RedItem: return -90;
				default: return -90;
			}
		}

		private double InternalValueOfCreature()
		{
			double k = 0.25;
			double x = 1;
			double y = 1.25;

			//TODO COMMENT TRAITER WARD ?
			//TODO COMMENT TRAITER DRAW HEALTHCHANGE ?

			//ajouter les bonus
			if (Abilities.HasFlag(Bonus.Breakthrough))
			{
				x += k;
			}

			if (Abilities.HasFlag(Bonus.Charge))
			{
				x += k;
			}

			if (Abilities.HasFlag(Bonus.Drain))
			{
				x += k;
			}

			if (Abilities.HasFlag(Bonus.Guard))
			{
				y += k;
			}

			if (Abilities.HasFlag(Bonus.Lethal) && Attack > 0)
			{
				y += k;
			}

			double numerateur = (x * Attack) + (y * Defense);
			double denominateur = Cost;

			if (denominateur == 0)
			{
				denominateur = 0.8;
			}

			return numerateur / denominateur;
		}
	}

	private static readonly int[] DraftCardsPerManaCount = new int[13];
	private const double MinValueToTake = 3.2;
	public const int MaxBoard = 6;
	public const int MaxHand = 8;

	private static readonly int[] DraftMaximalCards =
	{
		// 0 - 5
		1, 2, 2, 3, 4, 4,
		// 6 - 10
		3, 2, 2, 1, 0,
		// 11 - 12
		0, 1,
	};

	static class ActionHelper
	{
		public static string PickCard(int card) => $"PICK {card}";

		public static string PickFirstCard() => "PASS";

		public static string Summon(Card card) => $"SUMMON {card.InstanceId}";

		public static string Attack(Card attacker, Card defender = null) =>
			$"ATTACK {attacker.InstanceId} {defender?.InstanceId ?? -1};";

		public static string UseItem(Card itemUsed, Card target = null) =>
			$"USE {itemUsed.InstanceId} {target?.InstanceId ?? -1}";

		public static string SkipTurn() => "PASS";
	}

	static void Main(string[] args)
	{
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
		}
	}

	private static string Draft(List<Card> cards)
	{
		double[] cardValues = new double[3];
		int bestCardIndex = -1;
		double bestValue = -100;

		for (var i = 0; i < cards.Count; i++)
		{
			Card card = cards[i];

			int currentManaCount = DraftCardsPerManaCount[card.Cost];
			int expectedManaCount = DraftMaximalCards[card.Cost];

			double value = card.CalculerValeur();

			cardValues[i] = value;

			if (currentManaCount >= expectedManaCount && value < MinValueToTake)
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

		return ActionHelper.PickCard(bestCardIndex);
	}

	private static string Play(Player me, Player opponent, int opponentHand, List<Card> cards)
	{
		var myBoard = cards.Where(x => x.Location == CardLocation.MyBoard).ToList();

		var myHand = cards.Where(x => x.Location == CardLocation.MyHand && x.Cost <= me.Mana).ToList();

		var opponentBoard = cards.Where(x => x.Location == CardLocation.OpponentBoard).ToList();

		var lethal = IsLethal(opponent.Health, me.Mana, myBoard, opponentBoard, myHand);

		if (lethal.Item1)
		{
			return lethal.Item2;
		}

		return ActionHelper.SkipTurn();


		///TODO REWORK
		//		if (opponentBoard.All(x => !x.Abilities.HasFlag(Bonus.Guard)) && IsLethal(opponent.Health, myBoard))
		//		{
		//			return string.Join(";", myBoard.Select(x => $"ATTACK {x.InstanceId} -1 Lethal mode"));
		//		}
		//
		//		string summonAction = null;
		//
		//		if (myHand.Length > 0)
		//		{
		//			List<Card> summon = SummonAction(me, myHand);
		//
		//			if (summon.Count > 0)
		//			{
		//				summonAction = string.Join($";", summon.Select(x => $"SUMMON {x.InstanceId}"));
		//
		//				myBoard = myBoard.Concat(summon.Where(x => x.Abilities.HasFlag(Bonus.Charge)))
		//					.OrderBy(x => x.Attack).ThenBy(x => x.Defense).ToArray();
		//			}
		//		}
		//
		//		string attackAction = null;
		//		if (myBoard.Length > 0)
		//		{
		//			//attack
		//
		//			// Trier nos cartes par attaque puis vie (done plus haut)
		//
		//			//source / target
		//			List<Tuple<int, int>> result = new List<Tuple<int, int>>();
		//
		//			List<Card> opponentGuards = opponentBoard.Where(x => x.Abilities.HasFlag(Bonus.Guard)).ToList();
		//			foreach (Card myCard in myBoard)
		//			{
		//				if (opponentGuards.Count > 0)
		//				{
		//					Card[] killableGuards = opponentGuards.Where(x => x.Defense <= myCard.Attack).OrderBy(x => x.Attack)
		//						.ToArray();
		//					if (killableGuards.Length == 0)
		//					{
		//						Card guard = opponentGuards.OrderBy(x => x.Defense).First();
		//						guard.Defense -= myCard.Attack;
		//						result.Add(Tuple.Create(myCard.InstanceId, guard.InstanceId));
		//						continue;
		//					}
		//
		//					result.Add(Kill(myCard, killableGuards, opponentBoard, opponentGuards, true));
		//					continue;
		//				}
		//
		//				Card[] killable = opponentBoard.Where(x => x.Defense <= myCard.Attack).OrderBy(x => x.Attack).ToArray();
		//				if (killable.Length == 0)
		//				{
		//					result.Add(Tuple.Create(myCard.InstanceId, -1));
		//					continue;
		//				}
		//
		//				result.Add(Kill(myCard, killable, opponentBoard, null, false));
		//				continue;
		//			}
		//
		//			if (result.Count > 0)
		//			{
		//				attackAction = string.Join(";", result.Select(x => $"ATTACK {x.Item1} {x.Item2}"));
		//			}
		//		}
		//
		//		if (summonAction != null || attackAction != null)
		//		{
		//			if (summonAction == null)
		//			{
		//				return attackAction;
		//			}
		//
		//			if (attackAction == null)
		//			{
		//				return summonAction;
		//			}
		//
		//			return $"{summonAction};{attackAction}";
		//		}
		//
		//		return "PASS";
	}

	private static Tuple<bool, string> IsLethal(int opponentHealthPoint, int mana, List<Card> myBoard, List<Card> ennemyBoard, List<Card> myHand)
	{
		var ennmyGuard = ennemyBoard.Where(x => x.Guard).ToList();
		if (ennmyGuard.Count > 0)
		{
			//kill + breakthrough + item
		}
		else
		{
			//Pas de Guard
			int myAttack = myBoard.Sum(x => x.Attack);
			if (myAttack >= opponentHealthPoint)
			{
				//Créature suffisant
				return Tuple.Create(true, string.Join(";", myBoard.Select(x => ActionHelper.Attack(x))));
			}
			else
			{
				int health = opponentHealthPoint - myAttack;

				var chargeableCreatures = myHand.Where(x => !x.IsItem && x.Charge).ToList();
				var dealOnSummonCard = myHand.Where(x => x.OpponentHealthChange > 0).ToList();
				var dealItem = myHand.Where(x => x.CardType == CardType.BlueItem && x.Attack > 0).ToList();
				var boostItem = myHand.Where(x => x.CardType == CardType.GreenItem && x.Attack > 0).ToList();

				if (chargeableCreatures.Count > 0 || dealOnSummonCard.Count > 0 || dealItem.Count > 0 || boostItem.Count > 0)
				{
					//si je peux ajouter de la puissance
					if (myBoard.Count < MaxBoard)
					{
						//si mon board n'est pas rempli
						var playableCards = chargeableCreatures.Concat(dealOnSummonCard).Concat(dealItem).Concat(boostItem).ToHashSet();
						var allCombo = DoCombination(playableCards.ToArray(), mana);
						foreach (var combo in allCombo)
						{
							if (combo.Count(x => x.CardType == CardType.Creature) < (MaxBoard - myBoard.Count))
							{
								//si j'ai assez de place pour jouer les créatures
								string greenItem = string.Empty;
								if (combo.Any(x => x.CardType == CardType.GreenItem))
								{
									Card monster;
									if (myBoard.Count > 0)
									{
										monster = myBoard[0];
									}
									else
									{
										monster = combo.FirstOrDefault(x => x.CardType == CardType.Creature);
										if (monster is null)
										{
											continue;
										}
									}

									greenItem = string.Join(";", combo.Where(x => x.CardType == CardType.GreenItem).Select(x => ActionHelper.UseItem(x, monster)));
								}

								var summon = string.Join(";", combo.Where(x => x.CardType == CardType.Creature).Select(ActionHelper.Summon));
								var monsterAttack = string.Join(";", myBoard.Select(x => ActionHelper.Attack(x)));
								var blueAttack = string.Join(";", combo.Where(x => x.CardType == CardType.BlueItem).Select(x => ActionHelper.UseItem(x)));

								return Tuple.Create(true, string.Join(";", summon, blueAttack, greenItem, monsterAttack));
							}
						}

						return Tuple.Create(false, string.Empty);
					}
					else
					{
						var playableCards = dealOnSummonCard.Concat(dealItem).Concat(boostItem).ToHashSet();
						var allCombo = DoCombination(playableCards.ToArray(), mana);
						foreach (var combo in allCombo)
						{
							if (combo.Sum(x => x.Attack + x.OpponentHealthChange) >= health)
							{
								var monsterAttack = string.Join(";", myBoard.Select(x => ActionHelper.Attack(x)));
								var blueAttack = string.Join(";", combo.Where(x => x.CardType == CardType.BlueItem).Select(x => ActionHelper.UseItem(x)));
								var firstMonster = myBoard[0];
								var greenItem = string.Join(";", combo.Where(x => x.CardType == CardType.GreenItem).Select(x => ActionHelper.UseItem(x, firstMonster)));

								return Tuple.Create(true, string.Join(";", blueAttack, greenItem, monsterAttack));
							}
						}

						return Tuple.Create(false, string.Empty);
					}
				}
				else
				{
					return Tuple.Create(false, string.Empty);
				}
			}
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

	private static List<Card> SummonAction(Player me, Card[] hand)
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

		List<Tuple<List<Card>, int>> filteredItems = playableCombinations
			.TakeWhile(x => x.Item1 == firstMana && x.Item2.Count == firstCount)
			.Select(x => Tuple.Create(x.Item2, Ecart(x.Item2)))
			.OrderBy(x => x.Item2)
			.ToList();

		int diff = filteredItems[0].Item2;

		List<Card> items = filteredItems.TakeWhile(x => x.Item2 == diff)
			.Select(x => Tuple.Create(x.Item1, x.Item1.Sum(y => y.Defense)))
			.OrderBy(x => x.Item2)
			.Select(x => x.Item1)
			.First();

		return items;
	}

	private static int Ecart(List<Card> cards)
	{
		double average = cards.Average(x => x.Cost);
		return (int) Math.Floor(cards.Sum(x => x.Cost - average));
	}

	private static Tuple<int, int> Kill(Card myCard, Card[] killable, List<Card> opponentBoard, List<Card> opponentGuard, bool allowDying)
	{
		Card current = killable[0];
		if (current.Attack > myCard.Defense)
		{
			Card lastCard = killable.Last();
			opponentBoard.Remove(lastCard);
			opponentGuard?.Remove(lastCard);
			return Tuple.Create(myCard.InstanceId, lastCard.InstanceId);
		}

		int index = -1;
		int usedAttackPoint = 0;

		for (var i = 0; i < killable.Length; i++)
		{
			current = killable[i];
			if (current.Attack > myCard.Defense)
			{
				if (!allowDying)
				{
					continue;
				}
			}

			int spendPoints = Math.Min(current.Defense, myCard.Attack);
			if (usedAttackPoint < spendPoints)
			{
				usedAttackPoint = spendPoints;
				index = i;
			}
		}

		if (index < 0)
		{
			index = 0;
		}

		Card selectedCard = killable[index];
		opponentBoard.Remove(selectedCard);
		opponentGuard?.Remove(selectedCard);
		return Tuple.Create(myCard.InstanceId, selectedCard.InstanceId);
	}
}