using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;  // ΩÃ±€≈Ê

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(gameObject);
    }

    public List<Item> items = new List<Item>();
    public int capacity = 20;

    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChangedCallback;

    public bool Add(Item item)
    {
        if (items.Count >= capacity)
        {
            Debug.Log("¿Œ∫•≈‰∏Æ∞° ∞°µÊ √°Ω¿¥œ¥Ÿ.");
            return false;
        }

        items.Add(item);
        onInventoryChangedCallback?.Invoke();
        return true;
    }

    public void Remove(Item item)
    {
        items.Remove(item);
        onInventoryChangedCallback?.Invoke();
    }
}
