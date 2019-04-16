using System.Globalization;

namespace Recommendation.Service
{
    public static class Matrix
    {
        public static string MatrixToString(double[,] matrix)
        {
            var matrixString = "";

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrixString += matrix[i, j].ToString(new CultureInfo("en-US"));

                    if (j < matrix.GetLength(1) - 1)
                        matrixString += " ";
                }

                if (i < matrix.GetLength(0) - 1)
                    matrixString += ";";
            }

            return matrixString;
        }

        public static double[,] JoinMatrices(double[,] m1, double[,] m2)
        {
            if (m1.GetLength(0) != m2.GetLength(0))
            {
                if (m1.GetLength(0) == 0)
                    return m2;

                if (m2.GetLength(0) == 0)
                    return m1;

                return new double[0, 0];
            }

            var rowCount = m1.GetLength(0);
            var columnCount = m1.GetLength(1) + m2.GetLength(1);
            var matrix = new double[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    if (j + 1 <= m1.GetLength(1))
                        matrix[i, j] = m1[i, j];
                    else if (j + 1 - m1.GetLength(1) <= m2.GetLength(1))
                        matrix[i, j] = m2[i, j - m1.GetLength(1)];
                }
            }

            return matrix;
        }

        public static double[,] CastMatrix(long[,] matrix)
        {
            var newMatrix = new double[matrix.GetLength(0), matrix.GetLength(1)];

            for (int i = 0; i < newMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < newMatrix.GetLength(1); j++)
                {
                    newMatrix[i, j] = (double)matrix[i, j];
                }
            }
            return newMatrix;
        }
    }
}
