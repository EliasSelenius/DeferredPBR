using System;

using System.Collections.Generic;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new List<int>();

            list.Add(44);

            var ro_list = list.AsReadOnly();
            

            list.Add(12);

            foreach(var item in ro_list) {
                System.Console.WriteLine(item);
            }

        }
    }
}
