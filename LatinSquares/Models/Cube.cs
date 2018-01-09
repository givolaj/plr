using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LatinSquares.Models
{

    //count on the fact that symbols > rows (more symbols that rows and columns)
    public class Cube
    {
        private int[,,] values;
        public static int EMPTY = 0;
        public static int NON_EMPTY = 1;

        public Cube(int rows)
        {
            values = new int[rows, rows, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < rows; k++)
                    {
                        values[i, j, k] = EMPTY;
                    }
                }
            }
        }

        public Cube(Rectangle square)
        {
            int size = square.BiggestDimension();
            values = new int[size, size, size];
            int rows = square.GetRowsNumber();
            int cols = square.GetColumnsNumber();
            int symbols = square.SymbolCount();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    for (int k = 0; k < symbols; k++)
                    {
                        if(square.values[i,j] == Utils.SYMBOLS[k])
                            values[i, j, k] = NON_EMPTY;
                        else
                            values[i, j, k] = EMPTY;
                    }
                }
            }
        }

        public Cube GetCubeWithSymbolsAsRowsTranspose()
        {
            int size = values.GetLength(0);
            Cube cube = new Cube(size);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        cube.values[i, j, k] = values[k, i, j];
                    }
                }
            }
            return cube;
        }

        public void initAsUnitCube()
        {
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    values[i, j, (i + j) % (values.GetLength(2))] = NON_EMPTY;
                }
            }
        }

        public Rectangle toRectangle(int rows, int columns)
        {
            Rectangle sq = new Rectangle(rows, columns);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    for (int k = 0; k < values.GetLength(2); k++)
                    {
                        if(values[i,j,k] == 1)
                            sq.Set(Utils.SYMBOLS[k],i,j);
                    }
                }
            }
            return sq;
        }


        bool isProper = true;
        int improper_x, improper_y, improper_z;
        Random rnd = new Random();

        public void runJacobsonMatthewsSteps(int minSteps)
        {          
            for (int t = 0; t < minSteps; t++)
            {
                JacobsonMatthewsRandomStep();
            }
            while (!isProper)
            {
                JacobsonMatthewsRandomStep();
            }
        }

        void JacobsonMatthewsRandomStep()
        {
            int x, y, z;
            int n = values.GetUpperBound(0) + 1;
            if (isProper)
            {
                x = rnd.Next() % n;
                y = rnd.Next() % n;
                z = rnd.Next() % n;
                while (values[x,y,z] != 0)
                {
                    x = rnd.Next() % n;
                    y = rnd.Next() % n;
                    z = rnd.Next() % n;
                }
            }
            else
            {
                x = improper_x;
                y = improper_y;
                z = improper_z;
            }

            int[] x_ones = new int[n],
                  y_ones = new int[n],
                  z_ones = new int[n];
            int x_nr_ones = 0, y_nr_ones = 0, z_nr_ones = 0;

            for (int i = 0; i < n; i++)
            {
                if (values[i,y,z] == 1)
                {
                    x_ones[x_nr_ones] = i;
                    x_nr_ones++;
                }
                if (values[x,i,z] == 1)
                {
                    y_ones[y_nr_ones] = i;
                    y_nr_ones++;
                }
                if (values[x,y,i] == 1)
                {
                    z_ones[z_nr_ones] = i;
                    z_nr_ones++;
                }
            }

            int a = x_ones[rnd.Next() % x_nr_ones], b = y_ones[rnd.Next() % y_nr_ones], c = z_ones[rnd.Next() % z_nr_ones];

            values[x,y,z]++;
            values[x,b,c]++;
            values[a,y,c]++;
            values[a,b,z]++;
            values[a,y,z]--;
            values[x,b,z]--;
            values[x,y,c]--;
            values[a,b,c]--;

            if (values[a,b,c] == 0) isProper = true;
            else
            {
                isProper = false;
                improper_x = a;
                improper_y = b;
                improper_z = c;
            }
        }

    }
}