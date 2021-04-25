using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CM9798TacticsEditor
{
    public partial class TacticsEditor : Form
    {
        int pitchX = 6;
        int pitchY = 95;    

        // Images
        Bitmap cm9798Pitch;
        Bitmap tacticsButton;
        Bitmap tacticsButtonPressed;
        bool mouseDownInTacticsButton = false;

        // Font
        PrivateFontCollection MSSansSerifCollection;
        FontFamily MSSansSerif;
        Font font8pt;
        Font font9pt;
        Font font10pt;

        // Colors (Brushes and Pens)
        Color cm9798GreyTextColor = Color.FromArgb(203, 203, 203);
        Brush cm9798GreyTextBrush;
        Color cm9897YellowTextColor = Color.FromArgb(255, 255, 36);
        Brush cm9897YellowTextBrush;
        Color cm9897DarkGreyTextColor = Color.FromArgb(48, 48, 48);
        Brush cm9897DarkGreyTextBrush;
        Color cm9897GreenLineColor = Color.FromArgb(0, 195, 0);
        Pen dashGreenLinePen;
        Color cm9897GreyLineColor = Color.FromArgb(150, 150, 150);
        Pen dashGreyLinePen;
        Color cm9798YellowCrossColor = Color.FromArgb(255, 211, 28);
        Pen yellowLinePen;

        // Data 
        List<Formation> Formations = new List<Formation>();
        int selectedFormation = -1;

        public TacticsEditor()
        {
            InitializeComponent();

            DoubleBuffered = true;

            // Load the background pitch image
            cm9798Pitch = LoadImage("cm9798pitch.png");

            // Load the tactics button
            tacticsButton = LoadImage("cm9798button.png");
            tacticsButtonPressed = LoadImage("cm9798button_pressed.png");

            // Load the Font
            MSSansSerifCollection = LoadFont("MS Sans Serif.ttf");
            MSSansSerif = MSSansSerifCollection.Families[0];
            font8pt = new Font(MSSansSerif, 8);
            font9pt = new Font(MSSansSerif, 9);
            font10pt = new Font(MSSansSerif, 10);

            // Load up the right CM9798 colors
            cm9798GreyTextBrush = new SolidBrush(cm9798GreyTextColor);
            cm9897YellowTextBrush = new SolidBrush(cm9897YellowTextColor);
            cm9897DarkGreyTextBrush = new SolidBrush(cm9897DarkGreyTextColor);

            // Green dashed line
            dashGreenLinePen = new Pen(cm9897GreenLineColor);
            dashGreenLinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            float[] dashValues = { 6, 4};
            dashGreenLinePen.DashPattern = dashValues;

            // Grey Dashed line (when selecting players)
            dashGreyLinePen = new Pen(cm9897GreyLineColor);
            dashGreyLinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            float[] greyDashValues = { 4, 4 };
            dashGreyLinePen.DashPattern = greyDashValues;

            // Yellow cross lines
            yellowLinePen = new Pen(cm9798YellowCrossColor);
        }

        private void TacticsEditor_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            // Set so it gets that pixel font look
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;

            // Draw Pitch
            g.DrawImageUnscaled(cm9798Pitch, pitchX, pitchY);

            if (selectedFormation >= 0)
            {
                var formation = Formations[selectedFormation];
                DrawTacticSelectButton(g, formation.Name, formation.MaxNameSize);

                // Draw the players
                for (int i = 0; i < formation.Players.Count; i++)
                {
                    int x, y, linex1, liney1, linex2, liney2;
                    var player = formation.Players[i];
                    GetPlayerCoords(player, formation, out x, out y, out linex1, out liney1, out linex2, out liney2);
                    DrawPlayer(g, player.Number, player.Name, x, y);

                    //g.DrawRectangle(dashGreyLinePen, GetPlayersRectangle(player, formation));

                    // If it has a running line, draw it
                    if (linex1 != -1)
                    {
                        g.DrawLine(dashGreenLinePen, pitchX + linex1, pitchY + liney1, pitchX + linex2, pitchY + liney2);
                        g.DrawLine(yellowLinePen, (pitchX + linex2) - 2, (pitchY + liney2) - 2, (pitchX + linex2) + 2, (pitchY + liney2) + 2);
                        g.DrawLine(yellowLinePen, (pitchX + linex2) - 2, (pitchY + liney2) + 2, (pitchX + linex2) + 2, (pitchY + liney2) - 2);
                    }
                }
            }
            else
                DrawTacticSelectButton(g, "Select Tactic", "Select Tactic".Length);
        }

        /*
            (L, CL, C, CR, R)(maybe other way around ??)
            ST: 1A, 1B, 1C, 1D, 1E
            AM: 15, 16, 17, 18, 19
            MF: 10, 11, 12, 13, 14
            DM: 0B, 0C, 0D, 0E, 0F
            DF: 06, 07, 08, 09, 0A
            SW: 01, 02, 03, 04, 05
            GK:         00
        */

        Rectangle GetPlayersRectangle(Player player, Formation formation)
        {
            int x, y, linex1, liney1, linex2, liney2;
            GetPlayerCoords(player, formation, out x, out y, out linex1, out liney1, out linex2, out liney2);
            return new Rectangle((pitchX + x) - 28, (pitchY + y) - 20, 56, 26);
        }

        void GetPlayerCoords(Player player, Formation formation, out int x, out int y, out int linex1, out int liney1, out int linex2, out int liney2)
        {
            int GKLine = cm9798Pitch.Height - 7;
            int SWLine = GKLine - 34;
            int DFLine = SWLine - 43;
            int DMLine = DFLine - 62;
            int MFLine = DMLine - 70;
            int AMLine = MFLine - 72;
            int STLine = AMLine - 72;

            bool lineHasACenterPlayer = false;
            int FarLeft = 42;
            int Left = 110;
            int Center = cm9798Pitch.Width / 2;
            int Right = cm9798Pitch.Width - Left;
            int FarRight = cm9798Pitch.Width - FarLeft;

            y = -1;
            x = -1;
            if (player.Position == 0)
            {
                y = GKLine;
                x = Center;
            }
            else if (player.Position >= 1 && player.Position <= 5)
            {
                y = SWLine;
                if (formation.Players.Exists(z => z.Position == 3))
                    lineHasACenterPlayer = true;
            }
            else if (player.Position >= 6 && player.Position <= 0xa)
            {
                y = DFLine;
                if (formation.Players.Exists(z => z.Position == 8))
                    lineHasACenterPlayer = true;
            }
            else if (player.Position >= 0xb && player.Position <= 0xf)
            {
                y = DMLine;
                //if (formation.Players.Exists(z => z.Position == 0xd))  // Always pushes to far edge for DMs
                    lineHasACenterPlayer = true;
            }
            else if (player.Position >= 0x10 && player.Position <= 0x14)
            {
                y = MFLine;
                if (formation.Players.Exists(z => z.Position == 0x12))
                    lineHasACenterPlayer = true;
            }
            else if (player.Position >= 0x15 && player.Position <= 0x19)
            {
                y = AMLine;
                if (formation.Players.Exists(z => z.Position == 0x17))
                    lineHasACenterPlayer = true;
            }
            else if (player.Position >= 0x1a && player.Position <= 0x1e)
            {
                y = STLine;
                if (formation.Players.Exists(z => z.Position == 0x1c))
                    lineHasACenterPlayer = true;
            }

            if (lineHasACenterPlayer)
            {
                FarLeft -= 20;
                Left -= 20;
                Right += 20;
                FarRight += 20;
            }

            if (player.Position != 0)
            { 
                switch (player.Position % 5)
                {
                    default:
                    case 1:
                        x = FarLeft;
                        break;
                    case 2:
                        x = Left;
                        break;
                    case 3:
                        x = Center;
                        break;
                    case 4:
                        x = Right;
                        break;
                    case 0:
                        x = FarRight;
                        break;
                }
            }

            // Now check the line
            if (player.RunningTo == player.Position)
            {
                linex1 = liney1 = linex2 = liney2 = -1;
            }
            else
            {
                if (player.RunningTo == 0)
                    linex2 = Center;
                else
                { 
                    switch (player.RunningTo % 5)
                    {
                        default:
                        case 1:
                            linex2 = FarLeft;
                            break;
                        case 2:
                            linex2 = Left;
                            break;
                        case 3:
                            linex2 = Center;
                            break;
                        case 4:
                            linex2 = Right;
                            break;
                        case 0:
                            linex2 = FarRight;
                            break;
                    }
                }

                liney2 = -1;
                if (player.RunningTo == 0)
                    liney2 = GKLine;
                else if (player.RunningTo >= 1 && player.RunningTo <= 5)
                    liney2 = SWLine;
                else if (player.RunningTo >= 6 && player.RunningTo <= 0xa)
                    liney2 = DFLine;
                else if (player.RunningTo >= 0xb && player.RunningTo <= 0xf)
                    liney2 = DMLine;
                else if (player.RunningTo >= 0x10 && player.RunningTo <= 0x14)
                    liney2 = MFLine;
                else if (player.RunningTo >= 0x15 && player.RunningTo <= 0x19)
                    liney2 = AMLine;
                else if (player.RunningTo >= 0x1a && player.RunningTo <= 0x1e)
                    liney2 = STLine;

                linex1 = x;
                liney1 = y;

                if (linex1 > linex2)    // Line is going left
                {
                    linex2 += 8;
                }
                else if (linex1 < linex2)   // Line is going right
                {
                    linex2 -= 8;
                }

                if (liney1 > liney2)    // Line is going up
                {
                    liney1 -= 24;
                    liney2 += 8;
                }
                else
                if (liney1 <= liney2)    // Line is going down
                {
                    if (liney1 == liney2)       // If on the same line but going horizontally
                    {
                        if (linex1 < linex2) // quirk that if going horizontally, but on the right it is actually from above the name
                        {
                            liney1 -= 24;
                            liney2 -= 24;
                        }
                        else
                        {
                            liney2 += 8;
                            liney1 += 8;
                        }
                    }
                    else
                    {
                        liney2 -= 8;
                        liney1 += 8;
                    }
                }
            }
        }

        void DrawTacticSelectButton(Graphics g, string tacticName, int maxsize)
        {
            g.DrawImageUnscaled(mouseDownInTacticsButton ? tacticsButtonPressed : tacticsButton, pitchX, pitchY - (tacticsButton.Height + 2));

            int modifier = mouseDownInTacticsButton ? 2 : 0;

            using (StringFormat format = new StringFormat())
            {
                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Center;

                g.DrawString(tacticName, font8pt, cm9897DarkGreyTextBrush, new RectangleF((pitchX + 16) + 2 + modifier, ((pitchY - (tacticsButton.Height + 2)) + 2) + 2 + modifier, tacticsButton.Width - 32, tacticsButton.Height - 2), format);
                g.DrawString(tacticName, font8pt, Brushes.White, new RectangleF(pitchX + 16 + modifier, (pitchY - (tacticsButton.Height + 2)) + 2 + modifier, tacticsButton.Width - 32, tacticsButton.Height - 2), format);

            }

            textBoxTacticName.Text = tacticName;
            textBoxTacticName.MaxLength = maxsize;
        }

        void DrawPlayer(Graphics g, string number, string name, int x, int y)
        {
            x += pitchX;
            y += pitchY;

            // Get the dimensions of the name so we can do centering
            var nameDimensions = g.MeasureString(name, font8pt, 100);
            var nameWidth = nameDimensions.Width;
            var nameHeight = nameDimensions.Height;

            // Write the name
            g.DrawString(name, font8pt, Brushes.Black, (x - (nameWidth / 2)) + 2, (y - (nameHeight / 2)) + 2);
            g.DrawString(name, font8pt, cm9798GreyTextBrush, (x - (nameWidth / 2)), (y - (nameHeight / 2)));

            // Get the dimensions of the number
            var numberDimensions = g.MeasureString(number, font9pt, 100);
            var numberWidth = numberDimensions.Width;
            var numberHeight = numberDimensions.Height;

            // Write the number (Above the name)
            y -= 12;
            g.DrawString(number, font9pt, Brushes.Black, (x - (numberWidth / 2)) + 2, (y - (numberHeight / 2)) + 2);
            g.DrawString(number, font9pt, cm9897YellowTextBrush, (x - (numberWidth / 2)), (y - (numberHeight / 2)));
        }

        public static Bitmap LoadImage(string imgFileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(imgFileName));
            using (var imgStream = assembly.GetManifestResourceStream(resourceName))
            return new Bitmap(imgStream); 
        }

        PrivateFontCollection LoadFont(string fontName)
        {
            var pfc = new PrivateFontCollection();

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fontName));

            using (var fontStream = assembly.GetManifestResourceStream(resourceName))
            {
                // Copy font stream into a byte array
                var fontBytes = new byte[fontStream.Length];
                fontStream.Read(fontBytes, 0, fontBytes.Length);

                // Create unsafe memory block
                var newMemory = Marshal.AllocCoTaskMem(fontBytes.Length);

                // copy the bytes to the unsafe memory block
                Marshal.Copy(fontBytes, 0, newMemory, fontBytes.Length);
                pfc.AddMemoryFont(newMemory, fontBytes.Length);

                // Free the memory
                Marshal.FreeCoTaskMem(newMemory);
            }

            return pfc;
        }

        private void TacticsEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Y > (pitchY - (tacticsButton.Height + 2)) && e.Y < pitchY && 
                ((e.X > pitchX && e.X < pitchX + 20) ||
                (e.X > ((pitchX + tacticsButton.Width)-20) && e.X < (pitchX + tacticsButton.Width)) ))
            {
                Cursor.Current = Cursors.Hand;
            }
            else
                Cursor.Current = Cursors.Default;
        }

        private void TacticsEditor_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Y > (pitchY - (tacticsButton.Height + 2)) && e.Y < pitchY &&
                ((e.X > pitchX && e.X < pitchX + 20) ||
                (e.X > ((pitchX + tacticsButton.Width) - 20) && e.X < (pitchX + tacticsButton.Width))))
            {
                mouseDownInTacticsButton = true;
                Invalidate();
            }

            // Now check if it's clicked in the players
            if (selectedFormation != -1)
            {
                var formation = Formations[selectedFormation];
                for (int i = 0; i < formation.Players.Count; i++)
                {
                    int x, y, linex1, liney1, linex2, liney2;
                    var player = formation.Players[i];
                    GetPlayerCoords(player, formation, out x, out y, out linex1, out liney1, out linex2, out liney2);

                    if (e.X >= pitchX + x && e.X <= pitchX + x + 40 &&
                        e.Y >= pitchY + y && e.Y <= pitchY + y + 40)
                    {
                        MessageBox.Show("Yep");
                    }
                }
            }
        }

        private void TacticsEditor_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDownInTacticsButton = false;

            if (Formations.Count != 0)
            {
                if (e.Y > (pitchY - (tacticsButton.Height + 2)) && e.Y < pitchY &&
                    ((e.X > pitchX && e.X < pitchX + 20)))
                {
                    selectedFormation--;
                    if (selectedFormation < 0)
                        selectedFormation = Formations.Count - 1;
                }
                else
                if (e.Y > (pitchY - (tacticsButton.Height + 2)) && e.Y < pitchY &&
                   (e.X > ((pitchX + tacticsButton.Width) - 20) && e.X < (pitchX + tacticsButton.Width)))
                {
                    selectedFormation++;
                    if (selectedFormation >= Formations.Count)
                        selectedFormation = 0;
                }
            }

            Invalidate();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "CM2E16.exe Files|*.exe|All files (*.*)|*.*";
            ofd.Title = "Select a CM2E16.exe file";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBoxExeFile.Text = ofd.FileName;

                LoadEXEData();
            }
        }

        private void LoadEXEData()
        {
            using (var fs = File.Open(textBoxExeFile.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var grid = new byte[(5 * 6) + 1];
                var grid2 = new byte[(5 * 6) + 1];

                fs.Seek(0x153EE4, SeekOrigin.Begin);

                Formations = new List<Formation>();

                for (int formationNo = 0; formationNo < Formation.Formations.Length; formationNo++)
                {
                    var formation = new Formation();
                    formation.Name = Formation.Formations[formationNo];
                    formation.MaxNameSize = Formation.Formations[formationNo].Length;

                    for (int i = 0; i < 11; i++)
                    {
                        formation.Players[i].Position = fs.ReadByte();
                    }

                    // Read Subs1
                    fs.Read(formation.subs1, 0, 5);

                    for (int i = 0; i < 11; i++)
                    {
                        formation.Players[i].RunningTo = fs.ReadByte();
                    }

                    // Read Subs2
                    fs.Read(formation.subs2, 0, 5);

                    Formations.Add(formation);
                }

                selectedFormation = 0;
                Invalidate();
            }
        }

        private void textBoxTacticName_TextChanged(object sender, EventArgs e)
        {
            if (selectedFormation != -1)
            {
                Formations[selectedFormation].Name = textBoxTacticName.Text;
                Invalidate();
            }
        }
    }

    public class Formation
    {
        public static string[] Formations = new string[]
        {
            "Ultra Defensive",
            "5-3-2 Defensive",
            "Sweeper Defensive",
            "4-4-2 Defensive",
            "4-5-1 Defensive",
            "Counter Attack",
            "5-3-2 Formation",
            "3-5-2 Sweeper",
            "3-5-2 Formation",
            "3-1-3-3 Formation",
            "4-2-2 Formation",
            "Christmas Tree",
            "Diamond Formation",
            "4-3-3 Formation",
            "5-3-2 Attacking",
            "4-4-2 Attacking",
            "4-3-3 Attacking",
            "4-2-4 Attacking",
            "All Out Attack"
        };

        public Formation()
        {
            Players = new List<Player>()
            {
                new Player { Number = "1", Name = "Nick+Co" },
                new Player { Number = "2", Name = "Tor" },
                new Player { Number = "3", Name = "Nikolai" },
                new Player { Number = "4", Name = "Norbert" },
                new Player { Number = "5", Name = "Manager Old" },
                new Player { Number = "6", Name = "Ben 04" },
                new Player { Number = "7", Name = "jjglvz" },
                new Player { Number = "8", Name = "Cam F" },
                new Player { Number = "9", Name = "King O'Rooks" },
                new Player { Number = "10", Name = "Dave Black" },
                new Player { Number = "11", Name = "Molloy" },
            };
        }
        
        public List<Player> Players;
        public string Name;
        public int MaxNameSize;
        public byte[] subs1 = new byte[5];
        public byte[] subs2 = new byte[5];
    }

    public class Position
    {
        public int value;
        public int x;
        public int y;
    }

    public class Player
    {
        public string Number;
        public string Name;
        public int Position;
        public int RunningTo;
    }
}

