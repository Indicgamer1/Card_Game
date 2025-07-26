using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private int spriteID;
    private bool flipped;
    private bool turning;
    private bool isActive = true; // Track if card is still in play
    
    [SerializeField]
    private Image img;

    // flip card animation
    // if changeSprite specified, will 90 degree, change to back/front sprite before flipping another 90 degree
    private IEnumerator Flip90(Transform _thisTransform, float _time, bool _changeSprite)
    {
        Quaternion startRotation = _thisTransform.rotation;
        Quaternion endRotation = _thisTransform.rotation * Quaternion.Euler(new Vector3(0, 90, 0));
        float rate = 1.0f / _time;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            _thisTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }
        //change sprite and flip another 90degree
        if (_changeSprite)
        {
            flipped = !flipped;
            ChangeSprite();
            StartCoroutine(Flip90(transform, _time, false));
        }
        else
            turning = false;
    }
    
    // perform a 180 degree flip
    public void Flip()
    {
        turning = true;
        AudioPlayer.Instance.PlayAudio(0);
        StartCoroutine(Flip90(transform, 0.25f, true));
    }
    
    // Force flip without animation (for loading saved state)
    public void ForceFlip()
    {
        flipped = !flipped;
        ChangeSprite();
        
        // Set appropriate rotation based on flip state
        if (flipped)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0)); // Face up
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0)); // Face down
        }
    }
    
    // Set flip state directly for loading (more reliable)
    public void SetFlipStateForLoad(bool shouldBeFlipped)
    {
        flipped = shouldBeFlipped;
        turning = false; // Ensure not turning
        
        ChangeSprite();
    }
    
    // Set sprite ID without changing flip state (for loading)
    public void SetSpriteIDForLoad(int id)
    {
        spriteID = id;
        // Don't automatically change flip state here
    }
    
    // toggle front/back sprite
    private void ChangeSprite()
    {
        if (spriteID == -1 || img == null) return;
        img.sprite = flipped ? GameManager.Instance.GetSprite(spriteID) : GameManager.Instance.CardBack();
    }
    
    // call fade animation
    public void Inactive()
    {
        isActive = false;
        StartCoroutine(Fade());
    }
    
    // play fade animation by changing alpha of images color
    private IEnumerator Fade()
    {
        float rate = 1.0f / 2.5f;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            img.color = Color.Lerp(img.color, Color.clear, t);

            yield return null;
        }
    }
    
    // set card to be active color
    public void Active()
    {
        isActive = true;
        if (img)
            img.color = Color.white;
    }
    
    // Check if card is active (not matched yet)
    public bool IsActive()
    {
        return isActive;
    }
    
    // Check if card is currently flipped (showing front)
    public bool IsFlipped()
    {
        return flipped;
    }
    
    // spriteID getter and setter
    public int SpriteID
    {
        set
        {
            spriteID = value;
            flipped = true;
            ChangeSprite();
        }
        get => spriteID;
    }
    
    // card ID getter and setter
    public int ID { set; get; }

    // reset card default rotation
    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        flipped = true;
    }
    
    // card onclick event
    public void CardBtn()
    {
        // Debug logging to help troubleshoot
        Debug.Log($"Card {ID} clicked - Flipped: {flipped}, Turning: {turning}, Active: {isActive}, CanClick: {GameManager.Instance.CanClick()}");
        
        if (flipped || turning || !isActive) return;
        if (!GameManager.Instance.CanClick()) return;
        Flip();
        StartCoroutine(SelectionEvent());
    }
    
    // inform manager card is selected with a slight delay
    private IEnumerator SelectionEvent()
    {
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.CardClicked(spriteID, ID);
    }
}