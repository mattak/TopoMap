using System.Linq;
using UnityEngine;

namespace TopoMap
{
    public static class TopoLine
    {
        public static GameObject CreateLine(
            GameObject Prefab,
            Transform parent,
            Vector2[] points,
            float width)
        {
            // 東京 -> 富山 -> 大阪
            // 35.719051, 139.731716
            // 36.706789, 137.218482
            // 34.662423, 135.502534

            // p12とp23の交点を求める

            //   . p2 -- . p2'
            //   |       |
            //  -       -
            // |       |
            // . p1 -- . p1'
            // <-  d  ->

            // p1p2 と p1'p2'は傾き同じ
            // p1p2: a*x + b*y + c = 0
            // p2p3: a*x + b*y + c + c_delta = 0
            // c_delta = d*sin(θ) + d*cos(θ)

            // pa: a1x + b1y + c2 = 0
            // pb: a2x + b2y + c2 = 0
            // pt:
            // a1*x + b1*y + c1 = 0
            // a2*x + b2*y + c2 = 0
            // (a1*b2 - a2*b1)x = c2 - c1
            // (b1*a2 - b2*a1)y = c2 - c1
            // x = (c2 - c1)/(a1*b2 - a2*b1)
            // y = (c2 - c1)/(b1*a2 - b2*a1)

            // y = ax + b
            // a = (p2.y - p1.y) / (p2.x - p1.x)
            // b = p2.y - (p2.y - p1.y) / (p2.x - p1.x) * p2.x
            // y = (p2.y - p1.y) / (p2.x - p1.x) * x + p2.y - (p2.y - p1.y) / (p2.x - p1.x) * p2.x
            // (p2.x - p1.x) y - (p2.y -p1.y) x - p2.y (p2.x - p1.x) + p2.x (p2.y - p1.y) = 0
            // (p2.x - p1.x) y - (p2.y -p1.y) x - p2.y * p2.x + p2.y * p1.x + p2.x * p2.y - p2.x * p1.y = 0
            // (p2.x - p1.x) y + (p1.y -p2.y) x + p1.x*p2.y - p2.x*p1.y = 0
            // (p1.y -p2.y) x + (p2.x - p1.x) y + p1.x*p2.y - p2.x*p1.y = 0

            // a1 = (p1.y - p2.y)
            // b1 = (p2.x - p1.x)
            // c1 = p1.x*p2.y - p2.x*p1.y

            // a2 = (p2.y - p3.y)
            // b2 = (p3.x - p2.x)
            // c2 = p2.x*p3.y - p3.x*p2.y

            // x = (c2 - c1) / (a1*b2 - a2*b1)
            // y = (c2 - c1) / (b1*a2 - b2*a1)

            // c2 - c1 = (p2.x*p3.y - p3.x*p2.y - p1.x*p2.y + p2.x*p1.y)
            // a1*b2   = (p1.y - p2.y) * (p3.x - p2.x)
            //         = p1.y*p3.x  + p2.y*p2.x - p1.y*p2.x - p2.y*p3.x
            // a2*b1   = (p2.y - p3.y) * (p2.x - p1.x)
            //         = p3.y*p1.x + p2.y*p2.x - p2.y*p1.x - p3.y*p2.x

            // a1*b2 - a2*b1 = (p1.y*p3.x  + p2.y*p2.x - p1.y*p2.x - p2.y*p3.x)
            //               - (p3.y*p1.x + p2.y*p2.x - p2.y*p1.x - p3.y*p2.x)
            //               = p1.y*p3.x - p1.y*p2.x - p2.y*p3.x - p3.y*p1.x + p2.y*p1.x + p3.y*p2.x

            Vector2[] verts = CreateVerts(points, width);
            int[] triangles = CreateTriangles(points.Length);

            var obj = GameObject.Instantiate(Prefab, parent, false) as GameObject;
            var meshFilter = obj.GetComponent<MeshFilter>();

            Mesh mesh = new Mesh();
            mesh.vertices = verts.Select(p => new Vector3(p.x, 0, p.y)).ToArray();
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;

            return obj;
        }

        private static int[] CreateTriangles(int point_count)
        {
            int[] triangles = new int[(point_count - 1) * 6];

            for (int i = 0; i < point_count - 1; i++)
            {
                int triangle_index = i * 6;
                int vert_index = i << 1;
                triangles[triangle_index] = vert_index;
                triangles[triangle_index + 1] = vert_index + 1;
                triangles[triangle_index + 2] = vert_index + 2;
                triangles[triangle_index + 3] = vert_index + 2;
                triangles[triangle_index + 4] = vert_index + 1;
                triangles[triangle_index + 5] = vert_index + 3;
            }

            return triangles;
        }


        public static Vector2[] CreateVerts(Vector2[] points, float width)
        {
            if (points.Length < 2)
            {
                Debug.Log("count of points should be more than 2");
                return new Vector2[0];
            }

            int vertsLength = points.Length << 1;

            Vector2[] result = new Vector2[vertsLength];
            float theta_previous = Mathf.Atan2(points[1].y - points[0].y, points[1].x - points[0].x);
            float sin_theta_previous = Mathf.Sin(theta_previous);
            float cos_theta_previous = Mathf.Cos(theta_previous);
            Vector2[] p1_updown = CalculateUpDownPoints(points[0], sin_theta_previous, cos_theta_previous, width);

            result[0] = p1_updown[0];
            result[1] = p1_updown[1];

            for (int i = 1; i < points.Length - 1; i++)
            {
                float theta = Mathf.Atan2(points[i + 1].y - points[i].y, points[i + 1].x - points[i].x);

                float sin_theta = Mathf.Sin(theta);
                float cos_theta = Mathf.Cos(theta);

                Vector2[] p2_updown = CalculateUpDownPoints(points[i], sin_theta, cos_theta, width);

                Vector2[] mediums = CalculateMidium(
                    p1_updown,
                    p2_updown,
                    sin_theta_previous,
                    cos_theta_previous,
                    sin_theta,
                    cos_theta);

                int i2 = i << 1; // i*2
                result[i2] = mediums[0];
                result[i2 + 1] = mediums[1];

                sin_theta_previous = sin_theta;
                cos_theta_previous = cos_theta;
                p1_updown = p2_updown;
            }

            {
                int length = points.Length;
                float theta = Mathf.Atan2(points[length - 1].y - points[length - 2].y,
                    points[length - 1].x - points[length - 2].x);
                float sin_theta = Mathf.Sin(theta);
                float cos_theta = Mathf.Cos(theta);

                Vector2[] p2_updown = CalculateUpDownPoints(points[length - 1], sin_theta, cos_theta, width);
                result[vertsLength - 2] = p2_updown[0];
                result[vertsLength - 1] = p2_updown[1];
            }

            return result;
        }

        private static Vector2[] CalculateUpDownPoints(Vector2 p, float sin_theta, float cos_theta, float width)
        {
            float width_cos_theta = width * cos_theta;
            float width_sin_theta = width * sin_theta;
            Vector2 p_up = new Vector2(p.x + width_sin_theta, p.y - width_cos_theta);
            Vector2 p_down = new Vector2(p.x - width_sin_theta, p.y + width_cos_theta);
            return new Vector2[] {p_up, p_down};
        }

        // Input: p1,p2, p12_theta, p23_theta
        // Output: p2', p2''
        public static Vector2[] CalculateMidium(
            Vector2[] p1_updown,
            Vector2[] p2_updown,
            float sin_theta1, float cos_theta1, float sin_theta2, float cos_theta2)
        {
            // ax + by + c = 0
            // a = -sin(theta)
            // b = cos(theta)
            // c = p.x*sin(theta) - p.y*cos(theta)

            // a1x + b1y + c1 = 0
            // a2x + b2y + c2 = 0

            // a1a2x + a2b1y + a2c1 = 0
            // a1a2x + a1b2y + a1c2 = 0
            // (a2b1 - a1b2)y + (a2c1 - a1c2) = 0
            // y = (a1c2 - a2c1) / (a2b1 - a1b2)

            // a1b2x + b1b2y + b2c1 = 0
            // a2b1x + b1b2y + b1c2 = 0
            // (a1b2 - a2b1)x + (b2c1 - b1c2) = 0
            // x = (b1c2 - b2c1) / (a1b2 - a2b1)

            // x = (b1c2 - b2c1) / (a1b2 - a2b1)
            // y = (a2c1 - a1c2) / (a1b2 - a2b1)

            // c1b2-c2b1 = (p1.x*sin(theta1) - p1.y*cos(theta1)) * cos(theta2)
            //           - (p2.x*sin(theta2) - p2.y*cos(theta2)) * cos(theta1)
            // c1a2-c2a1 = (p1.x*sin(theta1) - p1.y*cos(theta1)) * -sin(theta2)
            //           - (p2.x*sin(theta2) - p2.y*cos(theta2)) * -sin(theta1)
            // a1*b2 = -sin(theta12) * cos(theta23)
            // a2*b1 = -sin(theta23) * cos(theta12)
            // a1*b2 - a2*b1 = -sin(theta12)*cos(theta23) + sin(theta23)*cos(theta12) = sin(theta23 - theta12)

            float a1b2_minus_a2b1 = -sin_theta1 * cos_theta2 + sin_theta2 * cos_theta1;

            if (a1b2_minus_a2b1 == 0f)
            {
                return new Vector2[]
                {
                    (p1_updown[0] + p2_updown[0]) * 0.5f,
                    (p1_updown[1] + p2_updown[1]) * 0.5f,
                };
            }

            Vector2 p1_up = p1_updown[0];
            Vector2 p1_down = p1_updown[1];
            Vector2 p2_up = p2_updown[0];
            Vector2 p2_down = p2_updown[1];

            float c1b2_c2b1_up = (p1_up.x * sin_theta1 - p1_up.y * cos_theta1) * cos_theta2 -
                                 (p2_up.x * sin_theta2 - p2_up.y * cos_theta2) * cos_theta1;

            float c1a2_c2a1_up = (p1_up.x * sin_theta1 - p1_up.y * cos_theta1) * -sin_theta2 +
                                 (p2_up.x * sin_theta2 - p2_up.y * cos_theta2) * sin_theta1;

            float c1b2_c2b1_down = (p1_down.x * sin_theta1 - p1_down.y * cos_theta1) * cos_theta2 -
                                   (p2_down.x * sin_theta2 - p2_down.y * cos_theta2) * cos_theta1;
            float c1a2_c2a1_down = (p1_down.x * sin_theta1 - p1_down.y * cos_theta1) * -sin_theta2 +
                                   (p2_down.x * sin_theta2 - p2_down.y * cos_theta2) * sin_theta1;

            return new Vector2[]
            {
                new Vector2(c1b2_c2b1_up / -a1b2_minus_a2b1, c1a2_c2a1_up / a1b2_minus_a2b1),
                new Vector2(c1b2_c2b1_down / -a1b2_minus_a2b1, c1a2_c2a1_down / a1b2_minus_a2b1),
            };
        }
    }
}