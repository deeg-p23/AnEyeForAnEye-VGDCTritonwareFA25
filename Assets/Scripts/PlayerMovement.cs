using System.Diagnostics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //WASD - Ingredients 
        if (Input.GetKeyDown(KeyCode.W))
        {
            UnityEngine.Debug.Log("Pressed W");
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
    }
} 
