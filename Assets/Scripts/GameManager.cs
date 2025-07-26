using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private static int gameRows = 2;
    private static int gameCols = 2;
    
    // game object instance
    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private GameObject prefab;
    // parent object of cards
    [SerializeField]
    private GameObject cardList;
    // sprite for card back
    [SerializeField]
    private Sprite cardBack;
    // all possible sprite for card front
    [SerializeField]
    private Sprite[] sprites;
    // list of card
    private Card[] cards;

    //we place card on this panel
    [FormerlySerializedAs("panel")]
    [SerializeField]
    private GameObject cardPanel;

    [FormerlySerializedAs("panel")]
    [SerializeField]
    private GameObject gamePanel;

    [SerializeField]
    private GameObject info;
    // for preloading
    [SerializeField]
    private Card spritePreload;
    [SerializeField]
    private TMP_InputField rowsInputField;
    [SerializeField]
    private TMP_InputField colsInputField;
    [SerializeField]
    private TextMeshProUGUI turnText;
    [SerializeField]
    private TextMeshProUGUI comboText;
    
    // Save/Load UI buttons
    [SerializeField]
    private UnityEngine.UI.Button saveButton;
    [SerializeField]
    private UnityEngine.UI.Button loadButton;
    [SerializeField]
    private UnityEngine.UI.Button deleteSaveButton;
    
    private int turns, combo;
    private int spriteSelected;
    private int cardSelected;
    private int cardLeft;
    private bool gameStart;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        gameStart = false;
        gamePanel.SetActive(false);
        
        // Initialize input fields
        if (rowsInputField != null)
        {
            rowsInputField.text = gameRows.ToString();
            rowsInputField.onEndEdit.AddListener(OnRowsChanged);
        }
        
        if (colsInputField != null)
        {
            colsInputField.text = gameCols.ToString();
            colsInputField.onEndEdit.AddListener(OnColsChanged);
        }
        
        // Setup save/load buttons
        SetupSaveLoadButtons();
        
        // Enable auto-save every 30 seconds
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.EnableAutoSave(30f);
        }
    }
    
    private void SetupSaveLoadButtons()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveGame);
            
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(LoadGame);
            // Update load button state based on save data availability
            UpdateLoadButtonState();
        }
            
        if (deleteSaveButton != null)
        {
            deleteSaveButton.onClick.AddListener(DeleteSaveData);
            // Update delete button state
            UpdateDeleteButtonState();
        }
    }
    
    private void UpdateLoadButtonState()
    {
        if (loadButton != null && GameSaveManager.Instance != null)
        {
            loadButton.interactable = GameSaveManager.Instance.HasSaveData();
        }
    }
    
    private void UpdateDeleteButtonState()
    {
        if (deleteSaveButton != null && GameSaveManager.Instance != null)
        {
            deleteSaveButton.interactable = GameSaveManager.Instance.HasSaveData();
        }
    }
    
    // Save/Load Methods
    public void SaveGame()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveGame();
            UpdateLoadButtonState();
            UpdateDeleteButtonState();
        }
    }
    
    public void LoadGame()
    {
        if (GameSaveManager.Instance != null)
        {
            if (GameSaveManager.Instance.LoadGame())
            {
                // Game loaded successfully, UI will be updated by LoadGameState
            }
        }
    }
    
    public void DeleteSaveData()
    {
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.DeleteSaveData();
            UpdateLoadButtonState();
            UpdateDeleteButtonState();
        }
    }
    
    // Load game state from save data
    public void LoadGameState(GameSaveData saveData)
    {
        // Stop current game if running
        if (gameStart)
        {
            EndGame();
        }
        
        // Load game parameters
        gameRows = saveData.gameRows;
        gameCols = saveData.gameCols;
        turns = saveData.turns;
        combo = saveData.combo;
        cardLeft = saveData.cardLeft;
        spriteSelected = saveData.spriteSelected;
        cardSelected = saveData.cardSelected;
        
        // Update input fields
        if (rowsInputField != null)
            rowsInputField.text = gameRows.ToString();
        if (colsInputField != null)
            colsInputField.text = gameCols.ToString();
        
        // If the saved game was in progress, restore it
        if (saveData.gameStart && saveData.cardData.Count > 0)
        {
            gameStart = true;
            menu.SetActive(false);
            gamePanel.SetActive(true);
            info.SetActive(false);
            
            // Setup the game panel
            SetGamePanel();
            
            // Restore card states
            for (int i = 0; i < saveData.cardData.Count && i < cards.Length; i++)
            {
                CardSaveData cardData = saveData.cardData[i];
                
                if (cards[i] != null)
                {
                    // First set the sprite ID
                    cards[i].SetSpriteIDForLoad(cardData.spriteID);
                    cards[i].ID = cardData.cardID;
                    
                    // Set card activity state first
                    if (cardData.isActive)
                    {
                        cards[i].Active();
                        // For active cards, set them to face down (clickable state)
                        cards[i].SetFlipStateForLoad(false);
                    }
                    else
                    {
                        // For inactive (matched) cards, they should be face up and faded
                        cards[i].SetFlipStateForLoad(true);
                        cards[i].Inactive();
                    }
                }
            }
            
            // Update UI
            UpdateTurnText();
            UpdateComboText();
        }
    }
    
    // Getter methods for save system
    public int GetGameRows() => gameRows;
    public int GetGameCols() => gameCols;
    public int GetTurns() => turns;
    public int GetCombo() => combo;
    public int GetCardLeft() => cardLeft;
    public bool IsGameStarted() => gameStart;
    public int GetSpriteSelected() => spriteSelected;
    public int GetCardSelected() => cardSelected;
    public Card[] GetCards() => cards;
    
    // Handle rows input field change
    private void OnRowsChanged(string _value)
    {
        if (int.TryParse(_value, out int rows))
        {
            gameRows = Mathf.Clamp(rows, 2, 6); // Limit between 2 and 6
            rowsInputField.text = gameRows.ToString();
        }
        else
        {
            rowsInputField.text = gameRows.ToString();
        }
    }
    
    // Handle columns input field change
    private void OnColsChanged(string _value)
    {
        if (int.TryParse(_value, out int cols))
        {
            gameCols = Mathf.Clamp(cols, 2, 6); // Limit between 2 and 6
            colsInputField.text = gameCols.ToString();
        }
        else
        {
            colsInputField.text = gameCols.ToString();
        }
    }
    
    // Start a game
    public void StartCardGame()
    {
        if (gameStart) return; // return if game already running
        
        // Validate that we have enough sprites for pairs
        int totalCards = gameRows * gameCols;
        int isOdd = totalCards % 2;
        int pairsNeeded = (totalCards - isOdd) / 2;
        
        if (pairsNeeded > sprites.Length)
        {
            Debug.LogWarning($"Not enough sprites! Need {pairsNeeded} different sprites but only have {sprites.Length}");
            return;
        }
        
        gameStart = true;
        // toggle UI
        menu.SetActive(false);
        gamePanel.SetActive(true);
        info.SetActive(false);
        // set cards, size, position
        SetGamePanel();
        // renew gameplay variables
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        // allocate sprite to card
        SpriteCardAllocation();
        StartCoroutine(HideFace());
        turns = 0;
        combo = 0;
        UpdateTurnText();
        UpdateComboText();
    }

    // Initialize cards, size, and position based on rows and columns
    private void SetGamePanel()
    {
        int totalCards = gameRows * gameCols;
        // if total cards is odd, we should have 1 card less
        int isOdd = totalCards % 2;

        cards = new Card[totalCards - isOdd];
        
        // remove all game object from parent
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        
        // calculate position between each card & start position of each card based on the Panel
        RectTransform rectTransform = cardPanel.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        if (rectTransform != null){
            float panelWidth = rectTransform.sizeDelta.x;
            float panelHeight = rectTransform.sizeDelta.y;
        
            // Calculate scale to fit cards nicely in the panel
            float scaleX = 1.0f / gameCols;
            float scaleY = 1.0f / gameRows;
            float scale = Mathf.Min(scaleX, scaleY) * 0.9f; // 0.9f for some padding
        
            float xInc = panelWidth / gameCols;
            float yInc = panelHeight / gameRows;
        
            // Start position (top-left corner)
            float startX = -panelWidth / 2 + xInc / 2;
            float startY = panelHeight / 2 - yInc / 2;

            int cardIndex = 0;
        
            // Calculate middle position if odd number of cards
            int middleIndex = -1;
            if (isOdd == 1)
            {
                middleIndex = totalCards / 2; // This gives us the middle position
            }
        
            // for each row
            for (int row = 0; row < gameRows; row++)
            {
                // for each column
                for (int col = 0; col < gameCols; col++)
                {
                    int currentGridIndex = row * gameCols + col;
                
                    // Skip the middle card if total is odd (we'll place it at the end)
                    if (isOdd == 1 && currentGridIndex == middleIndex)
                        continue;
                    
                    // If this is the last position, and we have an odd total, place the middle card here
                    if (isOdd == 1 && row == gameRows - 1 && col == gameCols - 1)
                    {
                    
                        // create card prefab
                        GameObject c = Instantiate(prefab);
                        // assign parent
                        if (c != null){
                            c.transform.parent = cardList.transform;

                            cards[cardIndex] = c.GetComponent<Card>();
                            cards[cardIndex].ID = cardIndex;

                            // modify its size
                            c.transform.localScale = new Vector3(scale, scale, 1);

                            // calculate position for last spot
                            float posX = startX + col * xInc;
                            float posY = startY - row * yInc;

                            // assign location
                            c.transform.localPosition = new Vector3(posX, posY, 0);
                        }

                        cardIndex++;
                    
                    }
                    else{
                        // create card prefab
                        GameObject c = Instantiate(prefab, cardList.transform);

                        cards[cardIndex] = c.GetComponent<Card>();
                        cards[cardIndex].ID = cardIndex;

                        // modify its size
                        c.transform.localScale = new Vector3(scale, scale, 1);

                        // calculate position
                        float posX = startX + col * xInc;
                        float posY = startY - row * yInc;

                        // assign location
                        c.transform.localPosition = new Vector3(posX, posY, 0);

                        cardIndex++;
                    }
                }
            }
        }
    }
        
    // Flip all cards after a short period
    IEnumerator HideFace()
    {
        //display for a short moment before flipping
        yield return new WaitForSeconds(0.3f);
        foreach (var t in cards)
            t.Flip();

        yield return new WaitForSeconds(0.5f);
    }
    
    // Allocate pairs of sprite to card instances
    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];
        
        // sprite selection
        for (i = 0; i < cards.Length / 2; i++)
        {
            // get a random sprite
            int value = Random.Range(0, sprites.Length);
            
            // check previous number has not been selected
            // if the number of cards is larger than number of sprites, it will reuse some sprites
            for (j = i; j > 0; j--)
            {
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprites.Length;
            }
            selectedID[i] = value;
        }

        // card sprite deallocation
        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }
        
        // card sprite pairing allocation
        for (i = 0; i < cards.Length / 2; i++)
        {
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }
        }
    }
    
    // return Sprite based on its id
    public Sprite GetSprite(int _spriteId)
    {
        return sprites[_spriteId];
    }
    
    // return card back Sprite
    public Sprite CardBack()
    {
        return cardBack;
    }
    
    // check if clickable
    public bool CanClick()
    {
        if (!gameStart)
            return false;
        return true;
    }
    
    // card onclick event
    public void CardClicked(int _spriteId, int _cardId)
    {
        // first card selected
        if (spriteSelected == -1)
        {
            spriteSelected = _spriteId;
            cardSelected = _cardId;
        }
        else
        { // second card selected
            if (spriteSelected == _spriteId)
            {
                //correctly matched
                cards[cardSelected].Inactive();
                cards[_cardId].Inactive();
                cardLeft -= 2;
                combo++; // Increment combo for scoring
                
                if (cardLeft == 0){
                    EndGame();
                    AudioPlayer.Instance.PlayAudio(1);
                }
                else{
                    AudioPlayer.Instance.PlayAudio(2);
                }
            }
            else
            {
                // incorrectly matched
                cards[cardSelected].Flip();
                cards[_cardId].Flip();
                combo = 0; // Reset combo on mismatch
                AudioPlayer.Instance.PlayAudio(3, 0.8f);
                
                combo = 0;
            }
            cardSelected = spriteSelected = -1;

            turns++;
            UpdateTurnText();
            UpdateComboText();
        }
    }
    
    // stop game
    private void EndGame()
    {
        gameStart = false;
        gamePanel.SetActive(false);
        menu.SetActive(true);
        
        // Delete save data when game ends normally
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.DeleteSaveData();
            UpdateLoadButtonState();
            UpdateDeleteButtonState();
        }
    }
    
    public void GiveUp()
    {
        EndGame();
    }

    private void UpdateTurnText()
    {
        if (turnText != null)
            turnText.text = "Turns: " + turns.ToString();
    }
    
    private void UpdateComboText()
    {
        if (comboText != null)
            comboText.text = "Combo: x" + combo.ToString();
    }
}