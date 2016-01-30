﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace Starbounder.Project
{
	class IProject
	{
		public static string projectName { get; set; }
		public static string projectPath { get; set; }
		public static string sbPath { get; set; }

		public IProject(string path)
		{
			projectPath = path;
			projectName = Path.GetFileName(path);
		}

		public static void CreateProject()
		{
			var folder = Functions.Dialogs.SaveFileDialog("Create a new Starbound Project");

			if (folder.FileName != string.Empty)
			{
				FreshProject(folder.FileName);
			}
		}

		public static bool LoadProject()
		{
			var folder = Functions.Dialogs.FolderBrowserDialog("Load Starbound Mod Folder");

			if (folder.SelectedPath != string.Empty)
			{
				projectPath = folder.SelectedPath;
				projectName = Path.GetFileName(folder.SelectedPath);

				return true;
			}

			return false;
		}

		// Child functions

		private static void FreshProject(string path)
		{
			Starbounder.Functions.Actions.CreateFolder(path);
			Project.ModInfo.Create(path, new Project.ModInfo().setDefault());
		}

		#region TreeView
		/// <summary>
		/// Populates the treeview with the folderpath.
		/// </summary>
		/// <returns></returns>
		public static TreeNode[] TreeViewPopulate()
		{
			List<TreeNode> nodes = new List<TreeNode>();

			if (Directory.Exists(projectPath))
			{
				DirectoryInfo info = new DirectoryInfo(projectPath);

				if (info.Exists)
				{
					foreach (var folder in info.GetDirectories())
					{
						nodes.Add(CreateDirectoryNodes(folder));
					}

					foreach (var file in info.GetFiles())
					{
						TreeNode newNode = new TreeNode(file.Name);
						newNode.Tag = file.FullName;
						nodes.Add(newNode);
					}
				}
			}

			return nodes.ToArray();
		}

		/// <summary>
		/// Creates a new node from a DirectoryInfo.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		private static TreeNode CreateDirectoryNodes(DirectoryInfo info)
		{
			TreeNode directoryNode = new TreeNode(info.Name);
			directoryNode.Tag = info.FullName;

			foreach (var folder in info.GetDirectories())
			{
				directoryNode.Tag = folder.FullName;
				directoryNode.Nodes.Add(CreateDirectoryNodes(folder));
			}

			foreach (var file in info.GetFiles())
			{
				TreeNode newNode = new TreeNode(file.Name);
				newNode.Tag = file.FullName;
				directoryNode.Nodes.Add(newNode);
			}

			return directoryNode;
		}
		#endregion

	}
}
