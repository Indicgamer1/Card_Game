using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public int gameRows;
    public int gameCols;
    public int turns;
    public int combo;
    public int cardLeft;
    public bool gameStart;
    public int spriteSelected;
    public int cardSelected;
    
    // Card data for each card
    public List<CardSaveData> cardData;
    
    // Constructor
    public GameSaveData()
    {
        cardData = new List<CardSaveData>();
    }
}

[System.Serializable]
public class CardSaveData
{
    public int spriteID;
    public int cardID;
    public bool isActive; // Whether card is still in play (not matched)
    public bool isFlipped; // Current flip state
    
    public CardSaveData(int spriteID, int cardID, bool isActive, bool isFlipped)
    {
        this.spriteID = spriteID;
        this.cardID = cardID;
        this.isActive = isActive;
        this.isFlipped = isFlipped;
    }
}

public class GameSaveManager : MonoBehaviour
{
    private const string SAVE_KEY = "CardMatchGameSave";
    
    public static GameSaveManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData();
        GameManager gm = GameManager.Instance;
        
        if (gm == null) return;
        
        // Save basic game state
        saveData.gameRows = gm.GetGameRows();
        saveData.gameCols = gm.GetGameCols();
        saveData.turns = gm.GetTurns();
        saveData.combo = gm.GetCombo();
        saveData.cardLeft = gm.GetCardLeft();
        saveData.gameStart = gm.IsGameStarted();
        saveData.spriteSelected = gm.GetSpriteSelected();
        saveData.cardSelected = gm.GetCardSelected();
        
        // Save card states
        Card[] cards = gm.GetCards();
        if (cards != null)
        {
            foreach (Card card in cards)
            {
                if (card != null)
                {
                    // For active cards, we want to save them as face down (not flipped)
                    // For inactive cards, they should be saved as face up (flipped)
                    bool savedFlipState = card.IsActive() ? false : true;
                    
                    CardSaveData cardSaveData = new CardSaveData(
                        card.SpriteID,
                        card.ID,
                        card.IsActive(),
                        savedFlipState
                    );
                    saveData.cardData.Add(cardSaveData);
                    
                    Debug.Log($"Saving Card {card.ID}: Active={card.IsActive()}, SavedAsFlipped={savedFlipState}, SpriteID={card.SpriteID}");
                }
            }
        }
        
        // Convert to JSON and save
        string jsonData = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(SAVE_KEY, jsonData);
        PlayerPrefs.Save();
        
        Debug.Log("Game saved successfully!");
    }
    
    public bool LoadGame()
    {
        if (!HasSaveData())
        {
            Debug.Log("No save data found.");
            return false;
        }
        
        string jsonData = PlayerPrefs.GetString(SAVE_KEY);
        
        try
        {
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            GameManager gm = GameManager.Instance;
            
            if (gm == null) return false;
            
            // Load basic game state
            gm.LoadGameState(saveData);
            
            Debug.Log("Game loaded successfully!");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load game: " + e.Message);
            return false;
        }
    }
    
    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
    
    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("Save data deleted.");
    }
    
    // Auto-save functionality
    public void EnableAutoSave(float interval = 30f)
    {
        StartCoroutine(AutoSaveCoroutine(interval));
    }
    
    private IEnumerator AutoSaveCoroutine(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            
            // Only auto-save if game is running
            if (GameManager.Instance != null && GameManager.Instance.IsGameStarted())
            {
                SaveGame();
            }
        }
    }
}