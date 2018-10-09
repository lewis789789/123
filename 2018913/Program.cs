// OpenGL3DNavigationWithTaoCSharp 
// Ercan Polat 02/20/2008
//
// I think one of the most important knowledge for beginners in OpenGL is to 
// learn how to create 3D space and navigate in this space.
// It can be very confiusing if you don't know where are you actually looking at.
// This program shows how to navigate in 3D space. 
//*****************************************************************************************************
// The base code is from the redbook "OpenGL Programming Guide (Third Edition)"
// This is a beginner level program to show how to navigate in 3D space. It uses 
// glRotatef(),	glTranslatef()and glLookAt() function for navigation. In order to make it easy
// I divided the 3D space with lines. The intersection of the x,y,z axis is the
// location (0,0,0). The doted part of the line is the negative side for each axis. Notice that the z-axis 
// is not viewable, because we look at the space from (0,0,15)coordinates. 
// Green for x axis
// Red for y axis
// Blue for z axis  
// x,X - rotates on x axis						// uses glRotatef() funxtion
// y,Y - rotates on y axis									"
// z,Z - rotates on z axis									"
// left_key - translates to left (x axis)       // uses glTranslatef()
// right_key - translates to right (x axis)					  "
// up_key - translates up	(y axis)						  "
// down_key - translates down	(y axis)					  "
// page_up  - translates on z axis (zoom in)				  "
// page_down - translates on z axis (zoom out)				  "
// j,J translates  on x axis					// uses glLookAt()              
// k,K translates  on y axis							"
// l,L translates on z axis								"
// b,B rotates (+/-)90 degrees on x axis
// n,N rotates (+/-)90 degrees on y axis
// m,M rotates (+/-)90 degrees on z axis
// o,O brings everything to default (starting coordinates)
//
// Notice that glTranslatef and glLookAt() looks like exaclty same. However, it has some differences.
// First of all when you use glTranslatef the whole coordinate system is moving to left,right,up or down.
// But, in glLookAt() function the center does not change. When you zoom with glLookAt when it hits negative 
// numbers than it views the whole scene from the backside. So in order to prevent this I just put a if statement
// where the view does not change.
// Make sure to use the keys to understand how to navigate in 3D. I didn't really test the program carefully 
//******************************************************************************************************


using System;
using System.Collections.Generic;
using System.Text;
using Tao.OpenGl;
using Tao.FreeGlut;
using System.IO.Ports;
using System.IO;
using System.Threading;
using MySql.Data.MySqlClient;

namespace OpenGLNavigationWithTaoCSharp
{

    sealed class Program
    {
        static float anglex = 0.0f, angley = 0.0f;
        static SerialPort serialPort2;
        static int count=0;
        static int[] zone = new int[9];
        //static int avg1 = 0, avg2 = 0, avg3 = 0, wrong = 0;
        static int sum1 = 0, sum2 = 0, sum3 = 0;
        static int max12 = 0, max15 = 0, max16 = 0, min12 = 100, min15 = 100, min16 = 100;
        static int  Avg12=0,Avg15=0,Avg16=0;
        static int[] rssi12 = new int[30];
        static int[] rssi15 = new int[30];
        static int[] rssi16 = new int[30];
        static Thread t1 = new Thread(Thread1);
        static Thread t2 = new Thread(Thread2);
        //static SerialPort serialPort1;
        //static double PI = 3.1415926 / 180;
        //static float[][][] Vertex = new float[19][][];
        //static float X = 0.0f;		// Translate screen to x direction (left or right)
        //static float Y = 0.0f;		// Translate screen to y direction (up or down)
        //static float Z = 0.0f;		// Translate screen to z direction (zoom in or out)
        static float rotX = 0.0f;	// Rotate screen on x axis 
        static float rotY = 0.0f;	// Rotate screen on y axis
        static float rotZ = 0.0f;	// Rotate screen on z axis
        static float a, b, c;
        
        static float rotLx = 0.0f;   // Translate screen by using  the glulookAt function (left or right)
        static float rotLy = 0.0f;   // Translate screen by using  the glulookAt function (up or down)
        static float rotLz = 0.0f;   // Translate screen by using  the glulookAt function (zoom in or out)

        // Initiliaze the OpenGL window
        static void init()
        {
            /*
            int i, j;
            for (i = 0; i < 19; i++)
            {
                for (j = 0; j < 19; j++)
                {
                    Vertex[i][j][0] = (float)(0.5 * Math.Cos(j * 20 * PI) * Math.Sin(i * 20 * PI));
                    Vertex[i][j][1] = (float)(0.5 * Math.Sin(j * 20 * PI) * Math.Sin(i * 20 * PI));
                    Vertex[i][j][2] = (float)(0.5 * Math.Cos(i * 20 * PI));
                }
            }*/


            Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);            // Clear the color 
            Gl.glShadeModel(Gl.GL_FLAT);                        // Set the shading model to GL_FLAT
            Gl.glEnable(Gl.GL_LINE_SMOOTH);
            Gl.glHint(Gl.GL_LINE_SMOOTH_HINT, Gl.GL_NICEST);	// Set Line Antialiasing
        }
        // Draw the lines (x,y,z)
        static void display()
        {

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT); //clear buffers to preset values
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();                 // load the identity matrix
            Gl.glTranslated(0, 0, -4);          //moves our figure (x,y,z)
            Gl.glRotated(rotX+anglex, 1, 0, 0); //rotate on x
            Gl.glRotated(rotY+angley, 0, 1, 0); //rotate on y
            Gl.glRotated(rotZ, 0, 0, 1); //rotate on z
            /*//face 1 pink
            Gl.glBegin(Gl.GL_LINE_LOOP);    //start drawing GL_LINE_LOOP is the connection mode
            Gl.glColor3ub(255, 0, 255);
            Gl.glVertex3d(2, 0, -1);
            Gl.glVertex3d(2, 0, 1);
            Gl.glVertex3d(-2, 0, 1);
            Gl.glVertex3d(-2, 0, -1);
            Gl.glEnd();*/
            //face 2 light blue
            Gl.glBegin(Gl.GL_LINE_LOOP);
            Gl.glColor3ub(0, 255, 255);
            Gl.glVertex3d(-2, -1, -1);
            Gl.glVertex3d(2, -1, -1);
            Gl.glVertex3d(2, -1, 1);
            Gl.glVertex3d(-2, -1, 1);
            Gl.glEnd();
            //face 3 yellow
            Gl.glBegin(Gl.GL_LINE_LOOP);
            Gl.glColor3ub(255, 255, 0);
            Gl.glVertex3d(-2, 1, -1);
            Gl.glVertex3d(-2, -1, -1);
            Gl.glVertex3d(-2, -1, 1);
            Gl.glVertex3d(-2, 1, 1);
            Gl.glEnd();
            //face 4 blue
            Gl.glBegin(Gl.GL_LINE_LOOP);
            Gl.glColor3ub(0, 0, 255);
            Gl.glVertex3d(2, 1, 1);
            Gl.glVertex3d(2, -1, 1);
            Gl.glVertex3d(2, -1, -1);
            Gl.glVertex3d(2, 1, -1);
            Gl.glEnd();
            //face 5 green
            Gl.glBegin(Gl.GL_LINE_LOOP);
            Gl.glColor3ub(0, 255, 0);
            Gl.glVertex3d(-2, 1, -1);
            Gl.glVertex3d(-2, 1, 1);
            Gl.glVertex3d(2, 1, 1);
            Gl.glVertex3d(2, 1, -1);
            Gl.glEnd();
            //face 6 red
            Gl.glBegin(Gl.GL_LINE_LOOP);
            Gl.glColor4d(255, 0, 0, 100);
            Gl.glVertex3d(-2, 1, 1);
            Gl.glVertex3d(-2, -1, 1);
            Gl.glVertex3d(2, -1, 1);
            Gl.glVertex3d(2, 1, 1);
            Gl.glEnd();
            //face 九
            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3ub(0, 55, 0);
            Gl.glVertex3d(-0.65, -1, -1);
            Gl.glVertex3d(-0.65, -1, 1);
            Gl.glVertex3d(0.65, -1, -1);
            Gl.glVertex3d(0.65, -1, 1);


            Gl.glVertex3d(-2, -1, -0.35);
            Gl.glVertex3d(2, -1, -0.35);
            Gl.glVertex3d(-2, -1, 0.35);
            Gl.glVertex3d(2, -1, 0.35);

            Gl.glEnd();

            Gl.glRasterPos3d(-1.8, -1, 0.8);    // Set the position for the string (text) 
            text("Zone 1");                      // Display the text "Front" 
            Gl.glRasterPos3d(-0.5, -1, 0.8);    // Set the position for the string (text) 
            text("Zone 2");                      // Display the text "Front" 
            Gl.glRasterPos3d(0.8, -1, 0.8);    // Set the position for the string (text) 
            text("Zone 3");                      // Display the text "Front" 
            Gl.glRasterPos3d(-1.8, -1, 0.1);    // Set the position for the string (text) 
            text("Zone 4");                      // Display the text "Front" 
            Gl.glRasterPos3d(-0.5, -1, 0.1);    // Set the position for the string (text) 
            text("Zone 5");                      // Display the text "Front" 
            Gl.glRasterPos3d(0.8, -1, 0.1);    // Set the position for the string (text) 
            text("Zone 6");                      // Display the text "Front" 
            Gl.glRasterPos3d(-1.8, -1, -0.6);    // Set the position for the string (text) 
            text("Zone 7");                      // Display the text "Front" 
            Gl.glRasterPos3d(-0.5, -1, -0.6);    // Set the position for the string (text) 
            text("Zone 8");                      // Display the text "Front" 
            Gl.glRasterPos3d(0.8, -1, -0.6);    // Set the position for the string (text) 
            text("Zone 9");                      // Display the text "Front" 
            /*Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3ub(0, 55, 0);
            Gl.glVertex3d(-0.6, 0, -1);
            Gl.glVertex3d(-0.6, 0, 1);
            Gl.glVertex3d(0.6, 0, -1);
            Gl.glVertex3d(0.6, 0, 1);

            Gl.glVertex3d(-2, 0, 0);
            Gl.glVertex3d(2, 0, -0);

            Gl.glEnd();*/


            //Gl.glBegin(Gl.GL_POLYGON);

            /*            
                         float radious = 0.05f;     //radious为圆半径
                         float step = 0.1f;        //step为画圆的步长
                         for (double angle = 0.0f; angle <= (2.0f * 3.14159f); angle += step)
                         {
                             double x = radious * Math.Sin(angle);
                             double y = radious * Math.Cos(angle);
                            double z = radious * Math.Cos(angle);
                            Gl.glColor3f(1.0f, 1.0f, 1.0f);

                             // Specify the point and move the Z value up a little	
                             Gl.glVertex3f((float)x, (float)y, (float)z);
                         }

                         Gl.glEnd();
            */
            /////////////////////////////畫點
            int Theta, Phi;
            float radious = 0.05f;
            float PI = 3.14159f / 180;
            double x, y, z;
            for (Theta = 0; Theta < 360; Theta += 20)
            {
                Gl.glColor3ub(255, 255, 255);
                Gl.glBegin(Gl.GL_LINE_STRIP);
                for (Phi = 0; Phi < 360; Phi += 20)
                {
                    x = radious * Math.Cos(Phi * PI) * Math.Sin(Theta * PI);
                    y = radious * Math.Sin(Phi * PI) * Math.Sin(Theta * PI);
                    z = radious * Math.Cos(Theta * PI);
                    Gl.glVertex3f((float)x + a, (float)y + b, (float)z + c);
                }
                Gl.glEnd();
            }
            for (Phi = 0; Phi < 360; Phi += 20)
            {
                Gl.glColor3ub(255, 255, 255);
                Gl.glBegin(Gl.GL_LINE_STRIP);
                for (Theta = 0; Theta < 360; Theta += 20)
                {
                    x = radious * Math.Cos(Phi * PI) * Math.Sin(Theta * PI);
                    y = radious * Math.Sin(Phi * PI) * Math.Sin(Theta * PI);
                    z = radious * Math.Cos(Theta * PI);
                    Gl.glVertex3f((float)x + a, (float)y + b, (float)z + c);
                }
                Gl.glEnd();
            }

            //////////////////////////////////

            /* int i, j;
             Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);
             for (i = 0; i < 18; i++)
             {
                 Gl.glBegin(Gl.GL_QUAD_STRIP);

                 // 畫好第一個 QUAD
                 Gl.glVertex3fv(Vertex[0][i]);
                 Gl.glVertex3fv(Vertex[0][i + 1]);
                 Gl.glVertex3fv(Vertex[1][i + 1]);
                 Gl.glVertex3fv(Vertex[1][i]);
                 // 照順序把後面的的畫出來
                 for (j = 2; j < 18; j++)
                 {
                     Gl.glVertex3fv(Vertex[j][i + 1]);
                     Gl.glVertex3fv(Vertex[j][i]);
                 }
                 Gl.glEnd();
             }*/


            Gl.glFlush();

            Gl.glPopMatrix();                           // Don't forget to pop the Matrix


            Glut.glutSwapBuffers();
        }
        static void text(string c)
        {
            for (int i = 0; i < c.Length; i++)
            {
                // Render bitmap character 
                Glut.glutBitmapCharacter(Glut.GLUT_BITMAP_TIMES_ROMAN_10, c[i]);
            }
        }
        // This function is called whenever the window size is changed
        static void reshape(int w, int h)
        {

            Gl.glViewport(0, 0, w, h);              // Set the viewport
            Gl.glMatrixMode(Gl.GL_PROJECTION);                              // Set the Matrix mode
            Gl.glLoadIdentity();
            Glu.gluPerspective(75f, (float)w / (float)h, 0.10f, 100.0f);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Glu.gluLookAt(rotLx, rotLy, 15.0f + rotLz, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f);
        }

        // This function is used for the navigation keys


        // called on special key pressed
        private static void specialKey(int key, int x, int y)
        {

            // The keys below are using the gluLookAt() function for navigation
            // Check which key is pressed
            switch (key)
            {
                case Glut.GLUT_KEY_LEFT:    // Rotate on x axis
                    rotY -= 2.0f;
                    break;
                case Glut.GLUT_KEY_RIGHT:   // Rotate on x axis (opposite)
                    rotY += 2.0f;
                    break;
                case Glut.GLUT_KEY_UP:      // Rotate on y axis 
                    rotX += 2.0f;
                    break;
                case Glut.GLUT_KEY_DOWN:    // Rotate on y axis (opposite)
                    rotX -= 2.0f;
                    break;
            }
            Glut.glutPostRedisplay();       // Redraw the scene
        }
        public static void getserialport()
        {
            SerialPort myport = new SerialPort();

            myport.BaudRate = 9600; //需跟arduno設定的一樣
            myport.PortName = "COM3"; //指定PortName 
            //myport.ReadTimeout = 500;
            //myport.WriteTimeout = 500;
            Console.WriteLine("start read");
            Console.Write("Time : ");
            Console.WriteLine(DateTime.Now.TimeOfDay);
            myport.Open();
            for (int p = 0;p < 30; ) {
                string data = myport.ReadLine();
                data = data.Replace('\r', ' ');
                data = data.Trim();
                string[] str2 = data.Split(',');
                if (str2.Length == 2 && str2[0] == "12" && str2[1].Length == 2)
                {
                    //Console.WriteLine(data);
                    rssi12[p] = Convert.ToInt32(str2[1], 16);
                    if (rssi12[p] >= max12)
                        max12 = rssi12[p];
                    if (rssi12[p] <= min12)
                        min12 = rssi12[p];
                    sum1 += rssi12[p];
                    while (true) {
                        data = myport.ReadLine();
                        data = data.Replace('\r', ' ');
                        data = data.Trim();
                        str2 = data.Split(',');
                        if (str2.Length == 2 && str2[0] == "15" && str2[1].Length == 2)
                        {
                           // Console.WriteLine(data);
                            rssi15[p] = Convert.ToInt32(str2[1], 16);
                            if (rssi15[p] >= max15)
                                max15 = rssi15[p];
                            if (rssi15[p] <= min15)
                                min15 = rssi15[p];
                            sum2 += rssi15[p];
                            while (true)
                            {
                                data = myport.ReadLine();
                                data = data.Replace('\r', ' ');
                                data = data.Trim();
                                str2 = data.Split(',');
                                if (str2.Length == 2 && str2[0] == "16" && str2[1].Length == 2)
                                {
                                   // Console.WriteLine(data);
                                    rssi16[p] = Convert.ToInt32(str2[1], 16);
                                    if (rssi16[p] >= max16)
                                        max16 = rssi16[p];
                                    if (rssi16[p] <= min16)
                                        min16 = rssi16[p];
                                    sum3 += rssi16[p];
                                    p++;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
                myport.Close();

            Avg12 = sum1 / 30;
            Avg15 = sum2 / 30;
            Avg16 = sum3 / 30;
            //int std=
            double m1, n1, m2, n2, m3, n3,q1=0, q2 = 0, q3 = 0;
            for (int i=0;i<30 ; i++)
            {
                 m1 = rssi12[i] - Avg12;
                 n1 = Math.Pow(m1, 2.0);
                q1 += n1;
            }
            double std1 = System.Math.Sqrt(q1 / 30);
            for (int i = 0; i < 30; i++)
            {
                m2 = rssi15[i] - Avg15;
                n2 = Math.Pow(m2, 2.0);
                q2 += n2;
            }
            double std2 = System.Math.Sqrt(q2 / 30);
            for (int i = 0; i < 30; i++)
            {
                m3 = rssi16[i] - Avg16;
                n3 = Math.Pow(m3, 2.0);
                q3 += n3;
            }
            double std3 = System.Math.Sqrt(q3 / 30);
            q1 = 0; q2 = 0; q3 = 0;

            sum1 = 0; sum2 = 0; sum3 = 0;
            
            Console.Write("12=");
            Console.WriteLine(Avg12);
            Console.Write("15=");
            Console.WriteLine(Avg15);
            Console.Write("16=");
            Console.WriteLine(Avg16);
            Console.Write("Max12=");
            Console.WriteLine(max12);
            Console.Write("Max15=");
            Console.WriteLine(max15);
            Console.Write("Max16=");
            Console.WriteLine(max16);
            Console.Write("Min12=");
            Console.WriteLine(min12);
            Console.Write("Min15=");
            Console.WriteLine(min15);
            Console.Write("Min16=");
            Console.WriteLine(min16);
            max12 = 0; max15 = 0; max16 = 0; min12 = 100; min15 = 100; min16 = 100;
            Glut.glutPostRedisplay();

        }
        static void test(int n)
        {
            /*
            if (count < 9)
                zone[count] = 1;
            else {
                count = 0;
                zone[count] = 1;
            }
            */

            String todo = Convert.ToString(/*count+1*/ n);
            //開啟連線
            MySqlConnection conn = new MySqlConnection("server=169.254.129.252;user=root;database=contact;port=3306;password=password;");
            conn.Open();
            

            //將 test_table 資料表中 id 欄位值為 1 的資料，修改 name 欄位值為 testName2
            string sql = "UPDATE contact SET position='zone ' @todo WHERE id='0001'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@todo", todo);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        static void point()
        {
            
            if (Avg12 <= 73 && Avg12 >= 56 && Avg15 <=66 && Avg15 >= 50 && Avg16 <=54 && Avg16>=43)
            {
                a = -1.25f;
                b = -1f;
                c = 0.7f;
                test(1);
            }
            else if (Avg12 <= 60 && Avg12 >= 58 && Avg15 <= 64 && Avg15 >= 59 && Avg16 <= 74 && Avg16 >= 58)
            {
                a = 0.0f;
                b = -1f;
                c = 0.7f;
                test(2);
            }
            else if (Avg12 <= 63 && Avg12 >= 57 && Avg15 <= 48 && Avg15 >= 47 && Avg16 <= 74 && Avg16 >= 62)
            {
                a = 1.25f;
                b = -1f;
                c = 0.7f;
                test(3);
            }
            else if (Avg12 <= 56 && Avg12 >= 52 && Avg15 <= 60 && Avg15 >= 56 && Avg16 <= 73 && Avg16 >= 63)
            {
                a = 1.25f;
                b = -1f;
                c = 0f;
                test(6);
            }
            else if (Avg12 <= 67 && Avg12 >= 63 && Avg15 <= 59 && Avg15 >= 56 && Avg16 <= 70 && Avg16 >= 59)
            {
                a = 0f;
                b = -1f;
                c = 0f;
                test(5);
            }
            else if (Avg12 <= 74 && Avg12 >= 61 && Avg15 <= 67 && Avg15 >= 57 && Avg16 <= 55 && Avg16 >= 50)
            {
                a = -1.25f;
                b = -1f;
                c = 0f;
                test(4);
            }
            else if (Avg12 <= 63 && Avg12 >= 58 && Avg15 <= 57 && Avg15 >= 54 && Avg16 <= 73 && Avg16 >= 60)
            {
                a = -1.25f;
                b = -1f;
                c = -0.7f;
                test(7);
            }
            else if (Avg12 <= 62 && Avg12 >= 54 && Avg15 <= 61 && Avg15 >= 56 && Avg16 <= 74 && Avg16 >= 59)
            {
                a = 0f;
                b = -1f;
                c = -0.7f;
                test(8);
            }
            else if (Avg12 <= 70 && Avg12 >= 59 && Avg15 <= 76 && Avg15 >= 63 && Avg16 <= 67 && Avg16 >= 59)
            {
                a = 1.25f;
                b = -1f;
                c = -0.7f;
                test(9);
            }
           

            /*
            if (zone[0]==1)
            {
                a = -1.25f;
                b = -1f;
                c = 0.7f;
            }
            else if (zone[1] == 1)
            {
                a = 0.0f;
                b = -1f;
                c = 0.7f;
            }
            else if (zone[2] == 1)
            {
                a = 1.25f;
                b = -1f;
                c = 0.7f;
            }
            else if (zone[3] == 1)
            {
                a = -1.25f;
                b = -1f;
                c = 0f;
            }
            else if (zone[4] == 1)
            {
                a = 0f;
                b = -1f;
                c = 0f;
            }
            else if (zone[5] == 1)
            {
                a = 1.25f;
                b = -1f;
                c = 0f;
            }
            else if (zone[6] == 1)
            {
                a = -1.25f;
                b = -1f;
                c = -0.7f;
            }
            else if (zone[7] == 1)
            {
                a = 0f;
                b = -1f;
                c = -0.7f;
            }
            else if (zone[8] == 1)
            {
                a = 1.25f;
                b = -1f;
                c = -0.7f;
            }
            */
        }

        public static void gesture()
        {
            serialPort2 = new SerialPort();
            serialPort2.PortName = "COM12";
            serialPort2.BaudRate = 9600;
            serialPort2.Open();
            serialPort2.DataReceived += serialPort2_DataReceived;
            while (true)
            {
                //原本讀值的工作交給 myport_DataReceived 去完成
            }

        }
        public static void serialPort2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort2.ReadLine();
            //Console.WriteLine(data);
            char[] str2 = data.ToCharArray();
            if (str2[0] == 'd')
            {
                //specialKey(Glut.GLUT_KEY_UP, 0, 0);
                timerdown(15);
            }
            else if (str2[0] == 'l')
            {
                //specialKey(Glut.GLUT_KEY_LEFT, 0, 0);
                timerleft(15);
            }
            else if (str2[0] == 'u')
            {
                //specialKey(Glut.GLUT_KEY_DOWN, 0, 0);
                timerup(15);
            }
            else if (str2[0] == 'r')
            {
                //specialKey(Glut.GLUT_KEY_RIGHT, 0, 0);
                timerright(15);
            }

        }

        static void timerdown(int p)
        {
            //point();
            if (p > 5)
            {
                anglex += 1.5f;
                Glut.glutPostRedisplay();
                Glut.glutTimerFunc(10, timerdown, p - 1);
            }
            else if (p > 0)
            {
                anglex += 1.5f;
                Glut.glutPostRedisplay();
                Glut.glutTimerFunc(30, timerdown, p - 1);
            }
        }
        static void timerup(int p)
        {
            //point();
            if (p > 5)
            {
                anglex -= 1.5f;
                Glut.glutPostRedisplay();
                Glut.glutTimerFunc(10, timerup, p - 1);
            }
            else if (p > 0)
            {
                anglex -= 1.5f;
                Glut.glutPostRedisplay();
                Glut.glutTimerFunc(30, timerup, p - 1);
            }
        }
        static void timerleft(int p)
        {
            //point();
            if (p > 5)
            {
                angley -= 1.5f;
                Glut.glutPostRedisplay();
                Glut.glutTimerFunc(10, timerleft, p - 1);
            }
            else if (p > 0)
            {
                angley -= 1.5f;
                Glut.glutPostRedisplay();
                Glut.glutTimerFunc(30, timerleft, p - 1);
            }
        }
        static void timerright(int p)
        {
            //point();
            if (p > 5)
            {
                angley += 1.5f;
                Glut.glutPostRedisplay();
                Glut.glutTimerFunc(10, timerright, p - 1);
            }
            else if (p > 0)
            {
                angley += 1.5f;
                Glut.glutPostRedisplay();
                Glut.glutTimerFunc(30, timerright, p - 1);
            }
        }


        static void Thread1()
        {
            while (true)
            {
                gesture();
                getserialport();
                Thread.Sleep(1);
            }
        }
        
        static void Thread2()
        {
            while (true)
            {
                /*
                test();
                zone[count] = 0;
                count++;
                Glut.glutPostRedisplay();
                */
                getserialport();
                Thread.Sleep(1);
            }
        }
        
        static void TimerFunction(int p)
        {
            point();
            Glut.glutTimerFunc(100, TimerFunction, 1);
        }
        static void Main(string[] args)
        {

            t1.Start();
            t1.IsBackground = true;
            
            t2.Start();
            t2.IsBackground = true;
            
            //getserialport();


            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_RGB);		// Setup display mode to double buffer and RGB color
            Glut.glutInitWindowSize(800, 800);						        // Set the screen size
            Glut.glutCreateWindow("OpenGL 3D Navigation Program With Tao");
            init();
            Glut.glutReshapeFunc(reshape);
            Glut.glutDisplayFunc(display);
            Glut.glutSpecialFunc(new Glut.SpecialCallback(specialKey));	     // set window's to specialKey callback
            TimerFunction(0);
            //TimerFunction2(0);
            Glut.glutMainLoop();
        }
    }
}


