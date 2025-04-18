using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private GameObject numberedBlockPrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Collider dragArea;
    [SerializeField] private Collider playingField;
    [SerializeField] private float spawnTime;
    [SerializeField] private float shootForce;
    public MainMenuLogo mainMenuLogo;
    [SerializeField] private GameObject directionLine;
    [SerializeField] private ParticleSystem shootParticles;

    private Vector3 startPos;
    public GameObject currentBlock;
    private bool wasMouseOnDragArea;
    private bool pointerOnUI;
    private bool pointerDownOnUI;

    private Vector3 lastPointerPos;
    private Vector3 targetSpawnerPos;

    private void Awake()
    {
        startPos = new Vector3(0, 1, -11.5f);
    }

    void Start()
    {
        
    }

    void Update()
    {
        // Stop Block Spawner on game over
        if (GameManager.Instance.gameOver)
        {
            StopAllCoroutines();
            if (currentBlock)
            {
                // If a bomb is selected when the game finishes, refund the bomb because it hasn't been used.
                if(currentBlock.GetComponent<BombItem>())
                {
                    GameManager.Instance.AddCoins(GameManager.Instance.bombPrice);
                }
                Destroy(currentBlock);
            }
            currentBlock = null;
            return;
        }
        else
        {
            ChangeSpawnerPosition();

            // Prevent shooting and moving when clicking on UI elements
            if (pointerOnUI)
            {
                pointerOnUI = IsPointerOverUI();
                return;
            }
            else if(HasPointerMoved() && !IsPointerOverUI()) MoveBlock();

            if (Input.GetMouseButtonDown(0) && IsPointerOverUI())
            {
                pointerDownOnUI = true;
            }

            // Check if the pointer presses down on the drag area.
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider == dragArea)
                    {
                        wasMouseOnDragArea = true;
                    }
                    else
                    {
                        wasMouseOnDragArea = false;
                    }
                }
            }

            // If pointer pressing down started on the drag area and then moves onto the playing field (for example by swiping up), shoot the block.
            if (wasMouseOnDragArea)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider == playingField)
                    {
                        ShootBlock();
                    }
                }
            }

            // If pointer clicks on drag area just shoot the block.
            if (Input.GetMouseButtonUp(0) && !IsPointerOverUI() && !pointerDownOnUI)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider == playingField || hit.collider == dragArea)
                    {
                        ShootBlock();
                    }
                }
            }
        }
        pointerDownOnUI = false;
        lastPointerPos = Input.mousePosition;
    }

    private bool HasPointerMoved()
    {
        return Input.mousePosition != lastPointerPos;
    }

    // Helper function to check if pointer/touch is over UI
    private bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true; // For mouse
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true; // For touch
        return false;
    }

    private void ShootBlock()
    {
        if(currentBlock == null)
        {
            return;
        }
        wasMouseOnDragArea = false;
        currentBlock.transform.parent = null;
        currentBlock.GetComponent<Collider>().enabled = true;
        currentBlock.GetComponent<Rigidbody>().isKinematic = false;
        currentBlock.GetComponent<Rigidbody>().AddForce(0f, 100f, shootForce);
        if (currentBlock.GetComponent<NumberedBlock>())
        {
            currentBlock.GetComponent<NumberedBlock>().canMerge = true;
            GameManager.Instance.AddBlock(currentBlock.GetComponent<NumberedBlock>());
        }
        else if(currentBlock.GetComponent<BombItem>())
        {
            currentBlock.GetComponent<BombItem>().canExplode = true;
        }
        currentBlock = null;
        directionLine.SetActive(false);
        GetComponent<AudioSource>().pitch = 1 + Random.Range(-0.1f, 0.1f);
        if (GameManager.Instance.soundMuted == 0) GetComponent<AudioSource>().volume = 1;
        else GetComponent<AudioSource>().volume = 0;
        GetComponent<AudioSource>().Play();
        shootParticles.Play();
        StartCoroutine(DelayedBlockSpawn(spawnTime, numberedBlockPrefab));
    }


    private void ChangeSpawnerPosition()
    {
        transform.position = Vector3.Lerp(transform.position, targetSpawnerPos, 0.6f);
    }

    private void MoveBlock()
    { 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == dragArea || hit.collider == playingField)
            {
                targetSpawnerPos = new Vector3(Mathf.Clamp(hit.point.x, -2.0f, 2.0f), hit.point.y + 0.5f, Mathf.Clamp(hit.point.z, -12f, -10.6f));
                
                if(currentBlock != null && !directionLine.activeSelf)
                {
                    directionLine.SetActive(true);
                }
            }
        }
    }

    public void SpawnBomb()
    {
        pointerOnUI = true;
        // Don't do anything if a bomb is already selected.
        if(currentBlock != null)
        {
            if(currentBlock.GetComponent<BombItem>())
            {
                return;
            }
        }
        // Don't do anything if the player doesn't have enough money
        if(GameManager.Instance.coins < GameManager.Instance.bombPrice)
        {
            return;
        }
        StopAllCoroutines();
        if (currentBlock != null)
        {
            Destroy(currentBlock);
        }
        targetSpawnerPos = startPos;
        transform.position = startPos;
        SpawnNewBlock(bombPrefab);
        GameManager.Instance.AddCoins(-GameManager.Instance.bombPrice);
    }

    public void SpawnNewBlock(GameObject block, int value = 0)
    {
        targetSpawnerPos = startPos;
        transform.position = startPos;
        GameObject newCube = Instantiate(block, transform);
        currentBlock = newCube;

        currentBlock.GetComponent<Collider>().enabled = false;
        currentBlock.GetComponent<Rigidbody>().isKinematic = true;
        if (currentBlock.GetComponent<NumberedBlock>())
        {
            NumberedBlock blockComponent = currentBlock.GetComponent<NumberedBlock>();
            blockComponent.canMerge = false;
            if(value == 0)
            {
                value = (int)Mathf.Pow(2, Random.Range(1, 4));
            }
            blockComponent.value = value;
        }
    }

    IEnumerator DelayedBlockSpawn(float delayInSeconds, GameObject block)
    {
        yield return new WaitForSeconds(delayInSeconds);
        SpawnNewBlock(block);
    }
}
