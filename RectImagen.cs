namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Windows.Media.Imaging;
    using System.Drawing;

    class RectImagen
    {
        private Point pos;
        private int alto;
        private int ancho;
        private static bool cogidoI;
        private static bool cogidoD;
        private bool cogido;
        private BitmapImage img;
        private String mano_cog;
        private Point p_old;

        public RectImagen(int x, int y)
        {
            pos = new Point(x, y);
            ancho = 100;
            alto = 100;
            cogidoD = false;
            cogidoI = false;
        }
        public RectImagen(int x, int y, int anc,int alt,BitmapImage bitmatI)
        {
            pos = new Point(x, y);
            ancho = anc;
            alto = alt;
            img = bitmatI;
        }
        public bool getCogido() { return cogido; }
        public String getMano() { return mano_cog; }
        public Point getPosicion() { return pos; }
        public void setPosicion(Point p) { pos = p; }
        public Point getPosicionOld() { return p_old; }
        public void setPosicionOld(Point p) { p_old = p; }
        public bool inRectImagen(int x, int y) {
            if (x > pos.X && y > pos.Y && x < pos.X + ancho && y < pos.Y + alto)
                return true;
            else
                return false;
        }
        public void coger(String mano,Point md2d,Point mi2d) {
           if (md2d.X >= pos.X && md2d.X <= pos.X + ancho
                && md2d.Y <= pos.Y + alto && md2d.Y >= pos.Y  &&
                    !cogidoI&& !cogidoD && mano=="right")
            {
                cogidoD = true;
                cogido = true;
                mano_cog = mano;

            }
            else if (mi2d.X >= pos.X - ancho / 2 && mi2d.X <= pos.X + ancho / 2
                && mi2d.Y <= pos.Y + alto / 2 && mi2d.Y >= pos.Y - alto / 2 &&
                    !cogidoD && !cogidoI && mano =="left")
            {
                cogidoI = true;
                cogido = true;
                mano_cog = mano;

            }
        

        }

        public void soltar(String mano)
        {
            if (mano == "right")
            {
                RectImagen.cogidoD = false;
                cogido = false;
            }
            else if (mano == "left")
            {
                RectImagen.cogidoI = false;
                cogido = false;
            }

        }
        public void dibujar(DrawingContext dc) {
            dc.DrawImage(img, new System.Windows.Rect(pos.X, pos.Y, ancho, alto));
        }
        public void dibujar( DrawingContext dc, KinectSensor se, Point posd, Point posi)
        {
            
            if (cogidoI)
            {
               
                dc.DrawImage(img, new System.Windows.Rect(posi.X - ancho / 2, posi.Y - alto / 2,ancho, alto));
                
            }
            else if (cogidoD) {
                dc.DrawImage(img, new System.Windows.Rect(posd.X- ancho / 2, posd.Y- alto / 2, ancho, alto));

            }
            else
            {
                dc.DrawImage(img, new System.Windows.Rect(pos.X, pos.Y, ancho,  alto));
            }
        }





    }
}
