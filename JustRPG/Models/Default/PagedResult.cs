using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using Serilog;

namespace JustRPG.Models;

public class PagedResult<T>
{
    [BsonElement("current_page")] public int CurrentPage { get; set; } = 0;
    [BsonElement("current_item_index")] public int CurrentItemIndex { get; set; } = 0;
    [BsonElement("search_results")] public List<T> Items { get; set; } = new List<T>();

    public int GetCountOfPages()
    {
        return (int)Math.Ceiling((double)Items.Count / 5);
    }

    public bool IsLastPage()
    {
        return CurrentPage == GetCountOfPages() - 1;
    }

    public void IncrementPage()
    {
        int nextPage = CurrentPage + 1;
        List<T> itemsOnNextPage = GetItemsOnPage(nextPage);

        if (IsLastPage() && CurrentItemIndex == GetItemsOnPage(CurrentPage).Count - 1)
        {
            CurrentPage = 0;
            CurrentItemIndex = 0;
            return;
        }
        
        if (itemsOnNextPage.Count > 0)
        {
            CurrentPage = nextPage;
            CurrentItemIndex = 0;
        }

        
    }

    private T?[] GetItems(int count = 5)
    {
        T?[] res = new T?[count];
        for (int i = 0; i < Items.Count; i++)
        {
            res[i] = Items[i];
        }

        return res;
    }

    public void DecrementPage()
    {
        switch (CurrentPage)
        {
            case > 0:
                CurrentPage--;
                break;
            case 0:
                CurrentPage = GetCountOfPages() - 1;
                break;
        }

        CurrentItemIndex = GetItemsOnPage(CurrentPage).Count - 1;
    }

    public void IncrementItemIndex()
    {
        List<T> itemsOnCurrentPage = GetItemsOnPage(CurrentPage);

        if (CurrentItemIndex < itemsOnCurrentPage.Count - 1)
            CurrentItemIndex++;
        else if (CurrentItemIndex == itemsOnCurrentPage.Count - 1)
            IncrementPage();
    }

    public void DecrementItemIndex()
    {
        switch (CurrentItemIndex)
        {
            case > 0:
                CurrentItemIndex--;
                break;
            case 0:
                DecrementPage();
                break;
        }
    }
    

    public List<T> GetItemsOnPage(int pageIndex)
    {
        return Items.Skip(5 * pageIndex).Take(5).ToList();
    }
}