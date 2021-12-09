using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using NETWORKLIST;

namespace InternetAvailability
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopWatch = Stopwatch.StartNew();
            bool isNetworkAvailableCom = InternetAvailableNlm();
            stopWatch.Stop();
            Console.WriteLine($"Internet available COM Network List Manager: {isNetworkAvailableCom}; time elapsed to check: {stopWatch.ElapsedMilliseconds}ms");

            var stopWatch2 = Stopwatch.StartNew();
            bool isNetworkAvailablePinvoke = IsInternetAvailablePinvoke();
            stopWatch2.Stop();
            Console.WriteLine($"Internet available PInvoke: {isNetworkAvailablePinvoke}; time elapsed to check: {stopWatch2.ElapsedMilliseconds}ms");

            var stopWatch3 = Stopwatch.StartNew();
            bool isNetworkAvailableNetCall = NetworkInterface.GetIsNetworkAvailable();
            stopWatch3.Stop();
            Console.WriteLine($"Internet available .net call: {isNetworkAvailableNetCall}; time elapsed to check: {stopWatch3.ElapsedMilliseconds}ms");

            var stopWatch4 = Stopwatch.StartNew();
            bool isNetworkAvailableIfaceCheck = IfaceCheck();
            stopWatch4.Stop();
            //found in https://stackoverflow.com/a/43195952
            Console.WriteLine($"Internet available adapter iteration: {isNetworkAvailableIfaceCheck}; time elapsed to check: {stopWatch4.ElapsedMilliseconds}ms");

            Console.ReadLine();
        }


        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int description, int res);
        private static bool IsInternetAvailablePinvoke()
        {
            try
            {
                return InternetGetConnectedState(out _, 0);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static bool IfaceCheck()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            return (from face in interfaces
                    where face.OperationalStatus == OperationalStatus.Up
                    where (face.NetworkInterfaceType != NetworkInterfaceType.Tunnel) && (face.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    where (!(face.Name.ToLower().Contains("virtual") || face.Description.ToLower().Contains("virtual")))
                    select face.GetIPv4Statistics()).Any(statistics => (statistics.BytesReceived > 0) && (statistics.BytesSent > 0));
        }

        private static bool InternetAvailableNlm()
        {
            var networkListManager = new NetworkListManager();
            return networkListManager.IsConnectedToInternet;
        }
    }
}
