namespace SnowfallForm
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public partial class SnowfallForm : Form
    {
        private List<Snowflake> snowflakes;
        private Timer timer;

        // Constants for topmost state
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public SnowfallForm()
        {
            InitializeComponent();

            // Set window properties
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TransparencyKey = BackColor;
            DoubleBuffered = true;

            // Always stay on top
            TopMost = true;

            InitializeSnowfall();

            timer = new Timer();
            timer.Interval = 16; // Use a more common frame rate (around 60 FPS)
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void InitializeSnowfall()
        {
            snowflakes = new List<Snowflake>();

            SnowflakeInfo[] snowflakeInfoArray = {
        new SnowflakeInfo("snowflake1"),
        new SnowflakeInfo("snowflake2"),
        new SnowflakeInfo("snowflake3"),
        new SnowflakeInfo("snowflake4"),
        // Add more images as needed
    };

            for (int i = 0; i < 100; i++)
            {
                int x = new Random().Next(ClientSize.Width);
                int y = new Random().Next(ClientSize.Height);
                int speed = new Random().Next(5, 15);
                int size = new Random().Next(15, 60);
                float opacity = 1.0f;
                snowflakes.Add(new Snowflake(x, y, speed, snowflakeInfoArray[i % snowflakeInfoArray.Length], size, opacity));
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateSnowflakes();
            Invalidate(); // Force repaint
        }

        private void UpdateSnowflakes()
        {
            foreach (var snowflake in snowflakes)
            {
                snowflake.Update(ClientSize.Height, ClientSize.Width);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var offScreenBuffer = new Bitmap(ClientSize.Width, ClientSize.Height))
            using (var graphics = Graphics.FromImage(offScreenBuffer))
            {
                graphics.Clear(TransparencyKey);

                foreach (var snowflake in snowflakes)
                {
                    snowflake.Draw(graphics);
                }

                e.Graphics.DrawImage(offScreenBuffer, 0, 0);
            }
        }

        // Override CreateParams to make the form topmost
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x8; // WS_EX_TOPMOST
                return createParams;
            }
        }
    }

    public class Snowflake
    {
        private static readonly Random random = new Random();

        public int X { get; set; }
        public int Y { get; set; }
        public int Speed { get; }
        public SnowflakeInfo Info { get; }
        public int Size { get; }
        public float Opacity { get; set; }
        public float RotationAngle { get; private set; }
        private float horizontalSpeed;
        private int oscillationCounter;

        public Snowflake(int x, int y, int speed, SnowflakeInfo info, int size, float opacity)
        {
            X = x;
            Y = y;
            Speed = speed;
            Info = info;
            Size = size;
            Opacity = opacity;

            horizontalSpeed = speed / 2.0f;
            oscillationCounter = random.Next(100);
        }

        public void Update(int clientHeight, int clientWidth)
        {
            X += (int)horizontalSpeed;
            oscillationCounter++;
            Y += (int)(Math.Sin(oscillationCounter * 0.1) * 2);

            if (Y > clientHeight)
            {
                Y = 0;
                X = random.Next(clientWidth);
                Opacity = 1.0f;
            }

            if (X > clientWidth)
            {
                X = 0;
                Opacity = 1.0f;
            }

            if (random.Next(100) < 5)
            {
                Opacity -= 0.05f;
                if (Opacity < 0.1f)
                    Opacity = 0.1f;
            }

            // Update rotation angle (adjust the multiplier for desired rotation speed)
            RotationAngle += 1.0f;
        }

        public void Draw(Graphics g)
        {
            using (var imageAttributes = new ImageAttributes())
            {
                var colorMatrix = new ColorMatrix
                {
                    Matrix33 = Opacity
                };
                imageAttributes.SetColorMatrix(colorMatrix);

                // Apply rotation transformation
                g.TranslateTransform(X + Size / 2, Y + Size / 2);
                g.RotateTransform(RotationAngle);
                g.TranslateTransform(-(X + Size / 2), -(Y + Size / 2));

                // Draw the rotated image
                g.DrawImage(
                    Info.Image,
                    new Rectangle(X, Y, Size, Size),
                    0, 0, Info.Width, Info.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes
                );

                // Reset the transformations
                g.ResetTransform();
            }
        }
    }

    public class SnowflakeInfo
    {
        public Image Image { get; }
        public int Width { get; }
        public int Height { get; }

        public SnowflakeInfo(string imageName)
        {
            Image = Snowfall.Properties.Resources.ResourceManager.GetObject(imageName) as Image;

            if (Image == null)
            {
                throw new ArgumentNullException(nameof(imageName), "Image not found in resources.");
            }

            Width = Image.Width;
            Height = Image.Height;
        }
    }


}
