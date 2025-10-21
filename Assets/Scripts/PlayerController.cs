using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private int _id; // dictates which player this controller is. 0: left, 1: right
    // note: very ungraceful way of referencing certain objects in GameManager ^^^
    private IngredientItem _nextIngredient;

    // player state timers
    private float _grabTimer = 0f;

    private float _frogPortion = 0f;
    private float _ravenPortion = 0f;
    private float _heartPortion = 0f;
    private float _stirProgress = 0f;
    
    // inputs initialized by gamemanager
    public Dictionary<String, KeyCode> Inputs;
    public CharacterSprite character; // scriptable object containing every frame from spritesheet

    private Image _img;

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
                ActiveActionInput();
                break;
            case States.Grabbing:
                if (_grabTimer <= 0f) { _state = States.Idle; }
                break;
            case States.Pouring:
                activeActionPerformed = ActiveActionInput();
                if (activeActionPerformed) EndPourEvent();
                break;
            case States.Stirring:
                activeActionPerformed = ActiveActionInput();
                if (activeActionPerformed) EndStirEvent();
                break;
            case States.Harvesting:
                break;
        }
    }

    // checking if a new active action is occurring (grabbing, harvesting, or mixing)
    // active actions are ones that interrupt the state of idle or stirring
    bool ActiveActionInput()
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
            activeActionPerformed = true;
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
                break;
        }
        
        if (!activelyStirring) _stirProgress -= Time.deltaTime * 5f;
        _stirProgress = Mathf.Clamp(_stirProgress, 0f, 100f);
        GameManager.Instance.SetStirMeter(_id, _stirProgress);
    }

    private void StartPourEvent()
    {
        _state = States.Pouring;
        GameManager.Instance.ShowPourMeter(_id, _item.GetItemType());
    }

    private void EndPourEvent()
    {
        // do not set state to idle, EndPourEvent() always called after state -> idle due to active action
        GameManager.Instance.HidePourMeter(_id);
    }

    private void StartStirEvent()
    {
        _state = States.Stirring;
        GameManager.Instance.ShowMetronome(_id);
    }

    private void EndStirEvent()
    {
        // do not set state to idle, EndStirEvent() always called after state -> idle due to active action
        GameManager.Instance.HideMetronome(_id);
    }
    
    private void StartIngredientGrab(IngredientItem ingredient)
    {
        _nextIngredient = ingredient;
        _grabTimer = GameManager.GrabTimerMax;
        
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
        StateUpdate();
        StateTransition();
    }
    
    public void SetID(int id) { _id = id; }
} 
