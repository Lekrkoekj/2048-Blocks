using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private GameObject numberedBlockPrefab;
    [SerializeField] private Collider dragArea;
    [SerializeField] private Collider playingField;
    [SerializeField] private float spawnTime;

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
        if(GameManager.Instance.gameOver)
        {
            StopAllCoroutines();
            Destroy(currentBlock);
            currentBlock = null;
        }

        if(Input.GetMouseButtonDown(0))
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

        if(wasMouseOnDragArea)
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

        if(Input.GetMouseButtonUp(0) && wasMouseOnDragArea)
        {
            ShootBlock();
        }

        MoveBlock();
    }

    private void ShootBlock()
    {
        wasMouseOnDragArea = false;
        currentBlock.transform.parent = null;
        currentBlock.GetComponent<Collider>().enabled = true;
        currentBlock.GetComponent<Rigidbody>().isKinematic = false;
        currentBlock.GetComponent<Rigidbody>().AddForce(0f, 100f, 700f);
        currentBlock.GetComponent<NumberedBlock>().canMerge = true;
        currentBlock = null;
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

    public void SpawnNewBlock(GameObject block, int value = 0)
    {
        transform.position = startPos;
        MoveBlock();
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
