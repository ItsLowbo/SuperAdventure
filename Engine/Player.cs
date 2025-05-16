using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Player : LivingCreature
    {
     

        public int Gold {  get; set; }
        public int ExperiencePoints { get; set; }
        public int Level { get; set; }

        public int ManaMax { get; set; }

        public int ManaCurrent { get; set; }

        public List<InventoryItem> Inventory {  get; set; }

        public List<PlayerQuest> Quests { get; set; }

        public Location CurrentLocation {  get; set; }

        public Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints, int level, int manaMax = 0, int manaCurrent = 0) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            Level = level;
            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
            ManaMax = manaMax;
            ManaCurrent = manaCurrent;
        }
        public bool PlayerCanEnterLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                return true;
            }

            return Inventory.Exists(i => i.Details.ID == location.ItemRequiredToEnter.ID);

        }

        public bool HasQuest(Quest quest)
        {
            return Quests.Exists(q => q.Details.ID == quest.ID);
        }

        public bool QuestCompleted(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests)
            {
                if (playerQuest.Details.ID == quest.ID)
                {
                    return playerQuest.IsCompleted;
                }
            }
            return false;
        }

        public bool CanCompleteQuest(Quest quest)
        {
            foreach (QuestCompletionItem completionItem in quest.QuestCompletionItems)
            {
                if(!Inventory.Exists(i => i.Details.ID == completionItem.Details.ID && i.Quantity >= completionItem.Quantity))
                {
                    return false;
                }
            }
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach(QuestCompletionItem questCompletionItem in quest.QuestCompletionItems)
            {
                InventoryItem item = Inventory.FirstOrDefault(i => i.Details.ID == questCompletionItem.Details.ID);
                if(item == null)
                {
                    item.Quantity -= questCompletionItem.Quantity;
                }
            }
        }
        public void AddItemToInventory(Item itemToAdd, int quantity)
        {
            InventoryItem item = Inventory.FirstOrDefault(i => i.Details.ID == itemToAdd.ID);
            if(item != null)
            {
                item.Quantity += quantity;
                return;
            }
            Inventory.Add(new InventoryItem(itemToAdd, quantity));
        }



        public void MarkQuestCompleted(Quest quest)
        {
            PlayerQuest playerQuest = Quests.FirstOrDefault(q => q.Details.ID == quest.ID);
            if (playerQuest != null)
            {
                playerQuest.IsCompleted = true;
                return;
            }
        }
    }
    
}
