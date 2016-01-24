﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Starbounder.FileTypes.Armors
{
	class ArmorLegs
	{
		public string itemName { get; set; }
		public int price { get; set; }
		public string inventoryIcon { get; set; }
		public int maxStack { get; set; }
		public string rarity { get; set; }
		public string description { get; set; }
		public string shortdescription { get; set; }
		public string tooltipKind { get; set; }
		public string maleFrames { get; set; }
		public string femaleFrames { get; set; }
		public List<StatusEffect> statusEffects { get; set; }
		public List<object> colorOptions { get; set; }
		public List<string> learnBlueprintsOnPickup { get; set; }

		public class StatusEffect
		{
			public string stat { get; set; }
			public double baseMultiplier { get; set; }
			public int? amount { get; set; }
		}

		public ArmorLegs SetDefault()
		{
			this.itemName                = "Untitled Leggings";
			this.price                   = 0;
			this.inventoryIcon           = "untitled.png";
			this.maxStack                = 1;
			this.rarity                  = "common";
			this.description             = "A piece of equipment to protect your legs.";
			this.shortdescription        = "Legs Equipment";
			this.tooltipKind             = "armor";
			this.maleFrames              = "male.png";
			this.femaleFrames            = "female.png";
			this.statusEffects           = new List<StatusEffect>();
			this.colorOptions            = new List<object>();
			this.learnBlueprintsOnPickup = new List<string>();

			return this;
		}
	}
}
