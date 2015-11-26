namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Windows.Media.Media3D;
    

    class RectMov
    {
        private Point3D pos;
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
        public RectMov(float x, float y, float z)
        {
            pos = new Point3D(x, y, z);
            ancho = 100;
            alto = 100;
            cogidoD = false;
            cogidoI = false;
        }
        public RectMov(float x, float y, float z, Boolean cogD,Boolean cogI)
        {
            pos = new Point3D(x, y, z);
            ancho = 10;
            alto = 10;
            cogidoD = cogD;
            cogidoI = cogI;
        }
        public void coger() {
            if (sk != null)
            {
                if (sk.Joints[JointType.HandRight].Position.X >= pos.X && sk.Joints[JointType.HandRight].Position.X <= pos.X + ancho
                    && sk.Joints[JointType.HandRight].Position.Y <= pos.Y + alto&& sk.Joints[JointType.HandRight].Position.Y >= pos.Y - alto &&
                     !cogidoD)
                {
                    cogidoD = true;
                    cogidoI = false;

                }
                else if (sk.Joints[JointType.HandLeft].Position.X >= pos.X - ancho / 2 && sk.Joints[JointType.HandLeft].Position.X <= pos.X + ancho / 2
                    && sk.Joints[JointType.HandLeft].Position.Y <= pos.Y + alto / 2 && sk.Joints[JointType.HandLeft].Position.Y >= pos.Y - alto / 2 &&
                     !cogidoI)
                {
                    cogidoI = true;
                    cogidoD = false;

                }
            }

        }

        public void soltar()
        {

            cogidoD = false;
            cogidoI = false;

        }
        public void dibujar(Skeleton skel, DrawingContext dc, KinectSensor se)
        {
            // Asignamos el sensor pasado por argumentos.
            sensor = se;
            sk = skel;
            if (cogidoI)
            {
                pos.X = skel.Joints[JointType.HandLeft].Position.X;
                pos.Y = skel.Joints[JointType.HandLeft].Position.Y;
                pos.Z = skel.Joints[JointType.HandLeft].Position.Z;
                dc.DrawRectangle(null, new Pen(Brushes.Red, 3), new Rect(Point3DToScreen(pos).X - ancho / 2, Point3DToScreen(pos).Y - alto / 2, alto, ancho));
                
            }
            else if (cogidoD) {
                pos.X = skel.Joints[JointType.HandRight].Position.X;
                pos.Y = skel.Joints[JointType.HandRight].Position.Y;
                pos.Z = skel.Joints[JointType.HandRight].Position.Z;
                dc.DrawRectangle(null, new Pen(Brushes.Red, 3), new Rect(Point3DToScreen(pos).X- ancho / 2, Point3DToScreen(pos).Y- alto / 2, alto, ancho));

            }
            else
            {
                dc.DrawRectangle(null, new Pen(Brushes.Red, 3), new Rect(Point3DToScreen(pos).X, Point3DToScreen(pos).Y, alto, ancho));
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
