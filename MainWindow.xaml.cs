//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


// Práctica realizada por:
//	José Miguel Navarro Moreno
//	José Antonio Larrubia García
//

//Usamos como base para la práctica los proyectos del kinect developer toolkit
//correspondientes al de imagen a color: ColorBasics-WPF y el que detecta el esqueleto: SkeletonBasics-WPF. 
namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);


        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

		/// <summary>
        /// Variable booleana que comprobará si el usuario está o no en la posicion inicial especificada
        /// </summary>
        private Boolean comprobadaPosIni = false;

		/// <summary>
        /// Variable de clase creada por nosotros, ver Mov.cs para ver lo que hace la clase, se encarga de que el usuario realice el movimiento.
		/// </summary>
        Mov movimientos;
        RectMov Rec;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
			/// <summary>
			/// Inicializamos los componentes, tanto los de la interfaz gráfica como los del movimiento a realizar.
			/// </summary>
            movimientos = new Mov(this);
            Rec = new RectMov(0,0,2.7F);
            InitializeComponent();
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);
            
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();
                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.imageSource;
                

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;
                


                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {

                if (skeletonFrame != null)
                {
                    
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Dibuja lo que contenga colorBitmap con el tamaño especificado
                dc.DrawImage(this.colorBitmap, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                //CroppedBitmap l = new CroppedBitmap(colorBitmap,new Int32Rect(0, 0, (int)RenderWidth / 2, (int)RenderHeight / 2));
                //dc.DrawImage(l, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                //System.Drawing.Image i = new System.Drawing.Bitmap(@"Images\img1.jpg");
                //ImageSourceConverter a = new ImageSourceConverter();
                //dc.DrawImage((ImageSource)a.ConvertFrom(i), new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                //Comprobamos que kinect nos haya leido el esqueleto
                if (skeletons.Length != 0)
                {
					// Coge cada esqueleto de cada frame para manejarlo dentro.
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

						//Asignamos al esqueleto de este frame el estado del rastreo del esqueleto
						//Si detecta los puntos de las articulaciones entra en este bloque.
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
							// Detectamos si el esqueleto esta en la posición que queremos
							// Si no está en la posición correcta entrará
                            if (skel.Position.Z < 2.7 || skel.Position.Z>3.3)
                            { 	
								// Reiniciamos el primer movimiento para que si ha salido de la zona
								// tenga que volver a colocarse y empezar de nuevo el movimiento.
                                //movimientos.ReiniciarMovimiento();
								
								// Dibuja una flecha en el suelo que indica la posición que debe tener el usuario.
								// A mayor distancia de la posición más grande será la flecha.
								// Calculamos la posición del usuario y a donde tiene que dirigirse.								
                                SkeletonPoint p_aux1 = new SkeletonPoint();
                                p_aux1.X = (skel.Joints[JointType.FootLeft].Position.X + skel.Joints[JointType.FootRight].Position.X)/2;
                                p_aux1.Y = skel.Joints[JointType.FootLeft].Position.Y;
                                p_aux1.Z = 3.0F;
                                SkeletonPoint p_aux2 = new SkeletonPoint();
                                p_aux2.X = (skel.Joints[JointType.FootLeft].Position.X + skel.Joints[JointType.FootRight].Position.X) / 2;
                                p_aux2.Y = skel.Joints[JointType.FootLeft].Position.Y;
                                p_aux2.Z = (skel.Joints[JointType.FootLeft].Position.Z + skel.Joints[JointType.FootRight].Position.Z) / 2;
                                
                                this.DrawFlecha(p_aux2, p_aux1, dc);
                            }
							// Si hemos comprobado que el usuario se encuentra en la posición correcta entramos en este bloque.
                            else // detectamos postura
                            {
                                // Primero detectaremos si el usuario está en la postura inicial para poder iniciar el movimiento.
                                // de primeras nunca estará en la posicion inicial, usamos una booleana declarada e inicializada arriba para controlarlo.
                                /*if (!comprobadaPosIni)
                                {
									//Pintamos una figura que indica cual es la postura inicial que debe tener el usuario y mostramos un texto de ayuda.
                                    pi1.Visibility = Visibility.Visible;
                                    textoAyuda.Text = "Adopte posición inicial";
									//Comprobamos que esté o no en la postura correcta.
                                    comprobadaPosIni = comprobarPosicion(skel);
                                }
								//Si el usuario se encuentra en la posición y postura correcta entra en este bloque donde se empieza el primer movimiento.
                                else
                                {
                                    textoAyuda.Text = "Realice el movimiento";
                                    movimientos.Empezar(skel, dc, sensor);

                                }*/
                                Rec.dibujar(skel, dc, sensor);
                            }

                            //Dibujamos las articulaciones del esqueleto.
                            //this.DrawBonesAndJoints(skel, dc);
                            dc.DrawEllipse(Brushes.Red, null, this.SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position), JointThickness, JointThickness);
                            dc.DrawEllipse(Brushes.Red, null, this.SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position), JointThickness, JointThickness);

                        }
						// Si no detecta las articulaciones y sólo detecta la posicion entra en este bloque.
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
							// Pinta sólo el centro del esqueleto.
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }
		
		/// <summary>
        /// Dibuja una flecha que usamos para que el usuario se coloque en la posición inicial.
        /// </summary>
        /// <param name="p1"> Punto donde se encuentra el usuario</param>
        /// <param name="p2"> Punto a donde tiene que dirigirse el usuario</param>
		/// <param name="d"> Para poder dibujar lineas.</param>
        private void DrawFlecha(SkeletonPoint p1, SkeletonPoint p2, DrawingContext d) {
			//Dibuja una linea hacia la posición que queremos a partir de la posición del usuario.
            d.DrawLine(new Pen(Brushes.Red, 4), this.SkeletonPointToScreen(p1), this.SkeletonPointToScreen(p2));
            
			// Calculamos los puntos que se usarán para dibujar la flecha, 
			// para que sea más grande si está más lejos usamos como cáculos el módulo del vector que forman los puntos introducidos.
			double mod = Math.Sqrt(Math.Pow((p1.X - p2.X), 2)+ Math.Pow((p1.Y - p2.Y), 2)+ Math.Pow((p1.Z - p2.Z), 2));
            SkeletonPoint p_aux1 = new SkeletonPoint();
            p_aux1.X = (float)(p2.X + 0.25);
            p_aux1.Y = p2.Y;
            if (p1.Z < p2.Z)
            {
                p_aux1.Z = p2.Z - 0.35F;
            }
            else
            {
                p_aux1.Z = p2.Z + 0.35F;
            }
            d.DrawLine(new Pen(Brushes.Red, 4), this.SkeletonPointToScreen(p_aux1), this.SkeletonPointToScreen(p2));

            SkeletonPoint p_aux2 = new SkeletonPoint();
            p_aux2.X = (float)(p2.X - 0.25);
            p_aux2.Y = p2.Y;
            if (p1.Z < p2.Z)
            {
                p_aux2.Z = p2.Z - 0.35F;
            }
            else
            {
                p_aux2.Z = p2.Z + 0.35F;
            }
            d.DrawLine(new Pen(Brushes.Red, 4), this.SkeletonPointToScreen(p_aux2), this.SkeletonPointToScreen(p2));
            

        }
		
		/// <summary>
        /// Comprueba la postura del usuario.
        /// </summary>
        /// <param name="skeleton"> esqueleto a comprobar su postura</param>
        private Boolean comprobarPosicion(Skeleton skeleton) {
			// Es llamada después de estar en la posición correcta del espacio.
			// Si no tiene la postura correcta los puntos de las articulaciones se pintarán de rojo, y devolverá false
			// si tiene la postura correcta se pintarán de verde no notandose ya que el esqueleto es verde y devuelve true.
            if ((skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ElbowLeft].Position.X + 0.1F ||
                skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ElbowLeft].Position.X - 0.1F) ||
                (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X + 0.1F ||
                skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X - 0.1F) ||
                (skeleton.Joints[JointType.ShoulderLeft].Position.X > skeleton.Joints[JointType.ElbowLeft].Position.X + 0.1F ||
                skeleton.Joints[JointType.ShoulderLeft].Position.X < skeleton.Joints[JointType.ElbowLeft].Position.X - 0.1F) ||
                (skeleton.Joints[JointType.ShoulderRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X + 0.1F ||
                skeleton.Joints[JointType.ShoulderRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X - 0.1F) ||
                (skeleton.Joints[JointType.ShoulderCenter].Position.X > skeleton.Joints[JointType.HipCenter].Position.X + 0.1F)
                )
            {
                
                trackedJointBrush = Brushes.Red;
                return false;

            }else if (skeleton.Joints[JointType.ShoulderCenter].Position.Z > skeleton.Joints[JointType.ElbowLeft].Position.Z + 0.1F ||
                   skeleton.Joints[JointType.ShoulderCenter].Position.Z > skeleton.Joints[JointType.HandLeft].Position.Z + 0.1F ||
                   skeleton.Joints[JointType.ShoulderCenter].Position.Z > skeleton.Joints[JointType.ElbowRight].Position.Z + 0.1F ||
                   skeleton.Joints[JointType.ShoulderCenter].Position.Z > skeleton.Joints[JointType.HandRight].Position.Z + 0.1F ||
                   skeleton.Joints[JointType.ShoulderCenter].Position.Z > skeleton.Joints[JointType.HipCenter].Position.Z + 0.1F)
            {
                trackedJointBrush = Brushes.Red;
                return false;
            }
            else {
                trackedJointBrush = Brushes.Green;
                return true;
            }

        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }
		
		/// <summary>
        /// Evento para controlar el margen que queremos para hacer el movimiento más o menos preciso que el que hemos puesto por defecto.
        /// </summary>
        private void mError_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            movimientos.setMargenError(0.01F * ((float)(mError.Value)));
        }
    }
}