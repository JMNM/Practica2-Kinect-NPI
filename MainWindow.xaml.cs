//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


// Práctica realizada por:
//	José Miguel Navarro Moreno
//	José Antonio Larrubia García


//Usamos como base para la práctica los proyectos del kinect developer toolkit
//correspondientes al de imagen a color: ColorBasics-WPF y el que detecta el esqueleto: SkeletonBasics-WPF.
//Además de código concreto sacado de webs que se especificará en su parte correspondiente. 
namespace Microsoft.Samples.Kinect.ColorBasics
{

    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.Interaction;
    using System.Globalization;

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
        private readonly Pen trackedBonePen1 = new Pen(Brushes.Green, 6);
        private readonly Pen trackedBonePen2 = new Pen(Brushes.Blue, 6);

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
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private DepthImagePixel[] depthPixels;
        
		/// <summary>
        /// Variable para usar los comandos de agarre y soltar implementados a partir de la versión 1.7 de kinect.
		/// </summary>
        private InteractionStream interactionStream;

        
		/// <summary>
        /// Información del usuario que necesita la clase InteractionStream.
		/// </summary>
        public UserInfo[] userInfos;

        /// <summary>
        /// Variable de clase creada por nosotros, ver Puzzle.cs para ver lo que hace la clase, se encarga de partir la imagen del puzzle y controlar los rectangulos de la clase RectImagen.
		/// </summary>
        private Puzzle puzzle;

        /// <summary>
        /// Imagen de la mano derecha abierta
		/// </summary>
        private BitmapImage manoDerA;

        /// <summary>
        /// Imagen de la mano derecha cerrada
		/// </summary>
        private BitmapImage manoDerC;

        /// <summary>
        /// Imagen de la mano derecha que se dibujará en cada momento.
		/// </summary>
        private BitmapImage manoDer;

        /// <summary>
        /// Imagen de la mano izquierdaa abierta
		/// </summary>
        private BitmapImage manoIzqA;

        /// <summary>
        /// Imagen de la mano izquierda cerrada
		/// </summary>
        private BitmapImage manoIzqC;

        /// <summary>
        /// Imagen de la mano izquierda que se dibujará en cada momento.
		/// </summary>
        private BitmapImage manoIzq;

        /// <summary>
        /// Imagen del tutorial.
		/// </summary>
        private BitmapImage tutorial;

        /// <summary>
        /// Variable que indica si ha acabado el tutorial.
		/// </summary>
        private bool fin_tuto = false;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            //Leemos las imagenes de las manos, el puzzle y el tutorial.
            puzzle = new Puzzle(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) +@"\Images\img1.jpg");
            manoDerA = new BitmapImage(new System.Uri(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + @"\Images\manoAbiertaDer.png"));
            manoDerC= new BitmapImage(new System.Uri(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + @"\Images\manoCerradaDer.png"));
            manoDer = manoDerA;
            manoIzqA = new BitmapImage(new System.Uri(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + @"\Images\manoAbiertaIzq.png"));
            manoIzqC = new BitmapImage(new System.Uri(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + @"\Images\manoCerradaIzq.png"));
            manoIzq = manoIzqA;
            tutorial = new BitmapImage(new System.Uri(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + @"\Images\tuto.png"));

            /// <summary>
            /// Inicializamos los componentes, tanto los de la interfaz gráfica como los del Puzzle.
            /// </summary>
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
                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Turn on the skeleton stream to receive skeleton frames
                sensor.SkeletonStream.Enable();

                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Turn on the depth stream to receive depth frames
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                //se actualiza la informacion del usuario que devuelva kinect.
                this.userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.imageSource;
                
                //Iniciamos el evento para que pueda detectar el abrir y cerrar de las manos.
                this.interactionStream = new InteractionStream(sensor, new DummyInteractionClient());

                //Añadimos un manejador de eventos que se llamará cuando detecte un nuevo InteractionStream.
                this.interactionStream.InteractionFrameReady += InteractionStreamOnInteractionFrameReady;
                
                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                //Asignamos al puzzle el sensor que está activo.
                this.puzzle.asignarSensor(this.sensor);

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
                //Bloque usado por InteractionStream
                if (skeletonFrame != null)
                {
                    //Se hace una copia del esqueleto se asigna la lectura del accelerometro y procesa interactionstream el esqueleto usando lo anterior.
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    //Este bloque está sacado de:
                    //////////////////////////////////////////////////////////////////////////////////////////////////
                    //http://dotneteers.net/blogs/vbandi/archive/2013/05/03/kinect-interactions-with-wpf-part-iii-demystifying-the-interaction-stream.aspx
                    //////////////////////////////////////////////////////////////////////////////////////////////////
                    Vector4 accelerometerReading = sensor.AccelerometerGetCurrentReading();
                    interactionStream.ProcessSkeleton(skeletons, accelerometerReading, skeletonFrame.Timestamp);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                //Dibuja los rectángulos que no están cogidos.
                puzzle.DrawPuzzle(dc);

                //Mostramos por pantalla el tiempo que nos queda.
                FormattedText t= new FormattedText(puzzle.getTiempo()+" s", CultureInfo.GetCultureInfo("es-es"), FlowDirection.LeftToRight, new Typeface("Verdana"),
                                            24,
                                            Brushes.Black);
                dc.DrawText(t, new Point(570, 10));

                //Comprobamos que kinect nos haya leido el esqueleto
                if (skeletons.Length != 0)
                {
					// Coge cada esqueleto de cada frame para manejarlo dentro.
                    foreach (Skeleton skel in skeletons)
                    {
						//Asignamos al esqueleto de este frame el estado del rastreo del esqueleto
						//Si detecta los puntos de las articulaciones entra en este bloque.
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            //Comprobamos si se ha acabado el tutorial.
                            if (!fin_tuto)
                            {
                                //Si el tutorial no se ha acabado dibujamos la imagen del tutorial y pintamos un circulo rojo para que al tocarlo se acabe el tutorial
                                dc.DrawImage(tutorial, new Rect(0, 0, RenderWidth, RenderHeight));
                                FormattedText ft = new FormattedText("Toque el\ncirculo para\nfin Tutorial", CultureInfo.GetCultureInfo("es-es"), FlowDirection.LeftToRight, new Typeface("Consola"),
                                            14,
                                            Brushes.Black);
                                dc.DrawText(ft, new Point(550, 60));
                                dc.DrawEllipse(Brushes.Red, null, new Point(600, 30), 30, 30);

                                //Si la mano toca el circulo.
                                if (SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position).X > 560 &&
                                    SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position).Y < 60)
                                {
                                    //Ponemos a true el final del tutorial y empieza a contar el tiempo.
                                    fin_tuto = true;
                                    puzzle.iniciarTiempo();
                                }
                            }
                            else
                            {   //Dibujamos la pieza que esté cogida.
                                puzzle.DrawPuzzleCogidos(dc);
                            }

                            //Si se ha acabado el tiempo para resolver el puzzle mostramos la puntuación.
                            if (puzzle.getFin())
                            { 
                                FormattedText punt = new FormattedText("Puntuacion: "+puzzle.getPuntuacion()+"/100", CultureInfo.GetCultureInfo("es-es"), FlowDirection.LeftToRight, new Typeface("Verdana"),
                                            24,
                                            Brushes.Black);
                                dc.DrawText(punt, new Point(240, 240));

                            }

                            //Actualizamos el esqueleto del puzzle
                            puzzle.actualizarSkeleto(skel);

                            //Dibujamos las manos.
                            dc.DrawImage(manoDer, new Rect(this.SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position).X- manoDer.Width/2, this.SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position).Y- manoDer.Height/2, manoDer.Width,manoDer.Height));
                            dc.DrawImage(manoIzq, new Rect(this.SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position).X - manoIzq.Width/2, this.SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position).Y - manoIzq.Height/2, manoIzq.Width, manoIzq.Height));
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
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
        /// Event handler for Kinect sensor's DepthFrameReady event sacado del proyecto de developer toolkit browser 1.8 depth basics-WPF 
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    // Le pasamos los datos de profundidad a InteractionStream.
                    interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthPixels.Length; ++i)
                    {
                        // Get the depth for this pixel
                        short depth = depthPixels[i].Depth;

                        // To convert to a byte, we're discarding the most-significant
                        // rather than least-significant bits.
                        // We're preserving detail, although the intensity will "wrap."
                        // Values outside the reliable depth range are mapped to 0 (black).

                        // Note: Using conditionals in this loop could degrade performance.
                        // Consider using a lookup table instead when writing production code.
                        // See the KinectDepthViewer class used by the KinectExplorer sample
                        // for a lookup table example.
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                        // Write out blue byte
                        this.colorPixels[colorPixelIndex++] = intensity;

                        // Write out green byte
                        this.colorPixels[colorPixelIndex++] = intensity;

                        // Write out red byte                        
                        this.colorPixels[colorPixelIndex++] = intensity;

                        // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                        // If we were outputting BGRA, we would write alpha here.
                        ++colorPixelIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Manejador de eventos para el evento del sensor de kinect InteractionStream sacado y adaptado de:
        /// http://dotneteers.net/blogs/vbandi/archive/2013/05/03/kinect-interactions-with-wpf-part-iii-demystifying-the-interaction-stream.aspx
        /// y https://msdn.microsoft.com/es-es/library/system.windows.media.formattedtext(v=vs.110).aspx
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void InteractionStreamOnInteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (InteractionFrame frame = e.OpenInteractionFrame())
            {
                if (frame != null)
                {
                    if (this.userInfos == null)
                    {
                        this.userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
                    }
                    //Copiamos los datos de interacción.
                    frame.CopyInteractionDataTo(this.userInfos);
                }
                else
                {
                    return;
                }
            }

            foreach (UserInfo userInfo in this.userInfos)
            {
                foreach (InteractionHandPointer handPointer in userInfo.HandPointers)
                {
                    //Inicializamos la acción que realizará.
                    string action = null;

                    //Comprobamos cual es la acción y asignamos la acción que realice el evento de la mano.
                    switch (handPointer.HandEventType)
                    {
                        //Si cerramos la mano asignamos gripped a la acción
                        case InteractionHandEventType.Grip:
                            action = "gripped";
                            break;

                        //Si abrimos la mano asinamos released a la acción
                        case InteractionHandEventType.GripRelease:
                            action = "released";

                            break;
                    }

                    if (action != null)
                    {
                        //Iniciamos que mano es la que está realizando la acción a desconocida.
                        string handSide = "unknown";

                        //Comprobamos que mano es la que realiza la acción y asignamos left o right dependiendo de que mano es.
                        switch (handPointer.HandType)
                        {
                            case InteractionHandType.Left:
                                handSide = "left";
                                break;

                            case InteractionHandType.Right:
                                handSide = "right";
                                break;
                        }

                        // Dependiendo de la mano hará una acción u otra al abrir y cerrar,
                        // en nuestro caso son las mismas acciones para ambas manos pero dibujará un puño cerrado o abierto dependiendo de que mano esté realizando la acción.
                        //Si la acción es released llamaremos a la función soltar de la clase Puzzle.cs 
                        //Si es coger llamará a la función coger de la clase Puzzle.cs
                        //Ver Puzzle.cs para saber lo que hacen.
                        if (handSide == "left")
                        {
                            if (action == "released")
                            {
                                puzzle.soltar(handSide);
                                manoIzq = manoIzqA;
                            }
                            else
                            {
                                puzzle.coger(handSide);
                                manoIzq = manoIzqC;
                            }
                        }
                        else
                        {
                            if (action == "released")
                            {
                                puzzle.soltar(handSide);
                                manoDer = manoDerA;
                            }
                            else
                            {
                                puzzle.coger(handSide);
                                manoDer = manoDerC;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Clase que usa InteractionStream para funcionar como cliente de interacción dando información a InteractionStream sacado de:
    ///http://dotneteers.net/blogs/vbandi/archive/2013/05/03/kinect-interactions-with-wpf-part-iii-demystifying-the-interaction-stream.aspx
    /// </summary>
    public class DummyInteractionClient : IInteractionClient
    {
        public InteractionInfo GetInteractionInfoAtLocation(
            int skeletonTrackingId,
            InteractionHandType handType,
            double x,
            double y)
        {
            var result = new InteractionInfo();
            result.IsGripTarget = true;
            result.IsPressTarget = true;
            result.PressAttractionPointX = 0.5;
            result.PressAttractionPointY = 0.5;
            result.PressTargetControlId = 1;

            return result;
        }
    }
}