namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Windows.Media.Media3D;
    

    class RectMov
    {
        private Point pos;
        private int alto;
        private int ancho;
        private Boolean cogidoI;
        private Boolean cogidoD;
        private SkeletonPoint c;
        private int tem_pos = 0;
        private Skeleton sk;
        /// <summary>
        /// Sensor de kinect, será pasado por parámetros al necesitarlo
        /// </summary>
        private KinectSensor sensor;
        public RectMov(float x, float y)
        {
            pos = new Point(x, y);
            ancho = 100;
            alto = 100;
            cogidoD = false;
            cogidoI = false;
        }
        public RectMov(float x, float y, float z, Boolean cogD,Boolean cogI)
        {
            pos = new Point(x, y);
            ancho = 100;
            alto = 100;
            cogidoD = cogD;
            cogidoI = cogI;
        }
        public void coger(String mano) {
            if (sk != null)
            {
                Point3D md = new Point3D(sk.Joints[JointType.HandRight].Position.X, sk.Joints[JointType.HandRight].Position.Y, sk.Joints[JointType.HandRight].Position.Z);
                Point md2d = Point3DToScreen(md);
                Point3D mi = new Point3D(sk.Joints[JointType.HandLeft].Position.X, sk.Joints[JointType.HandLeft].Position.Y, sk.Joints[JointType.HandLeft].Position.Z);
                Point mi2d = Point3DToScreen(mi);
                if (md2d.X >= pos.X && md2d.X <= pos.X + ancho
                    && md2d.Y <= pos.Y + alto && md2d.Y >= pos.Y  &&
                     !cogidoI && mano=="right")
                {
                    cogidoD = true;
                    cogidoI = false;

                }
                else if (mi2d.X >= pos.X - ancho / 2 && mi2d.X <= pos.X + ancho / 2
                    && mi2d.Y <= pos.Y + alto / 2 && mi2d.Y >= pos.Y - alto / 2 &&
                     !cogidoD && mano =="left")
                {
                    cogidoI = true;
                    cogidoD = false;

                }
            }

        }

        public void soltar(String mano)
        {
            if(mano=="right")
                cogidoD = false;
            else if(mano=="left")
                cogidoI = false;

        }
        public void dibujar(Skeleton skel, DrawingContext dc, KinectSensor se)
        {
            // Asignamos el sensor pasado por argumentos.
            sensor = se;
            sk = skel;
            if (cogidoI)
            {
                pos = Point3DToScreen(new Point3D(skel.Joints[JointType.HandLeft].Position.X, skel.Joints[JointType.HandLeft].Position.Y, skel.Joints[JointType.HandLeft].Position.Z));
                dc.DrawRectangle(null, new Pen(Brushes.Red, 3), new Rect(pos.X - ancho / 2, pos.Y - alto / 2, alto, ancho));
                
            }
            else if (cogidoD) {
                pos = Point3DToScreen(new Point3D(skel.Joints[JointType.HandRight].Position.X, skel.Joints[JointType.HandRight].Position.Y, skel.Joints[JointType.HandRight].Position.Z));
                dc.DrawRectangle(null, new Pen(Brushes.Red, 3), new Rect(pos.X- ancho / 2, pos.Y- alto / 2, alto, ancho));

            }
            else
            {
                dc.DrawRectangle(null, new Pen(Brushes.Red, 3), new Rect(pos.X - ancho / 2, pos.Y - alto / 2, alto, ancho));
            }
        }





        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point Point3DToScreen(Point3D s)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            SkeletonPoint skelpoint = new SkeletonPoint();
            skelpoint.X = (float)s.X;
            skelpoint.Y = (float)s.Y;
            skelpoint.Z = (float)s.Z;
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
    }
}
