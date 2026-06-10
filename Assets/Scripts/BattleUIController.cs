using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BattleUIController : MonoBehaviour
{
    public static BattleUIController Instance { get; private set; }

    public enum ActionChoice
    {
        None,
        Attack,
        Item
    }

    [Header("Battle Box")]
    [SerializeField] private GameObject battleBox;

    [Header("Main Action Menu")]
    [SerializeField] private GameObject actionOptions;

    [Header("Attack Menu")]
    [SerializeField] private GameObject attackOptionsMenu;
    [SerializeField] private Transform attackButtonContainer;
    [SerializeField] private Button attackButtonTemplate;

    [Header("Target Menu")]
    [SerializeField] private GameObject targetOptionsMenu;
    [SerializeField] private Transform targetButtonContainer;
    [SerializeField] private Button targetButtonTemplate;

    [Header("Item Menu")]
    [SerializeField] private GameObject itemOptionsMenu;
    [SerializeField] private Transform itemButtonContainer;
    [SerializeField] private Button itemButtonTemplate;

    [Header("Message Menu")]
    [SerializeField] private GameObject messageMenu;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text continueText;

    [Header("Shared Back Button Template")]
    [SerializeField] private Button backButtonTemplate;

    private readonly List<Button> generatedAttackButtons = new List<Button>();
    private readonly List<Button> generatedTargetButtons = new List<Button>();
    private readonly List<Button> generatedItemButtons = new List<Button>();

    private ActionChoice selectedMenuAction = ActionChoice.None;
    private Action chosenAttack;
    private Item chosenItem;
    private EnemyBattle chosenTarget;
    private bool backPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        HideAllPages();

        if (battleBox != null)
        {
            battleBox.SetActive(false);
        }

        Debug.Log("BattleUIController ready and menus initialized.");
    }

    // -------------------------------------------------------------------------
    // Main Action Menu
    // -------------------------------------------------------------------------

    public IEnumerator PlayerMenu()
    {
        if (battleBox == null)
        {
            Debug.LogError("BattleUIController: Battle Box is missing.");
            yield break;
        }

        if (actionOptions == null)
        {
            Debug.LogError("BattleUIController: Action Options should be MainActionMenu, not BattleBox.");
            yield break;
        }

        selectedMenuAction = ActionChoice.None;

        ShowMainActionMenu();

        yield return new WaitUntil(
            () => selectedMenuAction != ActionChoice.None
        );

        HideAllPagesButKeepBattleBox();
    }

    public void AttackButtonAction()
    {
        selectedMenuAction = ActionChoice.Attack;
        Debug.Log("ATTACK selected.");
    }

    public void ItemButtonAction()
    {
        selectedMenuAction = ActionChoice.Item;
        Debug.Log("ITEM selected.");
    }

    public ActionChoice GetAction()
    {
        return selectedMenuAction;
    }

    // -------------------------------------------------------------------------
    // Attack Menu
    // -------------------------------------------------------------------------

    public IEnumerator PlayerAttacks(PlayerBattle player)
    {
        if (player == null)
        {
            Debug.LogWarning("Cannot open attack menu because PlayerBattle is null.");
            yield break;
        }

        chosenAttack = null;
        backPressed = false;

        RemoveButtons(generatedAttackButtons);
        GenerateAttacks(player);

        if (generatedAttackButtons.Count == 0)
        {
            Debug.LogWarning(player.entityName + " has no available attack buttons.");
            yield break;
        }

        ShowAttackMenu();

        yield return new WaitUntil(
            () => chosenAttack != null || backPressed
        );

        HideAllPagesButKeepBattleBox();
        RemoveButtons(generatedAttackButtons);
    }

    public Action GetAttack()
    {
        return chosenAttack;
    }

    private void GenerateAttacks(PlayerBattle player)
    {
        if (attackOptionsMenu == null ||
            attackButtonContainer == null ||
            attackButtonTemplate == null)
        {
            Debug.LogError("Attack menu references are missing on BattleUIController.");
            return;
        }

        foreach (Action attack in player.getActionList().Values)
        {
            if (attack == null)
            {
                continue;
            }

            Action capturedAttack = attack;

            Button button = Instantiate(
                attackButtonTemplate,
                attackButtonContainer
            );

            button.gameObject.SetActive(true);
            button.interactable = true;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                chosenAttack = capturedAttack;
                Debug.Log("Attack chosen: " + capturedAttack.getActionName());
            });

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                label.text = capturedAttack.getActionName();
            }

            generatedAttackButtons.Add(button);
        }

        GenerateBackButton(attackButtonContainer, generatedAttackButtons);
    }

    // -------------------------------------------------------------------------
    // Target Menu
    // -------------------------------------------------------------------------

    public IEnumerator Targeting(List<EnemyBattle> enemies)
    {
        chosenTarget = null;
        backPressed = false;

        RemoveButtons(generatedTargetButtons);
        GenerateTargets(enemies);

        if (generatedTargetButtons.Count == 0)
        {
            Debug.LogWarning("No available target buttons were generated.");
            yield break;
        }

        ShowTargetMenu();

        yield return new WaitUntil(
            () => chosenTarget != null || backPressed
        );

        HideAllPagesButKeepBattleBox();
        RemoveButtons(generatedTargetButtons);
    }

    public EnemyBattle ReturnTarget()
    {
        return chosenTarget;
    }

    private void GenerateTargets(List<EnemyBattle> enemies)
    {
        if (targetOptionsMenu == null ||
            targetButtonContainer == null ||
            targetButtonTemplate == null)
        {
            Debug.LogError("Target menu references are missing on BattleUIController.");
            return;
        }

        if (enemies == null)
        {
            return;
        }

        foreach (EnemyBattle enemy in enemies)
        {
            if (enemy == null || enemy.isEntityDead())
            {
                continue;
            }

            EnemyBattle capturedEnemy = enemy;

            Button button = Instantiate(
                targetButtonTemplate,
                targetButtonContainer
            );

            button.gameObject.SetActive(true);
            button.interactable = true;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                chosenTarget = capturedEnemy;
                Debug.Log("Target chosen: " + capturedEnemy.entityName);
            });

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                label.text = capturedEnemy.entityName;
            }

            generatedTargetButtons.Add(button);
        }

        GenerateBackButton(targetButtonContainer, generatedTargetButtons);
    }

    // -------------------------------------------------------------------------
    // Item Menu
    // -------------------------------------------------------------------------

    public IEnumerator PlayerItems()
    {
        chosenItem = null;
        backPressed = false;

        if (itemOptionsMenu == null ||
            itemButtonContainer == null ||
            itemButtonTemplate == null)
        {
            Debug.LogWarning("Item menu is not configured. Returning to the action menu.");
            yield break;
        }

        RemoveButtons(generatedItemButtons);
        GenerateItems();

        if (generatedItemButtons.Count == 0)
        {
            Debug.Log("There are no battle items available.");
            yield break;
        }

        ShowItemMenu();

        yield return new WaitUntil(
            () => chosenItem != null || backPressed
        );

        HideAllPagesButKeepBattleBox();
        RemoveButtons(generatedItemButtons);
    }

    public Item GetItem()
    {
        return chosenItem;
    }

    private void GenerateItems()
    {
        if (BattleController.Instance == null)
        {
            Debug.LogWarning("BattleController is missing. Items cannot be generated.");
            return;
        }

        foreach (Item item in BattleController.Instance.getItems())
        {
            if (item == null)
            {
                continue;
            }

            Item capturedItem = item;

            Button button = Instantiate(
                itemButtonTemplate,
                itemButtonContainer
            );

            button.gameObject.SetActive(true);
            button.interactable = true;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                chosenItem = capturedItem;
                Debug.Log("Item chosen: " + capturedItem.getName());
            });

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                int quantity = InventoryManager.Instance != null
                    ? InventoryManager.Instance.GetItemQuantity(capturedItem.getName())
                    : 1;

                label.text = capturedItem.getName() + "  x" + quantity;
            }

            generatedItemButtons.Add(button);
        }

        GenerateBackButton(itemButtonContainer, generatedItemButtons);
    }

    // -------------------------------------------------------------------------
    // Battle Messages
    // -------------------------------------------------------------------------

    public IEnumerator ShowBattleMessage(string message)
    {
        if (battleBox == null)
        {
            Debug.LogError("BattleUIController: Battle Box is missing.");
            yield break;
        }

        battleBox.SetActive(true);
        HideAllPagesButKeepBattleBox();

        if (messageMenu != null)
        {
            messageMenu.SetActive(true);
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (continueText != null)
        {
            continueText.gameObject.SetActive(true);
        }

        yield return new WaitUntil(() =>
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame
        );

        if (messageMenu != null)
        {
            messageMenu.SetActive(false);
        }
    }

    // -------------------------------------------------------------------------
    // Page Switching
    // -------------------------------------------------------------------------

    private void ShowMainActionMenu()
    {
        if (battleBox != null)
        {
            battleBox.SetActive(true);
        }

        SetMenuActive(actionOptions, true);
        SetMenuActive(attackOptionsMenu, false);
        SetMenuActive(targetOptionsMenu, false);
        SetMenuActive(itemOptionsMenu, false);
        SetMenuActive(messageMenu, false);
    }

    private void ShowAttackMenu()
    {
        if (battleBox != null)
        {
            battleBox.SetActive(true);
        }

        SetMenuActive(actionOptions, false);
        SetMenuActive(attackOptionsMenu, true);
        SetMenuActive(targetOptionsMenu, false);
        SetMenuActive(itemOptionsMenu, false);
        SetMenuActive(messageMenu, false);
    }

    private void ShowTargetMenu()
    {
        if (battleBox != null)
        {
            battleBox.SetActive(true);
        }

        SetMenuActive(actionOptions, false);
        SetMenuActive(attackOptionsMenu, false);
        SetMenuActive(targetOptionsMenu, true);
        SetMenuActive(itemOptionsMenu, false);
        SetMenuActive(messageMenu, false);
    }

    private void ShowItemMenu()
    {
        if (battleBox != null)
        {
            battleBox.SetActive(true);
        }

        SetMenuActive(actionOptions, false);
        SetMenuActive(attackOptionsMenu, false);
        SetMenuActive(targetOptionsMenu, false);
        SetMenuActive(itemOptionsMenu, true);
        SetMenuActive(messageMenu, false);
    }

    public void HideBattleUI()
    {
        if (battleBox != null)
        {
            battleBox.SetActive(false);
        }

        HideAllPages();
    }

    private void HideAllPagesButKeepBattleBox()
    {
        SetMenuActive(actionOptions, false);
        SetMenuActive(attackOptionsMenu, false);
        SetMenuActive(targetOptionsMenu, false);
        SetMenuActive(itemOptionsMenu, false);
        SetMenuActive(messageMenu, false);
    }

    private void HideAllPages()
    {
        HideAllPagesButKeepBattleBox();
    }

    // -------------------------------------------------------------------------
    // Back Button / Cleanup
    // -------------------------------------------------------------------------

    public void BackButtonAction()
    {
        backPressed = true;
        Debug.Log("Back selected.");
    }

    private void GenerateBackButton(
        Transform container,
        List<Button> buttonList
    )
    {
        if (backButtonTemplate == null || container == null)
        {
            return;
        }

        Button button = Instantiate(
            backButtonTemplate,
            container
        );

        button.gameObject.SetActive(true);
        button.interactable = true;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(BackButtonAction);

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();

        if (label != null)
        {
            label.text = "Back";
        }

        buttonList.Add(button);
    }

    private void RemoveButtons(List<Button> buttons)
    {
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }

        buttons.Clear();
    }

    private void SetMenuActive(GameObject menu, bool active)
    {
        if (menu != null)
        {
            menu.SetActive(active);
        }
    }

    // Compatibility with existing Inspector button events and older code.
    public void attackButtonAction() => AttackButtonAction();
    public void itemButtonAction() => ItemButtonAction();
    public IEnumerator playerMenu() => PlayerMenu();
    public IEnumerator playerAttacks(PlayerBattle player) => PlayerAttacks(player);
    public IEnumerator targeting(List<EnemyBattle> enemies) => Targeting(enemies);
    public IEnumerator playerItems() => PlayerItems();
    public ActionChoice getAction() => GetAction();
    public Action getAttack() => GetAttack();
    public EnemyBattle returnTarget() => ReturnTarget();
    public Item getItem() => GetItem();
}