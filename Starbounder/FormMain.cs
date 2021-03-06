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
using Starbounder.Generate;
using Starbounder.Structure;
using Newtonsoft.Json;

namespace Starbounder
{
	public partial class FormMain : Form
	{

		private ReSize resizeForm;
		private Templates templates;

		private bool toggleMove = false; // Mouse has toggle move form

		private int mouseX;
		private int mouseY;

		#region Functions
		private void ColorChangeOnMenuStrips(MenuStrip ms)
		{
			foreach (var item in ms.Items)
			{
				if (item is ToolStripMenuItem)
				{
					RecursiveMenuItems((ToolStripMenuItem)item);
				}
			}
		}

		private void ColorChangeOnMenuStrips(ContextMenuStrip ms)
		{
			foreach (var item in ms.Items)
			{
				if (item is ToolStripMenuItem)
				{
					RecursiveMenuItems((ToolStripMenuItem)item);
				}
			}
		}

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

		private string GetNodeFolderPath()
		{
			return treeViewFolder.SelectedNode.Tag.ToString();
		}

		private string GetNodeAssetsPath()
		{
			return treeViewAssets.SelectedNode.Tag.ToString();
		}

		private void RefreshWorkTreeView()
		{
			treeViewFolder.Nodes.Clear();
			treeViewFolder.Nodes.AddRange(Project.Project.TreeViewPopulate(Project.Settings.LoadWorkingDirectory()));
			//treeViewFolder.ExpandAll();
		}
		private void RefreshAssetsTreeView()
		{
			treeViewAssets.Nodes.Clear();
			treeViewAssets.Nodes.AddRange(Project.Project.TreeViewPopulate(Project.Settings.LoadAssetsFolder()));
		}
		#endregion

		public FormMain()
		{
			InitializeComponent();

			MainMenuStrip.Renderer = new Renderer.ToolStrips();
			contextMenuStripTreeView.Renderer = new Renderer.ToolStrips();

			this.DoubleBuffered = true;
			this.SetStyle(ControlStyles.ResizeRedraw, true);
		}

		private void FormMain_Load(object sender, EventArgs e)
		{
			resizeForm = new ReSize(this);

			templates = new Templates(this, templatesToolStripMenuItem, templatesGeneratedToolStripMenuItem_Click);
			templates.Refresh();

			this.Icon = Properties.Resources.StarbounderIcon;
			Structure.Placement.SetFormToLeftEdge(this);

			RefreshAssetsTreeView();
			RefreshWorkTreeView();

			ColorChangeOnMenuStrips(menuStripMain);
			ColorChangeOnMenuStrips(contextMenuStripTreeView);
		}

		#region TreeView + SplitContainer

		private void treeViewFolder_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				treeViewFolder.SelectedNode = treeViewFolder.GetNodeAt(e.X, e.Y);

				contextMenuStripTreeView.Show(treeViewFolder, e.X, e.Y);
			}
		}

		private void treeViewFolder_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (treeViewFolder.SelectedNode != null)
			{
				string nodePath = GetNodeFolderPath();

				if (Path.HasExtension(nodePath))
				{
					string fe = Path.GetExtension(nodePath); // File Extension

					if (fe == ".png")
					{
						Functions.Processes.OpenFileWithImageEditor(nodePath);
					}
					else
					{
						Functions.Processes.OpenFileWithTextEditor(nodePath);
					}
				}
			}
		}

		private void treeViewAssets_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				treeViewAssets.SelectedNode = treeViewAssets.GetNodeAt(e.X, e.Y);

				Functions.Processes.OpenFolder(GetNodeAssetsPath());
			}
		}

		private void treeViewAssets_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (treeViewAssets.SelectedNode != null)
			{
				if (e.Button == MouseButtons.Left)
				{
					string nodePath = GetNodeAssetsPath();

					if (Path.HasExtension(nodePath))
					{
						string fe    = Path.GetExtension(nodePath); // File Extension

						if (fe == ".png")
						{
							Functions.Processes.OpenFileWithImageEditor(nodePath);
						}
						else
						{
							Functions.Processes.OpenFileWithTextEditor(nodePath);
						}
					}
				}
			}
		}

		private void splitContainerFolders_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var scf = splitContainerFolders;

			if (scf.Orientation == Orientation.Horizontal)
			{
				scf.Orientation = Orientation.Vertical;
				scf.SplitterDistance = scf.Width * 1 / 2;
			}
			else
			{
				scf.Orientation = Orientation.Horizontal;
				scf.SplitterDistance = scf.Height * 1 / 4;
			}
		}

		#endregion

		#region MenuStripMain

		// File -> Project
		private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Project.Project.CreateProject();
		}
		private void loadProjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Project.Project.LoadProject())
			{
				var nodes = Project.Project.TreeViewPopulate(Project.Settings.LoadWorkingDirectory());

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
			Functions.Processes.LaunchStarbound(Properties.Settings.Default.OperationSystem64, openGLToolStripMenuItem.Checked);
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
			var mf = new FormConfiguration();

			//mf.FormClosed += (s, args) => this.Close();
			//mf.FormClosed += (s, args) => { RefreshWorkTreeView(); RefreshAssetsTreeView(); };

			mf.Icon = this.Icon;
			mf.ShowDialog(this);

			if (mf.ApplySettings)
			{
				RefreshAssetsTreeView();
				RefreshWorkTreeView();
			}
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
			templates.Refresh();
		}

		#endregion

		#region ContextMenuStrip

		#region item
		private void itemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Items.ItemItem().SetDefault(), ".item");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void materialItemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Items.ItemMaterial().SetDefault(), ".matitem");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void liquidItemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Items.ItemLiquid().SetDefault(), ".liqitem");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		#endregion
		#region weapon
		private void swordToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Weapons.WeaponSword().SetDefault(), ".sword");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void staffToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Weapons.WeaponStaff().SetDefault(), ".staff");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void gunToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Weapons.WeaponGun().SetDefault(), ".gun");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void thrownToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Weapons.WeaponThrown(), ".thrown");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		#endregion
		#region tool
		private void miningToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Tools.ToolMining().SetDefault(), ".miningtool");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void beamaxeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Tools.ToolBeamaxe().SetDefault(), ".beamaxe");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void harvestingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Tools.ToolMining().SetDefault(), ".harvestingtool");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		#endregion
		#region armor
		private void headToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Armors.ArmorHead().SetDefault(), ".head");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void chestToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Armors.ArmorChest().SetDefault(), ".chest");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void legsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Armors.ArmorLegs().SetDefault(), ".legs");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void backToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Armors.ArmorBack().SetDefault(), ".back");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}

		#endregion

		#region others
		private void consumableToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Others.Consumable().SetDefault(), ".consumable");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void liquidToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Others.Liquid().SetDefault(), ".liquid");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16, "tex");
			RefreshWorkTreeView();
		}
		private void framesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateFrames(GetNodeFolderPath());
			RefreshWorkTreeView();
		}
		private void animationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreateJson(GetNodeFolderPath(), new Generate.Others.Animation().setDefault(), ".animation");
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		private void imagePNGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate.FileTypes.CreatePNG(GetNodeFolderPath(), 16, 16);
			RefreshWorkTreeView();
		}
		#endregion
		#region actions
		private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string folderName = Path.GetFileNameWithoutExtension(GetNodeFolderPath());
			string folderPath = (Path.HasExtension(GetNodeFolderPath())) ? Path.GetDirectoryName(GetNodeFolderPath()) : Path.GetFullPath(GetNodeFolderPath());

			Functions.Actions.CreateFolder(folderPath + "\\" + folderName);

			RefreshWorkTreeView();
		}
		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Actions.Rename(GetNodeFolderPath());

			RefreshWorkTreeView();
		}
		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Actions.Delete(GetNodeFolderPath());

			RefreshWorkTreeView();
		}
		private void moveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Actions.Move(GetNodeFolderPath());

			RefreshWorkTreeView();
		}
		#endregion

		private void templatesGeneratedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem item = (ToolStripMenuItem)sender;

			if (File.Exists(item.Tag.ToString()))
			{
				string filePath = Path.GetFullPath(GetNodeFolderPath());
				string fileName = Path.GetFileName(item.Tag.ToString());

				filePath = Path.HasExtension(filePath) ? Path.GetDirectoryName(filePath) : filePath + "\\";

				File.Copy(item.Tag.ToString(), filePath + fileName);

				RefreshWorkTreeView();
			}
		}

		#endregion

		#region Move & Resize (Panel)

		private void panelBorder_Paint(object sender, PaintEventArgs e)
		{
			Rectangle iRect = new Rectangle(0, 0, panelBorder.Height, panelBorder.Height);
			Font fnt = new Font("Century Gothic", 12, FontStyle.Regular);

			e.Graphics.DrawImage(Properties.Resources.StarbounderIconPNG, iRect);
			e.Graphics.DrawString("Starbounder v1.1", fnt, SystemBrushes.Control, iRect.Width, iRect.Height / 2 - fnt.Height / 2);
		}

		private void panelBorder_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var monitor = Screen.FromControl(this);
			this.DesktopBounds = monitor.Bounds;
		}

		private void panelBorder_MouseDown(object sender, MouseEventArgs e)
		{
			resizeForm.EnableResize();

			if (!toggleMove)
			{
				mouseX = MousePosition.X - this.DesktopLocation.X;
				mouseY = MousePosition.Y - this.DesktopLocation.Y;

				toggleMove = true;
			}
		}

		private void panelBorder_MouseMove(object sender, MouseEventArgs e)
		{
			// if resizing is enable, check either toleft or toright
			if (resizeForm.IsResizing)
			{
				var cursorPosition = Placement.GetCursorWithinScreen(MousePosition);

				if (cursorPosition != null)
				{
					if (resizeForm.ResizingLeft)
					{
						resizeForm.ToLeft(cursorPosition.Value);
					}
					else
					{
						resizeForm.ToRight(cursorPosition.Value);
					}
				}
				
			} else
			{
				this.Cursor = Cursors.Default;
			}

			// Change cursor depending on mouse is right or left side of the panel
			if (resizeForm.IsLeft(MousePosition))
			{
				this.Cursor = resizeForm.Cursor;
			}
			else if (resizeForm.IsRight(MousePosition))
			{
				this.Cursor = resizeForm.Cursor;
			}

			if (toggleMove && !resizeForm.IsResizing)
			{
				this.SetDesktopLocation(MousePosition.X - mouseX, MousePosition.Y - mouseY);
			}
		}

		private void panelBorder_MouseUp(object sender, MouseEventArgs e)
		{
			resizeForm.DisableResize();

			toggleMove = false;
			Placement.SnapToEdge(this);
		}

		private void panelBorder_MouseLeave(object sender, EventArgs e)
		{
			resizeForm.DisableResize();
			this.Cursor = Cursors.Default;
		}

		#endregion

		#region Websites

		// Guides -> Starbound - Forum
		private void starboundForumToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Processes.OpenWebsite("http://community.playstarbound.com/threads/modding-guides.34001/");
		}

		// Mods -> Starbound - Mods
		private void starboundModsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Processes.OpenWebsite("http://community.playstarbound.com/resources/");
		}

		// Mods -> Nexusmods
		private void nexusmodsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.Processes.OpenWebsite("http://www.nexusmods.com/starbound/mods/categories/?");
		}


		#endregion

		
	}
}