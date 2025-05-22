using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    // Random number generator used to pick a random card type
    private System.Random rand = new System.Random();

    // Returns a new Card with a randomly selected CardType
    public Card GetRandomCard()
    {
        // Generates a random integer between 0 (inclusive) and 4 (exclusive),
        // and casts it to a CardType enum
        int value = rand.Next(0, 4);
        return new Card((CardType)value);
    }
}

