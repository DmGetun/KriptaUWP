using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP.Помошники;

namespace App3.Помошники
{
    class Stribog
    {
        private int BLOCK_LENGTH = 64;
        private byte[] SBox;
        private byte[] Ttable;
        private ulong[] Atable;
        private byte[][] Ctable;
        private byte[] iv,N,sigma;
        private byte[] N_0 ={
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
            };
        public byte[] GetHashMessage(string text)
        {
            SBox = Tables.StribogSbox;
            Ttable = Tables.StribogT;
            Atable = Tables.StribogA;
            Ctable = Tables.StribogC;
            byte[] byteText = Encoding.Unicode.GetBytes(text);
            iv = new byte[BLOCK_LENGTH];
            N = new byte[BLOCK_LENGTH];
            sigma = new byte[BLOCK_LENGTH];
            byte[] h = new byte[BLOCK_LENGTH];
            int len = byteText.Length * 8;
            int inc = 0;
            byte[] N_512 = BitConverter.GetBytes(512);
            byte[] paddedMes = new byte[BLOCK_LENGTH];
            while (len >= 512)
            {
                inc++;
                byte[] temp = new byte[BLOCK_LENGTH];
                Array.Copy(byteText, byteText.Length - inc * 64, temp, 0, 64); // последние 512 бит
                h = G(N, h, temp);
                N = Add(N, N_512.Reverse().ToArray());
                sigma = Add(sigma, temp);
                len -= 512;
            }

            byte[] message = new byte[byteText.Length - inc * 64];
            Array.Copy(byteText, 0, message, 0, byteText.Length - inc * 64);
            if(message.Length < 64)
            {
                for (int i = 0; i < (64 - message.Length - 1); i++)
                    paddedMes[i] = 0;
                paddedMes[64 - message.Length - 1] = 0x_01;
                Array.Copy(message, 0, paddedMes, 64 - message.Length, message.Length);
            }
            h = G(N, h, paddedMes);
            byte[] MesLen = BitConverter.GetBytes(message.Length * 8);
            N = Add(N, MesLen.Reverse().ToArray());
            sigma = Add(sigma, paddedMes);
            h = G(N_0, h, N);
            h = G(N_0, h, sigma);
            return h;
        }

        private byte[] Add(byte[] a, byte[] b)
        {
            byte[] temp = new byte[64];
            int i = 0, t = 0;
            byte[] tempA = new byte[64];
            byte[] tempB = new byte[64];
            Array.Copy(a, 0, tempA, 64 - a.Length, a.Length);
            Array.Copy(b, 0, tempB, 64 - b.Length, b.Length);
            for (i = 63; i >= 0; i--)
            {
                t = tempA[i] + tempB[i] + (t >> 8);
                temp[i] = (byte)(t & 0xFF);
            }
            return temp;
        }

        private byte[] X(byte[] a, byte[] b)
        {
            byte[] x = new byte[BLOCK_LENGTH];
            for (int i = 0; i < BLOCK_LENGTH; i++)
            {
                x[i] = (byte)(a[i] ^ b[i]);
            }
            return x;
        }

        private byte[] S(byte[] s)
        {
            byte[] r = new byte[BLOCK_LENGTH];
            for(int i = 0; i < 64; i++)
            {
                r[i] = SBox[s[i]];
            }
            return r;
        }

        private byte[] P(byte[] s)
        {
            byte[] r = new byte[BLOCK_LENGTH];
            for(int i = 0; i < BLOCK_LENGTH; i++)
            {
                r[i] = s[Ttable[i]];
            }

            return r;
        }

        private byte[] L(byte[] s)
        {
            byte[] result = new byte[64];
            for (int i = 0; i < 8; i++)
            {
                ulong t = 0;
                byte[] tempArray = new byte[8];
                Array.Copy(s, i * 8, tempArray, 0, 8);
                tempArray = tempArray.Reverse().ToArray();
                BitArray tempBits1 = new BitArray(tempArray);
                bool[] tempBits = new bool[64];
                tempBits1.CopyTo(tempBits, 0);
                tempBits = tempBits.Reverse().ToArray();
                for (int j = 0; j < 64; j++)
                {
                    if (tempBits[j] != false)
                        t = t ^ Atable[j];
                }
                byte[] ResPart = BitConverter.GetBytes(t).Reverse().ToArray();
                Array.Copy(ResPart, 0, result, i * 8, 8);
            }
            return result;
        }

        private byte[] G(byte[] N, byte[] h, byte[] m)
        {
            byte[] K = X(h, N);
            K = S(K);
            K = P(K);
            K = L(K);
            byte[] t = E(K, m);
            t = X(h, t);
            byte[] G = X(t, m);
            return G;
        }

        private byte[] E(byte[] k, byte[] m)
        {
            byte[] state = X(k, m);
            for(int i = 0;i < 12; i++)
            {
                state = S(state);
                state = P(state);
                state = L(state);
                k = KeyS(k, i);
                state = X(state, k);
            }
            return state;
        }

        private byte[] KeyS(byte[] k, int i)
        {
            k = X(k, Ctable[i]);
            k = S(k);
            k = P(k);
            k = L(k);
            return k;
        }
    }
}
