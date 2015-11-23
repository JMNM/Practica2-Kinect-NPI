//------------------------------------------------------------------------------
//  Práctica realizada por:
//		José Miguel Navarro Moreno
//		José Antonio Larrubia García
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
 
	/// <summary>
	/// Clase para realizar movimientos.
	/// </summary>
	class Mov
    {
		/// <summary>
        /// Sensor de kinect, será pasado por parámetros al necesitarlo
        /// </summary>
        private KinectSensor sensor;
		
		/// <summary>
        /// Lapiz en rojo con grosor 3.
        /// </summary>
        public Pen c1 = new Pen(Brushes.Red,3);
        
		/// <summary>
        /// Lapiz en rojo con grosor 3.
        /// </summary>
		public Pen c2 = new Pen(Brushes.Red, 3);
        
		/// <summary>
        /// Lapiz en rojo con grosor 3.
        /// </summary>
		public Pen c3 = new Pen(Brushes.Red, 3);
        
		/// <summary>
        /// Punto de control en el movimiento.
        /// </summary>
		private Boolean puntoControl1 = false;
		
		/// <summary>
        /// Punto de control en el movimiento.
        /// </summary>
        private Boolean puntoControl2 = false;
       
		/// <summary>
        /// Punto donde acaba el movimiento.
        /// </summary>
 	    private Boolean puntoControl3 = false;
 
		/// <summary>
        /// Temporizador para comprobar que esté un tiempo en un punto de control
        /// </summary>
		private int tem_pose = 0;
		
		/// <summary>
        /// Variable para comprobar si ha acabado un movimiento.
        /// </summary>
        private Boolean finMovIzq = false;
		
		/// <summary>
        /// Variable para comprobar si ha acabado un movimiento.
        /// </summary>
        private Boolean finMovDer = false;
	
		/// <summary>
        /// Objeto MainWindow que ha creado la instancia de la clase.
        /// </summary>
        private MainWindow padre;
		
		
		/// <summary>
        /// Margen de error al realizar el movimiento.
        /// </summary>
        private float margenError = 0.15F;

		
		/// <summary>
		/// Constructor de la clase.
		/// </summary>
		/// <param name="v"> objeto que usamos para poder recibir y manejar eventos de la interfaz gráfica</param>
        public Mov( MainWindow v) {
            padre = v;
        }
		
		/// <summary>
		/// Guía al usuario en la primera parte del movimiento, en este caso levantar la mano izquierda.
		/// </summary>
		/// <param name="skel"> Esqueleto que usaremos para hacer comprobaciones de que hace correctamente el movimiento</param>
		/// <param name="d"> La usaremos para poder dibujar</param>
        private void RealizarMovimientoIzquierda(Skeleton skel, DrawingContext d) {
			// Calculamos la distancias entre mano y codo izquierdos y codo y hombro izquierdo.
            Point p1L = new Point();
            Point p2L = new Point();
            Point p3L = new Point();
            SkeletonPoint pL = new SkeletonPoint();
            double hip=0;
            double man2codo = Math.Sqrt(Math.Pow((skel.Joints[JointType.HandLeft].Position.X - skel.Joints[JointType.ElbowLeft].Position.X), 2) +
               Math.Pow((skel.Joints[JointType.HandLeft].Position.Y - skel.Joints[JointType.ElbowLeft].Position.Y), 2) +
               Math.Pow((skel.Joints[JointType.HandLeft].Position.Z - skel.Joints[JointType.ElbowLeft].Position.Z), 2));
            double cod2hom= Math.Sqrt(Math.Pow((skel.Joints[JointType.ShoulderLeft].Position.X - skel.Joints[JointType.ElbowLeft].Position.X), 2) +
               Math.Pow((skel.Joints[JointType.ShoulderLeft].Position.Y - skel.Joints[JointType.ElbowLeft].Position.Y), 2) +
               Math.Pow((skel.Joints[JointType.ShoulderLeft].Position.Z - skel.Joints[JointType.ElbowLeft].Position.Z), 2));
            hip = man2codo + cod2hom;

			// Calculamos el punto donde se debe situar el primer punto de control.
            pL.X = skel.Joints[JointType.ShoulderLeft].Position.X - (float)(Math.Sin(60 * Math.PI / 180)*hip);
            pL.Y = skel.Joints[JointType.ShoulderLeft].Position.Y - (float)(Math.Cos(60 * Math.PI / 180)*hip);
            pL.Z = skel.Joints[JointType.ShoulderLeft].Position.Z;
            p1L = this.SkeletonPointToScreen(pL);

			// Si el primer punto de control esta en false y no ha finalizado el primer movimiento comprobamos que el usuario pasa la mano por el primer punto de control.
			// Si pasa por el punto de control, lo pintamos en azul y ponemos que ha pasado por él.
            if (!puntoControl1 && !finMovIzq) {
                if ((skel.Joints[JointType.HandLeft].Position.X>pL.X-margenError) && (skel.Joints[JointType.HandLeft].Position.X< pL.X + margenError)&&
                    (skel.Joints[JointType.HandLeft].Position.Y > pL.Y - margenError) && (skel.Joints[JointType.HandLeft].Position.Y < pL.Y + margenError)) {
                    puntoControl1 = true;
                    c1.Brush = Brushes.Blue;
                }
            }
			
			// Calculamos el punto donde se debe situar el segundo punto de control.
            pL.X = skel.Joints[JointType.ShoulderLeft].Position.X - (float)(Math.Sin(60* Math.PI / 180)*hip);
            pL.Y = skel.Joints[JointType.ShoulderLeft].Position.Y +(float)(Math.Cos(60 * Math.PI / 180)*hip);
            pL.Z = skel.Joints[JointType.ShoulderLeft].Position.Z;
            p2L = this.SkeletonPointToScreen(pL);

			// Si el segundo punto de control esta en false ,el usuario ha pasado por el primer punto de control y no ha finalizado el primer movimiento comprobamos que el usuario pasa por el segundo punto de control.
			// Si pasa por el punto de control, lo pintamos en azul y ponemos que ha pasado por él.
            if (!puntoControl2 && puntoControl1 && !finMovIzq)
            {
                if ((skel.Joints[JointType.HandLeft].Position.X > pL.X - margenError) && (skel.Joints[JointType.HandLeft].Position.X < pL.X + margenError)&&
                    (skel.Joints[JointType.HandLeft].Position.Y > pL.Y - margenError) && (skel.Joints[JointType.HandLeft].Position.Y < pL.Y + margenError))
                {
                    puntoControl2 = true;
                    c2.Brush = Brushes.Blue;
                    
                }
            }
			
			// Calculamos el punto donde se debe situar el final del movimiento.
            pL.X = skel.Joints[JointType.ShoulderLeft].Position.X;
            pL.Y = skel.Joints[JointType.ShoulderLeft].Position.Y + (float)hip;
            pL.Z = skel.Joints[JointType.ShoulderLeft].Position.Z;
            p3L = this.SkeletonPointToScreen(pL);

			// Si el punto de finalizar el movimiento esta en false, 
			// el usuario ha pasado por los dos punts de control anteriores 
			// y aun no ha finalizado el primer movimiento comprobamos que el usuario llega a este punto y mantiene la postura un momento para dar por finalizado el movimiento.
			// Si pasa por el punto de control, lo pintamos en azul y ponemos que ha pasado por él, inicializamos la variable que controlará el tiempo que está en esa postura.
            if (!puntoControl3 && puntoControl2 && puntoControl1 && !finMovIzq)
            {
                if ((skel.Joints[JointType.HandLeft].Position.X > pL.X - margenError) && (skel.Joints[JointType.HandLeft].Position.X < pL.X + margenError) &&
                    (skel.Joints[JointType.HandLeft].Position.Y > pL.Y - margenError) && (skel.Joints[JointType.HandLeft].Position.Y < pL.Y + margenError))
                {
                    puntoControl3 = true;
                    c3.Brush = Brushes.Blue;
                    tem_pose = DateTime.Now.Hour*3600+ DateTime.Now.Minute * 60 + DateTime.Now.Second;
                }
            }
			// Cuando este en la postura correcta comprobamos que se mantiene un tiempo.
			// Para ello dibujamos una barra de progreso que avisará de cuanto le queda.
			// Una vez terminado ese tiempo ponemos los valores de los puntos de control a false que usará otro movimiento al igual que los pinceles y pondremos que se ha acabado este movimiento.
            else if (puntoControl2 && puntoControl1 && !finMovIzq){
                int relleno = DateTime.Now.Hour*3600+ DateTime.Now.Minute * 60 + DateTime.Now.Second - tem_pose;
                if ((skel.Joints[JointType.HandLeft].Position.X > pL.X - margenError) && (skel.Joints[JointType.HandLeft].Position.X < pL.X + margenError) &&
                    (skel.Joints[JointType.HandLeft].Position.Y > pL.Y - margenError) && (skel.Joints[JointType.HandLeft].Position.Y < pL.Y + margenError))
                {      
                    if (relleno < 5)
                    {
                        c3.Brush = Brushes.Green;

                        //Dibujo de barra de progreso improvisada con dos linas 
                        Point ilin = new Point(p3L.X+10, p3L.Y - 15);
                        Point flin = new Point(p3L.X +10+ 50, p3L.Y - 15);
                        Point ilin2 = new Point(p3L.X+11, p3L.Y - 15);
                        Point flin2 = new Point(p3L.X + 11 + relleno * 12, p3L.Y - 15);
                        d.DrawLine(new Pen(Brushes.Black, 20), ilin, flin);
                        d.DrawLine(new Pen(Brushes.Green, 18), ilin2, flin2);
                        
                    }else if (relleno >= 5)
                    {
                        finMovIzq = true;
                        puntoControl1 = false;
                        puntoControl2 = false;
                        puntoControl3 = false;
                        c1.Brush = Brushes.Red;
                        c2.Brush = Brushes.Red;
                        c3.Brush = Brushes.Red;
                    }
                }
				// Si el usuario deja de estar en la postura correcta y no ha acabado el tiempo que debería estar en esa postura volvemos a poner los valores a "0" que usamos para controlarlo.
                else {
                    relleno = 0;
                    tem_pose =DateTime.Now.Hour*3600+ DateTime.Now.Minute * 60 + DateTime.Now.Second;
                    c3.Brush = Brushes.Red;
                }
            }
			
			//Siempre que el movimiento no haya finalizado pintamos 3 elipses con los puntos de control del color que tengan en ese momento.
            if (!finMovIzq)
            {
                d.DrawEllipse(null, c1, p1L, 20, 20);
                d.DrawEllipse(null, c2, p2L, 20, 20);
                d.DrawEllipse(null, c3, p3L, 20, 20);
            }

        }
		
		/// <summary>
		/// Guía al usuario en la segunda parte del movimiento.
		/// </summary>
		/// <param name="skel"> Esqueleto que usaremos para hacer comprobaciones de que hace correctamente el movimiento</param>
		/// <param name="d"> La usaremos para poder dibujar</param>
        private void RealizarMovimientoDerecha(Skeleton skel, DrawingContext d) {
            Point p1R = new Point();
            Point p2R = new Point();
            Point p3R = new Point();
            SkeletonPoint pR = new SkeletonPoint();
            double hip = 0;
            double man2codo = Math.Sqrt(Math.Pow((skel.Joints[JointType.HandRight].Position.X - skel.Joints[JointType.ElbowRight].Position.X), 2) +
               Math.Pow((skel.Joints[JointType.HandRight].Position.Y - skel.Joints[JointType.ElbowRight].Position.Y), 2) +
               Math.Pow((skel.Joints[JointType.HandRight].Position.Z - skel.Joints[JointType.ElbowRight].Position.Z), 2));
            double cod2hom = Math.Sqrt(Math.Pow((skel.Joints[JointType.ShoulderRight].Position.X - skel.Joints[JointType.ElbowRight].Position.X), 2) +
               Math.Pow((skel.Joints[JointType.ShoulderRight].Position.Y - skel.Joints[JointType.ElbowRight].Position.Y), 2) +
               Math.Pow((skel.Joints[JointType.ShoulderRight].Position.Z - skel.Joints[JointType.ElbowRight].Position.Z), 2));
            hip = man2codo + cod2hom;

            pR.X = skel.Joints[JointType.ShoulderRight].Position.X + ((float)(Math.Sin(60 * Math.PI / 180) * hip));
            pR.Y = skel.Joints[JointType.ShoulderRight].Position.Y - ((float)(Math.Cos(60 * Math.PI / 180) * hip));
            pR.Z = skel.Joints[JointType.ShoulderRight].Position.Z;
            p1R = this.SkeletonPointToScreen(pR);

            if (!puntoControl1 && !finMovDer)
            {
                if ((skel.Joints[JointType.HandRight].Position.X > pR.X - margenError) && (skel.Joints[JointType.HandRight].Position.X < pR.X + margenError) &&
                    (skel.Joints[JointType.HandRight].Position.Y > pR.Y - margenError) && (skel.Joints[JointType.HandRight].Position.Y < pR.Y + margenError))
                {
                    puntoControl1 = true;
                    c1.Brush = Brushes.Blue;
                }
            }


            pR.X = skel.Joints[JointType.ShoulderRight].Position.X + (float)(Math.Sin(60 * Math.PI / 180) * hip);
            pR.Y = skel.Joints[JointType.ShoulderRight].Position.Y + (float)(Math.Cos(60 * Math.PI / 180) * hip);
            pR.Z = skel.Joints[JointType.ShoulderRight].Position.Z;
            p2R = this.SkeletonPointToScreen(pR);

            if (!puntoControl2 && puntoControl1 && !finMovDer)
            {
                if ((skel.Joints[JointType.HandRight].Position.X > pR.X - margenError) && (skel.Joints[JointType.HandRight].Position.X < pR.X + margenError) &&
                    (skel.Joints[JointType.HandRight].Position.Y > pR.Y - margenError) && (skel.Joints[JointType.HandRight].Position.Y < pR.Y + margenError))
                {
                    puntoControl2 = true;
                    c2.Brush = Brushes.Blue;

                }
            }

            pR.X = skel.Joints[JointType.ShoulderRight].Position.X;
            pR.Y = skel.Joints[JointType.ShoulderRight].Position.Y + (float)hip;
            pR.Z = skel.Joints[JointType.ShoulderRight].Position.Z;
            p3R = this.SkeletonPointToScreen(pR);

            if (!puntoControl3 && puntoControl2 && puntoControl1 && !finMovDer)
            {
                if ((skel.Joints[JointType.HandRight].Position.X > pR.X - margenError) && (skel.Joints[JointType.HandRight].Position.X < pR.X + margenError) &&
                    (skel.Joints[JointType.HandRight].Position.Y > pR.Y - margenError) && (skel.Joints[JointType.HandRight].Position.Y < pR.Y + margenError))
                {
                    puntoControl3 = true;
                    c3.Brush = Brushes.Blue;
                    tem_pose = DateTime.Now.Hour*3600+ DateTime.Now.Minute * 60 + DateTime.Now.Second;
                }
            }
            else if (puntoControl2 && puntoControl1 && !finMovDer)
            {
                int relleno = DateTime.Now.Hour*3600+ DateTime.Now.Minute * 60 + DateTime.Now.Second - tem_pose;
                if ((skel.Joints[JointType.HandRight].Position.X > pR.X - margenError) && (skel.Joints[JointType.HandRight].Position.X < pR.X + margenError) &&
                    (skel.Joints[JointType.HandRight].Position.Y > pR.Y - margenError) && (skel.Joints[JointType.HandRight].Position.Y < pR.Y + margenError))
                {

                    if (relleno < 5)
                    {
                        c3.Brush = Brushes.Green;
                        Point ilin = new Point(p3R.X + 10, p3R.Y - 15);
                        Point flin = new Point(p3R.X + 10 + 50, p3R.Y - 15);
                        Point ilin2 = new Point(p3R.X + 11, p3R.Y - 15);
                        Point flin2 = new Point(p3R.X + 11 + relleno * 12, p3R.Y - 15);
                        d.DrawLine(new Pen(Brushes.Black, 20), ilin, flin);
                        d.DrawLine(new Pen(Brushes.Green, 18), ilin2, flin2);
                        
                    }else if (relleno >= 5)
                    {
                        /*System.Diagnostics.Process proc = new System.Diagnostics.Process();
                        proc.EnableRaisingEvents = false;
                        proc.StartInfo.FileName = "iexplore";
                        proc.StartInfo.Arguments = "http://www.microsoft.com";
                        proc.Start();
                        proc.WaitForExit();*/
                        finMovDer = true;
                        c1.Brush = Brushes.Red;
                        c2.Brush = Brushes.Red;
                        c3.Brush = Brushes.Red;
                        
                    }

                }
                else {
                    relleno = 0;
                    tem_pose = DateTime.Now.Hour*3600+ DateTime.Now.Minute * 60 + DateTime.Now.Second;
                    c3.Brush = Brushes.Red;
                }
            }
            if (!finMovDer)
            {
                d.DrawEllipse(null, c1, p1R, 20, 20);
                d.DrawEllipse(null, c2, p2R, 20, 20);
                d.DrawEllipse(null, c3, p3R, 20, 20);
               
            }
        }
        
		
		/// <summary>
		/// Reincia el movimiento.
		/// </summary>
        public void ReiniciarMovimiento() {
			
			// Pone las componentes de la clase a sus valores por defecto
			// al menos las componentes necesarias.
            c1 = new Pen(Brushes.Red, 3);
            c2 = new Pen(Brushes.Red, 3);
            c3 = new Pen(Brushes.Red, 3);
        
            puntoControl1 = false;
            puntoControl2 = false;
            puntoControl3 = false;
            tem_pose = 0;
            finMovIzq = false;
            finMovDer = false;
            padre.mov1.Visibility = Visibility.Hidden;
            padre.mov2.Visibility = Visibility.Hidden;
        }
		
		/// <summary>
		/// Procedimiento principal de la clase, llamará a las funciones que realizan movimientos.
		/// </summary>
		/// <param name="skel"> Esqueleto que usaremos para hacer comprobaciones de que hace correctamente el movimiento</param>
		/// <param name="d"> La usaremos para poder dibujar</param>
		/// <param name="se"> Sensor de kinect</param>
        public void Empezar(Skeleton skel, DrawingContext dc, KinectSensor se) {
            // Asignamos el sensor pasado por argumentos.
			sensor = se;
            
			// Si no ha acabado el primer movimiento se muestra la imagen del movimiento a realizar en la interfaz y un texto y se llama a la función para que realice el movimiento 
            if (!finMovIzq)
            {
                if (padre != null)
                {
                    padre.mov1.Visibility = Visibility.Visible;
                    padre.pi1.Visibility = Visibility.Hidden;
                }
                this.RealizarMovimientoIzquierda(skel, dc);
            }
			
			// Si ya ha acabado el primer movimiento
			// comprueba que no ha acabado el segundo movimiento se muestra la imagen del movimiento a realizar en la interfaz y un texto y se llama a la función para que realice el movimiento
            else if (!finMovDer)
            {
                if (padre != null)
                {
                    padre.mov1.Visibility = Visibility.Hidden;
                    padre.mov2.Visibility = Visibility.Visible;
                }
                this.RealizarMovimientoDerecha(skel, dc);
            }
			
			//Si se ha acabado ambos movimientos oculta las imagenes.
            else {
                if(padre!=null)
                    padre.mov2.Visibility = Visibility.Hidden;
            }
        }
		
		/// <summary>
		/// Cambia el margen de error para que sea más fácil o más dificil hacer el movimiento.
		/// </summary>
		/// <param name="a"> Margen que usaremos a partir de ahora</param>
        public void setMargenError(float a) {
             if(a<0.25 && a > 0.05)
            {
                margenError = a;
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
    }
}
