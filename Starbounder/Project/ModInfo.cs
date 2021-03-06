﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Starbounder.Project
{
	class ModInfo
	{
		public string name { get; set; }
		public string version { get; set; }
		public string path { get; set; }
		public List<object> dependencies { get; set; }
		public Metadata metadata { get; set; }

		public class Metadata
		{
			public Metadata(string author, string version, string description)
			{
				this.author = author;
				this.version = version;
				this.description = description;
			}

			public string author { get; set; }
			public string version { get; set; }
			public string description { get; set; }
		}

		// Constructors

		public ModInfo setDefault()
		{
			this.name = "Untitled";
			this.version = Settings.LoadSBVersion();
			this.path = ".";
			this.dependencies = new List<object>();
			this.metadata = new Metadata(Settings.LoadAuthor(), "1.0.0", "Generated by Starbounder.");

			return this;
		}

		public static void Create(string path, ModInfo mi)
		{
			string fileName = Path.GetFileName(path); mi.name = fileName;

			TextWriter tw = new StreamWriter(path + "\\" + fileName + ".modinfo", true);
			tw.Write(JsonConvert.SerializeObject(mi, Formatting.Indented));
			tw.Dispose();
		}

		public static ModInfo Load(string path)
		{
			string stringJSON = File.ReadAllText(path);
			return JsonConvert.DeserializeObject<ModInfo>(stringJSON);
		}

	}
}
