// Copyright (c) 2024, zxeltor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace TestConsole;

internal class Program
{
    private static void Main(string[] args)
    {
        var dateStart = DateTime.Now.AddDays(-1);
        var dateEnd = DateTime.Now.AddDays(1);

        var dateDiff = dateEnd - dateStart; //> TimeSpan.FromSeconds(90)

        if (dateEnd - dateStart > TimeSpan.FromSeconds(90))
            Console.WriteLine("> 90");
        else
            Console.WriteLine("<= 90");
    }
}