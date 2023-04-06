using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySystem : MonoBehaviour {

    public int maxInventorySize = 3; // the maximum number of items the player can carry
    public List<GameObject> inventory; // the list of items in the player's inventory
    public Image[] sprites;
    public TMP_Text[] texts;
    // Start is called before the first frame update
    void Start() {
        inventory = new List<GameObject>();
    }

    // Add an item to the inventory
    public void AddItem(GameObject item) {
        if (inventory.Count < maxInventorySize) {
            inventory.Add(item);
            // update the most recent empty image on canvas to the sprite of the item find by tag
            // also update the text on its bottom to the type of powerup
            if (inventory.Count == 1) {
                sprites[0].sprite = item.GetComponent<PowerupScript>().sprite;
                texts[0].text = item.GetComponent<PowerupScript>().type.ToString();
            } else if (inventory.Count == 2) {
                sprites[1].sprite = item.GetComponent<PowerupScript>().sprite;
                texts[1].text = item.GetComponent<PowerupScript>().type.ToString();
            } else if (inventory.Count == 3) {
                sprites[2].sprite = item.GetComponent<PowerupScript>().sprite;
                texts[2].text = item.GetComponent<PowerupScript>().type.ToString();
            }
            // Notify other scripts that the inventory has been updated
            SendMessage("OnInventoryUpdated", SendMessageOptions.DontRequireReceiver);
        } else {
            Debug.LogWarning("Inventory is full.");
        }
    }

    // Remove an item from the inventory
    public void RemoveItem(GameObject item) {
        inventory.Remove(item);
        // Notify other scripts that the inventory has been updated
        SendMessage("OnInventoryUpdated", SendMessageOptions.DontRequireReceiver);
    }

    // Check if an item is in the inventory
    public bool HasItem(GameObject item) {
        return inventory.Contains(item);
    }

    // Get the list of items in the inventory
    public List<GameObject> GetInventory() {
        return inventory;
    }
}