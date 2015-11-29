
// Práctica realizada por:
//	José Miguel Navarro Moreno
//	José Antonio Larrubia García
//

namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Windows.Media.Imaging;
    using System.Drawing;

    /// <summary>
    /// Clase que usa Puzzle.cs para manejar cada rectángulo del puzzle.
    /// </summary>
    class RectImagen
    {
        /// <summary>
        /// Punto que indica la posición que tiene el rectángulo
        /// </summary>
        private Point pos;

        /// <summary>
        /// Tamaño en píxeles del alto del rectángulo
        /// </summary>
        private int alto;

        /// <summary>
        ///  Tamaño en píxeles del ancho del rectángulo
        /// </summary>
        private int ancho;
        
        /// <summary>
        /// Variable de clase que informará al resto de objetos de la clase si el rectángulo está cogido con la mano izquierda.
        /// </summary>
        private static bool cogidoI;
        
        /// <summary>
        ///  Variable de clase que informará al resto de objetos de la clase si el rectángulo está cogido con la mano derecha.
        /// </summary>
        private static bool cogidoD;
        
        /// <summary>
        ///  Variable que informa si está cogido el rectaángulo.
        /// </summary>
        private bool cogido;
        
        /// <summary>
        /// Imagen que estará contenida en el rectangulo.
        /// </summary>
        private BitmapImage img;
        
        /// <summary>
        /// Variable que se indicará la mano con la que está cogido el rectángulo.
        /// </summary>
        private String mano_cog;
        
        /// <summary>
        /// Punto que guardará la posición antigua del rectángulo para luego hacer el intercambio con otro rectángulo.
        /// </summary>
        private Point p_old;

        /// <summary>
        /// Constructor de clase que hará un rectángulo con la posición que pasemos de un ancho x alto puesto por defecto.
        /// </summary>
        /// <param name="x"> coordenada x de la posición que tendrá el rectángulo </param>
        /// <param name="y"> coordenada y de la posición que tendrá el rectángulo </param>
        public RectImagen(int x, int y)
        {
            pos = new Point(x, y);
            ancho = 100;
            alto = 100;
            cogidoD = false;
            cogidoI = false;
        }

        /// <summary>
        /// Constructor de clase que hará un rectángulo con la posición, ancho y alto del rectángulo y la imagen que contendrá el mismo que pasemos por parámetros.
        /// </summary>
        /// <param name="x"> coordenada x de la posición que tendrá el rectángulo </param>
        /// <param name="y"> coordenada y de la posición que tendrá el rectángulo </param>
        /// <param name="anc"> Tamaño que tendrá el rectangulo de ancho </param>
        /// <param name="alt"> Tamaño que tendrá el rectangulo de alto </param>
        /// <param name="bitmatI"> Imagen que usaremos dentro del rectángulo </param>
        public RectImagen(int x, int y, int anc,int alt,BitmapImage bitmatI)
        {
            pos = new Point(x, y);
            ancho = anc;
            alto = alt;
            img = bitmatI;
        }

        /// <summary>
        /// Devuelve si está o no cogido el rectángulo.
        /// </summary>
        public bool getCogido() {
             return cogido;
         }
        
        /// <summary>
        /// Devuelve la mano que tiene cogido el rectángulo.
        /// </summary>
        public String getMano() {
             return mano_cog;
         }
        
        /// <summary>
        /// Devuelve la posición actual del rectángulo.
        /// </summary>
        public Point getPosicion() {
             return pos; 
        }
        
        /// <summary>
        /// Cambia la posición del rectángulo.
        /// </summary>
        /// <param name="p"> variable que se le asignará a pos para hacer el cambio de posición </param>
        public void setPosicion(Point p) { 
            pos = p; 
        }
        
        /// <summary>
        /// Devuelve la posición antigua del rectángulo.
        /// </summary>
        public Point getPosicionOld() { 
            return p_old; 
        }
        
        /// <summary>
        /// Cambia la posición antigua del rectángulo
        /// </summary>
        /// <param name="p"> variable que se le asignará a p_old para hacer el cambio de la posición antigua </param>
        public void setPosicionOld(Point p) { 
            p_old = p; 
        }
        
        /// <summary>
        /// Devuelve si el rectángulo está o no en la posición de otro rectángulo
        /// </summary>
        /// <param name="x"> coordenada x con la que hará la comprobación </param>
        /// <param name="y"> coordenada y con la que hará la comprobación </param>
        public bool inRectImagen(int x, int y) {
            if (x > pos.X && y > pos.Y && x < pos.X + ancho && y < pos.Y + alto)
                return true;
            else
                return false;
        }
        
        /// <summary>
        /// Función que cogerá el rectángulo.
        /// </summary>
        /// <param name="mano"> mano que cogerá el rectángulo </param>
        /// <param name="md2d"> Posición de la mano derecha </param>
        /// <param name="mi2d"> Posición de la mano izquierda </param>
        public void coger(String mano,Point md2d,Point mi2d) {
           //Comprobamos que la mano derecha está en sobre el rectángulo,
           //si lo está y no hay cogido ningún otro rectángulo y la mano que pasamos es la derecha se coge el rectángulo.
           if (md2d.X >= pos.X && md2d.X <= pos.X + ancho
                && md2d.Y <= pos.Y + alto && md2d.Y >= pos.Y  &&
                    !cogidoI&& !cogidoD && mano=="right")
            {
                cogidoD = true;
                cogido = true;
                mano_cog = mano;

            }
            //Comprobamos que la mano izquierda está en sobre el rectángulo,
           //si lo está y no hay cogido ningún otro rectángulo y la mano que pasamos es la izquierda se coge el rectángulo.
            else if (mi2d.X >= pos.X - ancho / 2 && mi2d.X <= pos.X + ancho / 2
                && mi2d.Y <= pos.Y + alto / 2 && mi2d.Y >= pos.Y - alto / 2 &&
                    !cogidoD && !cogidoI && mano =="left")
            {
                cogidoI = true;
                cogido = true;
                mano_cog = mano;

            }
        }
        
        /// <summary>
        /// Función que soltará un rectángulo.
        /// </summary>
        /// <param name="mano"> Mano que soltará el rectángulo </param>
        public void soltar(String mano)
        {
            //Comprobamos que mano es la que tiene el rectángulo y cambiamos las variables de estado correspondientes para decir que no está cogido.
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
        
        /// <summary>
        /// Función que dibuja un rectángulo.
        /// </summary>
        /// <param name="dc"> parámetro necesario para dibujar la imagen </param>
        public void dibujar(DrawingContext dc) {
            dc.DrawImage(img, new System.Windows.Rect(pos.X, pos.Y, ancho, alto));
        }
        
        /// <summary>
        /// Función para dibujar el rectángulo en una posición dada.
        /// </summary>
        /// <param name="dc"> parámetro necesario para dibujar la imagen </param>
        /// <param name="posd"> Posición donde se dibujará el rectángulo si está cogido por la mano derecha</param>
        /// <param name="posi"> Posición donde se dibujará el rectángulo si está cogido por la mano izquierda </param>
        public void dibujar( DrawingContext dc, Point posd, Point posi)
        {
            //Dibujamos el rectángulo dependiendo de que mano lo tenga cogido, si no lo tiene cogido se dibuja donde estaba.
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
