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
    
    void StateTransition()
    {
        switch (_state)
        {
            case States.Idle:
                if (Input.GetKeyDown(Inputs["GrabUp"]))
                {
                    _img.sprite = character.sprites[2]; // grab up sprite
                    StartIngredientGrab(_id == 0 ? GameManager.Instance.upItemA : GameManager.Instance.upItemB);
                }
                else if (Input.GetKeyDown(Inputs["GrabRight"]))
                {
                    _img.sprite = character.sprites[3]; // grab right sprite
                    StartIngredientGrab(_id == 0 ? GameManager.Instance.rightItemA : GameManager.Instance.rightItemB);
                }
                else if (Input.GetKeyDown(Inputs["GrabLeft"]))
                {
                    _img.sprite = character.sprites[4]; // grab left sprite
                    StartIngredientGrab(_id == 0 ? GameManager.Instance.leftItemA : GameManager.Instance.leftItemB);
                }
                else if (Input.GetKeyDown(Inputs["Mix"]))
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
                break;
            case States.Grabbing:
                if (_grabTimer <= 0f) { _state = States.Idle; }
                break;
            case States.Pouring:
                if (Input.GetKeyUp(Inputs["Mix"]))
                {
                    EndPourEvent();
                }
                break;
            case States.Stirring:
                break;
            case States.Harvesting:
                break;
        }
    }

    private void StartPourEvent()
    {
        _state = States.Pouring;
    }

    private void EndPourEvent()
    {
        _state = States.Idle;
    }

    private void StartStirEvent()
    {
        
        
        _state = States.Stirring;
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
                switch (_item.GetItemType())
                {
                    case IngredientItem.ItemType.Frog:
                        _frogPortion += Time.deltaTime;
                        frame_t = _frogPortion % (GameManager.MixFrameMax * 2f);
                        frame_i = 12;
                        if (_frogPortion >= 5f) EndPourEvent();
                        break;
                    case IngredientItem.ItemType.Heart:
                        _heartPortion += Time.deltaTime;
                        frame_t = _heartPortion % (GameManager.MixFrameMax * 2f);
                        frame_i = 9;
                        if (_heartPortion >= 5f) EndPourEvent();
                        break;
                    case IngredientItem.ItemType.Raven:
                        _ravenPortion += Time.deltaTime;
                        frame_t = _ravenPortion % (GameManager.MixFrameMax * 2f);
                        frame_i = 6;
                        if (_ravenPortion >= 5f) EndPourEvent();
                        break;
                }

                frame_i = (frame_t / (GameManager.MixFrameMax * 2f) > 0.5f) ? frame_i + 1 : frame_i;
                _img.sprite = character.sprites[frame_i];
                
                break;
            case States.Stirring:
                break;
            case States.Harvesting:
                break;
        }
        
        StateTransition();
        
        /*
        //WASD - Ingredients
        if (Input.GetKeyDown(KeyCode.W))
        {
            // UnityEngine.Debug.Log("Pressed W");
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            UnityEngine.Debug.Log("Stopped Pressing W");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            UnityEngine.Debug.Log("Pressed A");
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            UnityEngine.Debug.Log("Stopped Pressing A");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            UnityEngine.Debug.Log("Pressed D");
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            UnityEngine.Debug.Log("Stopped Pressing D");
        }

        if (Input.GetKey(KeyCode.S))
        {
            UnityEngine.Debug.Log("Holding S");
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            UnityEngine.Debug.Log("Stopped Holding S");
        }

        //IJKL - Ingredients
        if (Input.GetKeyDown(KeyCode.I))
        {
            UnityEngine.Debug.Log("Pressed I");
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            UnityEngine.Debug.Log("Stopped Pressing I");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            UnityEngine.Debug.Log("Pressed J");
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            UnityEngine.Debug.Log("Stopped Pressing J");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            UnityEngine.Debug.Log("Pressed L");
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            UnityEngine.Debug.Log("Stopped Pressing L");
        }

        if (Input.GetKey(KeyCode.K))
        {
            UnityEngine.Debug.Log("Holding K");
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            UnityEngine.Debug.Log("Stopped Holding K");
        }

        //Harvesting
        if (Input.GetKeyDown(KeyCode.E))
        {
            UnityEngine.Debug.Log("Pressed E");
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            UnityEngine.Debug.Log("Stopped Pressing E");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            UnityEngine.Debug.Log("Pressed O");
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            UnityEngine.Debug.Log("Stopped Pressing O");
        }

        //Spinning
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.Debug.Log("Pressed R");
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            UnityEngine.Debug.Log("Stopped Pressing R");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            UnityEngine.Debug.Log("Pressed P");
        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            UnityEngine.Debug.Log("Stopped Pressing P");
        }

        */
    }
    
    public void SetID(int id) { _id = id; }
} 
