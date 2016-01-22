using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Game2D
{
    // This class defines rectangular viewport on a piece of the 2D world.
    // It provides a way to map (float float) world coordinates onto
    // (int int) screen coordinates.

    // It also lets you move the camera, and zoom in/out

    public class Camera
    {
        Rectangle screen_region; // region on the screen where the camera is rendering onto
        Vector2 BL;              // bottom-left corner of the corresponding world region
        // (can't use a Rectangle for this, 'cause we need floats)
        float world_W, world_H;  // dimensions of the world rectangle
        Matrix world_to_device;  // maps world coords to device coords
        Matrix device_to_world;  // maps the other way.
        float sx, sy;            // scale factors for mapping world region to screen region

        public static Camera Identity = new Camera();

        public Camera()
        {
            init(new Rectangle(0, 0, 1, 1), new Vector2(0, 1), new Vector2(1, 0));
        }

        public Camera(Rectangle screen_region_p, Vector3 BL_p, Vector3 TR_p)
        {
            init(screen_region_p, new Vector2(BL_p.X, BL_p.Y), new Vector2(TR_p.X, TR_p.Y));
        }

        public Camera(Rectangle screen_region_p, Vector2 BL_p, Vector2 TR_p)
        {
            init(screen_region_p, BL_p, TR_p);
        }

        private void init(Rectangle screen_region_p, Vector2 BL_p, Vector2 TR_p)
        {
            BL = BL_p;
            world_W = TR_p.X - BL_p.X;
            world_H = TR_p.Y - BL_p.Y;
            screen_region = screen_region_p;
            init_matrices();
        }

        private void init_matrices()
        {
            // decompose the world-to-device matrix into a scaling (sx sy)
            // and a translation (tx ty);\
            sx = (screen_region.Width - 1) / world_W;
            sy = -(screen_region.Height - 1) / world_H;
            float tx = screen_region.X - (screen_region.Width - 1) * BL.X / world_W;
            float ty = screen_region.Y + (screen_region.Height - 1) * (BL.Y + world_H) / world_H;
            Matrix translation = Matrix.CreateTranslation(tx, ty, 0);
            Matrix scaling = Matrix.CreateScale(sx, sy, 1);
            world_to_device = scaling * translation;
            device_to_world = Matrix.Invert(world_to_device);
        }

        public Vector2 get_screen_point(Vector2 world_point)
        {
            Vector3 device_point = Vector3.Transform(new Vector3(world_point.X, world_point.Y, 0), world_to_device);
            return new Vector2(device_point.X, device_point.Y);
        }

        public Vector3 get_screen_point(Vector3 world_point)
        {
            return Vector3.Transform(world_point, world_to_device);
        }

        public Vector3 get_world_point(int x, int y)
        {
            return Vector3.Transform(new Vector3(x, y, 0), device_to_world);
        }

        public void move(Vector2 displacement)
        {
            BL += displacement;
            init_matrices();
        }

        public void move_to(Vector2 new_center)
        {
            // Move the camera's bottom left corner into the correct position
            new_center.X -= world_W / 2.0f;
            new_center.Y -= world_H / 2.0f;
            BL = new_center;
            init_matrices();
        }

        public void resize(float scale)
        {
            Vector2 center = new Vector2(BL.X + world_W / 2,
                                         BL.Y + world_H / 2);
            world_W *= scale;
            world_H *= scale;
            BL = center - new Vector2(world_W / 2, world_H / 2);
            init_matrices();
        }

        public void setScale(float scale)
        {
            Vector2 center = new Vector2(BL.X + world_W / 2,
                                         BL.Y + world_H / 2);
            world_W = scale;
            world_H = scale;
            BL = center - new Vector2(world_W / 2, world_H / 2);
            init_matrices();
        }

        public void zoom_in()
        {
            resize(0.9f);
        }

        public void zoom_out()
        {
            resize(1 / 0.9f);
        }

        public Matrix View_Matrix
        {
            get { return world_to_device; }
            set { }
        }

        public Matrix View_Matrix_Inverse
        {
            get { return device_to_world; }
            set { }
        }

        public float x_scale
        {
            get { return sx; }
            set { }
        }

        public float y_scale
        {
            get { return -sy; }
            set { }
        }

        public Vector2 BottomLeft
        {
            get { return BL; }
            set { }
        }

        public float Width
        {
            get { return world_W; }
            set { }
        }

        public float Height
        {
            get { return world_H; }
            set { }
        }

        public override string ToString()
        {
            String result = "Camera: world BL=" + BL + ", world (W H)=(" + world_W + " " + world_H + ")\n";
            result += "device TL=(" + screen_region.X + " " + screen_region.Y
                   + "), device (W H)=(" + screen_region.Width + " " + screen_region.Height + ")\n";
            result += "view_matrix (world to device coords):\n";
            result += View_Matrix + "\n";
            result += "view_matrix^-1 (device to world coords):\n";
            result += View_Matrix_Inverse + "\n";
            return result;
        }

    }
}
