using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;
using Firmata.NET;                  // for Arduino class

namespace SkeletalViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private Arduino arduino;

        public MainWindow()
        {
            InitializeComponent();
            init();
        }
        private void init()
        {
            arduino = new Arduino("COM4", 57600, true, 500);
            arduino.pinMode(3, Arduino.PWM);
            arduino.pinMode(5, Arduino.PWM);
            arduino.pinMode(6, Arduino.PWM);
            arduino.pinMode(9, Arduino.PWM);
            arduino.pinMode(10, Arduino.PWM);
            arduino.pinMode(11, Arduino.PWM); 
        }

        double ShouRX, ShouRY, ShouRZ;
        double ElRX, ElRY,  ElRZ;
        double WrRX, WrRY, WrRZ;
        double HandRX, HandRY, HandRZ;
        double HandLY;
        double HipLY;

        Runtime nui;
        int totalFrames = 0;
        int lastFrames = 0;
        DateTime lastTime = DateTime.MaxValue;

        // We want to control how depth data gets converted into false-color data
        // for more intuitive visualization, so we keep 32-bit color frame buffer versions of
        // these, to be updated whenever we receive and process a 16-bit frame.
        const int RED_IDX = 2;
        const int GREEN_IDX = 1;
        const int BLUE_IDX = 0;
        byte[] depthFrame32 = new byte[320 * 240 * 4];
     
        Dictionary<JointID,Brush> jointColors = new Dictionary<JointID,Brush>() { 
            {JointID.HipCenter, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.Spine, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {JointID.ShoulderCenter, new SolidColorBrush(Color.FromRgb(168, 230, 29))},
            {JointID.Head, new SolidColorBrush(Color.FromRgb(200, 0,   0))},
            {JointID.ShoulderLeft, new SolidColorBrush(Color.FromRgb(79,  84,  33))},
            {JointID.ElbowLeft, new SolidColorBrush(Color.FromRgb(84,  33,  42))},
            {JointID.WristLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HandLeft, new SolidColorBrush(Color.FromRgb(215,  86, 0))},
            {JointID.ShoulderRight, new SolidColorBrush(Color.FromRgb(33,  79,  84))},
            {JointID.ElbowRight, new SolidColorBrush(Color.FromRgb(33,  33,  84))},
            {JointID.WristRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {JointID.HandRight, new SolidColorBrush(Color.FromRgb(37,   69, 243))},
            {JointID.HipLeft, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {JointID.KneeLeft, new SolidColorBrush(Color.FromRgb(69,  33,  84))},
            {JointID.AnkleLeft, new SolidColorBrush(Color.FromRgb(229, 170, 122))},
            {JointID.FootLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {JointID.HipRight, new SolidColorBrush(Color.FromRgb(181, 165, 213))},
            {JointID.KneeRight, new SolidColorBrush(Color.FromRgb(71, 222,  76))},
            {JointID.AnkleRight, new SolidColorBrush(Color.FromRgb(245, 228, 156))},
            {JointID.FootRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))}
        };

        private void Window_Loaded(object sender, EventArgs e)
        {
            nui = new Runtime();
            try
            {
                nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }

            try
            {
                nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Failed to open stream. Please make sure to specify a supported image type and resolution.");
                return;
            }

            lastTime = DateTime.Now;

            nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_ColorFrameReady);
        }

        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        byte[] convertDepthFrame(byte[] depthFrame16)
        {
            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 += 2, i32 += 4)
            {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16+1] << 5) | (depthFrame16[i16] >> 3);
                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));

                depthFrame32[i32 + RED_IDX] = 0;
                depthFrame32[i32 + GREEN_IDX] = 0;
                depthFrame32[i32 + BLUE_IDX] = 0;

                // choose different display colors based on player
                switch (player)
                {
                    case 0:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 2);
                        break;
                    case 1:
                        depthFrame32[i32 + RED_IDX] = intensity;
                        break;
                    case 2:
                        depthFrame32[i32 + GREEN_IDX] = intensity;
                        break;
                    case 3:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 4);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 4:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity / 4);
                        break;
                    case 5:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 4);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 6:
                        depthFrame32[i32 + RED_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(intensity / 2);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(intensity);
                        break;
                    case 7:
                        depthFrame32[i32 + RED_IDX] = (byte)(255 - intensity);
                        depthFrame32[i32 + GREEN_IDX] = (byte)(255 - intensity);
                        depthFrame32[i32 + BLUE_IDX] = (byte)(255 - intensity);
                        break;
                }
            }
            return depthFrame32;
        }

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage Image = e.ImageFrame.Image;
            byte[] convertedDepthFrame = convertDepthFrame(Image.Bits);

            depth.Source = BitmapSource.Create(
                Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, convertedDepthFrame, Image.Width * 4);

            ++totalFrames;
            
            DateTime cur = DateTime.Now;
            if (cur.Subtract(lastTime) > TimeSpan.FromSeconds(1))
            {
                int frameDiff = totalFrames - lastFrames;
                lastFrames = totalFrames;
                lastTime = cur;
                middle.Text = " ";
                upper.Text = " ";
                lower.Text = " ";
                
            }
        }

        private Point getDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            nui.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = Math.Max(0, Math.Min(depthX * 320, 320));  //convert to 320, 240 space
            depthY = Math.Max(0, Math.Min(depthY * 240, 240));  //convert to 320, 240 space
            int colorX, colorY;
            ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            return new Point((int)(skeleton.Width * colorX / 640.0), (int)(skeleton.Height * colorY / 480));
        }

        Polyline getBodySegment(Microsoft.Research.Kinect.Nui.JointsCollection joints, Brush brush, params JointID[] ids)
        {
            PointCollection points = new PointCollection(ids.Length);
            for (int i = 0; i < ids.Length; ++i )
            {
                points.Add(getDisplayPosition(joints[ids[i]]));
            }

            Polyline polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }

        int[] the;
        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;
            Brush[] brushes = new Brush[6];
            brushes[0] = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            brushes[1] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            brushes[2] = new SolidColorBrush(Color.FromRgb(64, 255, 255));
            brushes[3] = new SolidColorBrush(Color.FromRgb(255, 255, 64));
            brushes[4] = new SolidColorBrush(Color.FromRgb(255, 64, 255));
            brushes[5] = new SolidColorBrush(Color.FromRgb(128, 128, 255));

            skeleton.Children.Clear();
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    // Draw bones
                    Brush brush = brushes[iSkeleton % brushes.Length];
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter, JointID.Head));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderLeft, JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderRight, JointID.ElbowRight, JointID.WristRight, JointID.HandRight));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft, JointID.AnkleLeft, JointID.FootLeft));
                    skeleton.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipRight, JointID.KneeRight, JointID.AnkleRight, JointID.FootRight));

                                        JointsCollection jc = data.Joints;

                    StringBuilder sb = new StringBuilder();
                    StringBuilder up = new StringBuilder();
                    StringBuilder lo = new StringBuilder();
                    int k = 0;
                    foreach (JointID jid in Enum.GetValues(typeof(JointID)))
                    {
                        if (jid != JointID.Count)
                        {
                            k++;
                            Microsoft.Research.Kinect.Nui.Vector v = jc[jid].Position;
                            if (k <= 4)
                            {
                                sb.Append(string.Format("{0} = {1}, {2}, {3}\n", jid, v.X, v.Y, v.Z));
                            }
                            else if (k <= 12)
                            {
                                up.Append(string.Format("{0} = {1}, {2}, {3}\n", jid, v.X, v.Y, v.Z));
                                
                                if (k == 8)//HandLeft
                                {
                                    HandLY = v.Y;
                                    up.Append(string.Format("\n"));
                                }
                                if (k == 9)//ShoulderRight
                                {
                                    ShouRX = v.X;
                                    ShouRY = v.Y;
                                    ShouRZ = v.Z;
                                }
                                if (k == 10)//ElbowRight
                                {
                                    ElRX = v.X;
                                    ElRY = v.Y;
                                    ElRZ = v.Z;
                                }
                                if (k == 11)//WristRight
                                {
                                    WrRX = v.X;
                                    WrRY = v.Y;
                                    WrRZ = v.Z;
                                }
                                if (k == 12)//HandRight
                                {
                                    HandRX = v.X;
                                    HandRY = v.Y;
                                    HandRZ = v.Z;
                                }
                            }
                            else if (k <= 20)
                            {
                                lo.Append(string.Format("{0} = {1}, {2}, {3}\n", jid, v.X, v.Y, v.Z));
                                if (k == 13)//HipLeft
                                {   HipLY = v.Y; }
                                if (k == 16)
                                {   lo.Append(string.Format("\n"));  }
                            }
                        }
                    }

                    middle.Text = sb.ToString();
                    upper.Text = up.ToString();
                    lower.Text = lo.ToString();

                    //Arduino Control 1st Rotation Angle
                    double angROT1 = Math.Atan((HandRX - ShouRX) / (HandRY - ShouRY));
                    if (angROT1 < 0)
                    { angROT1 = 0.5 * Math.PI - angROT1; }
                    arduino.analogWrite(3,  Convert.ToInt32(Math.Floor(angROT1*180/Math.PI)));

                    //Arduino Control 2nd Rotation Angle (Shoulder)
                    double angROT2 = Math.Atan((ShouRZ - ElRZ) / Math.Sqrt(Math.Pow((ElRY - ShouRY), 2) + Math.Pow((ElRX - ShouRX), 2)));
                    if (angROT2 < 0)
                    { angROT2 += Math.PI; }
                    if (ElRX < ShouRX)
                    { angROT2 = Math.PI - angROT2; }
                    int degRot2 = Convert.ToInt32(Math.Floor(angROT2 * 180 / Math.PI)); 
                    //Constrained by mechanism
                    if (degRot2 < 25)
                    { degRot2 = 25; }
                    else if (degRot2 > 155)
                    { degRot2 = 155; }
                    arduino.analogWrite(5, degRot2);

                    //Arduino Control 3rd Rotation Angle (Elbo)
                    double ax = HandRX - ElRX;
                    double ay = HandRY - ElRY;
                    double az = HandRZ - ElRZ;
                    double bx = ElRX - ShouRX;
                    double by = ElRY - ShouRY;
                    double bz = ElRZ - ShouRZ;
                    double angROT3 = Math.Acos((ax * bx + ay * by + az * bz) / Math.Sqrt(ax * ax + ay * ay + az * az) / Math.Sqrt(bx * bx + by * by + bz * bz));
                    arduino.analogWrite(10, Convert.ToInt32(Math.Floor(angROT3 * 180 / Math.PI)));

                    //Arduino Control 4th Rotation Angle (Grab)
                    if (HandLY > HipLY)
                    { arduino.analogWrite(9, 30);  }
                    else
                    { arduino.analogWrite(9, 110); }

                    // Draw joints
                    foreach (Joint joint in data.Joints)
                    {
                        Point jointPos = getDisplayPosition(joint);
                        Line jointLine = new Line();
                        jointLine.X1 = jointPos.X - 3;
                        jointLine.X2 = jointLine.X1 + 6;
                        jointLine.Y1 = jointLine.Y2 = jointPos.Y;
                        jointLine.Stroke = jointColors[joint.ID];
                        jointLine.StrokeThickness = 6;
                        skeleton.Children.Add(jointLine);
                    }
                }
                iSkeleton++;
            } // for each skeleton
        }

        void nui_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            // 32-bit per pixel, RGBA image
            PlanarImage Image = e.ImageFrame.Image;
            video.Source = BitmapSource.Create(
                Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, Image.Bits, Image.Width * Image.BytesPerPixel);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                arduino.Close();    // this will close serial port
            }
            catch (Exception)
            {
            }
            nui.Uninitialize();
            Environment.Exit(0);
        }

        private void frameRate_TextChanged1(object sender, TextChangedEventArgs e)
        {
        }

        private void frameRate_TextChanged2(object sender, TextChangedEventArgs e)
        {
        }

        private void frameRate_TextChanged(object sender, TextChangedEventArgs e)
        {
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
        }
    }
}
