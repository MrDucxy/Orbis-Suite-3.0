using System.Runtime.InteropServices;
using System.Text;

namespace OrbisSuite
{
    class Utilities
    {
        /// <summary>
        /// Will convert byte array to a string and ensure a null terminator is set.
        /// </summary>
        /// <param name="bIn">Bytes to convert.</param>
        /// <returns>Returns a string form of the input bytes.</returns>
        public static string CleanByteToString(byte[] bIn)
        {
            try
            {
                if (bIn.Length <= 0)
                    return string.Empty;

                string Out = Encoding.Default.GetString(bIn);
                return Out.Substring(0, Out.IndexOf('\0'));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Will censor a part of an input string with the desired sensor char.
        /// </summary>
        /// <param name="In">The string we would like to censor.</param>
        /// <param name="CensorChar">The character we would like to use as the sensor.</param>
        /// <param name="AllowedCount">The allowed number of characters starting from 0.</param>
        /// <returns>Returns the censored string.</returns>
        public static string CensorString(string In, char CensorChar, int AllowedCount)
        {
            try
            {
                string Out = In.Substring(0, AllowedCount);
                Out += new string(CensorChar, In.Length - AllowedCount);
                return Out;
            }
            catch
            {
                return string.Empty;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);
    }
}
