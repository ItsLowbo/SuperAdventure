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

            foreach (InventoryItem inventoryItem in Inventory) {
                if (inventoryItem.Details.ID == location.ItemRequiredToEnter.ID) 
                { 
                    return true; 
                }
            } 
            
            return false;

        }

        public bool HasQuest(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests)
            {
                if (playerQuest.Details.ID == quest.ID)
                {
                    return true;
                }
            }
            return false;
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
                foreach (InventoryItem inventoryItem in Inventory)
                {
                    if (inventoryItem.Details.ID == completionItem.Details.ID)
                    {
                        if (inventoryItem.Quantity == completionItem.Quantity)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach(QuestCompletionItem questCompletionItem in quest.QuestCompletionItems)
            {
                foreach(InventoryItem inventoryItem in Inventory)
                {
                    if(inventoryItem.Details.ID == questCompletionItem.Details.ID)
                    {
                        inventoryItem.Quantity -= questCompletionItem.Quantity;
                    }
                }
            }
        }
        public void AddItemToInventory(Item itemToAdd, int quantity)
        {
            foreach(InventoryItem inventoryItem in Inventory)
            {
                if(inventoryItem.Details.ID == itemToAdd.ID)
                {
                    inventoryItem.Quantity += quantity;
                    return;
                }
            }

            Inventory.Add(new InventoryItem(itemToAdd, quantity));
        }



        public void MarkQuestCompleted(Quest quest)
        {
            foreach(PlayerQuest playerQuest in Quests)
            {
                if(playerQuest.Details.ID == quest.ID)
                {
                    playerQuest.IsCompleted = true;
                    return;
                }
            }
        }
    }
    
}
