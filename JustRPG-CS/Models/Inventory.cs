namespace JustRPG.Classes;

public class Inventory
{
    public string id { get; set; }
    public string interactionType { get; set; } = "info";
    public int currentPage { get; set; } = -1;
    public string[] items { get; set; } = Array.Empty<string>();
    public string?[] currentPageItems { get; set; } = { null , null, null, null, null };
    public int lastPage { get; set; } = 0;

    public void NextPage()
    {
        if (currentPage >= lastPage)
        {
            currentPage = lastPage;
            return;
        }

        currentPage++;
        int lastItem = (currentPage + 1) * 5 > items.Length ? items.Length : (currentPage + 1) * 5-1;
        Array.Fill(currentPageItems, null);
        for (int i = 0,itemIndex = currentPage*5-1; itemIndex < lastItem; i++, itemIndex++)
        {
            currentPageItems[i] = items[itemIndex];
        }
        
    }

    public void PrewPage()
    {
        if (currentPage == 0)
            return;

        currentPage--;
        int lastItem = (currentPage + 1) * 5 > items.Length ? items.Length : (currentPage + 1) * 5-1;
        Array.Fill(currentPageItems, null);
        for (int i = 0,itemIndex = currentPage*5; itemIndex < lastItem; i++, itemIndex++)
        {
            currentPageItems[i] = items[itemIndex];
        }
    }

    public void Reload(string[] inventory)
    {
        interactionType = "info";
        currentPage = 0;
        items = inventory;
        
        Array.Fill(currentPageItems, null);
        for (int i = 0; i < (items.Length > 5 ? 5 : items.Length); i++)
        {
            currentPageItems[i] = items[i];
        }
    }

    public Item?[] GetItems(DataBase dataBase)
    {
        Item?[] getItems = {(Item?)null, (Item?)null, (Item?)null, (Item?)null, (Item?)null };
        for (int i = 0; i < currentPageItems.Length; i++)
        {
            if (currentPageItems[i] == null)
                break;

            getItems[i] = (Item)dataBase.GetFromDataBase(Bases.Items, "id", currentPageItems[i]!)!;
        }

        return getItems;
    }
}
