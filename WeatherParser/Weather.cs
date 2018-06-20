using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;


namespace WeatherParser
{
    class Weather
    {

        public string LastCityName { get; set; }
        public bool EndsOK { get; set; }




        public async Task<string> GetAsync(string city)
        {

            return await Task.Run(() =>
            {
                string UserCityQuery = city;

                // Этот метод без аргументов отрезает пробелы справа и слева.
                UserCityQuery = UserCityQuery.Trim();

                //обращаемся к функции которая преобразует то что написал юзер в стандартное название города для подстановки в URL
                string uri_city_name = GetCityNameForURL(UserCityQuery);


                //Функция GetWeather в качестве входных данных принимает это стандартное имя города,
                //а на выходе выдает МАССИВ в первом элементе которого содержится URL картинки с солнышком а во втором готовую уже строку с погодой
                //Соответвенно переменной picture присваивается первый элемент массива, полученого в результате выполнения функции GetWeather, а переменной pogoda - второй элемент массива

                string Weather;
                Weather = GetWeather(uri_city_name);

                return Weather;
            });
        }

        //Функция для преобразования названия города, которое ввел пользователь в стандартное название для подстановки в URL
        string GetCityNameForURL(string UserInputCity)
        {
            //Приводим все буквы к нижнему регистру
            UserInputCity = UserInputCity.ToLower();

            string OutputCity = "";  //Объявляем переменную для результата


            // перебираем условия транслитерации
            switch (UserInputCity)
            {
                case "москва":
                    OutputCity = "moskva";
                    break;
                case "мск":
                    OutputCity = "moskva";
                    break;
                case "санкт-петербург":
                    OutputCity = "sankt-peterburg";
                    break;
                case "питер":
                    OutputCity = "sankt-peterburg";
                    break;
                case "спб":
                    OutputCity = "sankt-peterburg";
                    break;

                //По умолчанию используем автоматичесскую транслитерацию
                default:
                    OutputCity = Autotransliteration.Convert(UserInputCity);
                    break;

            }

            LastCityName = OutputCity;

            return OutputCity;
        }
        //Функция парсинга погоды
        string GetWeather(string cityname)
        {
            try
            {
                //При помощи функции GetHtmlPageText получаем всё содержимое web страницы в переменную data
                string data = GetHtmlPageText("https://legacy.meteoservice.ru/weather/now/" + cityname);




                //Создаем html объект и загружаем в него data
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(data);

                //Тут вычленяем заголовок города по тегу meta и атрибуту name равному title
                var CityTitle = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='title']");
                string CitytitleText = CityTitle.OuterHtml.Substring(28); // Обрезаем первые 28 символов заголовков html (OuterHtml - загружает всё вместе с заголовками)
                CitytitleText = CitytitleText.Substring(0, CitytitleText.Length - 2); //Из того что получилось удаяем последние 2 символа

                //Вычленяем кусок html а связанного с погодой
                var AllWhether = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='large-6 small-12 columns current-weather']");

                //Из куска связанного с погодой вычленяем кусок про температуру воздуха
                var Temperature = AllWhether.SelectSingleNode("//span[@class='temperature']");
                string TemperatureText = Temperature.InnerText; //Получаем стринг без html заголовков (.InnerText)
                TemperatureText = TemperatureText.Substring(0, TemperatureText.Length - 6); //Удаляем последние 6 символов







                //Начинаем формировать выходную итоговую переменную output - записываем туда Город и температуру
                string output = CitytitleText + " \n" + "Температура воздуха: " + TemperatureText + " C" + " \n";
                //Получаем из блока связанного с погодой массив из элементов внутри td шников таблицы
                var AnotherRates = AllWhether.SelectNodes("//td");
                // Добавляем в output элементы массива (оно там в формате: Влажность : 5%     те  название - значение     )
                output = output + AnotherRates[0].InnerText + " " + AnotherRates[1].InnerText + "\n" + AnotherRates[2].InnerText + " " + AnotherRates[3].InnerText + "\n" + AnotherRates[4].InnerText + " " + AnotherRates[5].InnerText + "\n" + AnotherRates[6].InnerText + " " + AnotherRates[7].InnerText + "\n" + AnotherRates[8].InnerText + " " + AnotherRates[9].InnerText.Replace("&deg;C", " C") + "\n" + AnotherRates[10].InnerText + " " + AnotherRates[11].InnerText.Replace("&deg;C", " C") + "\n";

                EndsOK = true;
                return output;
            }
            catch

            {
                // Если чтото не получилось возрващаем ошибки
                string output = "Ошибка получения данных для " + cityname + " . Вероятно неверно указан город.";
                EndsOK = false;
                return output;
            }
        }
        //Функция получения веб страницы
        string GetHtmlPageText(string url)
        {

            try
            {

                string text = "";
                WebRequest request = WebRequest.Create(url);
                request.Timeout = 10000;
                WebResponse response = request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        text = sr.ReadToEnd();
                    }
                }
                return text;
            }
            catch
            {
                return "";
            }
        }

    }
}
