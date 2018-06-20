using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Timers;

namespace WeatherParser
{
    class Program
    {
        static Timer timer1 = new Timer();
        static List<string> citys = new List<string>();

        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("Используйте команду вида: WeatherWebAPI.exe <город>");
                Console.WriteLine("Например: WheatherParser.exe Питер Москва Самара");
                return;
            }

            foreach (string arg in args)
            {
                citys.Add(arg);
            }

            UpdateWeather(); //Обновляем погоду первый раз

            //Далее подключаем таймер
            timer1.Elapsed += Timer1_Tick; //Указываем обработчик события для Elapsed
            timer1.Interval = 60000; // 1 минута
            timer1.Enabled = true;

            Console.WriteLine("Данные будут обновляться с интервалом 1 минута");
            Console.WriteLine("Для выхода нажмите ENTER");
            Console.ReadLine();
        }


        // Этот таймер следит за чатом Piter-Furry и если 3 часа никто ничего не пишет, то постит картинку.
        private static void Timer1_Tick(object sender, EventArgs e)
        {
            UpdateWeather();
            Console.WriteLine("Данные будут обновляться с интервалом 1 минута");
            Console.WriteLine("Для выхода нажмите ENTER");
        }



        static async void UpdateWeather()
        {
            string weatherdata;


            foreach (string city in citys)
            {
                Weather weather = new Weather();
                weatherdata = await weather.GetAsync(city);

                //Записываем погоду в файл
                SaveDATAtoFile(weather.LastCityName, weatherdata);

                //Записываем в лог.
                if (weather.EndsOK)
                {
                    string message = DateTime.Now + " OK - Погода для " + weather.LastCityName + " обновлена";
                    Console.WriteLine(message);
                    SaveLog(message);
                }
                else
                {
                    string message = DateTime.Now + " Error - Погоду для " + weather.LastCityName + " не удалось обновить";
                    Console.WriteLine(message);
                    SaveLog(message);
                }
            }
        }


        static void SaveDATAtoFile(string filename, string data)
        {
            //Записываем погоду в Файл в файл
            try
            {
                if (!Directory.Exists("Data"))
                {
                    Directory.CreateDirectory("Data");
                }


                using (StreamWriter sw = File.CreateText("Data\\" + filename + ".txt"))
                {
                    sw.Write(data);
                }
            }
            catch
            { }
        }

        static void SaveLog(string data)
        {
            //Записываем погоду в Файл в файл
            try
            {
                using (StreamWriter sw = File.AppendText("Log.txt"))
                {
                    sw.WriteLine(data);
                }
            }
            catch
            { }
        }



    }
}
