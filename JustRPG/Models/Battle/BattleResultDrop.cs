using JustRPG.Services;

namespace JustRPG.Models;

public class BattleResultDrop : PagedResult<Item>
{
    public string id { get; set; }
    public long userId { get; set; }
    public DateTime endTime { get; set; } = DateTime.Now.AddMinutes(5);

    public List<int> selectedItems { get; set; } = new List<int>();

    public BattleResultDrop(User user, List<Item> drop)
    {
        id = Guid.NewGuid().ToString().Split('-')[0];
        Items = drop;
        this.userId = user.id;
    }

    public void SelectItem(int index)
    {
       if (selectedItems.Contains(index))
       {
           selectedItems.Remove(index);
       }
       else
       {
           selectedItems.Add(index);
       }
    }

    public void SelectAll()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (!selectedItems.Contains(i))
            {
                selectedItems.Add(i);
            }
        }
    }
    
    public void DeselectAll()
    {
        selectedItems.Clear();
    }
    
    public async Task<bool> GiveRewards(DataBase dataBase)
    {
        User userdb = (await dataBase.UserDb.Get(userId))!;
        
        if (userdb.inventory.Count + selectedItems.Count > 30)
        {
            return false;
        }

        List<Item> items = selectedItems.Select(i => Items[i]).ToList();

        userdb.inventory.AddRange(items.Select(x => x.id));
        
        await dataBase.UserDb.Update(userdb);
        return true;
    }
}