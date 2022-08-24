using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Regex regextime = new Regex(@"([1-9]d*.?d*)| (0.d* [1-9])");
            string a = "runTime=60//默认为60分钟 单位为分钟";
            Console.WriteLine(regextime.Match(a).Value);

        }
    }
}