using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;
using WobbrockLib;

namespace LaserTask_Lab
{
    public partial class Form1 : Form
    {
        VideoCapture Capture;
        double[] maxValues;
        double[] minvalues;
        List<Point> GesturePoints = new List<Point>();
        Point[] maxPoints;
        Point[] minPoints;
        private const int MinNoPoints = 2;
        private Recognizer _rec;

        private List<TimePointF> _points;
        private bool _similar;
        private bool _protractor = false;
        bool finished = false;
        public string[] FileName = { "v.xml", "rectangle.xml", "triangle.xml", "circle.xml", "zigzag.xml", "delete.xml" };
        double LaserMinInt = 0;
        List<Point> minpts = new List<Point>();
        List<Point> maxpts = new List<Point>();
        bool isPaused = false;
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
            Reset_PauseButton();
            button3.Enabled = false;
            button1.Enabled = true;
            pictureBox1.Hide();
            _rec = new Recognizer();
            _rec.ProgressChangedEvent += new ProgressEventHandler(OnProgressChanged);
            _points = new List<TimePointF>();
            _similar = true;
            Load_Gestures();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(Capture == null)
            {
                Capture = new VideoCapture();
            }
            Capture.ImageGrabbed += Capture_ImageGrabbed;
            Capture.Start();
            button2.Enabled = true;
            button3.Enabled = true;
            pictureBox1.Show();
            button1.Enabled = false;
            GesturePoints = new List<Point>();
            _points = new List<TimePointF>();
          }
        public void OnProgressChanged(object source, ProgressEventArgs e)
        {
           // prgTesting.Value = (int)(e.Percent * 100.0);
            //Application.DoEvents();
        }

        void MatchPixels(Point maxPoints , Point minPoints , Image<Gray,Byte> img)
        {
            
                minpts = new List<Point>();
                maxpts = new List<Point>();
                GesturePoints.Add(minPoints);
                int mx = maxPoints.X;
                int my = maxPoints.Y;
                int m2x = minPoints.X;
                int m2y = minPoints.Y;
                byte s = img.Data[maxPoints.Y, maxPoints.X, 0];
                byte s2 = img.Data[minPoints.Y, minPoints.X, 0];
                for (int r = 0; r < img.Height; r++)
                {
                    for (int c = 0; c < img.Width; c++)
                    {
                        byte a = img.Data[r, c, 0];
                        if (a == s && (r != my || c != mx))
                        {
                            maxpts.Add(new Point(c, r));
                        }
                    }
                }
                for (int r = 0; r < img.Height; r++)
                {
                    for (int c = 0; c < img.Width; c++)
                    {
                        byte a = img.Data[r, c, 0];
                        if (a == s2 && (r != m2y || c != m2x))
                        {
                            minpts.Add(new Point(c, r));
                        }
                    }
                
            }
        }
        void DisplayMinMax()
        {
            label1.Text = "";

            for (int i = 0; i < maxPoints.Length; i++)
            {
                label1.Text += ("maxValue " + i + " : " + maxValues[i] + "  Max Point " + i + " : " + maxPoints[i].X.ToString() + " and " + maxPoints[i].Y.ToString() + "\n");
            }
            for (int i = 0; i < minPoints.Length; i++)
            {
                label1.Text += ("minValue " + i + " : " + minvalues[i] + "  Min Point " + i + " : " + minPoints[i].X.ToString() + " and " + minPoints[i].Y.ToString() + "\n");
            }

        }
        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {

                Mat ret = new Mat();
                Capture.Retrieve(ret);
                Image<Bgr, byte> img2 = ret.ToImage<Bgr, byte>();

                Image<Gray,byte> img = ret.ToImage<Gray, byte>();


                img.SmoothGaussian(11);
            img._EqualizeHist();
              img._GammaCorrect(1.8d);
               Image<Hsv, Byte> hsvimg = img.Convert<Hsv, Byte>();
                Image<Gray, Byte>[] channels = hsvimg.Split();
                Image<Gray, Byte> imghue = channels[0];
                Image<Gray, Byte> imgval = channels[2];
               Image<Gray, byte> huefilter = imghue.InRange(new Gray(0), new Gray(128));
               Image<Gray, byte> valfilter = imgval.InRange(new Gray(150), new Gray(255));
              Image<Gray, byte> colordetimg = huefilter.And(valfilter);
                ret = img.Mat;
                ret.MinMax(out minvalues, out maxValues, out maxPoints, out minPoints);
                DisplayMinMax();
                if (minvalues.Length > 0)
                {
                    if (minvalues[0] <= LaserMinInt)
                    {
                        int r = img2.Data[minPoints[0].Y, minPoints[0].X, 0];
                            int g = img2.Data[minPoints[0].Y, minPoints[0].X, 1];
                        int b = img2.Data[minPoints[0].Y, minPoints[0].X, 2];
                   //     MessageBox.Show("red " + r + " blue " + b + " green " + g);
                   //     if ( r > 90 && r < 257 && b  < 10 && g < 10 )
               //         {
                            MatchPixels(maxPoints[0], minPoints[0], img);
//
                    //    }
                    
                    }
                }
                pictureBox1.Image = img.ToBitmap();
                //pictureBox2.Image = colordetimg.ToBitmap();
                pictureBox3.Image = img2.ToBitmap();
            }
            catch
            {
               
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Capture != null)
            {
                if (!isPaused)
                {
                    Capture.Pause();
                    isPaused = true;
                    Button b = (Button)sender;
                    b.Text = "Resume";
                }
                else
                {
                    Capture.Start();
                    Reset_PauseButton();
                }
            }
        }
        void Reset_PauseButton()
        {
            button2.Text = "Pause";
            isPaused = false;
        }
    
        private void button3_Click(object sender, EventArgs e)
        {
            Capture.Stop();
            pictureBox1.Hide();
            Capture = null;
            button2.Enabled = false;
            Reset_PauseButton();
            button3.Enabled = false;
            button1.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
          
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush b = new SolidBrush(Color.Red);
            if (isPaused)
            {
                for (int i = 0; i < GesturePoints.Count; i++)
                {

                    Pen p = new Pen(Color.Green,5);
                    if (i + 1 < GesturePoints.Count)
                    {
                        e.Graphics.DrawLine(p, GesturePoints[i], GesturePoints[i + 1]);
                    }
                }
            }
       
            for (int i = 0; i < maxpts.Count; i++)
            {
                b = new SolidBrush(Color.Yellow);
              // e.Graphics.FillEllipse(b, maxpts[i].X, maxpts[i].Y, 2, 2);

            }
            for (int i = 0; i < minpts.Count; i++)
            {
                b = new SolidBrush(Color.Violet);
             e.Graphics.FillEllipse(b, minpts[i].X, minpts[i].Y, 2, 2);

            }
            if (minPoints != null)
            {
                for (int i = 0; i < minPoints.Length; i++)
                {

                    b = new SolidBrush(Color.Red);
                    e.Graphics.FillEllipse(b, minPoints[i].X, minPoints[i].Y, 10, 10);

                }
            }
            if (maxPoints != null)
            {
                for (int i = 0; i < maxPoints.Length; i++)
                {

                    b = new SolidBrush(Color.Blue);
                    e.Graphics.FillEllipse(b, maxPoints[i].X, maxPoints[i].Y, 10, 10);

                }
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Mat ret = new Mat("test.png");   
            Image<Gray, byte> img = ret.ToImage<Gray, byte>();
          
            Image<Hsv, Byte> hsvimg = img.Convert<Hsv, Byte>();

   
            Image<Gray, Byte>[] channels = hsvimg.Split();  
            Image<Gray, Byte> imghue = channels[0];            
            Image<Gray, Byte> imgval = channels[2];          
            Image<Gray, byte> huefilter = imghue.InRange(new Gray(0), new Gray(128));
            Image<Gray, byte> valfilter = imgval.InRange(new Gray(150), new Gray(255));
            Image<Gray, byte> colordetimg = huefilter.And(valfilter);
            ret = img.Mat;
            ret.MinMax(out minvalues, out maxValues, out maxPoints, out minPoints);
            DisplayMinMax();
            MatchPixels(maxPoints[0], minPoints[0], img);
            pictureBox1.Image = img.ToBitmap();
            pictureBox1.Show();
            pictureBox2.Image = colordetimg.ToBitmap();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _points = new List<TimePointF>();
            for (int i = 0; i < GesturePoints.Count; i++)
            {
                _points.Add(new TimePointF(GesturePoints[i], 0));
            }
                if (_points.Count >= MinNoPoints)
            {
                if (_rec.NumGestures > 0)
                {
                    RecognizeAndDisplayResults();
                }
            }
        }
        private void RecognizeAndDisplayResults()
        {
             

            NBestList result = _rec.Recognize(_points, _protractor); 
           label2.Text = result.Name + " , " + Math.Round(result.Score,2 );

        }
       private void Load_Gestures()
        {
            for (int i = 0; i < FileName.Length; i++)
            {
                string name = FileName[i];
                _rec.LoadGesture(name);
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
   
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Gestures (*.xml)|*.xml";
            dlg.Title = "Load Gestures";
            dlg.Multiselect = true;
            dlg.RestoreDirectory = false;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                for (int i = 0; i < dlg.FileNames.Length; i++)
                {
                    string name = dlg.FileNames[i];
                    _rec.LoadGesture(name);
                }
       
            
        }
    }
    }
}
