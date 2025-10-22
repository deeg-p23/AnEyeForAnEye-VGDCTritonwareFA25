using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class PlayerController : MonoBehaviour
{
    enum States
    {
        Idle,
        Grabbing,
        Stirring,
        Pouring,
        Harvesting
    }

    private States _state;
    private IngredientItem _item;
    private Recipe _recipe;
    private int _id; // dictates which player this controller is. 0: left, 1: right
    // note: very ungraceful way of referencing certain objects in GameManager ^^^
    private IngredientItem _nextIngredient;
    
    public CalloutManager cm;
    
    private bool _evilPunished = false;
    private float _spinCooldown = 0f;

    // player state timers
    private float _grabTimer = 0f;
    private float _grabTimerMax = 0.1f;

    private float _frogPortion = 0f;
    private float _ravenPortion = 0f;
    private float _heartPortion = 0f;
    private float _stirProgress = 0f;
    private float _eyeplotGrowth = 0f; // [0,9] by seconds elapsed while not harvesting
    private float _harvestProgress = 0f; // [0,1] by frames pressed while harvesting
    
    // key counters
    private int _totalEyes = 0;
    private int _totalScore = 0;
    
    // inputs initialized by gamemanager
    public Dictionary<String, KeyCode> Inputs;
    public CharacterSprite character; // scriptable object containing every frame from spritesheet

    private Image _img;
    public Slider frogRecipeFill;
    public Slider ravenRecipeFill;
    public Slider heartRecipeFill;

    void Start()
    {
        _state = States.Idle;
        _item = null;
        _img = GetComponent<Image>();
    }
    
    // update player state based on current state and action input
    void StateTransition()
    {
        bool activeActionPerformed = false;
        switch (_state)
        {
            case States.Idle:
                ActiveActionChange();
                break;
            case States.Grabbing:
                if (_grabTimer <= 0f) { _state = States.Idle; }
                break;
            case States.Pouring:
                activeActionPerformed = ActiveActionChange();
                if (activeActionPerformed) EndPourEvent(); // cancelled pour
                break;
            case States.Stirring:
                activeActionPerformed = ActiveActionChange();
                if (activeActionPerformed) EndStirEvent(false); // cancelled stir
                break;
            case States.Harvesting:
                activeActionPerformed = ActiveActionChange();
                if (activeActionPerformed) EndHarvestEvent(false); // cancelled harvest (incomplete)
                break;
        }
    }

    // checking if a new active action is occurring (grabbing, harvesting, or mixing)
    // active actions are ones that interrupt the state of idle or stirring
    bool ActiveActionChange()
    {
        bool activeActionPerformed = false;
        if (Input.GetKeyDown(Inputs["GrabUp"]))
        {
            activeActionPerformed = true;
            _img.sprite = character.sprites[2]; // grab up sprite
            StartIngredientGrab(_id == 0 ? GameManager.Instance.upItemA : GameManager.Instance.upItemB);
        }
        else if (Input.GetKeyDown(Inputs["GrabRight"]))
        {
            activeActionPerformed = true;
            _img.sprite = character.sprites[3]; // grab right sprite
            StartIngredientGrab(_id == 0 ? GameManager.Instance.rightItemA : GameManager.Instance.rightItemB);
        }
        else if (Input.GetKeyDown(Inputs["GrabLeft"]))
        {
            activeActionPerformed = true;
            _img.sprite = character.sprites[4]; // grab left sprite
            StartIngredientGrab(_id == 0 ? GameManager.Instance.leftItemA : GameManager.Instance.leftItemB);
        }
        else if (Input.GetKeyDown(Inputs["Mix"]))
        {
            if (_state != States.Stirring && _state != States.Pouring) // ignore redoing stir state
            {
                if (_item)
                {
                    bool canPourMore = true;
                    switch (_item.GetItemType())
                    {
                        case IngredientItem.ItemType.Frog:
                            canPourMore = _frogPortion < 5f;
                            break;
                        case IngredientItem.ItemType.Heart:
                            canPourMore = _heartPortion < 5f;
                            break;
                        case IngredientItem.ItemType.Raven:
                            canPourMore = _ravenPortion < 5f;
                            break;
                    }
                    if (canPourMore) StartPourEvent();
                }
                else StartStirEvent();
            }
        }
        else if (Input.GetKeyDown(Inputs["Harvest"]))
        {
            if (_state != States.Harvesting)
            {
                activeActionPerformed = true; // may need to be placed within this next state check conditional?
                StartHarvestEvent();
            }
        }

        return activeActionPerformed;
    }

    // update player + game based on current state
    void StateUpdate()
    {
        bool activelyStirring = false; // determines whether or not stir progress should deplete from inactivity
        switch (_state)
        {
            case States.Idle:
                // idle sprite frame
                if (_item)
                {
                    switch (_item.GetItemType())
                    {
                        case IngredientItem.ItemType.Raven:
                            _img.sprite = character.sprites[5];
                            break;
                        case IngredientItem.ItemType.Heart:
                            _img.sprite = character.sprites[8];
                            break;
                        case IngredientItem.ItemType.Frog:
                            _img.sprite = character.sprites[11];
                            break;
                    }
                }
                else
                {
                    _img.sprite = character.sprites[0];
                }
                break;
            case States.Grabbing:
                _grabTimer -= Time.deltaTime;
                if (_grabTimer <= 0f)
                {
                    CompleteIngredientGrab();
                }
                break;
            case States.Pouring:
                float frame_t = 0f;
                int frame_i = 0;
                float display_portion = 0f;
                
                // inactively holding ingredient:
                switch (_item.GetItemType())
                {
                    case IngredientItem.ItemType.Frog:
                        frame_i = 11;
                        display_portion = _frogPortion;
                        break;
                    case IngredientItem.ItemType.Heart:
                        frame_i = 8;
                        display_portion = _heartPortion;
                        break;
                    case IngredientItem.ItemType.Raven:
                        frame_i = 5;
                        display_portion = _ravenPortion;
                        break;
                }
                
                // actively pouring ingredient
                if (Input.GetKey(Inputs["Mix"]))
                {
                    switch (_item.GetItemType())
                    {
                        case IngredientItem.ItemType.Frog:
                            if (_frogPortion < 5f)
                            {
                                _frogPortion += Time.deltaTime * 2.5f;
                                frame_t = _frogPortion % (GameManager.MixFrameMax * 2f);
                                frame_i = 12;
                            }
                            break;
                        case IngredientItem.ItemType.Heart:
                            if (_heartPortion < 5f)
                            {
                                _heartPortion += Time.deltaTime * 2.5f;
                                frame_t = _heartPortion % (GameManager.MixFrameMax * 2f);
                                frame_i = 9;
                            }
                            break;
                        case IngredientItem.ItemType.Raven:
                            if (_ravenPortion < 5f)
                            {
                                _ravenPortion += Time.deltaTime * 2.5f;
                                frame_t = _ravenPortion % (GameManager.MixFrameMax * 2f);
                                frame_i = 6;
                            }
                            break;
                    } 
                    frame_i = (frame_t / (GameManager.MixFrameMax * 2f) > 0.5f) ? frame_i + 1 : frame_i;
                }
                _img.sprite = character.sprites[frame_i];
                GameManager.Instance.SetPourMeter(_id, display_portion);
                break;
            case States.Stirring:
                activelyStirring = true;
                
                // stir animation
                if (Input.GetKey(Inputs["Mix"])) _img.sprite = character.sprites[1]; 
                else _img.sprite = character.sprites[0];
                
                // stir frame register
                float stirValue = 0f;
                if (Input.GetKeyDown(Inputs["Mix"])) stirValue = GameManager.Instance.RegisterStirTick(_id);
                _stirProgress += stirValue;
                
                break;
            case States.Harvesting:
                if (Input.GetKey(Inputs["Harvest"]))
                {
                    _img.sprite = character.sprites[14];
                    if (Input.GetKeyDown(Inputs["Harvest"]))
                    {
                        _harvestProgress += 0.1f;
                        GameManager.Instance.SetHarvestMeter(_id, _harvestProgress);
                    }
                }
                else _img.sprite = character.sprites[15];
                
                if (_harvestProgress >= 1f)
                {
                    EndHarvestEvent(true);
                }
                
                break;
        }
        
        // stir progress updating
        if (!activelyStirring) _stirProgress -= Time.deltaTime * 5f;
        _stirProgress = Mathf.Clamp(_stirProgress, 0f, _recipe.stirAmount);
        GameManager.Instance.SetStirMeter(_id, _stirProgress, _recipe.stirAmount);

        if (_stirProgress >= _recipe.stirAmount)
        {
            EndStirEvent(true);
        }
        
        // eyeplot updating
        if (_state != States.Harvesting)
        {
            _eyeplotGrowth += Time.deltaTime / (_evilPunished ? 2f : 1f);
            _eyeplotGrowth = Mathf.Clamp(_eyeplotGrowth, 0f, (_evilPunished ? 9f : 9f));
            GameManager.Instance.AnimateEyeplot(_id, _eyeplotGrowth);
        }
        
        // spin action
        if (Input.GetKeyDown(Inputs["Spin"]))
        {
            GameManager.Instance.SpinWheel(_id, _spinCooldown);
        }
        _spinCooldown = Mathf.Max(_spinCooldown - Time.deltaTime, 0f);
    }

    private void StartPourEvent()
    {
        _state = States.Pouring;
        GameManager.Instance.ShowPourMeter(_id, _item.GetItemType());
    }

    // protection: public to be forcibly called by game manager's swap ingredient sabotage
    public void EndPourEvent()
    {
        // do not set state to idle, EndPourEvent() always called after state -> idle due to active action
        GameManager.Instance.HidePourMeter(_id);
    }

    private void StartStirEvent()
    {
        _state = States.Stirring;
        GameManager.Instance.ShowMetronome(_id);
    }

    private void EndStirEvent(bool success)
    {
        // do not set state to idle if unsuccessful, EndStirEvent() always called after state -> idle due to active action
        GameManager.Instance.HideMetronome(_id);

        // however, if successful, set state to idle to end the event
        if (success) SubmitPotion();
    }

    private void SubmitPotion()
    {
        const float alpha = 0.69314718056f;
        const float gamma = 6f;

        float sA = CompareIngredientPortions(_recipe.frogPortion, _frogPortion, alpha, gamma);
        float sB = CompareIngredientPortions(_recipe.heartPortion, _heartPortion, alpha, gamma);
        float sC = CompareIngredientPortions(_recipe.ravenPortion, _ravenPortion, alpha, gamma);
        float raw = (sA + sB + sC) / 3f * 100f;

        int score = (int)Mathf.CeilToInt(raw);
        cm.SpawnCallout(_id, "Potion complete!\n+" + score + " score", CalloutManager.recipe);
        _totalScore += score;
        GameManager.Instance.SetScoreCounter(_id, _totalScore);
        
        // reset player states/values
        _state = States.Idle;
        _stirProgress = 0f;
        _frogPortion = 0f;
        _ravenPortion = 0f;
        _heartPortion = 0f;
        // go to next potion
        SetRecipe(GameManager.Instance.GenerateRecipe());
    }

    private float CompareIngredientPortions(float A, float a, float alpha, float gamma)
    {
        A = Mathf.Clamp(A, 0, 5);
        a = Mathf.Clamp(a, 0, 5);

        float denom = Mathf.Max(A, 1f);
        float r = Mathf.Abs(a - A) / denom;

        if (r <= 0.2f)
            return Mathf.Exp(-alpha * Mathf.Pow(r / 0.2f, 2));
        else
            return Mathf.Exp(-alpha) * Mathf.Exp(-gamma * (r - 0.2f));
    }

    private void StartHarvestEvent()
    {
        if (_eyeplotGrowth < (_evilPunished ? 3f : 3f)) return; // not enough growth to harvest
            
        _state = States.Harvesting;
        _harvestProgress = 0f;
        GameManager.Instance.ShowHarvestMeter(_id);
        GameManager.Instance.SetHarvestMeter(_id, _harvestProgress);
    }

    private void EndHarvestEvent(bool completed)
    {
        // do not set state to idle IF incomplete, EndHarvestEvent() always called after state -> idle due to active action
        // if complete, set to idle
        if (completed) _state = States.Idle;

        _harvestProgress = 0f;
        GameManager.Instance.HideHarvestMeter(_id);

        if (!completed) return;

        int plotRemainder = Mathf.FloorToInt(_eyeplotGrowth / (_evilPunished ? 3f : 3f));
        
        cm.SpawnCallout(_id, "+" + plotRemainder + " eyes", CalloutManager.eyeball);
        
        _totalEyes += plotRemainder;
        _eyeplotGrowth = Mathf.Clamp(_eyeplotGrowth - plotRemainder, 0f, (_evilPunished ? 1f : 1f));
        
        GameManager.Instance.SetEyeCounter(_id, _totalEyes);
    }
    
    private void StartIngredientGrab(IngredientItem ingredient)
    {
        _nextIngredient = ingredient;
        _grabTimer = _grabTimerMax;
        
        _state = States.Grabbing;
    }

    private void CompleteIngredientGrab()
    {
        // putting back ingredient
        if (_item == _nextIngredient)
        {
            _item.EnableImage();
            _item = null;
        }
        // replacing ingredients
        else if (_item != null && _nextIngredient != null)
        {
            _item.EnableImage();
            _nextIngredient.DisableImage();
            _item = _nextIngredient;
        }
        // get new ingredient
        else if (_item == null && _nextIngredient != null)
        {
            _nextIngredient.DisableImage();
            _item = _nextIngredient;
        }

        if (_item != null)
        {
            Debug.Log(_item.GetItemType());
        }
        else
        {
            Debug.Log(_item);
        }
        
        _state = States.Idle;
    }
    
    void Update()
    {
        if (GameManager.Instance.gameIsRunning)
        {
            StateUpdate();
            StateTransition();   
        }
    }
    
    public void SetID(int id) { _id = id; }

    public int GetTotalEyes()
    {
        return _totalEyes; 
    }

    public void SetTotalEyes(int value)
    {
        _totalEyes = value;
        GameManager.Instance.SetEyeCounter(_id, _totalEyes);
    }

    public void SetRecipe(Recipe recipe)
    {
        Debug.Log("WTF");
        _recipe = recipe;
        Debug.Log(_recipe);
        frogRecipeFill.value = recipe.frogPortion;
        heartRecipeFill.value = recipe.heartPortion;
        ravenRecipeFill.value = recipe.ravenPortion;
    }

    public void ForceIdleState()
    {
        _state = States.Idle;
    }

    public void SetItem(IngredientItem ingredientItem)
    {
        _item = ingredientItem;
    }
    
    public IngredientItem GetItem() { return _item; }
    
    public void SetGrabTimerMax(float value) { _grabTimerMax = value; }
    
    public void SetEvilPunished(bool value) { _evilPunished = value; }
    
    public void setSpinCooldown(float value) { _spinCooldown = value; }
} 
