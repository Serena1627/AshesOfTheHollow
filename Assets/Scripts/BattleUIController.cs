using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIController : MonoBehaviour
{
    public static BattleUIController Instance { get; private set; }

    public enum ActionChoice
    {
        None,
        Attack,
        Item
    }

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
    }

    private void Start()
    {
        SetMenuActive(actionOptions, false);
        SetMenuActive(attackOptionsMenu, false);
        SetMenuActive(targetOptionsMenu, false);
        SetMenuActive(itemOptionsMenu, false);
    }

    // -------------------------------------------------------------------------
    // Main Action Menu
    // -------------------------------------------------------------------------

    public void AttackButtonAction()
    {
        Debug.Log("Main ATTACK button clicked.");
        selectedMenuAction = ActionChoice.Attack;
    }

    public void ItemButtonAction()
    {
        Debug.Log("Main ITEM button clicked.");
        selectedMenuAction = ActionChoice.Item;
    }

    public ActionChoice GetAction()
    {
        return selectedMenuAction;
    }

    public IEnumerator PlayerMenu()
    {
        selectedMenuAction = ActionChoice.None;

        SetMenuActive(actionOptions, true);

        yield return new WaitUntil(
            () => selectedMenuAction != ActionChoice.None
        );

        SetMenuActive(actionOptions, false);
    }

    // Compatibility wrappers if BattleController still uses your old method names.
    public void attackButtonAction() => AttackButtonAction();
    public void itemButtonAction() => ItemButtonAction();
    public ActionChoice getAction() => GetAction();
    public IEnumerator playerMenu() => PlayerMenu();

    // -------------------------------------------------------------------------
    // Attack Selection Menu
    // -------------------------------------------------------------------------

    public Action GetAttack()
    {
        return chosenAttack;
    }

    public void SelectAction(Action attack)
    {
        if (attack == null)
        {
            Debug.LogWarning("A generated attack button tried to select a null attack.");
            return;
        }

        chosenAttack = attack;
        Debug.Log("Selected attack: " + attack.getActionName());
    }

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

        SetMenuActive(attackOptionsMenu, true);

        Debug.Log("Attack selection menu opened.");

        yield return new WaitUntil(
            () => chosenAttack != null || backPressed
        );

        SetMenuActive(attackOptionsMenu, false);
        RemoveButtons(generatedAttackButtons);

        Debug.Log("Attack selection menu closed.");
    }

    private void GenerateAttacks(PlayerBattle player)
    {
        if (attackButtonTemplate == null || attackButtonContainer == null)
        {
            Debug.LogWarning("Attack button template or container is missing.");
            return;
        }

        Dictionary<string, Action> actions = player.getActionList();

        if (actions == null || actions.Count == 0)
        {
            Debug.LogWarning(player.entityName + " has no generated battle actions.");
            return;
        }

        foreach (Action attack in actions.Values)
        {
            if (attack == null)
            {
                continue;
            }

            Action capturedAttack = attack;

            Button generatedButton = Instantiate(
                attackButtonTemplate,
                attackButtonContainer
            );

            generatedButton.gameObject.SetActive(true);
            generatedButton.interactable = true;
            generatedButton.onClick.RemoveAllListeners();
            generatedButton.onClick.AddListener(
                () => SelectAction(capturedAttack)
            );

            TMP_Text buttonText =
                generatedButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text = capturedAttack.getActionName();
            }

            generatedAttackButtons.Add(generatedButton);

            Debug.Log("Generated attack button: " + capturedAttack.getActionName());
        }

        GenerateBackButton(attackButtonContainer, generatedAttackButtons);
    }

    // Compatibility wrappers.
    public Action getAttack() => GetAttack();
    public void selectAction(Action attack) => SelectAction(attack);
    public IEnumerator playerAttacks(PlayerBattle player) => PlayerAttacks(player);

    // -------------------------------------------------------------------------
    // Target Selection Menu
    // -------------------------------------------------------------------------

    public EnemyBattle ReturnTarget()
    {
        return chosenTarget;
    }

    public void SelectTarget(EnemyBattle enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("A target button tried to select a null enemy.");
            return;
        }

        chosenTarget = enemy;
        Debug.Log("Selected target: " + enemy.entityName);
    }

    public IEnumerator Targeting(List<EnemyBattle> enemies)
    {
        chosenTarget = null;
        backPressed = false;

        RemoveButtons(generatedTargetButtons);
        GenerateTargets(enemies);

        SetMenuActive(targetOptionsMenu, true);

        yield return new WaitUntil(
            () => chosenTarget != null || backPressed
        );

        SetMenuActive(targetOptionsMenu, false);
        RemoveButtons(generatedTargetButtons);
    }

    private void GenerateTargets(List<EnemyBattle> enemies)
    {
        if (targetButtonTemplate == null || targetButtonContainer == null)
        {
            Debug.LogWarning("Target button template or container is missing.");
            return;
        }

        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("There are no enemies available to target.");
            return;
        }

        foreach (EnemyBattle enemy in enemies)
        {
            if (enemy == null || enemy.isEntityDead())
            {
                continue;
            }

            EnemyBattle capturedEnemy = enemy;

            Button generatedButton = Instantiate(
                targetButtonTemplate,
                targetButtonContainer
            );

            generatedButton.gameObject.SetActive(true);
            generatedButton.interactable = true;
            generatedButton.onClick.RemoveAllListeners();
            generatedButton.onClick.AddListener(
                () => SelectTarget(capturedEnemy)
            );

            TMP_Text buttonText =
                generatedButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text = capturedEnemy.entityName;
            }

            generatedTargetButtons.Add(generatedButton);
        }

        GenerateBackButton(targetButtonContainer, generatedTargetButtons);
    }

    // Compatibility wrappers.
    public EnemyBattle returnTarget() => ReturnTarget();
    public void selectTarget(EnemyBattle enemy) => SelectTarget(enemy);
    public IEnumerator targeting(List<EnemyBattle> enemies) => Targeting(enemies);

    // -------------------------------------------------------------------------
    // Item Selection Menu
    // -------------------------------------------------------------------------

    public Item GetItem()
    {
        return chosenItem;
    }

    public void SelectItem(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("An item button tried to select a null item.");
            return;
        }

        chosenItem = item;
        Debug.Log("Selected item: " + item.getName());
    }

    public IEnumerator PlayerItems()
    {
        chosenItem = null;
        backPressed = false;

        RemoveButtons(generatedItemButtons);
        GenerateItems();

        SetMenuActive(itemOptionsMenu, true);

        yield return new WaitUntil(
            () => chosenItem != null || backPressed
        );

        SetMenuActive(itemOptionsMenu, false);
        RemoveButtons(generatedItemButtons);
    }

    private void GenerateItems()
    {
        if (itemButtonTemplate == null || itemButtonContainer == null)
        {
            Debug.LogWarning("Item button template or container is missing.");
            return;
        }

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

            Button generatedButton = Instantiate(
                itemButtonTemplate,
                itemButtonContainer
            );

            generatedButton.gameObject.SetActive(true);
            generatedButton.interactable = true;
            generatedButton.onClick.RemoveAllListeners();
            generatedButton.onClick.AddListener(
                () => SelectItem(capturedItem)
            );

            TMP_Text buttonText =
                generatedButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                int quantity = InventoryManager.Instance != null
                    ? InventoryManager.Instance.GetItemQuantity(capturedItem.getName())
                    : 0;

                buttonText.text = capturedItem.getName() + "  x" + quantity;
            }

            generatedItemButtons.Add(generatedButton);
        }

        GenerateBackButton(itemButtonContainer, generatedItemButtons);
    }

    // Compatibility wrappers.
    public Item getItem() => GetItem();
    public void selectItem(Item item) => SelectItem(item);
    public IEnumerator playerItems() => PlayerItems();

    // -------------------------------------------------------------------------
    // Back Button / Cleanup
    // -------------------------------------------------------------------------

    public void BackButtonAction()
    {
        backPressed = true;
        Debug.Log("Back button selected.");
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

        Button generatedBackButton = Instantiate(
            backButtonTemplate,
            container
        );

        generatedBackButton.gameObject.SetActive(true);
        generatedBackButton.interactable = true;
        generatedBackButton.onClick.RemoveAllListeners();
        generatedBackButton.onClick.AddListener(BackButtonAction);

        buttonList.Add(generatedBackButton);
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

    // Compatibility wrapper.
    public void instantiateBack() => BackButtonAction();
}