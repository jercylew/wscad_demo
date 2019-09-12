using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WSCAD_Demo.Model;
using WSCAD_Demo.Utility;

namespace WSCAD_Demo
{
    public partial class MainWindow : Form
    {
        private ImageList mImageList = new ImageList();
        private Dictionary<TabPage, GraphDoc> dicTPageGraphDoc = new Dictionary<TabPage, GraphDoc>();
        DualDictionary<TreeNode, TabPage> ddicTNodeTPage = new DualDictionary<TreeNode, TabPage>();
        DualDictionary<TreeNode, Shape> ddicTnodeShape = new DualDictionary<TreeNode, Shape>();

        private ToolBar toolBar = new ToolBar();
        private ToolBarButton tbBtnOpen = new ToolBarButton();
        private ToolBarButton tbBtnPrint = new ToolBarButton();
        private ToolBarButton tbBtnExport = new ToolBarButton();

        Timer timer = new Timer();

        public MainWindow()
        {
            InitializeComponent();
            SetupToolBar();

            treeView.NodeMouseClick += new TreeNodeMouseClickEventHandler(TreeView_NodeMouseClick);
        }

        ~MainWindow()
        {
            if (dicTPageGraphDoc != null)
            {
                dicTPageGraphDoc.Clear();
                dicTPageGraphDoc.Distinct();
            }
            if (mImageList != null)
            {
                mImageList.Dispose();
            }
            if (toolBar != null)
            {
                toolBar.Dispose();
            }
        }

        private void TabPage_Paint(object sender, PaintEventArgs e)
        {
            TabPage tabPage = sender as TabPage;
            
            //Get the list of shapes for the current tab page
            if (dicTPageGraphDoc.TryGetValue(tabPage, out GraphDoc graphDoc))
            {
                float width = tabPage.Width;
                float height = tabPage.Height;
                float scale = GetScale(graphDoc, tabPage);
                PaintUtility.DrawShapes(e.Graphics, graphDoc, width, height, scale);
            }
        }

        private void TabPage_Click(object sender, MouseEventArgs e)
        {
            TabPage tabPage = sender as TabPage;

            if (dicTPageGraphDoc.TryGetValue(tabPage, out GraphDoc graphDoc))
            {
                float scale = GetScale(graphDoc, tabPage);
                float mouseX = e.X / scale - (tabPage.Width / (2 * scale));
                float mouseY = (tabPage.Height / (2 * scale)) - e.Y / scale;

                foreach (Shape shape in graphDoc.Graphs)
                {
                    if (shape.ContainsPoint(new PointF(mouseX, mouseY)))
                    {
                        string shapeInfo = shape.ToString();
                        shapeInfoLabel.Text = shapeInfo;
                        shape.IsSelected = true;

                        if (ddicTnodeShape.TryGetKey(shape, out TreeNode node))
                        {
                            treeView.SelectedNode = node;
                        }

                        MessageBox.Show(shapeInfo, "Graph Detail");
                    }
                    else
                    {
                        shape.IsSelected = false;
                    }
                }

                tabPage.Refresh();
            }
        }

        private void TabPage_MouseMove(object sender, MouseEventArgs e)
        {
            TabPage tabPage = sender as TabPage;

            if (dicTPageGraphDoc.TryGetValue(tabPage, out GraphDoc graphDoc))
            {
                float scale = GetScale(graphDoc, tabPage);
                float mouseX = e.X / scale - (tabPage.Width / (2 * scale));
                float mouseY = (tabPage.Height / (2 * scale)) - e.Y / scale;

                mousePosLabel.Text = string.Format("({0:.0}, {1:.0})", mouseX, mouseY);
            }
        }

        private void ToolBarButton_Click(object sender, ToolBarButtonClickEventArgs e)
        {
            if (e.Button == tbBtnOpen) //toolBar.Buttons[0]
            {
                OpenFile();
            }
            else if (e.Button == tbBtnExport)
            {
                ExportToPDF();
            }
        }

        void TreeView_NodeMouseClick(object sender, 
            TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 1) //File level
            {
                if (ddicTNodeTPage.TryGetValue(e.Node, out TabPage tabPage))
                {
                    this.tabControl.SelectTab(tabPage);
                }
            }
            else if (e.Node.Level == 2) //Shape level
            {
                //Deselect others first
                TreeNode parent = e.Node.Parent;
                if (ddicTNodeTPage.TryGetValue(parent, out TabPage tabPage))
                {
                    if (dicTPageGraphDoc.TryGetValue(tabPage, out GraphDoc graphDoc))
                    {
                        foreach (Shape shape in graphDoc.Graphs)
                        {
                            shape.IsSelected = false;
                        }
                    }
                }

                if (ddicTnodeShape.TryGetValue(e.Node, out Shape selectedShape))
                {
                    selectedShape.IsSelected = true;
                }

                tabPage.Refresh();
            }
            else
            {
            }
        }

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = @"Primitive Graph Files(*.json;*.xml)|*.json;*.xml;|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;

                if (IsOpened(fileName))
                {
                    MessageBox.Show("File already opened!", "SVG Viewer");
                    return;
                }

                string ext = Path.GetExtension(fileName);
                GraphDoc graphDoc = new GraphDoc
                {
                    FileName = Path.GetFileName(fileName),
                    FilePath = fileName,
                    Graphs = new List<Shape>(),
                    IntersectPoints = new List<PointF>()
                };

                try
                {
                    if (ext.Equals(".json"))
                    {
                        DataUtility.LoadGraphsFromJson(fileName, ref graphDoc);
                    }
                    else if (ext.Equals(".xml"))
                    {
                        DataUtility.LoadGraphsFromXml(fileName, ref graphDoc);
                    }
                    else
                    {
                        throw new InvalidDataException("Unsupported file");
                    }
                }
                catch (Exception)
                {
                }

                SetupTabPage(fileName, out TabPage tabPage);
                SetupTree(fileName, graphDoc, tabPage);
                dicTPageGraphDoc.Add(tabPage, graphDoc);
            }
        }

        private void ExportToPDF()
        {
            TabPage tabPage = tabControl.SelectedTab;
            if (tabPage == null)
            {
                MessageBox.Show(this, "No file yet opened!", "Warning");
                return;
            }

            if (!dicTPageGraphDoc.TryGetValue(tabPage, out GraphDoc graphDoc))
            {
                return;
            }

            ExportPDFSettings settingDialog = new ExportPDFSettings();
            settingDialog.StartPosition = FormStartPosition.CenterParent;

            if (settingDialog.ShowDialog(this) == DialogResult.OK)
            {
                PdfDocument pdfDocument = new PdfDocument();
                PdfPage page = pdfDocument.AddPage();

                string savePath = settingDialog.PdfSavePath;
                float maxWidth = (float)page.Width - 30;
                float maxHeight = (float)page.Height - 30;

                float width = (settingDialog.PdfWidth < maxWidth) ? settingDialog.PdfWidth : maxWidth;
                float height = (settingDialog.PdfHeight < maxHeight) ? settingDialog.PdfHeight : maxHeight;

                pdfDocument.Info.Title = graphDoc.FileName;
                pdfDocument.Info.Author = "Jercy LIU";
                pdfDocument.Info.Subject = "Mini SVG Viewer";
                pdfDocument.Info.Keywords = "Mini SVG Viewer";

                float limitWorld = Math.Max(Math.Abs(graphDoc.maxX), Math.Abs(graphDoc.maxY));
                limitWorld = Math.Max(limitWorld, Math.Abs(graphDoc.minX));
                limitWorld = Math.Max(limitWorld, Math.Abs(graphDoc.minY));

                float limitWindow = (float)(Math.Min(width, height) / 2.0);
                float scale = (float)(limitWindow / limitWorld) * 0.85f;

                PDFUtility.DrawShapes(page, graphDoc, width, height, scale); //The actual width and height used for drawing

                pdfDocument.Save(savePath);
                pdfDocument.Dispose();
                Process.Start(savePath);
            }
        }

        private void SetupTabPage(string fileName, out TabPage tabPage)
        {
            tabPage = new TabPage();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainWindow));
            this.tabControl.Controls.Add(tabPage);
            tabPage.Cursor = System.Windows.Forms.Cursors.Cross;
            resources.ApplyResources(tabPage, Path.GetFileName(fileName));
            tabPage.Text = Path.GetFileName(fileName);
            tabPage.UseVisualStyleBackColor = true;
            tabPage.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TabPage_Click);
            tabPage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TabPage_MouseMove);
            tabPage.Paint += new System.Windows.Forms.PaintEventHandler(this.TabPage_Paint);

            //Add context menu
            ContextMenuStrip menuStrip = new ContextMenuStrip();
            menuStrip.Items.Add("Close");
            menuStrip.Items.Add("Export");
            menuStrip.ItemClicked += new ToolStripItemClickedEventHandler(ToolStrip_ItemClicked);
            tabControl.ContextMenuStrip = menuStrip;

            this.tabControl.SelectTab(tabPage);
        }

        private void ToolStrip_ItemClicked(Object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Close")
            {
                TabPage tabPage = tabControl.SelectedTab;

                tabControl.Controls.Remove(tabPage);
                if (ddicTNodeTPage.TryGetKey(tabPage, out TreeNode node))
                {
                    treeView.Nodes[0].Nodes.Remove(node);
                }

                if (dicTPageGraphDoc.TryGetValue(tabPage, out GraphDoc graphDoc))
                {
                    foreach (Shape shape in graphDoc.Graphs)
                    {
                        ddicTnodeShape.Remove(shape);
                    }

                    graphDoc.Graphs.Clear();
                    graphDoc.IntersectPoints.Clear();
                }

                dicTPageGraphDoc.Remove(tabPage);
                ddicTNodeTPage.Remove(tabPage);
                
            }
            else if (e.ClickedItem.Text == "Export")
            {
                ExportToPDF();
            }
            else
            {
            }
        }

        private void SetupTree(string fileName, GraphDoc graphDoc, TabPage tabPage)
        {
            TreeNode[] nodeArray = new TreeNode[graphDoc.Graphs.Count];
            for (int i = 0;i < nodeArray.Length;i++)
            {
                nodeArray[i] = new TreeNode(graphDoc.Graphs[i].Name);
                ddicTnodeShape.Add(nodeArray[i], graphDoc.Graphs[i]); //Level 2
            }

            TreeNode node = new TreeNode(Path.GetFileName(fileName), nodeArray);
            ddicTNodeTPage.Add(node, tabPage); //Level 1
            treeView.Nodes[0].Nodes.Add(node);
            treeView.Nodes[0].Expand();

            foreach(TreeNode n in treeView.Nodes[0].Nodes)
            {
                if (n != node)
                {
                    n.Collapse();
                }
                else
                {
                    n.Expand();
                }
            }
        }

        private void SetupToolBar()
        {
            //Image list
            mImageList.ImageSize = new System.Drawing.Size(32, 32);
            mImageList.TransparentColor = System.Drawing.Color.Transparent;
            mImageList.Images.Add(Image.FromFile("res\\open.png"));
            mImageList.Images.Add(Image.FromFile("res\\print.png"));
            mImageList.Images.Add(Image.FromFile("res\\export.png"));

            toolBar.ImageList = mImageList;
            toolBar.ShowToolTips = true;

            tbBtnOpen.Text = "Open";
            tbBtnOpen.Style = ToolBarButtonStyle.PushButton;
            tbBtnOpen.ToolTipText = "Open the document";
            tbBtnOpen.ImageIndex = 0;

            tbBtnExport.Text = "Export";
            tbBtnExport.Style = ToolBarButtonStyle.PushButton;
            tbBtnExport.ToolTipText = "Print";
            tbBtnExport.ImageIndex = 2;

            toolBar.Buttons.Add(tbBtnOpen);
            toolBar.Buttons.Add(tbBtnExport);

            toolBar.ButtonClick += new ToolBarButtonClickEventHandler(
               this.ToolBarButton_Click);
            this.Controls.Add(toolBar);
        }

        private bool IsOpened(string filePath)
        {
            bool ret = false;

            try
            {
                //TODO: using Dictionary<FilePath, bool> to store openning file flag to improve performance
                foreach (KeyValuePair<TabPage, GraphDoc> pair in dicTPageGraphDoc)
                {
                    if (pair.Value.FilePath.ToLower().Trim() == 
                        filePath.ToLower().Trim())
                    {
                        ret = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }

            return ret;
        }

        private float GetScale(GraphDoc graphDoc, TabPage tabPage)
        {
            float scale;
            float width = tabPage.Width;
            float height = tabPage.Height;

            float limitWorld = Math.Max(Math.Abs(graphDoc.maxX), Math.Abs(graphDoc.maxY));
            limitWorld = Math.Max(limitWorld, Math.Abs(graphDoc.minX));
            limitWorld = Math.Max(limitWorld, Math.Abs(graphDoc.minY));

            float limitWindow = (float)(Math.Min(width, height) / 2.0);
            scale = (float)(limitWindow / limitWorld) * 0.85f; //Set aside 0.85 for margin

            return scale;
        }

        private void TabControl_Selected(object sender, TabControlEventArgs e)
        {
            //New tab selected, select the corresponding tree node
            TabPage tabPage = e.TabPage;

            if (tabPage != null && 
                ddicTNodeTPage.TryGetKey(tabPage, out TreeNode node))
            {
                treeView.SelectedNode = node;
                foreach (TreeNode n in treeView.Nodes[0].Nodes)
                {
                    if (n != node)
                    {
                        n.Collapse();
                    }
                    else
                    {
                        n.Expand();
                    }
                }

                //Cannot call treeView.Focus() right from here, time is too short
                timer.Tick += new EventHandler(TimerEventProcessor);
                timer.Interval = 150;
                timer.Start();
            }
        }

        private void TimerEventProcessor(Object myObject,
                                            EventArgs myEventArgs)
        {
            timer.Stop();
            treeView.Focus();
        }
    }
}
