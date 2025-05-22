using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardObject : MonoBehaviour
{
    // Type of the card (e.g., DestroyOpponentCard, DrawCardForSelf, etc.)
    public CardType cardType;

    // Reference to the GameManager to notify actions or get game state
    private GameManager gameManager;

    // True if this card belongs to Player 1, false if it belongs to Player 2
    public bool isOwnedByPlayer1;

    // Initialize the card with its type, reference to the GameManager, and owner info
    public void Initialize(CardType type, GameManager gm, bool isPlayer1)
    {
        cardType = type;
        gameManager = gm;
        this.isOwnedByPlayer1 = isPlayer1;
        SetColor();// Set visual appearance based on card type
    }

    // Assigns a unique color to each card type for easy visual distinction
    void SetColor()
    {
        var renderer = GetComponent<Renderer>();
        switch (cardType)
        {
            case CardType.DestroyOpponentCard:
                renderer.material.color = new Color(0.78f, 0.17f, 0.28f);
                break;
            case CardType.DrawCardForSelf:
                renderer.material.color = new Color(0.31f, 0.78f, 0.47f);
                break;
            case CardType.DrawCardForOpponent:
                renderer.material.color = new Color(0.06f, 0.32f, 0.73f);
                break;
            case CardType.DrawSpecificCard:
                renderer.material.color = new Color(0.6f, 0.4f, 0.8f);
                break;
        }
    }

    // Called when the card is clicked by the player
    void OnMouseDown()
    {
        // Only allow the current player to interact with their own cards
        if (isOwnedByPlayer1 != gameManager.isPlayer1Turn) return;

        // Notify the GameManager that this card was clicked
        gameManager.OnCardClicked(this);
    }
}