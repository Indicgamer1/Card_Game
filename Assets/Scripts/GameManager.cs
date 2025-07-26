using System.Collections;
using TMPro;
using UnityEngine;

/*
public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public static int gameSize = 2;
    // gameobject instance
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
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private GameObject info;
    // for preloading
    [SerializeField]
    private Card spritePreload;
    // other UI
    [SerializeField]
    private Text sizeLabel;
    [SerializeField]
    private Slider sizeSlider;
    [SerializeField]
    private Text timeLabel;
    private float time;

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
        panel.SetActive(false);
    }
    // Purpose is to allow preloading of panel, so that it does not lag when it loads
    // Call this in the start method to preload all sprites at start of the script
    private void PreloadCardImage()
    {
        for (int i = 0; i < sprites.Length; i++)
            spritePreload.SpriteID = i;
        spritePreload.gameObject.SetActive(false);
    }
    // Start a game
    public void StartCardGame()
    {
        if (gameStart) return; // return if game already running
        gameStart = true;
        // toggle UI
        panel.SetActive(true);
        info.SetActive(false);
        // set cards, size, position
        SetGamePanel();
        // renew gameplay variables
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        // allocate sprite to card
        SpriteCardAllocation();
        StartCoroutine(HideFace());
        time = 0;
    }

    // Initialize cards, size, and position based on size of game
    private void SetGamePanel(){
        // if game is odd, we should have 1 card less
        int isOdd = gameSize % 2 ;

        cards = new Card[gameSize * gameSize - isOdd];
        // remove all gameobject from parent
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        // calculate position between each card & start position of each card based on the Panel
        RectTransform panelsize = panel.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;
        float scale = 1.0f/gameSize;
        float xInc = row_size/gameSize;
        float yInc = col_size/gameSize;
        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        if(isOdd == 0) {
            curX += xInc / 2;
            curY += yInc / 2;
        }
        float initialX = curX;
        // for each in y-axis
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            // for each in x-axis
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;
                // if is the last card and game is odd, we instead move the middle card on the panel to last spot
                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    c = cards[index].gameObject;
                }
                else
                {
                    // create card prefab
                    c = Instantiate(prefab);
                    // assign parent
                    c.transform.parent = cardList.transform;

                    int index = i * gameSize + j;
                    cards[index] = c.GetComponent<Card>();
                    cards[index].ID = index;
                    // modify its size
                    c.transform.localScale = new Vector3(scale, scale);
                }
                // assign location
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;

            }
            curY += yInc;
        }

    }
    // reset face-down rotation of all cards
    void ResetFace()
    {
        for (int i = 0; i < gameSize; i++)
            cards[i].ResetRotation();
    }
    // Flip all cards after a short period
    IEnumerator HideFace()
    {
        //display for a short moment before flipping
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }
    // Allocate pairs of sprite to card instances
    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];
        // sprite selection
        for (i = 0; i < cards.Length/2; i++)
        {
            // get a random sprite
            int value = Random.Range(0, sprites.Length - 1);
            // check previous number has not been selection
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
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }

    }
    // Slider update gameSize
    public void SetGameSize() {
        gameSize = (int)sizeSlider.value;
        sizeLabel.text = gameSize + " X " + gameSize;
    }
    // return Sprite based on its id
    public Sprite GetSprite(int spriteId)
    {
        return sprites[spriteId];
    }
    // return card back Sprite
    public Sprite CardBack()
    {
        return cardBack;
    }
    // check if clickable
    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }
    // card onclick event
    public void cardClicked(int spriteId, int cardId)
    {
        // first card selected
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        { // second card selected
            if (spriteSelected == spriteId)
            {
                //correctly matched
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cardLeft -= 2;
                CheckGameWin();
            }
            else
            {
                // incorrectly matched
                cards[cardSelected].Flip();
                cards[cardId].Flip();
            }
            cardSelected = spriteSelected = -1;
        }
    }
    // check if game is completed
    private void CheckGameWin()
    {
        // win game
        if (cardLeft == 0)
        {
            EndGame();
            AudioPlayer.Instance.PlayAudio(1);
        }
    }
    // stop game
    private void EndGame()
    {
        gameStart = false;
        panel.SetActive(false);
    }
    public void GiveUp()
    {
        EndGame();
    }
    public void DisplayInfo(bool i)
    {
        info.SetActive(i);
    }
    // track elasped time
    private void Update(){
        if (gameStart) {
            time += Time.deltaTime;
            timeLabel.text = "Time: " + time + "s";
        }
    }
}
*/

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int gameRows = 2;
    public static int gameCols = 2;
    
    // gameobject instance
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
    [SerializeField]
    private GameObject panel;
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
    private int turns;

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
        panel.SetActive(false);
        
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
    }
    
    // Handle rows input field change
    private void OnRowsChanged(string value)
    {
        if (int.TryParse(value, out int rows))
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
    private void OnColsChanged(string value)
    {
        if (int.TryParse(value, out int cols))
        {
            gameCols = Mathf.Clamp(cols, 2, 6); // Limit between 2 and 6
            colsInputField.text = gameCols.ToString();
        }
        else
        {
            colsInputField.text = gameCols.ToString();
        }
    }
    
    // Purpose is to allow preloading of panel, so that it does not lag when it loads
    // Call this in the start method to preload all sprites at start of the script
    private void PreloadCardImage()
    {
        for (int i = 0; i < sprites.Length; i++)
            spritePreload.SpriteID = i;
        spritePreload.gameObject.SetActive(false);
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
            // You could show a UI message here instead of just a debug warning
            return;
        }
        
        gameStart = true;
        // toggle UI
        menu.SetActive(false);
        panel.SetActive(true);
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
        UpdateTurnText();
    }

    // Initialize cards, size, and position based on rows and columns
private void SetGamePanel()
{
    int totalCards = gameRows * gameCols;
    // if total cards is odd, we should have 1 card less
    int isOdd = totalCards % 2;

    cards = new Card[totalCards - isOdd];
    
    // remove all gameobject from parent
    foreach (Transform child in cardList.transform)
    {
        GameObject.Destroy(child.gameObject);
    }
    
    // calculate position between each card & start position of each card based on the Panel
    RectTransform panelsize = panel.transform.GetComponent(typeof(RectTransform)) as RectTransform;
    float panelWidth = panelsize.sizeDelta.x;
    float panelHeight = panelsize.sizeDelta.y;
    
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
                
            // If this is the last position and we have an odd total, place the middle card here
            if (isOdd == 1 && row == gameRows - 1 && col == gameCols - 1)
            {
                // Calculate middle position
                int middleRow = middleIndex / gameCols;
                int middleCol = middleIndex % gameCols;
                
                // create card prefab
                GameObject c = Instantiate(prefab);
                // assign parent
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
                
                cardIndex++;
                
            }
            else{
                // create card prefab
                GameObject c = Instantiate(prefab, cardList.transform);
                // assign parent
                //c.transform.SetParent(cardList.transform);

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
    
    // reset face-down rotation of all cards
    void ResetFace()
    {
        for (int i = 0; i < cards.Length; i++)
            cards[i].ResetRotation();
    }
    
    // Flip all cards after a short period
    IEnumerator HideFace()
    {
        //display for a short moment before flipping
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
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
    public Sprite GetSprite(int spriteId)
    {
        return sprites[spriteId];
    }
    
    // return card back Sprite
    public Sprite CardBack()
    {
        return cardBack;
    }
    
    // check if clickable
    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }
    
    // card onclick event
    public void cardClicked(int spriteId, int cardId)
    {
        // first card selected
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        { // second card selected
            if (spriteSelected == spriteId)
            {
                //correctly matched
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cardLeft -= 2;
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
                cards[cardId].Flip();
                AudioPlayer.Instance.PlayAudio(3, 0.8f);
            }
            cardSelected = spriteSelected = -1;

            turns++;
            UpdateTurnText();
        }
    }
    
    // stop game
    private void EndGame()
    {
        gameStart = false;
        panel.SetActive(false);
        menu.SetActive(true);
    }
    
    public void GiveUp()
    {
        EndGame();
    }
    
    public void DisplayInfo(bool i)
    {
        info.SetActive(i);
    }

    public void UpdateTurnText()
    {
        turnText.text = "Turns: " + turns.ToString();
    }
}
