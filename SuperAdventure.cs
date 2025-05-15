using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        public SuperAdventure()
        {
            InitializeComponent();

            _player = new Player(10, 10, 20, 0, 1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.AddItemToInventory(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1);

            UpdateUI();
        }

        private void UpdateUI()
        {
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
            UpdateWeaponListInUI();
            UpdateQuestListInUI();
        }

        private void SuperAdventure_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void control_Click(object sender, EventArgs e)
        {

        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        public void CheckForLevelUp()
        {
            int expForNextLevel = _player.Level * 5;
            if (_player.ExperiencePoints > expForNextLevel)
            {
                _player.Level += 1;
                _player.ExperiencePoints -= expForNextLevel;
                SendMessage(string.Format("Level Up! You are now level {0}", _player.Level));
                int maxHPInc = RNG.RandomNumberBetween(2, 4);
                _player.MaximumHitPoints += maxHPInc;
                _player.CurrentHitPoints += maxHPInc;
                SendMessage(string.Format("You gain {0} Maximum HP!", maxHPInc));
                SendMessage(string.Format("Gain {0} EXP to level up again.", _player.Level * 3));
                UpdateUI();

            }
        }

        public void SendMessage(string message)
        {
            rtbMessages.Text += message + "\n";
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;
            int damageToDeal = RNG.RandomNumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);
            _currentMonster.CurrentHitPoints -= damageToDeal;
            SendMessage(string.Format("You deal {0} damage to {1}!", damageToDeal, _currentMonster.Name));
            if (_currentMonster.CurrentHitPoints <= 0)
            {
                SendMessage(string.Format("You have defeated {0}!", _currentMonster.Name));
                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                SendMessage(string.Format("You gain {0} Experience Points!", _currentMonster.RewardExperiencePoints));
                _player.Gold += _currentMonster.RewardGold;
                SendMessage(string.Format("You gain {0} Gold!", _currentMonster.RewardGold));
                foreach (LootItem lootItem in _currentMonster.LootTable)
                {
                    if (RNG.RandomNumberBetween(0, 99) <= lootItem.DropPercentage)
                    {
                        Item itemToAdd = World.ItemByID(lootItem.Details.ID);
                        _player.AddItemToInventory(itemToAdd, 1);
                        SendMessage(string.Format("You loot {0}!", itemToAdd.Name));
                    }

                }
                CheckForLevelUp();

                UpdateUI();

                MoveTo(_player.CurrentLocation);
            }
            else
            {
                MonsterAttack();
            }

        }

        private void MonsterAttack()
        {
            int incomingDamage = RNG.RandomNumberBetween(0, _currentMonster.MaximumDamage);
            SendMessage(string.Format("{0} deals {1} damage to you!", _currentMonster.Name, incomingDamage));
            _player.CurrentHitPoints -= incomingDamage;
            if (_player.CurrentHitPoints <= 0)
            {
                SendMessage("Oh no! You lost all your HP! You gotta get out of here!");
                SendMessage("On the way home, you dropped half your gold.");
                _player.Gold = _player.Gold / 2;
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }
            else
            {
                UpdateUI();
            }
        }
        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            HealingPotion currentPotion = (HealingPotion)cboPotions.SelectedItem;
            int amountToHeal = currentPotion.AmountToHeal;
            int playerMissingHealth = _player.MaximumHitPoints - _player.CurrentHitPoints;
            amountToHeal = Math.Min(amountToHeal, playerMissingHealth);
            _player.CurrentHitPoints += amountToHeal;
            SendMessage(string.Format("You quaff a healing potion! You healed for {0} health!", amountToHeal));
            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details.ID == currentPotion.ID)
                {
                    inventoryItem.Quantity -= 1;
                    SendMessage(string.Format("You have {0} Healing Potions remaining.", inventoryItem.Quantity));
                    break;
                }
            }
            UpdateUI();
            MonsterAttack();

        }

        private void MoveTo(Location newLocation)
        {
            if (!_player.PlayerCanEnterLocation(newLocation))
            {
                SendMessage(string.Format("You must have a {0} to enter here!", newLocation.ItemRequiredToEnter));
                return;
            }

            // Update player's current location
            _player.CurrentLocation = newLocation;

            // Show/hide available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnWest.Visible = (newLocation.LocationToWest != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);

            // Display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //Completely heal the player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            // Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                //If the player already has the quest:
                if (_player.HasQuest(newLocation.QuestAvailableHere))
                {
                    //If they haven't completed it...
                    if (!_player.QuestCompleted(newLocation.QuestAvailableHere))
                    {

                        if (_player.CanCompleteQuest(newLocation.QuestAvailableHere))
                        {
                            rtbMessages.Text += "\n";
                            SendMessage(string.Format("You complete the {0} quest!", newLocation.QuestAvailableHere.Name));

                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            // Grant quest rewards

                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                            SendMessage(string.Format("You receive: "));
                            SendMessage(string.Format("{0} EXP!", newLocation.QuestAvailableHere.RewardExperiencePoints));
                            SendMessage(string.Format("{0} Gold!", newLocation.QuestAvailableHere.RewardGold));
                            SendMessage(string.Format("{0}!", newLocation.QuestAvailableHere.RewardItem.Name));
                            rtbMessages.Text += Environment.NewLine;

                            _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            //Add reward item to player's inventory

                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem, 1);

                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                            CheckForLevelUp();

                        }
                        else
                        {
                            foreach (QuestCompletionItem questCompletionItem in newLocation.QuestAvailableHere.QuestCompletionItems)
                            {
                                int quantityNeeded = questCompletionItem.Quantity;
                                foreach (InventoryItem item in _player.Inventory)
                                {
                                    if (item.Details.ID == questCompletionItem.Details.ID)
                                    {
                                        quantityNeeded -= item.Quantity;
                                    }
                                }

                                rtbMessages.Text += "Not enough " + questCompletionItem.Details.NamePlural + "!" + Environment.NewLine;
                                rtbMessages.Text += "You need " + quantityNeeded.ToString() + " more." + Environment.NewLine;
                            }
                        }
                    }
                }
                else
                {
                    // Player doesn't already have quest

                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest!" + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    foreach (QuestCompletionItem questCompletionItem in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (questCompletionItem.Quantity == 1)
                        {
                            rtbMessages.Text += questCompletionItem.Quantity.ToString() + " " + questCompletionItem.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text = questCompletionItem.Quantity.ToString() + " " + questCompletionItem.Details.NamePlural + Environment.NewLine;
                        }
                    }
                    rtbMessages.Text += Environment.NewLine;

                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }
            // Does the location have a monster?

            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                // Make a new instance of the monster

                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUsePotion.Visible = true;
                btnUseWeapon.Visible = true;

            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
                btnUseWeapon.Visible = false;
            }
            UpdateUI();




        }

        private void UpdateInventoryListInUI()
        {
            dgvInentory.RowHeadersVisible = false;
            dgvInentory.ColumnCount = 2;
            dgvInentory.Columns[0].Name = "Name";
            dgvInentory.Columns[0].Width = 197;
            dgvInentory.Columns[1].Name = "Quantity";
            dgvInentory.Rows.Clear();
            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInentory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }
        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;
            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Complete?";
            dgvQuests.Rows.Clear();
            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });

            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();
            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count > 0)
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";
                cboWeapons.SelectedIndex = 0;
            }
            else
            {
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
        }
        private void UpdatePotionListInUI()
        {
            List<HealingPotion> potions = new List<HealingPotion>();
            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        potions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (potions.Count > 0)
            {
                cboPotions.DataSource = potions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";
                cboPotions.SelectedIndex = 0;
            }
            else
            {
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
        }

        private void rtbLocation_TextChanged(object sender, EventArgs e)
        {

        }

        private void rtbMessages_TextChanged(object sender, EventArgs e)
        {

        }

        private void cboPotions_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

