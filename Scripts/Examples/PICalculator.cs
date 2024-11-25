// Source: https://stackoverflow.com/a/28000950/2503153

/*
 * Computation of the n'th decimal digit of \pi with very little memory.
 * Written by Fabrice Bellard on January 8, 1997.
 * 
 * We use a slightly modified version of the method described by Simon
 * Plouffe in "On the Computation of the n'th decimal digit of various
 * transcendental numbers" (November 1996). We have modified the algorithm
 * to get a running time of O(n^2) instead of O(n^3log(n)^3).
 * 
 * This program uses mostly integer arithmetic. It may be slow on some
 * hardwares where integer multiplications and divisons must be done
 * by software. We have supposed that 'int' has a size of 32 bits. If
 * your compiler supports 'long long' integers of 64 bits, you may use
 * the integer version of 'mul_mod' (see HAS_LONG_LONG).  
 */

using System;

public class PICalculator
{
    static int InvMod(int x, int y)
    {
        int u = x, v = y, c = 1, a = 0;
        while (u != 0)
        {
            int q = v / u;
            int t = c;
            c = a - q * c;
            a = t;
            t = u;
            u = v - q * u;
            v = t;
        }
        a = a % y;
        if (a < 0) a = y + a;
        return a;
    }

    static int PowMod(int a, int b, int m)
    {
        int r = 1, aa = a;
        while (b > 0)
        {
            if ((b & 1) == 1) r = (r * aa) % m;
            b >>= 1;
            aa = (aa * aa) % m;
        }
        return r;
    }

    static bool IsPrime(int n)
    {
        if (n % 2 == 0) return false;
        int r = (int)Math.Sqrt(n);
        for (int i = 3; i <= r; i += 2)
        {
            if (n % i == 0) return false;
        }
        return true;
    }

    static int NextPrime(int n)
    {
        do
        {
            n++;
        } while (!IsPrime(n));
        return n;
    }

    public static int GetPiDigit(int n)
    {
        int t;
        int N = (int)((n + 20) * Math.Log(10) / Math.Log(2));
        double sum = 0.0;
        for (int a = 3; a <= 2 * N; a = NextPrime(a))
        {
            int vmax = (int)(Math.Log(2 * N) / Math.Log(a));
            int av = 1;
            for (int i = 0; i < vmax; i++) av *= a;
            int s = 0, num = 1, den = 1, v = 0, kq = 1, kq2 = 1;
            for (int k = 1; k <= N; k++)
            {
                t = k;
                if (kq >= a)
                {
                    do
                    {
                        t /= a;
                        v--;
                    } while (t % a == 0);
                    kq = 0;
                }
                kq++;
                num = (num * t) % av;
                t = 2 * k - 1;
                if (kq2 >= a)
                {
                    if (kq2 == a)
                    {
                        do
                        {
                            t /= a;
                            v++;
                        } while (t % a == 0);
                    }
                    kq2 -= a;
                }
                den = (den * t) % av;
                kq2 += 2;
                if (v > 0)
                {
                    t = InvMod(den, av);
                    t = (t * num) % av;
                    t = (t * k) % av;
                    for (int i = v; i < vmax; i++) t = (t * a) % av;
                    s += t;
                    if (s >= av) s -= av;
                }
            }
            t = PowMod(10, n - 1, av);
            s = (s * t) % av;
            sum = (sum + (double)s / av) % 1.0;
        }
        return (int)(sum * 1e9);
    }

    public static void Main()
    {
        Console.Write("Enter the position of the digit of Pi you want to find: ");
        int n = int.Parse(Console.ReadLine());
        int digit = GetPiDigit(n);
        Console.WriteLine($"The digit at position {n} of Pi is: {digit}");
    }
}
