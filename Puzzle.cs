

namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Windows.Media;
    using Microsoft.Kinect;

    using System.Collections;
    using System.Drawing;
    using System.Windows.Media.Imaging;
    using System.IO;
    using System.Drawing.Imaging;

    class Puzzle
    {
        private Image imagen;
        private Image[] imagenes=new Image[9];
        
        private int[] pos = new int[9];
        private RectImagen[] rect = new RectImagen[9];
        private Skeleton skel;
        private KinectSensor sensor;

        public Puzzle(String im) {
            try {
                imagen = Image.FromFile(im);
            }
            catch
            {
                System.Console.WriteLine("Imagen no encontrada");
            }
            for(int i = 0; i < 9; i++) { pos[i] = -1; }
            Random a = new Random();
            for (int i = 0; i < 9; i++)
            {
                int n = a.Next(0,9);
                while (pos[n] != -1) {
                    n=(n+1)%9;
                }
                pos[n] = i;
                System.Console.WriteLine(n);
            }
            for(int i = 0; i < 9; i++) {
                imagenes[i] = new Bitmap(imagen.Size.Width/3,imagen.Size.Height/3);
                var gra = Graphics.FromImage(imagenes[i]);
                int ipos = pos[i] / 3;
                int jpos = pos[i]%3;
                gra.DrawImage(imagen, new Rectangle(0, 0, imagen.Size.Width / 3, imagen.Size.Height / 3), new Rectangle(ipos* imagen.Size.Width / 3,jpos* imagen.Size.Height / 3, imagen.Size.Width / 3, imagen.Size.Height / 3),GraphicsUnit.Pixel);
                gra.Dispose();

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                MemoryStream ms = new MemoryStream();
                imagenes[i].Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = ms;
                bi.EndInit();
                int i_rec = i / 3;
                int j_rec = i % 3;
                rect[i] = new RectImagen(i_rec * 640 / 3, j_rec * 480 / 3, 640 / 3, 480 / 3, bi);
                rect[i].setPosicionOld(rect[i].getPosicion());

            }
        }
        public void actualizarSkeleto(Skeleton sk) { skel = sk; }
        public void asignarSensor(KinectSensor s) { sensor = s; }
        public void DrawPuzzle(DrawingContext dc)
        {
            for(int i = 0; i < 9; i++)
            {
                if(!rect[i].getCogido())
                    rect[i].dibujar(dc);

            }
           

        }
        public void DrawPuzzle( DrawingContext dc, KinectSensor se)
        {
            for (int i = 0; i < 9; i++)
            {
                if (rect[i].getCogido())
                {
                    rect[i].dibujar(dc, se, SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position), SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position));
                }
            }
            
        }
        public void coger(String mano) {
            for (int i = 0; i < 9; i++) {
                rect[i].coger(mano,SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position), SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position));
            }
        }
        public void soltar(String mano)
        {
            for (int i = 0; i < 9; i++)
            {
                if (rect[i].getCogido())
                    if (rect[i].getMano() == mano) {
                        if (mano == "left") {
                            Point p = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                            int rec_inter = -1;
                            for (int j = 0; j < 9; j++)
                            {
                                if (!rect[j].getCogido() && rect[j].inRectImagen(p.X, p.Y))
                                {
                                    rec_inter = j;
                                }
                            }
                            if (rec_inter != -1)
                            {
                                Point p_nuevo = rect[rec_inter].getPosicion();
                                rect[rec_inter].setPosicion(rect[i].getPosicionOld());
                                rect[i].soltar(mano);
                                rect[i].setPosicion(rect[rec_inter].getPosicionOld());
                                rect[rec_inter].setPosicionOld(rect[rec_inter].getPosicion());
                                rect[i].setPosicionOld(rect[i].getPosicion());
                            }
                        }else if(mano=="right"){
                            Point p = SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position);
                            int rec_inter = -1;
                            for (int j = 0; j < 9; j++)
                            {
                                if (!rect[j].getCogido() && rect[j].inRectImagen(p.X, p.Y))
                                {
                                    rec_inter = j;
                                }
                            }
                            if (rec_inter != -1)
                            {
                                Point p_nuevo = rect[rec_inter].getPosicion();
                                rect[rec_inter].setPosicion(rect[i].getPosicionOld());
                                rect[i].soltar(mano);
                                rect[i].setPosicion(rect[rec_inter].getPosicionOld());
                                rect[rec_inter].setPosicionOld(rect[rec_inter].getPosicion());
                                rect[i].setPosicionOld(rect[i].getPosicion());
                            }
                        }
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

    }
}
