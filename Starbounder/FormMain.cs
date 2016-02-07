﻿using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Starbounder.FileTypes;
using Starbounder.Structure;
using Newtonsoft.Json;

namespace Starbounder
{
	public partial class FormMain : Form
	{
		private bool toggleMove = false; // Mouse has toggle move form

		private int mouseX;
		private int mouseY;

		public FormMain()
		{
			InitializeComponent();

			MainMenuStrip.Renderer = new Renderer.ToolStrips();
			contextMenuStripTreeView.Renderer = new Renderer.ToolStrips();

			//this.DoubleBuffered = true;
			this.SetStyle(ControlStyles.ResizeRedraw, true);
		}

		#region Functions
		private void RecursiveMenuItems(ToolStripDropDownItem item)
		{
			if (item.HasDropDownItems)
			{
				foreach (var cItem in item.DropDownItems)
				{
					if (cItem is ToolStripMenuItem)
					{
						RecursiveMenuItems((ToolStripMenuItem)cItem);
					}
				}
			}

			SetColorsOnMenuItem((ToolStripMenuItem)item);
		}

		private void SetColorsOnMenuItem(ToolStripMenuItem item)
		{
			item.BackColor = SystemColors.Desktop;
			item.ForeColor = SystemColors.Control;
		}

		private string GetNodePath()
		{
			return treeViewFolder.SelectedNode.Tag.ToString();
		}

		private void RefreshWorkTreeView()
		{
			treeViewFolder.Nodes.Clear();
			treeViewFolder.Nodes.AddRange(Project.IProject.TreeViewPopulate(Project.Settings.LoadWorkingDirectory()));
			treeViewFolder.ExpandAll();
		}
		private void RefreshAssetsTreeView()
		{
			treeViewAssets.Nodes.Clear();
			treeViewAssets.Nodes.AddRange(Project.IProject.TreeViewPopulate(Project.Settings.LoadAssetsFolder()));
		}
		#endregion

		private void FormMain_Load(object sender, EventArgs e)
		{
			foreach (ToolStripDropDownItem item in menuStripMain.Items)
			{
				RecursiveMenuItems(item);
			}

			foreach (var item in contextMenuStripTreeView.Items)
			{
				if (item is ToolStripMenuItem)
				{
					RecursiveMenuItems((ToolStripMenuItem)item);
				}
			}

			this.Icon = Properties.Resources.StarbounderIcon;
			Structure.Placement.SetFormToLeftEdge(this);

			RefreshAssetsTreeView();
			RefreshWorkTreeView();
		}

		private void panelBorder_Paint(object sender, PaintEventArgs e)
		{
			Rectangle iRect = new Rectangle(0, 0, panelBorder.Height, panelBorder.Height);
			Font fnt = new Font("Century Gothic", 12, FontStyle.Regular);

			e.Graphics.DrawImage(Properties.Resources.StarbounderIconPNG, iRect);
			e.Graphics.DrawString("Starbounder", fnt, SystemBrushes.Control, iRect.Width, iRect.Height / 2 - fnt.Height / 2);
		}

		private void treeViewFolder_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				TreeNode path = treeViewFolder.SelectedNode = treeViewFolder.GetNodeAt(e.X, e.Y);

				if (path != null)
				{
					contextMenuStripTreeView.Show(treeViewFolder, e.X, e.Y);
				}
			}
		}

		private void treeViewFolder_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			string nodePath = GetNodePath();

			if (Path.HasExtension(nodePath))
			{
				string fe    = Path.GetExtension(nodePath); // File Extension
				
				if (fe == ".png" || fe == ".PNG")
				{
					Functions.Processes.OpenFileWithImageEditor(nodePath);
				} else
				{
					Functions.Processes.OpenFileWithTextEditor(nodePath);
				}
			}
		}

		#region MenuStripMain
		// File -> Project
		private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Project.IProject.CreateProject();
		}
		private void loadProjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Project.IProject.LoadProject())
			{
				var nodes = Project.IProject.TreeViewPopulate(Project.Settings.LoadWorkingDirectory());

				treeViewFolder.Nodes.Clear();
				treeViewFolder.Nodes.AddRange(nodes);
				treeViewFolder.ExpandAll();
			}
		}
		private void fileExplorerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Processes.OpenFolder(Project.Settings.LoadWorkingDirectory());
		}
		// File -> Starbound
		private void playToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Processes.LaunchStarbound(Properties.Settings.Default.OperationSystem64, true);
		}
		private void fileExplorerSBToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Processes.OpenFolder(Project.Settings.LoadStarboundFolder());
		}
		private void unpackAssetsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var cmd = Functions.Processes.UnpackStarbound(Properties.Settings.Default.OperationSystem64);
			if (cmd) MessageBox.Show("Unpacking Assets is complete.\nIf the process took less than a second, an error occured.", "Unpacking Assets", MessageBoxButtons.OK, MessageBoxIcon.Information);

			RefreshAssetsTreeView();
		}
		// File -> Exit
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
		// Settings -> Configurations
		private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Hide();
			var mf = new FormConfiguration();

			mf.FormClosed += (s, args) => this.Close();
			mf.Show();
		}
		// Settings -> Hide
		private void hideToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Minimized;
		}
		// Refresh
		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RefreshWorkTreeView();
		}
		#endregion

		#region ContextMenuStrip

		#region item
		private void itemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Items.ItemItem().SetDefault(), ".item");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void materialItemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Items.ItemMaterial().SetDefault(), ".matitem");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void liquidItemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Items.ItemLiquid().SetDefault(), ".liqitem");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		#endregion
		#region weapon
		private void swordToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Weapons.WeaponSword().SetDefault(), ".sword");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void staffToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Weapons.WeaponStaff().SetDefault(), ".staff");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void gunToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Weapons.WeaponGun().SetDefault(), ".gun");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void thrownToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Weapons.WeaponThrown(), ".thrown");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		#endregion
		#region tool
		private void miningToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Tools.ToolMining().SetDefault(), ".miningtool");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void beamaxeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Tools.ToolBeamaxe().SetDefault(), ".beamaxe");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void harvestingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Tools.ToolMining().SetDefault(), ".harvestingtool");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		#endregion
		#region armor
		private void headToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Armors.ArmorHead().SetDefault(), ".head");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void chestToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Armors.ArmorChest().SetDefault(), ".chest");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void legsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Armors.ArmorLegs().SetDefault(), ".legs");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void backToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateJson(GetNodePath(), new FileTypes.Armors.ArmorBack(), ".back");
			FileTypes.FileTypes.CreatePNG(GetNodePath(), 16, 16);
			RefreshWorkTreeView();
		}

		#endregion

		#region others
		private void consumableToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}
		private void liquidToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}
		private void framesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FileTypes.FileTypes.CreateFrames(GetNodePath());
			RefreshWorkTreeView();
		}
		#endregion
		#region actions
		private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string folderName = Path.GetFileNameWithoutExtension(GetNodePath());
			string folderPath = (Path.HasExtension(GetNodePath())) ? Path.GetDirectoryName(GetNodePath()) : Path.GetFullPath(GetNodePath());

			Functions.Actions.CreateFolder(folderPath + "\\" + folderName);

			RefreshWorkTreeView();
		}
		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Actions.Rename(GetNodePath());

			RefreshWorkTreeView();
		}
		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Actions.Delete(GetNodePath());

			RefreshWorkTreeView();
		}
		private void moveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Actions.Move(GetNodePath());

			RefreshWorkTreeView();
		}











		#endregion

		#endregion

		#region Move & Resize
		private void panelBorder_MouseDown(object sender, MouseEventArgs e)
		{
			if (!toggleMove)
			{
				mouseX = MousePosition.X - this.DesktopLocation.X;
				mouseY = MousePosition.Y - this.DesktopLocation.Y;

				toggleMove = true;
			}
		}

		private void panelBorder_MouseMove(object sender, MouseEventArgs e)
		{
			if (toggleMove)
			{
				this.SetDesktopLocation(MousePosition.X - mouseX, MousePosition.Y - mouseY);
			}
		}

		private void panelBorder_MouseUp(object sender, MouseEventArgs e)
		{
			toggleMove = false;
			Structure.Placement.SnapToEdge(this);
		}

		private void panelBorder_MouseLeave(object sender, EventArgs e)
		{
			toggleMove = false;
		}

		#endregion


	}
}