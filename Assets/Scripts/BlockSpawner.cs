using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] private MainMenuLogo mainMenuLogo;
    [SerializeField] private GameObject directionLine;

    private Vector3 startPos;
    private GameObject currentBlock;
    private bool wasMouseOnDragArea;

    private void Awake()
    {
        startPos = transform.position;
    }

    void Start()
    {
        SpawnNewBlock(numberedBlockPrefab, 2);
    }

    void Update()
    {
        if (GameManager.Instance.gameOver)
        {
            StopAllCoroutines();
            if (currentBlock) Destroy(currentBlock);
            currentBlock = null;
        }
        else
        {
            // Prevent shooting and moving when clicking on UI elements
            if (IsPointerOverUI())
            {
                //wasMouseOnDragArea = false;
                return;
            }
            MoveBlock();

            if (Input.GetMouseButtonDown(0))
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

            if (Input.GetMouseButtonUp(0) && wasMouseOnDragArea)
            {
                ShootBlock();
            }
        }
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
        if (mainMenuLogo.logoShown)
        {
            mainMenuLogo.HideLogo();
        }
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
        }
        else if(currentBlock.GetComponent<BombItem>())
        {
            currentBlock.GetComponent<BombItem>().canExplode = true;
        }
        currentBlock = null;
        directionLine.SetActive(false);
        GetComponent<AudioSource>().pitch = 1 + Random.Range(-0.1f, 0.1f);
        GetComponent<AudioSource>().Play();
        StartCoroutine(DelayedBlockSpawn(spawnTime, numberedBlockPrefab));
    }

    private void MoveBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == dragArea)
            {
                Vector3 pos = new Vector3(Mathf.Clamp(hit.point.x, -2.0f, 2.0f), hit.point.y + 0.5f, Mathf.Clamp(hit.point.z, -12f, -10.5f));
                transform.position = pos;
            }
        }
    }

    public void SpawnBomb()
    {
        if(GameManager.Instance.coins < GameManager.Instance.bombPrice)
        {
            return;
        }
        StopAllCoroutines();
        if (currentBlock != null)
        {
            Destroy(currentBlock);
        }
        SpawnNewBlock(bombPrefab);
        GameManager.Instance.AddCoins(-GameManager.Instance.bombPrice);
    }

    public void SpawnNewBlock(GameObject block, int value = 0)
    {
        transform.position = startPos;
        MoveBlock();
        GameObject newCube = Instantiate(block, transform);
        currentBlock = newCube;
        directionLine.SetActive(true);

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
