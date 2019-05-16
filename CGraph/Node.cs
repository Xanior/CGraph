using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGraph
{

    //Represents a node in the graph, contains a rectangle with text and a label. If you click on the label it shows all the respective ASM code
    class Node
    {
        private static Pen borderColorDefault = Pens.Aquamarine;
        private static Pen borderColorSelected = Pens.Red;
        private static Brush fillColor = Brushes.WhiteSmoke;
        private static Pen arrowPenDefault = new Pen(Color.FromArgb(255, 0, 0, 255), 2);
        private static Pen arrowPenJump = new Pen(Color.FromArgb(255, 0, 255, 0), 2);
        private static Pen arrowPenNoJump = new Pen(Color.FromArgb(255, 255, 0, 0), 2);

        private String label;
        private LineOfCode[] asmCode;


        private bool hasDefaultConnection = false;
        private Node connectedToAsDefault;

        private bool hasJumpConnection = false;
        private Node connectedToByJump;



        private Point topLeft;
        //The current size
        private Size size;
        private Size sizeCollapsed;
        private Size sizeExpanded;

        private bool visible = true;

        private bool clicked = false;
        private bool selected = false;
        private bool showDetails = false;
        private Point mouseOffset;

        private Rectangle rec;
        private static Font font;

        public Node(String label, LineOfCode[] asmCode, Point topLeft, Graphics graphics)
        {
            this.label = label;
            this.asmCode = asmCode;
            this.topLeft = topLeft;

            font = new Font("Times New Roman", 12.0f);
            AdjustableArrowCap myArrow = new AdjustableArrowCap(5, 5);
            arrowPenDefault.CustomEndCap = myArrow;
            arrowPenJump.CustomEndCap = myArrow;
            arrowPenNoJump.CustomEndCap = myArrow;


            SizeF tmp = graphics.MeasureString(label, font);
            size.Width = (int)Math.Floor(tmp.Width);
            size.Height = (int)Math.Floor(tmp.Height);
            sizeCollapsed = size;

            for(int i = 0; i < asmCode.Length; i++)
            {
                SizeF lineSize = graphics.MeasureString(asmCode[i].getFormattedLine(), font);
                if (tmp.Width < lineSize.Width) tmp = lineSize;
            }
            sizeExpanded.Width = (int)Math.Floor(tmp.Width);
            sizeExpanded.Height = (int)Math.Floor((asmCode.Length + 1) * tmp.Height);
        }


        public void Draw(System.Drawing.Graphics graphics)
        {
            if (!visible) return;
            rec = new Rectangle(topLeft.X, topLeft.Y, size.Width, size.Height);
            graphics.FillRectangle(fillColor, rec);

            //If selected, give it a different border
            if (selected) graphics.DrawRectangle(borderColorSelected, rec);
            else graphics.DrawRectangle(borderColorDefault, rec);

            //Draw the label
            graphics.DrawString(label, font, Brushes.Black, topLeft);

            if(showDetails)
            {
                float fontHeight = graphics.MeasureString(label, font).Height;
                for (int i = 0; i < asmCode.Length; i++)
                {
                    graphics.DrawString(asmCode[i].getFormattedLine(), font, Brushes.Black, new Point(topLeft.X, (int)(topLeft.Y + (i + 1) * fontHeight)));
                }
            }

            //Draw line to all connected nodes

            if (hasJumpConnection)
            {
                if(connectedToByJump.IsVisible())DrawConnection(connectedToByJump, graphics, arrowPenJump);
                if (hasDefaultConnection)
                {
                    if (connectedToAsDefault.IsVisible()) DrawConnection(connectedToAsDefault, graphics, arrowPenNoJump);
                }
            } else
            {
                if (hasDefaultConnection)
                {
                    if (connectedToAsDefault.IsVisible()) DrawConnection(connectedToAsDefault, graphics, arrowPenDefault);
                }
            }
            
        }

        private void DrawConnection(Node n, Graphics graphics, Pen arrowPen)
        {
            if (topLeft.Y < n.topLeft.Y)
            {
                graphics.DrawLine(arrowPen, new Point(topLeft.X + size.Width / 2, topLeft.Y + size.Height), new Point(n.topLeft.X + n.size.Width / 2, n.topLeft.Y));
            }
            else
            {
                graphics.DrawLine(arrowPen, new Point(topLeft.X + size.Width / 2, topLeft.Y), new Point(n.topLeft.X + n.size.Width / 2, n.topLeft.Y + n.size.Height));
            }
        }

        public void ConnectToAsDefault(Node to)
        {
            hasDefaultConnection = true;
            connectedToAsDefault = to;
        }

        public void ConnectToByJump(Node to)
        {
            hasJumpConnection = true;
            connectedToByJump = to;
        }

        public LineOfCode[] getASMCode()
        {
            return asmCode;
        }

        public bool hasAddress(string address)
        {
            foreach (LineOfCode loc in asmCode)
            {
                if (loc.getAddress().Equals(address))
                {
                    return true;
                }
            }

            return false;
        }

        public void ShowNeighbours(int depth)
        {
            if (depth < 1) return;

            this.Show();

            if (hasDefaultConnection) connectedToAsDefault.ShowNeighbours(depth - 1);
            if (hasJumpConnection) connectedToByJump.ShowNeighbours(depth - 1);
        }

        public void Hide()
        {
            visible = false;
        }

        public void Show()
        {
            visible = true;
        }

        public bool IsVisible()
        {
            return visible;
        }

        public void Click(Point mouse)
        {

            clicked = true;
            mouseOffset = new Point(topLeft.X - mouse.X, topLeft.Y - mouse.Y);
        }

        public void Select()
        {
            selected = true;
        }

        public void UnSelect()
        {
            selected = false;
        }

        public bool IsMouseOver(Point mouse)
        {
            if (    mouse.X > topLeft.X && mouse.X < topLeft.X + size.Width
                &&  mouse.Y > topLeft.Y && mouse.Y < topLeft.Y + size.Height)
            {
                return true;
            }
            return false;
        }

        public bool IsSelected()
        {
            return selected;
        }

        public void MouseRelease()
        {
            clicked = false;
        }

        public void DragIfClicked(Point mouse)
        {
            if (clicked)
            {
                topLeft = new Point(mouse.X + mouseOffset.X, mouse.Y + mouseOffset.Y);
            }
        }

        public void Drag(int offsetX, int offsetY)
        {
            topLeft.X -= offsetX;
            topLeft.Y -= offsetY;
        }

        public void ToggleDetails()
        {
            if (showDetails) HideDetails();
            else ShowDetails();
        }

        public void ShowDetails()
        {
            showDetails = true;
            size = sizeExpanded;
        }

        public void HideDetails()
        {
            showDetails = false;
            size = sizeCollapsed;
        }
    }
}
