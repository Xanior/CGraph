using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


/*TODO
 * Calls are treated equally to jmps
 * lots of cleaning up
 * Tracing from selected block
 * Doesnt work with 0xFFFF representation only FFFF
  */

namespace CGraph
{
    public partial class CGraph : Form
    {

        private List<Node> nodes = new List<Node>();

        Bitmap DrawArea;

        Point mousePos = new Point(0,0);
        Point mousePosWhenClicked = new Point(0,0);

        int traceDepth = -1;

        DateTime clickTime;
      
        public CGraph()
        {
            InitializeComponent();

            DrawArea = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            //DrawArea = new Bitmap(1000, 1000);
            pictureBox1.Image = DrawArea;
        }

        ~CGraph()
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromImage(DrawArea))
            {
                /*Node nodeInit = new Node("INIT", new String[] { "This is the", "initialization code" }, new Point(0, 0), g);
                Node nodeMain = new Node("MAIN", new String[] { "main code", "does something" }, new Point(50, 0), g);
                Node nodeEnd = new Node("END", new String[] { "destroy EVERYTHING HAHA", "BYE", "CYA" }, new Point(100, 0), g);
                nodes.Add(nodeInit);
                nodes.Add(nodeMain);
                nodes.Add(nodeEnd);
                nodeInit.ConnectToAsDefault(nodeMain);
                nodeMain.ConnectToByJump(nodeEnd);*/

                //textBox1.Text = Interpreter.InterpretEXE("D:\\OneDrive\\Uni\\Onlab\\objdump_test\\", "hello_c.exe");
            }
            
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            foreach (Node n in nodes)
            {
                n.Draw(e.Graphics);
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            clickTime = DateTime.Now;
            mousePosWhenClicked = e.Location;
            //Go through them backwards, so the one that was last drawn will be clicked.
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i].IsMouseOver(e.Location))
                {
                    nodes[i].Click(e.Location);
                    break;
                }
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if(DateTime.Now.Subtract(clickTime).TotalMilliseconds < 300 && mousePosWhenClicked.Equals(e.Location))
            {
                foreach (Node n in nodes)
                {
                    if (n.IsMouseOver(e.Location))
                    {
                        //If CTRL is down, then toggle selection
                        if (ModifierKeys.HasFlag(Keys.Control))
                        {
                            if (n.IsSelected()) n.UnSelect();
                            else n.Select();
                        }
                        else n.Select();
                    }
                    else
                    {
                        if (!ModifierKeys.HasFlag(Keys.Control)) n.UnSelect();
                    }
                }
                pictureBox1.Invalidate();
            }
            foreach (Node n in nodes)
            {
                n.MouseRelease();
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //Drag a node
            Boolean mouseOverNode = false;
            foreach (Node n in nodes)
            {
                if (e.Button == MouseButtons.Left)
                {
                    n.DragIfClicked(e.Location);
                    pictureBox1.Invalidate();
                }

                if (n.IsMouseOver(e.Location))
                {
                    mouseOverNode = true;
                }
            }

            //If no nodes were clicked, drag all nodes
            if (e.Button == MouseButtons.Left && !mouseOverNode)
            {
                foreach(Node n in nodes)
                {
                    n.Drag(mousePos.X - e.Location.X, mousePos.Y - e.Location.Y);
                }
            }

            //Cursor
            if (mouseOverNode)
            {
                Cursor.Current = Cursors.Hand;
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }

            //Save the last position so we can calculate the Delta when needed.
            mousePos = e.Location;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog newFileDialog = new OpenFileDialog()
            {
                //FileName = "Select an executable to open",
                //Filter = "Text files (*.txt)|*.txt",
                Title = "Open executable"
            };

            if (newFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = newFileDialog.FileName;
                string asmCode = Interpreter.InterpretEXE(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
                textBox1.Text = asmCode;
                nodes = Interpreter.CreateNodes(asmCode, Graphics.FromImage(DrawArea));
                pictureBox1.Invalidate();
            }

        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog newFileDialog = new OpenFileDialog()
            {
                //FileName = "Select an executable to open",
                Filter = "CGraph files (*.cgraph)|*.cgraph",
                Title = "Open project"
            };

            if (newFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = newFileDialog.FileName;
                textBox1.Text = Interpreter.InterpretEXE(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
            }
        }

        private void PictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (Node n in nodes)
            {
                if (n.IsMouseOver(e.Location))
                {
                    n.ToggleDetails();
                    break;
                }
            }
            pictureBox1.Invalidate();
        }


        private void ViewExpandAll_Click(object sender, EventArgs e)
        {
            foreach (Node n in nodes)
            {
                n.ShowDetails();
            }
            pictureBox1.Invalidate();
        }

        private void ViewCollapseSelected_Click(object sender, EventArgs e)
        {
            foreach (Node n in nodes)
            {
                if (n.IsSelected()) n.HideDetails();
            }
            pictureBox1.Invalidate();
        }

        private void ViewCollapseAll_Click(object sender, EventArgs e)
        {
            foreach (Node n in nodes)
            {
                n.HideDetails();
            }
            pictureBox1.Invalidate();
        }

        private void ViewExpandSelected_Click(object sender, EventArgs e)
        {
            foreach (Node n in nodes)
            {
                if (n.IsSelected()) n.ShowDetails();
            }
            pictureBox1.Invalidate();
        }

        private void TraceFromSelected_Click(object sender, EventArgs e)
        {
            traceDepth = 1;
            foreach (Node n in nodes) { n.Hide(); }
            foreach (Node n in nodes)
            {
                if(n.IsSelected()) n.Show();
            }

            pictureBox1.Invalidate();
        }

        private void TraceReset_Click(object sender, EventArgs e)
        {
            foreach (Node n in nodes) { n.Show(); }
            traceDepth = -1;

            pictureBox1.Invalidate();
        }

        private void AllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Node n in nodes)
            {
                n.Show();
            }
            pictureBox1.Invalidate();
        }

        private void ViewHideAll_Click(object sender, EventArgs e)
        {
            foreach (Node n in nodes)
            {
                n.Hide();
            }
            pictureBox1.Invalidate();
        }

        private void ViewHideSelected_Click(object sender, EventArgs e)
        {
            foreach (Node n in nodes)
            {
                if(n.IsSelected()) n.Hide();
            }
            pictureBox1.Invalidate();
        }

        private void TabControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            //NUM+ (for tracing)
            if (e.KeyChar == '+')
            {
                traceDepth++;
                foreach (Node n in nodes)
                {
                    if (n.IsSelected()) n.ShowNeighbours(traceDepth);
                }
            }
            if(e.KeyChar == '-')
            {
                foreach (Node n in nodes) n.Hide();
                traceDepth--;
                foreach (Node n in nodes)
                {
                    if (n.IsSelected()) n.ShowNeighbours(traceDepth);
                }
            }

            pictureBox1.Invalidate();
        }
    }
}
