using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Engine
{
    public class Player : LivingCreature
    {
     

        public int Gold {  get; set; }
        public int ExperiencePoints { get; private set; }
        public int Level { get; private set; }
        public int ExpForNextLevel { get; private set; }

        public int ManaMax { get; private set; }

        public int ManaCurrent { get; set; }

        public List<InventoryItem> Inventory {  get; set; }

        public List<PlayerQuest> Quests { get; set; }

        public Location CurrentLocation {  get; set; }

        public Weapon CurrentWeapon { get; set; }

        public delegate void LevelUpHandler(int newLevel, int maxHPInc, int maxManaInc, int newExpForNextLevel, string learnedSpellName);

        public event LevelUpHandler PlayerLevelUp;

        private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints, int level, int manaMax, int manaCurrent) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            Level = level;
            ExpForNextLevel = level * 5;
            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
            ManaMax = manaMax;
            ManaCurrent = manaCurrent;
        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 0, 0, 1, 0, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);
            return player;
        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();              
                playerData.LoadXml(xmlPlayerData);

                //Stats
                int currentHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);
                int level = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Level").InnerText);
                int expForNextLevel = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExpForNextLevel").InnerText);
                int manaMax = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumMana").InnerText);
                int manaCurrent = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentMana").InnerText);

                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints, level, manaMax, manaCurrent);

                player.ExpForNextLevel = expForNextLevel;

                int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocationID").InnerText);

                player.CurrentLocation = World.LocationByID(currentLocationID);

                if (playerData.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponID);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    player.AddItemToInventory(World.ItemByID(id), quantity);
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool completed = Convert.ToBoolean(node.Attributes["Completed"].Value);

                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = completed;
                    player.Quests.Add(playerQuest);
                }

                return player;

            }

            catch
            {
                // If there was an error with the XML data, return a default player
                return Player.CreateDefaultPlayer();
            }
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


        public void AddExperiencePoints(int exptoadd)
        {
            ExperiencePoints += exptoadd;
            if (ExperiencePoints >= ExpForNextLevel)
            {
                Level += 1;
                ExperiencePoints -= ExpForNextLevel;
                ExpForNextLevel = Level * 5;
                int maxHPInc = RNG.RandomNumberBetween(2, 4);
                MaximumHitPoints += maxHPInc;
                CurrentHitPoints += maxHPInc;
                int maxManaInc = RNG.RandomNumberBetween(4, 8);
                ManaMax += maxManaInc;
                ManaCurrent += maxManaInc;
                string newSpellName = null;
                foreach (Item item in World.Items)
                {
                    Spell spell = item as Spell;
                    if (spell != null)
                    {
                        if (Level >= spell.LevelRequirement && !spell.Learned)
                        {
                            AddItemToInventory(World.ItemByID(spell.ID), 1);
                            spell.Learned = true;
                            newSpellName = spell.Name;
                        }
                    }
                }
                PlayerLevelUp.Invoke(Level, maxHPInc, maxManaInc, ExpForNextLevel, newSpellName);
            }
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

        public string ToXMLString()
        {
            XmlDocument playerData = new XmlDocument();
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            // Stats
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);


            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(this.CurrentHitPoints.ToString())); 
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(this.MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode currentMana = playerData.CreateElement("CurrentMana");
            currentMana.AppendChild(playerData.CreateTextNode(this.ManaCurrent.ToString()));
            stats.AppendChild(currentMana);

            XmlNode maximumMana = playerData.CreateElement("MaximumMana");
            maximumMana.AppendChild(playerData.CreateTextNode(this.ManaMax.ToString()));
            stats.AppendChild(maximumMana);

            XmlNode level = playerData.CreateElement("Level");
            level.AppendChild(playerData.CreateTextNode(this.Level.ToString()));
            stats.AppendChild(level);

            XmlNode expForNextLevel = playerData.CreateElement("ExpForNextLevel");
            expForNextLevel.AppendChild(playerData.CreateTextNode(this.ExpForNextLevel.ToString()));
            stats.AppendChild(expForNextLevel);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(this.Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(this.ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);

            XmlNode currentLocation = playerData.CreateElement("CurrentLocationID");
            currentLocation.AppendChild(playerData.CreateTextNode(this.CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            if(CurrentWeapon != null)
            {
                XmlNode currentWeapon = playerData.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(playerData.CreateTextNode(this.CurrentWeapon.ID.ToString()));
                stats.AppendChild(currentWeapon);
            }

            //Inventory Items
            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            foreach(InventoryItem item in this.Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = item.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = item.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);
                inventoryItems.AppendChild(inventoryItem);
            }

            // Quests
            XmlNode playerQuests = playerData.CreateElement("Quests");
            player.AppendChild(playerQuests);

            foreach (PlayerQuest quest in this.Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("Quest");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = quest.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);

                XmlAttribute completedAttribute = playerData.CreateAttribute("Completed");
                completedAttribute.Value = quest.IsCompleted.ToString();
                playerQuest.Attributes.Append(completedAttribute);

                playerQuests.AppendChild(playerQuest);

            }

            return playerData.InnerXml;
        }
    }
    
}
