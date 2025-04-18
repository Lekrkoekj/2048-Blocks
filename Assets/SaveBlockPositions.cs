using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveBlockPositions : MonoBehaviour
{
    public GameState state = new GameState();
    public BlockSpawner spawner;
    public GameObject blockPrefab;
    private string filePath;

    private void Awake()
    {
        filePath = Application.persistentDataPath + "/LastGameState.json";

        if (!System.IO.File.Exists(filePath))
        {
            SaveToJson();
        }
    }

    public void SaveGameState()
    {
        ClearSavedData();

        state.score = GameManager.Instance.score;

        GameManager.Instance.activeBlocks.ForEach(block =>
        {
            state.blocksPos.Add(block.transform.position);
            state.blocksRot.Add(block.transform.rotation);
            state.blocksValue.Add(block.value);
        });
        if (spawner.currentBlock)
        {
            if (spawner.currentBlock.GetComponent<NumberedBlock>())
            {
                state.valueInSpawner = spawner.currentBlock.GetComponent<NumberedBlock>().value;
            }
            else if (spawner.currentBlock.GetComponent<BombItem>())
            {
                state.valueInSpawner = 1;
            }
        }
        else
        {
            state.valueInSpawner = 0;
        }
        SaveToJson();
    }

    public void ClearSavedData()
    {
        state.score = 0;
        state.valueInSpawner = 0;

        state.blocksRot.Clear();
        state.blocksPos.Clear();
        state.blocksValue.Clear();

        SaveToJson();
    }

    private void SaveToJson()
    {
        string data = JsonUtility.ToJson(state);
        System.IO.File.WriteAllText(filePath, data);
        Debug.Log("Game State saved to: " + filePath);
    }

    public void LoadFromJson()
    {
        string data = System.IO.File.ReadAllText(filePath);

        state = JsonUtility.FromJson<GameState>(data);

        LoadGameState();
    }

    private void LoadGameState()
    {
        GameManager.Instance.score = state.score;
        
        Vector3[] blockPositions = state.blocksPos.ToArray();
        Quaternion[] blockRotations = state.blocksRot.ToArray();
        int[] blockValues = state.blocksValue.ToArray();

        for (int i = 0; i < blockPositions.Length; i++) {
            GameObject newBlock = Instantiate(blockPrefab);
            newBlock.GetComponent<Collider>().enabled = false;
            newBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            newBlock.transform.position = blockPositions[i];
            newBlock.transform.rotation = blockRotations[i];
            newBlock.GetComponent<NumberedBlock>().value = blockValues[i];
            newBlock.GetComponent<Collider>().enabled = true;
            newBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            GameManager.Instance.activeBlocks.Add(newBlock.GetComponent<NumberedBlock>());
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameState();
    }

    private void OnApplicationPause()
    {
        if (Time.time > 0)
        {
            SaveGameState();
        }
    }
}

[System.Serializable]
public class GameState
{
    public List<Vector3> blocksPos = new List<Vector3>();
    public List<Quaternion> blocksRot = new List<Quaternion>();
    public List<int> blocksValue = new List<int>();
    public int score;
    public int valueInSpawner;
}