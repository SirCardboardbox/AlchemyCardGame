using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum defining the different types of cards available in the game
public enum CardType
{
    DestroyOpponentCard,    // Destroys one of the opponent's cards
    DrawCardForSelf,        // Draws a new card for the player who uses it
    DrawCardForOpponent,    // Forces the opponent to draw a card
    DrawSpecificCard        // Allows the player to draw a specific type of card
}


// Class representing a card data structure, used in the deck or in-hand
public class Card
{
    // The type of this card
    public CardType cardType;

    // Constructor to create a card of a specific type
    public Card(CardType type)
    {
        cardType = type;
    }
}
