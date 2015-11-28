

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
        private Image[] imagenes;
        
        private int[] pos;
        private RectImagen[] rect;
        private Skeleton skel;
        private KinectSensor sensor;
        private int width;
        private int height;
        private int piezas_width;
        private int piezas_height;

        public Puzzle(String im) {
            try {
                imagen = Image.FromFile(im);
            }
            catch
            {
                System.Console.WriteLine("Imagen no encontrada");
            }
            width=640;
            height = 480;
            piezas_width = 4;
            piezas_height = 3;
            pos = new int[piezas_height * piezas_width];
            rect = new RectImagen[piezas_height * piezas_width];
            imagenes = new Image[piezas_height * piezas_width];
            for (int i = 0; i < piezas_height*piezas_width; i++) { pos[i] = -1; }
            Random a = new Random();
            for (int i = 0; i < piezas_height * piezas_width; i++)
            {
                int n = a.Next(0, piezas_height * piezas_width);
                while (pos[n] != -1) {
                    n=(n+1)% piezas_height * piezas_width;
                }
                pos[n] = i;
                System.Console.WriteLine(n);
            }
            for(int i = 0; i < piezas_height * piezas_width; i++) {
                imagenes[i] = new Bitmap(imagen.Size.Width/ piezas_width, imagen.Size.Height/ piezas_height);
                var gra = Graphics.FromImage(imagenes[i]);
                int ipos = pos[i] / piezas_width;
                int jpos = pos[i]% piezas_width;
                gra.DrawImage(imagen, new Rectangle(0, 0, imagen.Size.Width / piezas_width, imagen.Size.Height / piezas_height), 
                    new Rectangle(ipos* imagen.Size.Width /piezas_width, jpos* imagen.Size.Height / piezas_height, imagen.Size.Width / piezas_width, imagen.Size.Height / piezas_height),
                    GraphicsUnit.Pixel);
                gra.Dispose();

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                MemoryStream ms = new MemoryStream();
                imagenes[i].Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = ms;
                bi.EndInit();
                int i_rec = i / piezas_width;
                int j_rec = i % piezas_width;
                rect[i] = new RectImagen(i_rec * width / piezas_width, j_rec * height / piezas_height, width / piezas_width, height / piezas_width, bi);
                rect[i].setPosicionOld(rect[i].getPosicion());

            }
        }
        public Puzzle(String im,int w,int h,int p_w,int p_h)
        {
            try
            {
                imagen = Image.FromFile(im);
            }
            catch
            {
                System.Console.WriteLine("Imagen no encontrada");
            }
            width = w;
            height = h;
            piezas_width = p_w;
            piezas_height = p_h;
            pos = new int[piezas_height * piezas_width];
            rect = new RectImagen[piezas_height * piezas_width];
            imagenes = new Image[piezas_height * piezas_width];
            for (int i = 0; i < piezas_height * piezas_width; i++) { pos[i] = -1; }
            Random a = new Random();
            for (int i = 0; i < piezas_height * piezas_width; i++)
            {
                int n = a.Next(0, piezas_height * piezas_width);
                while (pos[n] != -1)
                {
                    n = (n + 1) % piezas_height * piezas_width;
                }
                pos[n] = i;
                System.Console.WriteLine(n);
            }
            for (int i = 0; i < piezas_height * piezas_width; i++)
            {
                imagenes[i] = new Bitmap(imagen.Size.Width / piezas_width, imagen.Size.Height / piezas_height);
                var gra = Graphics.FromImage(imagenes[i]);
                int ipos = pos[i] / piezas_width;
                int jpos = pos[i] % piezas_width;
                gra.DrawImage(imagen, new Rectangle(0, 0, imagen.Size.Width / piezas_width, imagen.Size.Height / piezas_height),
                    new Rectangle(ipos * imagen.Size.Width / piezas_width, jpos * imagen.Size.Height / piezas_height, imagen.Size.Width / piezas_width, imagen.Size.Height / piezas_height),
                    GraphicsUnit.Pixel);
                gra.Dispose();

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                MemoryStream ms = new MemoryStream();
                imagenes[i].Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = ms;
                bi.EndInit();
                int i_rec = i / piezas_width;
                int j_rec = i % piezas_width;
                rect[i] = new RectImagen(i_rec * width / piezas_width, j_rec * height / piezas_height, width / piezas_width, height / piezas_width, bi);
                rect[i].setPosicionOld(rect[i].getPosicion());

            }
        }
        public void actualizarSkeleto(Skeleton sk) { skel = sk; }
        public void asignarSensor(KinectSensor s) { sensor = s; }

        public void DrawPuzzle(DrawingContext dc)
        {
            for(int i = 0; i < piezas_height * piezas_width; i++)
            {
                if(!rect[i].getCogido())
                    rect[i].dibujar(dc);

            }
           

        }
        public void DrawPuzzleCogidos( DrawingContext dc)
        {
            for (int i = 0; i < piezas_height * piezas_width; i++)
            {
                if (rect[i].getCogido())
                {
                    rect[i].dibujar(dc, SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position), SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position));
                }
            }
            
        }
        public void coger(String mano) {
            for (int i = 0; i < piezas_height * piezas_width; i++) {
                rect[i].coger(mano,SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position), SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position));
            }
        }
        public void soltar(String mano)
        {
            for (int i = 0; i < piezas_height * piezas_width; i++)
            {
                if (rect[i].getCogido())
                    if (rect[i].getMano() == mano) {
                        if (mano == "left") {
                            Point p = SkeletonPointToScreen(skel.Joints[JointType.HandLeft].Position);
                            int rec_inter = -1;
                            for (int j = 0; j < piezas_height * piezas_width; j++)
                            {
                                if (!rect[j].getCogido() && rect[j].inRectImagen(p.X, p.Y))
                                {
                                    rec_inter = j;
                                }
                            }
                            if (rec_inter != -1 && i!= rec_inter)
                            {
                                Point p_nuevo = rect[rec_inter].getPosicion();
                                rect[rec_inter].setPosicion(rect[i].getPosicionOld());
                                rect[i].soltar(mano);
                                rect[i].setPosicion(rect[rec_inter].getPosicionOld());
                                rect[rec_inter].setPosicionOld(rect[rec_inter].getPosicion());
                                rect[i].setPosicionOld(rect[i].getPosicion());
                            }else rect[i].soltar(mano);
                        }
                        else if(mano=="right"){
                            Point p = SkeletonPointToScreen(skel.Joints[JointType.HandRight].Position);
                            int rec_inter = -1;
                            for (int j = 0; j < piezas_height * piezas_width; j++)
                            {
                                if (!rect[j].getCogido() && rect[j].inRectImagen(p.X, p.Y))
                                {
                                    rec_inter = j;
                                }
                            }
                            if (rec_inter != -1 && i!=rec_inter)
                            {
                                Point p_nuevo = rect[rec_inter].getPosicion();
                                rect[rec_inter].setPosicion(rect[i].getPosicionOld());
                                rect[i].soltar(mano);
                                rect[i].setPosicion(rect[rec_inter].getPosicionOld());
                                rect[rec_inter].setPosicionOld(rect[rec_inter].getPosicion());
                                rect[i].setPosicionOld(rect[i].getPosicion());
                            }else rect[i].soltar(mano);
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
