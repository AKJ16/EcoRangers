using System.Collections;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public static TrashSpawner instance;
    public GameObject[] trashPrefabs;
    public Transform spawnPoint;

    [SerializeField] private float spawnInterval = 3f;
    private float nextSpawnTime = 0f;

    [SerializeField] private int initialList = 4;
    [SerializeField] private int firstMilestoneList = 6;
    [SerializeField] private int secondMilestoneList = 10;
    [SerializeField] private int thirdMilestoneList = 12;
    [SerializeField] private int fourthMilestoneList = 14;
    [SerializeField] private int fifthMilestoneList = 16;
    private int currentList;

    [SerializeField] private float boostValue = 0.05f;
    private bool boostThird;
    private bool boostFourth;
    private bool boostFifth;
    private bool isCheckingEmpty = false;

    private void Awake()
    {
        instance = this;
        currentList = initialList;
    }

    private void Start()
    {
        SpawnTrash();
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void Update()
    {
        if (Time.timeScale <= 0.01f) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnTrash();
            nextSpawnTime = Time.time + spawnInterval;
        }

        if (!isCheckingEmpty && CurrentTrashCount() == 0)
        {
            StartCoroutine(CheckEmptyAndRespawn());
        }
    }

    private int CurrentTrashCount()
    {
        return FindObjectsByType<TrashItem>(FindObjectsSortMode.None).Length;
    }

    public void SpawnTrash()
    {
        float roll = Random.value;
        float currentChance = 0f;

        if (boostThird)
        {
            currentChance += boostValue;
            if (roll < currentChance) 
            { 
                InstantiateTrash(thirdMilestoneList - 1); 
                return; 
            }
        }

        if (boostFourth)
        {
            currentChance += boostValue;
            if (roll < currentChance) 
            { 
                InstantiateTrash(fourthMilestoneList - 1); 
                return; 
            }
        }

        if (boostFifth)
        {
            currentChance += boostValue;
            if (roll < currentChance) 
            { 
                InstantiateTrash(fifthMilestoneList - 1); 
                return; 
            }
        }

        int index = Random.Range(0, currentList);
        InstantiateTrash(index);
    }

    private void InstantiateTrash(int index)
    {
        if (index >= trashPrefabs.Length) index = trashPrefabs.Length - 1;

        Instantiate(
            trashPrefabs[index],
            spawnPoint.position,
            Quaternion.Euler(0, 0, Random.Range(0f, 360f))
        );
    }

    public void TrashRemoved(bool disposed)
    {
        if (!isCheckingEmpty)
        {
            StartCoroutine(CheckEmptyAndRespawn());
        }
    }

    private IEnumerator CheckEmptyAndRespawn()
    {
        isCheckingEmpty = true;
        yield return new WaitForEndOfFrame();

        if (CurrentTrashCount() == 0)
        {
            SpawnTrash();
            nextSpawnTime = Time.time + spawnInterval;
        }

        isCheckingEmpty = false;
    }

    public void FirstMilestone() 
    {
        currentList = firstMilestoneList;}
    public void SecondMilestone() 
    { 
        currentList = secondMilestoneList;
    }
    public void ThirdMilestone() 
    { 
        currentList = thirdMilestoneList; boostThird = true; 
    }
    public void FourthMilestone() 
    { 
        currentList = fourthMilestoneList; boostFourth = true; 
    }
    public void FifthMilestone() 
    { 
        currentList = fifthMilestoneList; boostFifth = true; 
    }
    public void EndMilestone() 
    { 
        boostValue *= 2; 
    }
}