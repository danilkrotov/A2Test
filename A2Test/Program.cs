using A2Test.Class;
using A2Test.Class.Database;
using A2Test.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace A2Test
{
    internal class Program
    {
        static readonly LesegaisSite Site = new LesegaisSite();

        static void Main(string[] args)
        {
            while (true) 
            {
                Site.UpdateData(10000);
                Thread.Sleep(600000); //Между каждым обходом заложить ожидание 10 минут.
            }
            Console.ReadLine();
        }
    }
}
