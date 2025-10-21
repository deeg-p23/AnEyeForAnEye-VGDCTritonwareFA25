using UnityEngine;

public class PlayerStates : MonoBehaviour
{
    public enum PlayerState
    {
        //default
        Idle,

        //Action
        Mixing, Spinning, Harvesting,

        //movement
        ReachUp, ReachLeft, ReachRight,

        //unsure if needed?
        Sabotaged
    }

    [SerializeField] private PlayerState _playerState;

    //Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerState = PlayerState.Idle; //the player starts in Idle State
    }

    void Update()
    {
        HandlePlayerState();
    }

    void HandlePlayerState()
    {
        switch (_playerState)
        {
            case PlayerState.Idle:
                //idle 
                break;

            case PlayerState.Mixing:
                //mixing (S/K)
                break;

            case PlayerState.ReachUp:
                //reaching up (W/I)
                break;

            case PlayerState.ReachLeft:
                //reaching left (A/J)
                break;

            case PlayerState.ReachRight:
                //reaching right (D/L)
                break;

            case PlayerState.Spinning:
                //spinning wheel (R/P)
                break;

            case PlayerState.Harvesting:
                //harvesting eyes (E/O)
                break;

            case PlayerState.Sabotaged:
                //unsure if needed, delete if not
                break;

            default:
                //also unsure if needed
                break;
        }
    }

    //extra, also unsure if needed:
    void ChangeState(PlayerState newState)
    {
        _playerState = newState;
        //logic for triggering animations or sounds here?
        //delete if not needed 
    }
}
