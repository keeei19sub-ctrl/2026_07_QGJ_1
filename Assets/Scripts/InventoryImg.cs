using UnityEngine;

public static class InventoryImg
{
    public static string ItemImg(string itemId)
    {
        switch (itemId)
        {
            case "king_medicine":
                return "item_cake";
            case "large_umbrella_potion":
                return "item_umbrella";
            case "stone_stop_charm":
                return "item_pan";
            case "none":
            default:
                return "Item";
        }
    }
}
