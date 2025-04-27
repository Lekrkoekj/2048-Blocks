using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockShopManager : MonoBehaviour
{
    [SerializeField] private GameObject[] blockPrefabs;
    public Transform[] shopItems;

    [SerializeField] private float currentScrollOffset;
    [SerializeField] private float spaceBetweenBlocks;
    [SerializeField] private float movementAmount = 1;
    [SerializeField] [Range(0, 1)] private float dampingAmount = 0.99f;
    [SerializeField][Range(0, 1)] private float clampSpeed;
    [SerializeField] private float scaleMultiplier = 1;
    [SerializeField] [Range(0, 1)] private float buttonMoveSpeed;
    private float targetScrollOffset = 0.1f;

    private Vector2 beginMousePos;
    private Vector3 beginCameraPos;
    private bool isDragging = false;
    private float normalizedMouseDelta;
    float screenWidth;

    private float previousMousePosition;
    [SerializeField] private float scrollVelocity;

    private ShopItem currentItem;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemPriceText;
    [SerializeField] private TMP_Text currentCoinsText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;
    private int coins;
    // Start is called before the first frame update
    void Start()
    {
        coins = PlayerPrefs.GetInt("coins", 0);
        screenWidth = Screen.width;
        shopItems = new Transform[blockPrefabs.Length];
        for(int i = 0; i < blockPrefabs.Length; i++)
        {
            GameObject block = Instantiate(blockPrefabs[i], transform);
            block.transform.localPosition = new Vector3(0, 0, spaceBetweenBlocks * -i);
            shopItems[i] = block.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Dragging and scrolling
        float mouseDelta = previousMousePosition - Input.mousePosition.x / Screen.width;

        if(Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            beginMousePos = Input.mousePosition;
            beginCameraPos = Camera.main.transform.position;
            isDragging = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (IsPointerOverUI()) return;
            isDragging = false;
            scrollVelocity = mouseDelta * movementAmount;
            Debug.Log(scrollVelocity);
        }

        if (isDragging)
        {
            normalizedMouseDelta = (Input.mousePosition.x - beginMousePos.x) / screenWidth;
            currentScrollOffset = beginCameraPos.z + normalizedMouseDelta * movementAmount;
        }
        else
        {
            scrollVelocity *= dampingAmount;
            if (targetScrollOffset > 0)
            {
                currentScrollOffset -= scrollVelocity;
            }
            else
            {
                currentScrollOffset = Mathf.Lerp(currentScrollOffset, targetScrollOffset, buttonMoveSpeed);
                if(currentScrollOffset - targetScrollOffset > -0.1f && currentScrollOffset - targetScrollOffset < 0.1f)
                {
                    targetScrollOffset = 0.1f;
                }
            }
            currentScrollOffset = Mathf.Lerp(currentScrollOffset, Mathf.Clamp(currentScrollOffset, -(blockPrefabs.Length - 1) * spaceBetweenBlocks, 0), clampSpeed);
            if(scrollVelocity > -0.01f && scrollVelocity < 0.01f)
            {
                scrollVelocity = 0;
                currentScrollOffset = Mathf.Lerp(currentScrollOffset, Mathf.Round(currentScrollOffset / spaceBetweenBlocks) * spaceBetweenBlocks, clampSpeed / 2);
            }
        }

        Transform biggestItem = null;
        foreach(Transform item in shopItems)
        {
            if(biggestItem == null)
            {
                biggestItem = shopItems[0];
            }
            float distance = Mathf.Clamp(Vector3.Distance(new Vector3(0, 0, item.position.z), new Vector3(0, 0, Camera.main.transform.position.z)) * scaleMultiplier, 0, 1);
            item.localScale = new Vector3(1 - distance, 1 - distance, 1 - distance);
            if(item.localScale.x > biggestItem.localScale.x)
            {
                biggestItem = item;
            }
        }
        currentItem = biggestItem.GetComponent<ShopItem>();

        Vector3 cameraPos = Camera.main.transform.position;
        Camera.main.transform.position = new Vector3(cameraPos.x, cameraPos.y, currentScrollOffset);

        previousMousePosition = Input.mousePosition.x / Screen.width;

        // UI
        currentCoinsText.text = $"{coins}  <sprite=0>";
        buyButtonText.text = $"<sprite=0>  {currentItem.itemPrice}";
        buyButton.interactable = !(currentItem.itemPrice >= coins);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true; // For mouse
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true; // For touch
        return false;
    }

    public void MoveForward()
    {
        if (Mathf.Round(currentScrollOffset) <= -(blockPrefabs.Length - 1) * spaceBetweenBlocks)
        {
            currentScrollOffset = -(blockPrefabs.Length - 1) * spaceBetweenBlocks;
            return;
        }
        if(targetScrollOffset > 0)
        {
            targetScrollOffset = currentScrollOffset - spaceBetweenBlocks;
        }
        else
        {
            targetScrollOffset -= spaceBetweenBlocks;
        }
    }

    public void MoveBack()
    {
        if (targetScrollOffset > 0)
        {
            targetScrollOffset = currentScrollOffset + spaceBetweenBlocks;
        }
        else
        {
            targetScrollOffset += spaceBetweenBlocks;
        }
    }

    public void SelectItem()
    {

    }

    public void CloseShop()
    {
        SceneSwitcher.Instance.SwitchScene(0);
    }
}
