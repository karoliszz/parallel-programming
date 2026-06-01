using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

class Program
{
    static Random rand = new Random();

    static void Main()
    {
        Console.Write("kiek giju naudoti?");
        int threadCount = int.Parse(Console.ReadLine());

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        //ribos
        double x_min = -10, x_max = 10, y_min = -10, y_max = 10;

        //parduotuves
        int n_existing = 3;
        int m_new = 32;

        double[,] existingStores = RandomPoints(n_existing, x_min, x_max, y_min, y_max);
        double[,] newStores = RandomPoints(m_new, x_min, x_max, y_min, y_max);
        double[,] initialNewStores = (double[,])newStores.Clone();

        //pradinis vertinimas
        var (initialCost, initialCosts) = CalcTotalCost(newStores, existingStores, threadCount);

        Console.WriteLine("pradine bendra kaina: " + initialCost);
        Console.WriteLine("pradines nauju parduotuviu kainos: " + string.Join("\n", initialCosts));

        double stepSize = 0.1;
        int iterations = 4000;
        double tolerance = 1e-6;

        double[] allCosts = new double[iterations];

        //main
        for (int iter = 0; iter < iterations; iter++)
        {
            double[,] grad = Gradient(newStores, existingStores, threadCount);

            //gradient
            for (int i = 0; i < m_new; i++)
                for (int j = 0; j < 2; j++)
                    newStores[i, j] -= stepSize * grad[i, j];

            var (cost, _) = CalcTotalCost(newStores, existingStores, threadCount);
            allCosts[iter] = cost;

            if (Norm(grad, threadCount) < tolerance)
                break;
        }

        

        Console.WriteLine("\nsenu parduotuviu koordinates:");
        PrintMatrix(existingStores);

        Console.WriteLine("\npradines nauju parduotuviu koordinates:");
        PrintMatrix(initialNewStores);

        Console.WriteLine("\ngalutines optimizuotu parduotuviu koordinates:");
        PrintMatrix(newStores);

        var (finalTotal, finalCosts) = CalcTotalCost(newStores, existingStores, threadCount);

        Console.WriteLine("\nbendra kaina po optimizacijos: " + finalTotal);
        Console.WriteLine("kiekvienos parduotuves kaina po optimizacijos: " +
                          string.Join("\n", finalCosts));

        stopwatch.Stop();
        Console.WriteLine($"laiko trukme: {stopwatch.ElapsedMilliseconds} ms");
    }

 
    static double[,] RandomPoints(int n, double xMin, double xMax, double yMin, double yMax)
    {
        double[,] arr = new double[n, 2];
        for (int i = 0; i < n; i++)
        {
            arr[i, 0] = rand.NextDouble() * (xMax - xMin) + xMin;
            arr[i, 1] = rand.NextDouble() * (yMax - yMin) + yMin;
        }
        return arr;
    }


    static double StoreDistanceCost(double x1, double y1, double x2, double y2)
    {
        double dx = x1 - x2;
        double dy = y1 - y2;
        return Math.Exp(-0.1 * (dx * dx + dy * dy));
    }

    static double BoundaryDistanceCost(double x, double y,
                                       double xMin = -10, double xMax = 10,
                                       double yMin = -10, double yMax = 10)
    {
        if (x >= xMin && x <= xMax && y >= yMin && y <= yMax)
            return 0;

        double xDist = Math.Min(Math.Abs(x - xMin), Math.Abs(x - xMax));
        double yDist = Math.Min(Math.Abs(y - yMin), Math.Abs(y - yMax));

        return 0.5 * (xDist * xDist + yDist * yDist);
    }

   
    /// <summary>
    /// total cost 
    /// </summary>
    /// <param name="newStores"></param>
    /// <param name="existingStores"></param>
    /// <param name="threads"></param>
    /// <returns></returns>
   
    static (double totalCost, double[] storeCosts) CalcTotalCost(double[,] newStores, double[,] existingStores, int threads)
    {
        int m = newStores.GetLength(0);
        int n = existingStores.GetLength(0);

        double totalCost = 0;
        double[] storeCosts = new double[m];
        object lockObj = new object();

        Parallel.For(0, m, new ParallelOptions { MaxDegreeOfParallelism = threads }, i =>
        {
            double nx = newStores[i, 0];
            double ny = newStores[i, 1];
            double cost = 0;

            for (int j = 0; j < n; j++)
            {
                double ex = existingStores[j, 0];
                double ey = existingStores[j, 1];
                cost += StoreDistanceCost(nx, ny, ex, ey);
            }

            cost += BoundaryDistanceCost(nx, ny);
            storeCosts[i] = cost;

            lock (lockObj)
            {
                totalCost += cost;
            }
        });

        return (totalCost, storeCosts);
    }


    static double[,] Gradient(double[,] newStores, double[,] existingStores, int threads)
    {
        double h = 1e-5;
        int m = newStores.GetLength(0);

        double[,] grad = new double[m, 2];

        Parallel.For(0, m, new ParallelOptions { MaxDegreeOfParallelism = threads }, i =>
        {
            for (int j = 0; j < 2; j++)
            {
                double orig = newStores[i, j];

                newStores[i, j] = orig + h;
                double fPlus = CalcTotalCost(newStores, existingStores, threads).Item1;

                newStores[i, j] = orig - h;
                double fMinus = CalcTotalCost(newStores, existingStores, threads).Item1;

                grad[i, j] = (fPlus - fMinus) / (2 * h);

                newStores[i, j] = orig;
            }
        });

        return grad;
    }


    static double Norm(double[,] matrix, int threads)
    {
        return Math.Sqrt(
            Enumerable.Range(0, matrix.GetLength(0))
                .SelectMany(i => Enumerable.Range(0, matrix.GetLength(1))
                .Select(j => matrix[i, j] * matrix[i, j]))
                .AsParallel()
                .WithDegreeOfParallelism(threads)
                .Sum()
        );
    }


    static void PrintMatrix(double[,] arr)
    {
        int m = arr.GetLength(0);
        for (int i = 0; i < m; i++)
        {
            Console.WriteLine($"[{arr[i, 0]:F4}, {arr[i, 1]:F4}]");
        }
    }
}