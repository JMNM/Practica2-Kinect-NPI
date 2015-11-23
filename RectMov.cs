using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <summary>
        /// Sensor de kinect, será pasado por parámetros al necesitarlo
        /// </summary>
        private KinectSensor sensor;
        public RectMov(float x, float y, float z)
        {
            pos = new Point3D(x, y, z);
            ancho = 10;
            alto = 10;
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
        public void coger(Skeleton skel,DrawingContext d) {
            if (skel.Joints[JointType.HandRight].Position.X == pos.X && skel.Joints[JointType.HandRight].Position.Y == pos.Y &&
                skel.Joints[JointType.HandRight].Position.Z >= pos.Z && !cogidoD && !cogidoI)
            {
                /*
                cogidoD = true;
                cogidoI = false;
                pos.X = skel.Joints[JointType.HandRight].Position.X;
                pos.Y = skel.Joints[JointType.HandRight].Position.Y;
                pos.Z = skel.Joints[JointType.HandRight].Position.Z;
                */
                int relleno = DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second - tem_pos;
                if (relleno < 5)
                {
                    Point el1 = Point3DToScreen(pos);
                    
                    d.DrawEllipse(Brushes.White, null, el1, 30, 30);
                    d.DrawEllipse(null,new Pen(Brushes.Green, 18), el1, relleno*6,relleno*6);
                }
                else if (relleno >= 5)
                {
                    cogidoD = true;
                    cogidoI = false;
                }
            }
            else if (skel.Joints[JointType.HandLeft].Position.X == pos.X && skel.Joints[JointType.HandLeft].Position.Y == pos.Y &&
                skel.Joints[JointType.HandLeft].Position.Z >= pos.Z && !cogidoD && !cogidoI)
            {
                /*cogidoI = true;
                cogidoD = false;
                pos.X = skel.Joints[JointType.HandLeft].Position.X;
                pos.Y = skel.Joints[JointType.HandLeft].Position.Y;
                pos.Z = skel.Joints[JointType.HandLeft].Position.Z;
                */
                int relleno = DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second - tem_pos;
                if (relleno < 5)
                {
                    Point el1 = Point3DToScreen(pos);

                    d.DrawEllipse(Brushes.White, null, el1, 30, 30);
                    d.DrawEllipse(null, new Pen(Brushes.Green, 18), el1, relleno * 6, relleno * 6);
                }
                else if (relleno >= 5)
                {
                    cogidoD = true;
                    cogidoI = false;
                }
            }
            else
            {
                tem_pos = DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second;
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
            coger(skel, dc);
            if (cogidoI)
            {
                pos.X = skel.Joints[JointType.HandLeft].Position.X;
                pos.Y = skel.Joints[JointType.HandLeft].Position.Y;
                pos.Z = skel.Joints[JointType.HandLeft].Position.Z;
                dc.DrawRectangle(null, new Pen(Brushes.Red, 3), new Rect(Point3DToScreen(pos).X, Point3DToScreen(pos).Y, alto, ancho));

                Point3D pL = new Point3D();
                pL.X = skel.Joints[JointType.ShoulderRight].Position.X + 0.6F;
                pL.Y = skel.Joints[JointType.ShoulderRight].Position.Y + 0.6F;
                pL.Z = skel.Joints[JointType.ShoulderRight].Position.Z;
                dc.DrawEllipse(Brushes.Red,null, Point3DToScreen(pL), 20, 20);

                if (pos.Z < skel.Joints[JointType.ShoulderLeft].Position.Z - 0.60F) {
                    soltar();
                }
            }
            else if (cogidoD) {
                pos.X = skel.Joints[JointType.HandRight].Position.X;
                pos.Y = skel.Joints[JointType.HandRight].Position.Y;
                pos.Z = skel.Joints[JointType.HandRight].Position.Z;
                dc.DrawRectangle(null, new Pen(Brushes.Red, 3), new Rect(Point3DToScreen(pos).X, Point3DToScreen(pos).Y, alto, ancho));

                Point3D pL = new Point3D();
                pL.X = skel.Joints[JointType.ShoulderLeft].Position.X - 0.6F;
                pL.Y = skel.Joints[JointType.ShoulderLeft].Position.Y + 0.6F;
                pL.Z = skel.Joints[JointType.ShoulderLeft].Position.Z;
                dc.DrawEllipse(Brushes.Red, null, Point3DToScreen(pL), 20, 20);
                if (pos.Z < skel.Joints[JointType.ShoulderRight].Position.Z - 0.60F)
                {
                    soltar();
                }
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
