using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
    // Reference to the main camera, used to cast rays from the screen
    public Camera cam;

    // Reference to the GameManager to notify when a card is clicked
    public GameManager gameManager;

    void Update()
    {
        // Check if the left mouse button was pressed this frame
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera to the mouse cursor position
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // Perform a physics raycast to detect what the ray hits
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Try to get a CardObject component from the hit collider
                CardObject card = hit.collider.GetComponent<CardObject>();

                // If a CardObject was hit, notify the GameManager
                if (card != null)
                {
                    gameManager.OnCardClicked(card);
                }
            }
        }
    }
}
