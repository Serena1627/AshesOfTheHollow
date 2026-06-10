using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [Header("Current Battle Party")]
    [Tooltip("Battle prefabs for party members currently available in combat.")]
    [SerializeField] private List<GameObject> currentPartyPrefabs =
        new List<GameObject>();

    [Header("Recruitable Member Prefabs")]
    [Tooltip("Assign the Paladin battle prefab here, not the village NPC sprite/prefab.")]
    [SerializeField] private GameObject paladinBattlePrefab;

    [Header("Recruitment State")]
    [SerializeField] private bool paladinRecruited;

    public bool PaladinRecruited => paladinRecruited;

    [Header("Mira Recruitment")]
    [SerializeField] private bool miraRecruited = false;
    [SerializeField] private GameObject miraBattlePrefab;

    public bool MiraRecruited => miraRecruited;

    [System.Serializable]
    public class PartyMemberHealthState
    {
        public string memberId;
        public int currentHealth;
        public int maxHealth;
    }

    [Header("Persistent Party Health")]
    [SerializeField]
    private List<PartyMemberHealthState> partyHealthStates =
        new List<PartyMemberHealthState>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SyncRecruitedMembersToParty();
        RegisterAllCurrentPartyHealth();
    }

    public List<GameObject> GetCurrentPartyPrefabs()
    {
        SyncRecruitedMembersToParty();
        RegisterAllCurrentPartyHealth();

        return new List<GameObject>(currentPartyPrefabs);
    }

    public void AddPartyMember(GameObject partyMemberPrefab)
    {
        if (partyMemberPrefab == null)
        {
            return;
        }

        if (!currentPartyPrefabs.Contains(partyMemberPrefab))
        {
            currentPartyPrefabs.Add(partyMemberPrefab);
            Debug.Log(partyMemberPrefab.name + " added to current party.");
        }

        RegisterInitialHealthFromPrefab(partyMemberPrefab);
    }

    public void RemovePartyMember(GameObject partyMemberPrefab)
    {
        if (partyMemberPrefab == null)
        {
            return;
        }

        if (currentPartyPrefabs.Remove(partyMemberPrefab))
        {
            Debug.Log(partyMemberPrefab.name + " removed from the current party.");
        }
    }

    public bool HasPartyMember(GameObject partyMemberPrefab)
    {
        return partyMemberPrefab != null &&
               currentPartyPrefabs.Contains(partyMemberPrefab);
    }

    public void RecruitPaladin()
    {
        paladinRecruited = true;

        if (paladinBattlePrefab == null)
        {
            Debug.LogWarning("Paladin cannot be recruited because Paladin Battle Prefab is not assigned in PartyManager.");
            return;
        }

        AddPartyMember(paladinBattlePrefab);

        Debug.Log("Paladin has joined the party.");
    }

    public void RecruitMira()
    {
        miraRecruited = true;

        if (miraBattlePrefab == null)
        {
            Debug.LogWarning("Mira was recruited, but Mira Battle Prefab is not assigned.");
            return;
        }

        AddPartyMember(miraBattlePrefab);

        Debug.Log("Mira has joined the party.");
    }

    private void SyncRecruitedMembersToParty()
    {
        if (paladinRecruited && paladinBattlePrefab != null)
        {
            AddPartyMember(paladinBattlePrefab);
        }

        if (miraRecruited && miraBattlePrefab != null)
        {
            AddPartyMember(miraBattlePrefab);
        }
    }

    private void RegisterAllCurrentPartyHealth()
    {
        foreach (GameObject partyMemberPrefab in currentPartyPrefabs)
        {
            RegisterInitialHealthFromPrefab(partyMemberPrefab);
        }
    }

    public void RegisterInitialHealthFromPrefab(GameObject partyMemberPrefab)
    {
        if (partyMemberPrefab == null)
        {
            return;
        }

        PlayerBattle battleMember = partyMemberPrefab.GetComponent<PlayerBattle>();

        if (battleMember == null)
        {
            Debug.LogWarning(
                partyMemberPrefab.name +
                " does not contain PlayerBattle, so its health could not be registered."
            );

            return;
        }

        string memberId = battleMember.entityName;

        if (string.IsNullOrWhiteSpace(memberId))
        {
            memberId = partyMemberPrefab.name;
            Debug.LogWarning(
                partyMemberPrefab.name +
                " has an empty entityName. Using prefab name as health ID."
            );
        }

        PartyMemberHealthState existingState = GetHealthState(memberId);

        if (existingState != null)
        {
            return;
        }

        int startingHealth = Mathf.Max(1, battleMember.health);

        PartyMemberHealthState newState = new PartyMemberHealthState
        {
            memberId = memberId,
            currentHealth = startingHealth,
            maxHealth = startingHealth
        };

        partyHealthStates.Add(newState);

        Debug.Log(
            "Registered initial HP for " +
            newState.memberId +
            ": " +
            newState.currentHealth +
            " / " +
            newState.maxHealth
        );
    }

    public void SaveBattleHealth(PlayerBattle battleMember)
    {
        if (battleMember == null)
        {
            return;
        }

        string memberId = battleMember.entityName;

        if (string.IsNullOrWhiteSpace(memberId))
        {
            memberId = battleMember.name;
        }

        PartyMemberHealthState state = GetHealthState(memberId);

        if (state == null)
        {
            state = new PartyMemberHealthState
            {
                memberId = memberId
            };

            partyHealthStates.Add(state);
        }

        state.currentHealth = battleMember.CurrentHealth;
        state.maxHealth = battleMember.MaxHealth;

        Debug.Log(
            "Saved party HP for " +
            state.memberId +
            ": " +
            state.currentHealth +
            " / " +
            state.maxHealth
        );
    }

    public void ApplyStoredHealth(PlayerBattle battleMember)
    {
        if (battleMember == null)
        {
            return;
        }

        string memberId = battleMember.entityName;

        if (string.IsNullOrWhiteSpace(memberId))
        {
            memberId = battleMember.name;
        }

        PartyMemberHealthState state = GetHealthState(memberId);

        if (state == null)
        {
            state = new PartyMemberHealthState
            {
                memberId = memberId,
                currentHealth = battleMember.CurrentHealth,
                maxHealth = battleMember.MaxHealth
            };

            partyHealthStates.Add(state);

            Debug.Log(
                "Created missing HP state for " +
                state.memberId +
                ": " +
                state.currentHealth +
                " / " +
                state.maxHealth
            );

            return;
        }

        battleMember.RestoreHealthFromPartyState(
            state.currentHealth,
            state.maxHealth
        );
    }

    public bool TryGetPartyHealth(
        string memberId,
        out int currentHealth,
        out int maxHealth
    )
    {
        currentHealth = 0;
        maxHealth = 0;

        PartyMemberHealthState state = GetHealthState(memberId);

        if (state == null)
        {
            return false;
        }

        currentHealth = state.currentHealth;
        maxHealth = state.maxHealth;

        return true;
    }

    private PartyMemberHealthState GetHealthState(string memberId)
    {
        if (string.IsNullOrWhiteSpace(memberId))
        {
            return null;
        }

        return partyHealthStates.Find(
            state => string.Equals(
                state.memberId,
                memberId,
                System.StringComparison.OrdinalIgnoreCase
            )
        );
    }
}