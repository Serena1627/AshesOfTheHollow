using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject pauseCanvas;

    [Header("Party UI")]
    [SerializeField] private GameObject partyMenuCanvas;
    [SerializeField] private PartyMenuPageManager partyMenuPageManager;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryMenuCanvas;
    [SerializeField] private InventoryMenuUI inventoryMenuUI;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (partyMenuCanvas != null)
            partyMenuCanvas.SetActive(false);

        if (inventoryMenuCanvas != null)
            inventoryMenuCanvas.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (partyMenuCanvas != null && partyMenuCanvas.activeSelf)
            {
                ClosePartyMenu();
            }
            else if (inventoryMenuCanvas != null && inventoryMenuCanvas.activeSelf)
            {
                CloseInventoryMenu();
            }
            else
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        if (partyMenuCanvas != null)
            partyMenuCanvas.SetActive(false);

        if (inventoryMenuCanvas != null)
            inventoryMenuCanvas.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (partyMenuCanvas != null)
            partyMenuCanvas.SetActive(false);

        if (inventoryMenuCanvas != null)
            inventoryMenuCanvas.SetActive(false);

        Time.timeScale = 1f;
    }

    public void ShowPartyMenu()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("PauseMenu: No PartyManager instance found.");
            return;
        }

        if (partyMenuCanvas == null)
        {
            Debug.LogWarning("PauseMenu: Party Menu Canvas is not assigned.");
            return;
        }

        if (partyMenuPageManager == null)
        {
            Debug.LogWarning("PauseMenu: PartyMenuPageManager is not assigned.");
            return;
        }

        List<GameObject> currentPartyPrefabs =
            PartyManager.Instance.GetCurrentPartyPrefabs();

        partyMenuPageManager.LoadPartyFromPrefabs(currentPartyPrefabs);

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (inventoryMenuCanvas != null)
            inventoryMenuCanvas.SetActive(false);

        partyMenuCanvas.SetActive(true);

        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ClosePartyMenu()
    {
        if (partyMenuCanvas != null)
            partyMenuCanvas.SetActive(false);

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ShowInventoryMenu()
    {
        if (inventoryMenuCanvas == null)
        {
            Debug.LogWarning("PauseMenu: Inventory Menu Canvas is not assigned.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("PauseMenu: InventoryManager.Instance is missing.");
            return;
        }

        if (inventoryMenuUI != null)
            inventoryMenuUI.RefreshInventoryUI();

        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (partyMenuCanvas != null)
            partyMenuCanvas.SetActive(false);

        inventoryMenuCanvas.SetActive(true);

        isPaused = true;
        Time.timeScale = 0f;
    }

    public void CloseInventoryMenu()
    {
        if (inventoryMenuCanvas != null)
            inventoryMenuCanvas.SetActive(false);

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        isPaused = true;
        Time.timeScale = 0f;
    }
}