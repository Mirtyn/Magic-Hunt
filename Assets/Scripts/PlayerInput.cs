using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInput : ProjectBehaviour
{
    public static PlayerInput Instance;

    public event EventHandler Mouse0Pressed;
    public event EventHandler<AlphaKeyPressedEventArgs> AlphaKeyPressed;

    public class AlphaKeyPressedEventArgs : EventArgs
    {
        public KeyCode Key;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame = !PauseGame;

            if (PauseGame)
            {
                RunOnPauseGame(this);
            }
            else
            {
                RunOnPlayGame(this);
            }
        }

        if (!PauseGame)
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.I))
            {
                InventoryOpen = !InventoryOpen;

                if (InventoryOpen)
                {
                    RunOnInventoryOpened(this);
                }
                else
                {
                    RunOnInventoryClosed(this);
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Mouse0Pressed?.Invoke(this, EventArgs.Empty);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AlphaKeyPressed?.Invoke(this, new AlphaKeyPressedEventArgs { Key = KeyCode.Alpha1 });
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AlphaKeyPressed?.Invoke(this, new AlphaKeyPressedEventArgs { Key = KeyCode.Alpha2 });
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AlphaKeyPressed?.Invoke(this, new AlphaKeyPressedEventArgs { Key = KeyCode.Alpha3 });
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                AlphaKeyPressed?.Invoke(this, new AlphaKeyPressedEventArgs { Key = KeyCode.Alpha4 });
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                AlphaKeyPressed?.Invoke(this, new AlphaKeyPressedEventArgs { Key = KeyCode.Alpha5});
            }
        }
    }
}
