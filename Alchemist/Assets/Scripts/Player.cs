using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player
{   
    // List of cards currently in the player's hand
    public List<Card> hand = new List<Card>();

    // Checks if the player holds all four different card types
    public bool HasAllFourTypes()
    {
        // Uses LINQ to count how many distinct card types the player has
        return hand.Select(card => card.cardType).Distinct().Count() == 4;
    }

    // Adds a card to the player's hand if they have less than 4 cards
    public void DrawCard(Card card)
    {
        if (hand.Count < 4)
            hand.Add(card);
    }

    // Removes a card from the player's hand
    public void RemoveCard(Card card)
    {
        hand.Remove(card);
    }
}
