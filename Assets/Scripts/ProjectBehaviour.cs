using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Models;

public class ProjectBehaviour : MonoBehaviour
{
    public static bool PauseGame = false;
    public static bool InventoryOpen = false;

    public static event EventHandler OnPauseGame;
    public static event EventHandler OnPlayGame;

    public static event EventHandler OnInventoryOpened;
    public static event EventHandler OnInventoryClosed;

    public static void RunOnPauseGame(object sender)
    {
        Time.timeScale = 0;
        OnPauseGame?.Invoke(sender, EventArgs.Empty);
    }

    public static void RunOnPlayGame(object sender)
    {
        Time.timeScale = 1;
        OnPlayGame?.Invoke(sender, EventArgs.Empty);
    }

    public static void RunOnInventoryOpened(object sender)
    {
        OnInventoryOpened?.Invoke(sender, EventArgs.Empty);
    }

    public static void RunOnInventoryClosed(object sender)
    {
        OnInventoryClosed?.Invoke(sender, EventArgs.Empty);
    }

    public static GameManager GameManager;
    public static GameSettings GameSettings = new GameSettings();
}
