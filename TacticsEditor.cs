using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
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

        // Variables for dragging players
        Player selectedPlayer = null;
        bool selectedIsRunning = false;
        int currentMouseX = 0;
        int currentMouseY = 0;
        int mouseOffsetX = 0;
        int mouseOffsetY = 0;
        int selectedRectWidth = 75;
        int selectedRectHeight = 26;

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
                DrawTacticSelectButton(g, formation.Name, formation.MaxNameSize, Formation.FormationCode[selectedFormation]);

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
                        // Draw green line
                        g.DrawLine(dashGreenLinePen, pitchX + linex1, pitchY + liney1, pitchX + linex2, pitchY + liney2);
                        
                        // Draw yellow cross
                        g.DrawLine(yellowLinePen, (pitchX + linex2) - 2, (pitchY + liney2) - 2, (pitchX + linex2) + 2, (pitchY + liney2) + 2);
                        g.DrawLine(yellowLinePen, (pitchX + linex2) - 2, (pitchY + liney2) + 2, (pitchX + linex2) + 2, (pitchY + liney2) - 2);
                    }
                }

                // Draw the grey box around selected player
                if (selectedPlayer != null)
                {
                    g.DrawRectangle(dashGreyLinePen, new Rectangle(currentMouseX - mouseOffsetX, currentMouseY - mouseOffsetY, selectedRectWidth, selectedRectHeight));
                }
            }
            else
                DrawTacticSelectButton(g, "Select Tactic", "Select Tactic".Length, "");
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
            return new Rectangle((pitchX + x) - (selectedRectWidth / 2), (pitchY + y) - 20, selectedRectWidth, selectedRectHeight);
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
                        liney2 -= 24;
                        liney1 += 8;
                    }
                }
            }
        }

        void DrawTacticSelectButton(Graphics g, string tacticName, int maxsize, string shortCode)
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
            textBoxShortCode.Text = shortCode;
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
            var resNames = assembly.GetManifestResourceNames();
            string resourceName = "";
            foreach (var r in resNames)
            {
                if (r.EndsWith(imgFileName))
                {
                    resourceName = r;
                    break;
                }
            }
            using (var imgStream = assembly.GetManifestResourceStream(resourceName))
            return new Bitmap(imgStream); 
        }

        PrivateFontCollection LoadFont(string fontName)
        {
            var pfc = new PrivateFontCollection();

            var assembly = Assembly.GetExecutingAssembly();
            var resNames = assembly.GetManifestResourceNames();
            string resourceName = "";
            foreach (var r in resNames)
            {
                if (r.EndsWith(fontName))
                {
                    resourceName = r;
                    break;
                }
            }

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
            currentMouseX = e.X;
            currentMouseY = e.Y;

            if (e.Y > (pitchY - (tacticsButton.Height + 2)) && e.Y < pitchY && 
                ((e.X > pitchX && e.X < pitchX + 20) ||
                (e.X > ((pitchX + tacticsButton.Width)-20) && e.X < (pitchX + tacticsButton.Width)) ))
            {
                Cursor.Current = Cursors.Hand;
            }
            else
                Cursor.Current = Cursors.Default;

            // Keep drawing on mouse move if we are dragging player
            if (selectedPlayer != null)
                Invalidate();
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
            if (selectedFormation != -1 && selectedPlayer == null)
            {
                var formation = Formations[selectedFormation];
                for (int i = 0; i < formation.Players.Count; i++)
                {
                    var player = formation.Players[i];
                    var rect = GetPlayersRectangle(player, formation);

                    if (rect.Contains(e.X, e.Y))
                    {
                        selectedPlayer = player;
                        selectedIsRunning = e.Button == MouseButtons.Right;
                        mouseOffsetX = e.X - rect.X;
                        mouseOffsetY = e.Y - rect.Y;
                        Invalidate();
                        break;
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

            if (selectedPlayer != null)
            {
                // run through all possible positions. Fake players to get the coords
                var tempPlayer = new Player();
                bool playerSwapped = false;
                double score = -1;
                int closestPos = 0;
                for (int pos = 0; pos <= 0x1e; pos++)
                {
                    tempPlayer.Position = tempPlayer.RunningTo = pos;
                    var rect = GetPlayersRectangle(tempPlayer, Formations[selectedFormation]);

                    // Check if position is occupied
                    bool positionOccupied = false;
                    foreach (var possiblePlayer in Formations[selectedFormation].Players)
                    {
                        if (pos == possiblePlayer.Position)
                        {
                            positionOccupied = true;
                            break;
                        }
                    }

                    // If not occupied, make it easier to move into
                    if (!positionOccupied)
                        rect.Inflate(5, 15);

                    if (rect.Contains(e.X, e.Y))
                    {
                        var newScore = Math.Abs(GetDistance(e.X, e.Y, rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2)));
                        if (score == -1 || newScore < score)
                        {
                            closestPos = pos;
                            score = newScore;
                        }
                    }
                }

                if (score != -1)
                {
                    if (selectedIsRunning)
                    {
                        selectedPlayer.RunningTo = closestPos;
                    }
                    else
                    {
                        // See if another player already has this pos
                        var currentPlayers = Formations[selectedFormation].Players;
                        foreach (var currentPlayer in currentPlayers)
                        {
                            if (currentPlayer.Position == closestPos)
                            {
                                // Swap players
                                var savedPos = currentPlayer.Position;
                                var savedRunning = currentPlayer.RunningTo;
                                currentPlayer.Position = selectedPlayer.Position;
                                currentPlayer.RunningTo = selectedPlayer.RunningTo;
                                selectedPlayer.Position = savedPos;
                                selectedPlayer.RunningTo = savedRunning;
                                playerSwapped = true;
                            }
                        }
                        if (!playerSwapped)
                        {
                            selectedPlayer.Position = selectedPlayer.RunningTo = closestPos;
                        }
                    }
                }

                selectedPlayer = null;
            }

            UpdateTacticsHex();
            Invalidate();
        }

        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
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
            if (string.IsNullOrEmpty(textBoxExeFile.Text))
            {
                MessageBox.Show("Please select a CM9798 v2.93 CM2E16.EXE first!", "CM97/98 Tactics Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var fs = File.Open(textBoxExeFile.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // Check it's actually a 2.93
                    fs.Seek(0x145598, SeekOrigin.Begin);
                    byte[] versionBytes = new byte[3];
                    fs.Read(versionBytes, 0, 3);
                    if (versionBytes[0] != '2' || versionBytes[1] != '.' || versionBytes[2] != '9')
                    {
                        MessageBox.Show("The executable: " + textBoxExeFile.Text + " Does not appear to be a CM9798 v2.93 CM2E16.EXE!", "CM97/98 Tactics Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Read the formation names
                    string[] ourFormationNames = new string[Formation.Formations.Length];
                    fs.Seek(0x149660, SeekOrigin.Begin);
                    for (int i = 0; i < Formation.Formations.Length; i++)
                    {
                        var blankBytes = new byte[Formation.FormationsStringSize[i] + 1];
                        fs.Read(blankBytes, 0, blankBytes.Length);
                        ourFormationNames[i] = Encoding.ASCII.GetString(blankBytes);
                    }

                    // Read Positions
                    var grid = new byte[(5 * 6) + 1];
                    var grid2 = new byte[(5 * 6) + 1];

                    fs.Seek(0x153EE4, SeekOrigin.Begin);

                    Formations = new List<Formation>();

                    for (int formationNo = 0; formationNo < Formation.Formations.Length; formationNo++)
                    {
                        var formation = new Formation();
                        formation.Name = CleanString(ourFormationNames[formationNo]);
                        formation.MaxNameSize = Formation.FormationsStringSize[formationNo];

                        var byteData = new byte[11 + 5 + 11 + 5];
                        fs.Read(byteData, 0, byteData.Length);
                        BytesToFormation(formation, byteData);

                        Formations.Add(formation);
                    }

                    selectedFormation = 0;

                    UpdateTacticsHex();
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred when loading the executable: " + textBoxExeFile.Text + "\r\n\r\nError: " + ex.Message, "CM97/98 Tactics Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        string CleanString(string s)
        {
            var x = s.IndexOf('\0');
            return x == -1 ? s : s.Substring(0, x);
        }

        private void UpdateTacticsHex()
        {
            if (Formations.Count != 0)
            {
                var formation = Formations[selectedFormation];
                var dataBytes = FormationToBytes(formation);
                textBoxTacticsHex.Text = CleanString(formation.Name).Replace(" ", "\u00A0").Replace("-", "\u2011") + ":" + BytesToHexString(dataBytes);
            }
        }

        private void textBoxTacticName_TextChanged(object sender, EventArgs e)
        {
            if (selectedFormation != -1)
            {
                Formations[selectedFormation].Name = CleanString(textBoxTacticName.Text);
                UpdateTacticsHex();
                Invalidate();
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fs = File.Open(textBoxExeFile.Text, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    // Write new formation names
                    fs.Seek(0x149660, SeekOrigin.Begin);
                    for (int i = 0; i < Formations.Count; i++)
                    {
                        var blankBytes = new byte[Formation.FormationsStringSize[i] + 1];
                        var wordBytes = Encoding.ASCII.GetBytes(Formations[i].Name);
                        Array.Copy(wordBytes, blankBytes, wordBytes.Length);
                        fs.Write(blankBytes, 0, blankBytes.Length);
                    }

                    // Write the new formations
                    fs.Seek(0x153EE4, SeekOrigin.Begin);
                    for (int formationNo = 0; formationNo < Formation.Formations.Length; formationNo++)
                    {
                        var byteData = FormationToBytes(Formations[formationNo]);
                        fs.Write(byteData, 0, byteData.Length);
                    }

                    MessageBox.Show("Saved Successfully!", "CM97/98 Tactics Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred when saving the executable: " + textBoxExeFile.Text + "\r\n\r\nError: " + ex.Message, "CM97/98 Tactics Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void BytesToFormation(Formation formation, byte [] bytes)
        {
            var players = formation.Players;
            for (int i = 0; i < 11; i++)
            {
                players[i].Position = bytes[i];
            }
            Array.Copy(bytes, 11, formation.subs1, 0, 5);
            for (int i = 0; i < 11; i++)
            {
                players[i].RunningTo = bytes[i + 11 + 5];
            }
            Array.Copy(bytes, 11 + 5 + 11, formation.subs2, 0, 5);
        }

        byte[] FormationToBytes(Formation formation)
        {
            byte[] ret = new byte[11 + 5 + 11 + 5];
            var players = formation.Players;
            for (int i = 0; i < 11; i++)
            {
                ret[i] = (byte)players[i].Position;
            }
            Array.Copy(formation.subs1, 0, ret, 11, 5);
            for (int i = 0; i < 11; i++)
            {
                ret[11 + 5 + i] = (byte)players[i].RunningTo;
            }
            Array.Copy(formation.subs2, 0, ret, 11 + 5 + 11, 5);
            return ret;
        }

        byte[] HexStringToBytes(string hexString)
        {
            byte[] ret = new byte[hexString.Length / 2];
            hexString = hexString.ToLower();
            for (int i = 0; i < hexString.Length; i += 2)
            {
                ret[i / 2] = byte.Parse(hexString.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return ret;
        }

        string BytesToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        private void textBoxTacticsHex_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var hexString = textBoxTacticsHex.Text;
                if (hexString.Length > 0)
                {
                    var idx = hexString.IndexOf(':') == -1 ? 0 : hexString.IndexOf(':');
                    if (idx != 0)
                    {
                        var tacticName = hexString.Substring(0, idx).Replace("\u00A0", " ").Replace("\u2011", "-");
                        if (tacticName.Length > 0 && tacticName.Length <= Formation.FormationsStringSize[selectedFormation])
                            textBoxTacticName.Text = hexString.Substring(0, idx).Replace("\u00A0", " ").Replace("\u2011", "-");
                    }
                    hexString = hexString.Substring(idx + 1);
                    if (hexString.Length == 64)
                    {
                        var dataBytes = HexStringToBytes(hexString);
                        BytesToFormation(Formations[selectedFormation], dataBytes);
                        Invalidate();
                    }
                }
            }
            catch
            {
                // Do nothing - probably bad input
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            if (selectedFormation >= 0)
                textBoxTacticsHex.Text = Formation.OriginalFormations[selectedFormation];
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

        public static int[] FormationsStringSize = new int[]
        {
            "Ultra Defensive".Length,
            "5-3-2 Defensive".Length,
            "Sweeper Defensive".Length+2,
            "4-4-2 Defensive".Length,
            "4-5-1 Defensive".Length,
            "Counter Attack".Length+1,
            "5-3-2 Formation".Length,
            "3-5-2 Sweeper".Length+2,
            "3-5-2 Formation".Length,
            "3-1-3-3 Formation".Length+2,
            "4-2-2 Formation".Length,
            "Christmas Tree".Length+1,
            "Diamond Formation".Length+2,
            "4-3-3 Formation".Length,
            "5-3-2 Attacking".Length,
            "4-4-2 Attacking".Length,
            "4-3-3 Attacking".Length,
            "4-2-4 Attacking".Length,
            "All Out Attack".Length+1
        };

        public static string[] FormationCode = new string[]
        {
            "VDEF",
            "532VD",
            "SWPVD",
            "442D",
            "451D",
            "CNTN",
            "532N",
            "352SW",
            "352N",
            "AJAX",
            "442N",
            "CMSN",
            "DIAN",
            "433N",
            "532A",
            "442A",
            "433VA",
            "424VA",
            "VATT"
        };

        public static string[] OriginalFormations = new string[]
        {
            "Ultra Defensive:000A060903070E0C0D121C1200121C1C0019150403020E0C0D171C1200121C1C",
            "5‑3‑2 Defensive:000A06090307130D1D1B111C00121C08001915090D071E0D1D1B1A1C00121C08",
            "Sweeper Defensive:000F0B09070313121D1B111C00121C08001E1A09070D18121D1B161C00121C08",
            "4‑4‑2 Defensive:000A0609070E140C1D1B101C00081212000A0609071814161D1B101C00081212",
            "4‑5‑1 Defensive:000A0609070D14131C11101C001C1208000A0609070D181D1C1B161C001C1208",
            "Counter Attack:000F0B0809070E0C13111C1C001C0812001E1A08090719151D1B171C001C0812",
            "5‑3‑2 Formation:000F0B09080713121D1B111C00121C0800191509080713171D1B111C00121C08",
            "3‑5‑2 Sweeper:00090307131114121D1B101C00081C0800090D07131114171D1B101C00081C08",
            "3‑5‑2 Formation:00090807131114121D1B101C00081C0800090807131114171D1B101C00081C08",
            "3‑1‑3‑3 Formation:000A06080D12131119151C0008121C1C000F0B0808171D1B19151C0008121C1C",
            "4‑4‑2 Formation:000A0609071314111D1B101C00081212000A060907131E111D1B1A1C00081212",
            "Christmas Tree:000A0609070D14171C12101C00081212000F0B09070D191C1C17151C00081212",
            "Diamond Formation:000A0609070D14171D1B101C00081212000A0609071219121D1B151C00081212",
            "4‑3‑3 Formation:000A06090712131C1D1B111200081C12000F0B090712131C1E1A111200081C12",
            "5‑3‑2 Attacking:000F0B09080713171D1B111C00121C080019150903071E1C1D1B1A1C00121C08",
            "4‑4‑2 Attacking:000A0609071319111D1B151C00081212000F0B09070E1E0C1D1B1A1C00081212",
            "4‑3‑3 Attacking:000A06090712131C1D1B111200081C120014100907121E1C1D1B1A1200081C12",
            "4‑2‑4 Attacking:000A060907131E111D1B1A1200081208000A0609070E180C1D1B161200081208",
            "All Out Attack:000907081410121A1E1D1B0800120A06000A06080E0C121A1E1D1B0800120A06"
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

