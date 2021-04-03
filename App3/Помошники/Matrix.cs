using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWP.Помошники
{
    class Matrix
    {
        int[,] matrix;
        int determinant;
        public bool flag;

        public Matrix(int[,] matric)
        {
            this.matrix = matric;
            GetMajorOpredelitel();
        }

        public int GetMajorOpredelitel()
        {
            int[,] a = matrix;
            int opred = a[0, 0] * a[1, 1] * a[2, 2] - a[0, 0] * a[1, 2] * a[2, 1] - a[0, 1] * a[1, 0] * a[2, 2] +
                a[0, 1] * a[1, 2] * a[2, 0] + a[0, 2] * a[1, 0] * a[2, 1] - a[0, 2] * a[1, 1] * a[2, 0];

            determinant = opred;
            return opred;
        }

        public double[,] GetReverseMatric()
        {
            if (determinant == 0) flag = true;
            int[,] joinMatrix = GetJoinMatrics();
            int ROWS = joinMatrix.GetLength(0);
            int COLS = joinMatrix.GetLength(1);
            double[,] final = new double[ROWS, COLS];
            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    final[i, j] = Convert.ToDouble(joinMatrix[i, j]) / determinant;
                }
            }
            return final;
        }

        private int[,] GetJoinMatrics()
        {
            int ROWS = matrix.GetLength(0);
            int COLS = matrix.GetLength(1);
            int[,] a = new int[ROWS, COLS];
            int[,] temp = new int[ROWS - 1, COLS - 1];
            int r = 0, c = 0;
            for (int k = 0; k < ROWS; k++)
            {
                for (int n = 0; n < COLS; n++)
                {
                    for (int i = 0; i < ROWS; i++)
                    {
                        if (i == k) continue;
                        for (int j = 0; j < COLS; j++)
                        {
                            if (j == n)
                            {
                                continue;
                            }
                            temp[r, c++] = matrix[i, j];
                        }
                        r++;
                        c = 0;
                    }
                    r = 0;
                    a[n, k] = GetDopol(k, n, temp);
                }
            }
            return a;
        }

        private int GetDopol(int k, int n, int[,] temp)
        {
            double step = (k + 1) + (n + 1);
            int dopol = (int)Math.Pow(-1, step);
            int dop = temp[0, 0] * temp[1, 1] - temp[1, 0] * temp[0, 1];
            return dopol * dop;
        }
    }
}
