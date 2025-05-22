using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.tvOS;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class GameManager : MonoBehaviour
{
    // Players and deck used in the game
    public Player player1, player2;
    public Deck deck;

    // Positions where player hands (cards) will be shown in the scene
    public Transform player1HandPos, player2HandPos;

    // Reference to ad manager (assumed to handle interstitial ads)
    private Ads ads;

    // Prefab for spawning card objects in the scene
    public GameObject cardPrefab;

    // UI Panels and Buttons
    public GameObject CardTypeSelectionPanel;
    public GameObject WinPanel;
    public GameObject DrawCardButton;
    public GameObject PausePanel;
    public GameObject QuitMenuPanel;
    public GameObject turnNotificationPanel;

    // UI Text elements
    public Text turnTimerText;
    public Text WinText;
    public Text turnNotificationText;

    // Menu Buttons
    public Button ReturnMenuB;
    public Button NewGameB;
    public Button ReturnMenuB_P;
    public Button CancelB;
    public Button SettingsB;
    public Button YesB;
    public Button NoB;

    // Game state flags
    private bool gameOver = false;
    public bool isPlayer1Turn = true;
    private bool hasPlayedThisTurn = false;
    private bool isTurnTransitioning = false;
    private bool isTimerRunning = false;
    public bool paused = false;

    // Card spawning point in the scene
    public Transform cardSpawnPoint;

    // Turn settings
    public float notificationDuration = 1.5f;
    public float turnDuration = 60f;
    private float turnTimer = 0f;

    // Callback to be used when a specific card type is selected
    private Action<CardType> onCardTypeChosen;

    public void Start()
    {

        SettingsB.onClick.RemoveAllListeners();
        // Initialize players and deck
        player1 = new Player();
        player2 = new Player();
        deck = new Deck();

        // Hook up menu buttons to their functions
        ReturnMenuB_P.onClick.AddListener(AskToQuit);
        ReturnMenuB.onClick.AddListener(GoToMainMenu);
        NewGameB.onClick.AddListener(NewGame);
        CancelB.onClick.AddListener(ReturnToGame);
        SettingsB.onClick.AddListener(Pause);
        YesB.onClick.AddListener(GoToMainMenu);
        NoB.onClick.AddListener(ReturnToPause);

        // Draw one card for each player at the beginning
        DrawCard(player1, player1HandPos);
        DrawCard(player2, player2HandPos);

        // Display whose turn it is at the start
        StartCoroutine(ShowTurnNotification(isPlayer1Turn));

        // Load ads
        ads = FindAnyObjectByType<Ads>();
        ads.LoadAd();
    }

    void Update()
    {
        // Handle the turn countdown timer if game is running and not paused
        if (isTimerRunning && !gameOver && !paused)
        {
            turnTimer -= Time.deltaTime;
            turnTimer = Mathf.Max(turnTimer, 0);
            UpdateTimerUI();

            // End turn automatically if timer reaches zero
            if (turnTimer <= 0)
            {
                EndTurnDueToTimeout();
            }
        }
    }

    // Starts a new turn by resetting the timer
    void StartNewTurn()
    {
        turnTimer = turnDuration;
        isTimerRunning = false;
        turnTimerText.color = Color.black;
        UpdateTimerUI();
    }

    // If player doesn't act in time, automatically ends their turn
    void EndTurnDueToTimeout()
    {
        isTimerRunning = false;
        Debug.Log("Time is up! Switching turn automatically.");
        isPlayer1Turn = !isPlayer1Turn;
        hasPlayedThisTurn = false;
        StartCoroutine(ShowTurnNotification(isPlayer1Turn));
    }

    // Updates the timer display UI
    void UpdateTimerUI()
    {
        if (turnTimerText != null)
        {
            int seconds = Mathf.CeilToInt(turnTimer);
            if (seconds <= 10)
            {
                turnTimerText.color = Color.red;
            }
            turnTimerText.text = "Time: " + seconds.ToString() + " s";
        }
    }

    // Checks if a player has won by having all 4 card types
    public void CheckWinCondition(Player player)
    {
        if (player.HasAllFourTypes())
        {
            string winner = player == player1 ? "Player 1" : "Player 2";
            Debug.Log("Winner: " + winner);
            WinText.text = "Winner: " + winner;
            gameOver = true;
            DrawCardButton.SetActive(false);
            SettingsB.interactable = false;
            WinPanel.SetActive(true);
        }
    }

    // Draws a card from the deck and places it in the player's hand
    public bool DrawCard(Player player, Transform handPos, bool checkWin = true)
    {
        if (player.hand.Count >= 4)
        {
            Debug.Log("Cannot draw more than 4 cards.");
            return false;
        }

        Card newCard = deck.GetRandomCard();

        GameObject obj = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
        var cardObj = obj.GetComponent<CardObject>();
        bool isPlayer1 = player == player1;
        cardObj.Initialize(newCard.cardType, this, isPlayer1);
        player.hand.Add(newCard);

        Vector3 targetPos = handPos.position + Vector3.right * (player.hand.Count - 1) * 1.2f;
        StartCoroutine(AnimateCardMovement(obj, targetPos));

        if (checkWin) CheckWinCondition(player);

        return true;
    }

    // Called when a player clicks a card during their turn
    public void OnCardClicked(CardObject cardObject)
    {
        if (gameOver || hasPlayedThisTurn || paused || isTurnTransitioning) return;

        Player user = isPlayer1Turn ? player1 : player2;
        Player opponent = isPlayer1Turn ? player2 : player1;

        if (!user.hand.Any(c => c.cardType == cardObject.cardType)) return;

        Card clickedCard = user.hand.FirstOrDefault(c => c.cardType == cardObject.cardType);

        if (clickedCard != null)
        {
            // Special card that opens a UI for choosing a card type
            if (clickedCard.cardType == CardType.DrawSpecificCard)
            {
                UseCard(user, opponent, clickedCard);
                RemoveCard(user, clickedCard);
                return;
            }

            RemoveCard(user, clickedCard);

            // Apply card effect based on its type
            switch (clickedCard.cardType)
            {
                case CardType.DestroyOpponentCard:
                    if (opponent.hand.Count > 0)
                    {
                        var removed = opponent.hand[UnityEngine.Random.Range(0, opponent.hand.Count)];
                        RemoveCard(opponent, removed);
                    }
                    break;

                case CardType.DrawCardForSelf:
                    if (DrawCard(user, user == player1 ? player1HandPos : player2HandPos))
                    {
                        StartCoroutine(DelayedUpdateHandLayout(user, user == player1 ? player1HandPos : player2HandPos));
                        CheckWinCondition(user);
                    }
                    break;

                case CardType.DrawCardForOpponent:
                    if (DrawCard(opponent, opponent == player1 ? player1HandPos : player2HandPos, false))
                    {
                        StartCoroutine(DelayedUpdateHandLayout(opponent, opponent == player1 ? player1HandPos : player2HandPos));
                        CheckWinCondition(opponent);
                    }
                    break;
            }

            hasPlayedThisTurn = true;
            isPlayer1Turn = !isPlayer1Turn;
            StartCoroutine(ShowTurnNotification(isPlayer1Turn));
            hasPlayedThisTurn = false;
        }
    }

    // Removes and destroys a card from the scene
    public void DestroyCardObject(Player player, Card card)
    {
        var cardObjects = FindObjectsOfType<CardObject>();
        foreach (var obj in cardObjects)
        {
            if (player == player1 && obj.isOwnedByPlayer1 && obj.cardType == card.cardType)
            {
                Destroy(obj.gameObject);
                StartCoroutine(DelayedUpdateHandLayout(player, player1HandPos));
                break;
            }
            else if (player == player2 && !obj.isOwnedByPlayer1 && obj.cardType == card.cardType)
            {
                Destroy(obj.gameObject);
                StartCoroutine(DelayedUpdateHandLayout(player, player2HandPos));
                break;
            }
        }
    }

    // Waits one frame before updating hand layout
    public IEnumerator DelayedUpdateHandLayout(Player player, Transform handPos)
    {
        yield return null;
        UpdateHandLayout(player, handPos);
    }

    // Card movement animation from draw position to hand position
    IEnumerator AnimateCardMovement(GameObject cardObj, Vector3 targetPos, float duration = 0.5f)
    {
        Vector3 startPos = cardObj.transform.position;
        Vector3 startScale = Vector3.one * 0.3f;
        Vector3 endScale = Vector3.one;
        Quaternion startRot = Quaternion.Euler(0, 0, 30f);
        Quaternion endRot = Quaternion.identity;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep easing

            cardObj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cardObj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            cardObj.transform.rotation = Quaternion.Lerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cardObj.transform.position = targetPos;
        cardObj.transform.localScale = endScale;
        cardObj.transform.rotation = endRot;
    }

    // UI transition that shows whose turn it is
    IEnumerator ShowTurnNotification(bool isPlayer1)
    {
        if (gameOver) yield break;

        hasPlayedThisTurn = true;
        isTurnTransitioning = true;
        DrawCardButton.GetComponent<Button>().interactable = false;

        StartNewTurn();

        turnNotificationPanel.SetActive(true);
        turnNotificationText.text = isPlayer1 ? "Player 1's Turn" : "Player 2's Turn";

        CanvasGroup canvasGroup = turnNotificationPanel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            canvasGroup.alpha = t;
            yield return null;
        }

        yield return new WaitForSeconds(notificationDuration);

        t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 2f;
            canvasGroup.alpha = t;
            yield return null;
        }

        turnNotificationPanel.SetActive(false);

        hasPlayedThisTurn = false;
        isTurnTransitioning = false;
        DrawCardButton.GetComponent<Button>().interactable = true;
        isTimerRunning = true;
    }

    // Rearranges cards in the player's hand
    public void UpdateHandLayout(Player player, Transform handPos)
    {
        var allCardObjects = FindObjectsOfType<CardObject>();
        List<CardObject> ownedCards = allCardObjects
            .Where(c => c != null && c.gameObject != null && c.isOwnedByPlayer1 == (player == player1))
            .OrderBy(c => c.transform.position.x)
            .ToList();

        for (int i = 0; i < ownedCards.Count; i++)
        {
            ownedCards[i].transform.position = handPos.position + Vector3.right * i * 1.2f;
        }
    }

    // Special card behavior: player chooses a specific card type to draw
    public void UseCard(Player user, Player opponent, Card card)
    {
        if (card.cardType == CardType.DrawSpecificCard)
        {
            Player currentUser = user;
            Transform currentHandPos = (currentUser == player1) ? player1HandPos : player2HandPos;

            ShowCardTypeSelection(chosenType =>
            {
                DrawSpecificCard(currentUser, chosenType, currentHandPos);
                UpdateHandLayout(currentUser, currentHandPos);
                CheckWinCondition(currentUser);
                hasPlayedThisTurn = true;
                isPlayer1Turn = !isPlayer1Turn;
                StartCoroutine(ShowTurnNotification(isPlayer1Turn));
                hasPlayedThisTurn = false;
            });
        }
    }

    // Called when "Draw Card" button is pressed
    public void OnDrawCardButtonClicked()
    {
        if (gameOver || hasPlayedThisTurn || isTurnTransitioning) return;

        bool success;
        if (isPlayer1Turn)
        {
            success = DrawCard(player1, player1HandPos);
            if (success) StartCoroutine(DelayedUpdateHandLayout(player1, player1HandPos));
        }
        else
        {
            success = DrawCard(player2, player2HandPos);
            if (success) StartCoroutine(DelayedUpdateHandLayout(player2, player2HandPos));
        }

        if (success)
        {
            hasPlayedThisTurn = true;
            isPlayer1Turn = !isPlayer1Turn;
            StartCoroutine(ShowTurnNotification(isPlayer1Turn));
            hasPlayedThisTurn = false;
        }
    }

    // Adds a specific card type to the player's hand
    public void DrawSpecificCard(Player player, CardType type, Transform handPos)
    {
        if (player.hand.Count >= 4) return;

        Card newCard = new Card(type);
        GameObject obj = Instantiate(cardPrefab, handPos.position + Vector3.right * player.hand.Count * 1.2f, Quaternion.identity);
        var cardObj = obj.GetComponent<CardObject>();
        bool isPlayer1 = player == player1;
        cardObj.Initialize(type, this, isPlayer1);
        player.hand.Add(newCard);
    }

    // Called from UI buttons to confirm a card type selection
    public void OnCardTypeButtonClicked(string cardTypeStr)
    {
        CardTypeSelectionPanel.SetActive(false);
        if (Enum.TryParse(cardTypeStr, out CardType selectedType))
        {
            onCardTypeChosen?.Invoke(selectedType);
        }
    }

    // Shows the panel for selecting a specific card type
    public void ShowCardTypeSelection(Action<CardType> callback)
    {
        CardTypeSelectionPanel.SetActive(true);
        onCardTypeChosen = callback;
    }

    // Removes a card from player's hand and destroys it from scene
    public void RemoveCard(Player player, Card card)
    {
        if (player.hand.Contains(card))
        {
            player.hand.Remove(card);
            DestroyCardObject(player, card);
            UpdateHandLayout(player, player == player1 ? player1HandPos : player2HandPos);
        }
    }

    // Loads the current scene to start a new game
    public void NewGame()
    {
        ads.ShowInterstitialAd(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    // Navigates to the main menu
    public void GoToMainMenu()
    {
        ads.ShowInterstitialAd(() =>
        {
            Debug.Log("Returned to Main Menu");
            SceneManager.LoadScene("MainMenu");
        });
    }

    // Resumes game from pause
    public void ReturnToGame()
    {
        paused = false;
        DrawCardButton.SetActive(true);
        QuitMenuPanel.SetActive(false);
        PausePanel.SetActive(false);
    }

    // Asking player if they want to Quit
    public void AskToQuit()
    {
        QuitMenuPanel.SetActive(true);
        PausePanel.SetActive(false);
    }

    // Return to pause panel
    public void ReturnToPause()
    {
        PausePanel.SetActive(true);
        QuitMenuPanel.SetActive(false);
    }

    // Pauses the game and shows pause panel
    public void Pause()
    {
        if (!paused)
        {
            paused = true;
            DrawCardButton.SetActive(false);
            PausePanel.SetActive(true);
        }
        else 
        { 
            ReturnToGame(); 
        }
    }
}