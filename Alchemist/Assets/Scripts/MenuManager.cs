using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// MenuManager class handles the main menu logic, including tutorial display and game start/quit logic.
public class MenuManager : MonoBehaviour
{
    // UI panel that shows the tutorial
    public GameObject tutorialPanel;

    // Buttons on the main menu
    public Button GameStartB;    // Button to start the game
    public Button QuitB;         // Button to quit the game
    public Button closeButton;   // Button to close the tutorial panel

    // Reference to the Ads manager (handles ad loading/showing)
    private Ads ads;

    void Start()
    {
        // Check if the tutorial has already been shown before using PlayerPrefs
        if (!PlayerPrefs.HasKey("TutorialShown"))
        {
            // If not shown, show the tutorial panel
            ShowTutorial();
        }
        else
        {
            // If already shown, hide the tutorial panel
            tutorialPanel.SetActive(false);
        }

        // Attach functions to button click events
        GameStartB.onClick.AddListener(StartGame);
        QuitB.onClick.AddListener(QuitGame);

        // Load ads for later use
        ads = FindAnyObjectByType<Ads>();
        ads.LoadAd();

        // (Optional) Reset tutorial flag every time the menu opens (for testing)
        PlayerPrefs.DeleteKey("TutorialShown");
    }

    // Called when the Start button is pressed
    public void StartGame()
    {
        // Show a rewarded ad before starting the game
        ads.ShowRewardedAd(
            onRewardEarned: () => {
                // Optional: Logic when the player earns the reward
                Debug.Log("Ödül kazanýldý!");
            },
            onAdClosed: () => {
                // After the ad is closed, load the main game scene
                SceneManager.LoadScene("AlchemyGame");
            }
        );
    }

    // Called when the Quit button is pressed
    public void QuitGame()
    {
        // Exit the application (does not work in the editor)
        Application.Quit();
    }

    // Displays the tutorial panel and connects the close button
    void ShowTutorial()
    {
        tutorialPanel.SetActive(true);

        // Add listener to the close button of the tutorial
        closeButton.onClick.AddListener(CloseTutorial);
    }

    // Hides the tutorial and saves the flag so it's not shown next time
    void CloseTutorial()
    {
        tutorialPanel.SetActive(false);

        // Set the PlayerPrefs flag to remember tutorial has been shown
        PlayerPrefs.SetInt("TutorialShown", 1);
        PlayerPrefs.Save();
    }
}