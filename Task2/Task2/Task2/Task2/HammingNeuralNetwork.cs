using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2
{
    class HammingNeuralNetwork
    {
        private List<int[]> patterns;
        private int M; // Первый и второй слои имеют по m нейронов, где m – число образцов
        private int N; // Нейроны первого слоя имеют по n синапсов, соединенных со входами сети
        public int[,] Weight; // Матрица весов размера N x M

        // порог для функции активации
        private double F; 
        // 0 < e <= 1/M
        private double e = 0.01;   
        // eps для сравнения количества отличающихся элементов 
        // Здесь должно быть кол-во отличающихся элементов не более 5% от общей длинны вектора
        private double eps = 0.05;

        public HammingNeuralNetwork(List<int[]> p)
        {
            patterns = p;
            M = patterns.Count;
            N = patterns[0].Length;
            Weight = new int[N, M];
            F = N; 
            for (int i = 0; i < N; ++i)
                for (int j = 0; j < M; ++j)
                    Weight[i, j] = patterns[j][i];
        }

        // Расстояние Хэмминга
        private double distanceHamming(double [] arr1, double [] arr2)
        {
            int count = 0;
            double elemEps = 0.1;
            for (int i = 0; i < arr1.Length; ++i)
            {
                if (Math.Abs(arr1[i] - arr2[i]) > elemEps)
                    count++;
            }

            // какой процент составляет count от длинны массива 
            return (count * 100) / arr1.Length; 
        }

        // Функция активации
        public double funcActivation(double x)
        {
            if (x <= 0)
                return 0;
            else if (x > 0 && x < F)
                return x;
            else
                return F;
        }

        // Нахождение класса, к которому принадлежит изображение image
        public int solution(int [] image)
        {
            var y1 = new double[M];

            for (int j = 0; j < M; ++j)
            {
                double sum = 0;
                for (int i = 0; i < N; ++i)
                    sum += Weight[i, j] * image[i];

                y1[j] = sum + N / 2;
            }

            var y2_0 = y1;

            while (true)
            {
               
                for (int j = 0; j < M; ++j)
                {
                    var y_sum = 0.0;
                    for (int i = 0; i < M; ++i)
                        if (i != j)
                            y_sum += y1[i];
                    y2_0[j] = funcActivation(y1[j] - e * y_sum);
                }

                // кол-во отличающих элементов должно быть меньше 5 % (или 0.05 в данном случае)
                 if (distanceHamming(y2_0, y1) / 100 < eps)
                     break; 
                
                y1 = y2_0;
            }

            // Находим, к какому классу принадлежит изображение и возвращаем номер класса
            // (либо принадлежность установить невозможно, тогда возвращаем - 1)
            var maxElem = y2_0.Max();
            var resY = y2_0;
            var countMax = 0;
            var indexRes = 0;

            for (int i = 0; i < resY.Length; ++i)
            {
                if (resY[i] == maxElem)
                {
                    resY[i] = 1;
                    indexRes = i;
                    countMax++;
                }
                else
                    resY[i] = 0;
            }

            if (countMax > 1)
                return -1;
            else
                return indexRes;
        }

    }
}
