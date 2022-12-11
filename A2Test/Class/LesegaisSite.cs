using A2Test.Class.Database;
using A2Test.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace A2Test.Class
{
    /// <summary>
    /// Предварительно настроенный класс для запроса информации с сайта https://www.lesegais.ru/open-area/deal
    /// </summary>
    public class LesegaisSite
    {
        public string query { get; set; }
        public string operationName { get; set; }
        public Variables variables { get; set; }

        public class Variables
        {
            public int size { get; set; }
            public int number { get; set; }
            public object filter { get; set; }
            public object orders { get; set; }
        }
        /// <summary>
        /// Формирует данные в формате json для дальнейшей отправки
        /// </summary>
        private string CreateJsonData(int size, int pageNum) 
        {
            query = "\"query SearchReportWoodDeal($size: Int!, $number: Int!, $filter: Filter, $orders: [Order!]) {\n searchReportWoodDeal(filter: $filter, pageable: {number: $number, size: $size}, orders: $orders) {\n content {\n sellerName\n sellerInn\n buyerName\n buyerInn\n woodVolumeBuyer\n woodVolumeSeller\n dealDate\n dealNumber\n __typename\n    }\n __typename\n  }\n}\n\"";
            operationName = "SearchReportWoodDeal";
            variables = new Variables();
            variables.size = size;
            variables.number = pageNum;
            variables.filter = null;
            variables.orders = null;

            string json = JsonSerializer.Serialize(this);
            json = json.Replace("\\u0022", "");
            return json;
        }

        /// <summary>
        /// Возвращает данные в формате json с сайта https://www.lesegais.ru/open-area/deal
        /// </summary>
        private string GetJsonData(int size = 20, int pageNum = 0) 
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.lesegais.ru/open-area/graphql");
            request.Method = "POST";
            request.Host = "www.lesegais.ru";
            request.Accept = "*/*";
            request.ContentType = "application/json";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36";
            var byteArray = Encoding.UTF8.GetBytes(CreateJsonData(size, pageNum));
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Обновляет данные в базе данных
        /// </summary>
        public void UpdateData(int size = 20)
        {
            int pageNum = 0;
            while (true)
            {
                //Выводим на экран успех о получении данных
                WoodDeal jsonDeals = JsonSerializer.Deserialize<WoodDeal>(GetJsonData(size, pageNum));
                Console.WriteLine("Data recieved size: " + jsonDeals.data.searchReportWoodDeal.content.Count + " page: " + pageNum);

                //Сохраняем данные
                using (var db = new DataBaseContext())
                {
                    //Проходим по полученным данным
                    for (int i = 0; i < jsonDeals.data.searchReportWoodDeal.content.Count; i++)
                    {
                        //Получаем сохранённую запись из БД, если отсутствует вернёт null
                        var woodDeal = db.WoodBuys.FirstOrDefault(x => x.DealNumber == jsonDeals.data.searchReportWoodDeal.content[i].dealNumber);

                        //Если записи о покупке с dealNumber не найдено, сохраняем как новую
                        if (woodDeal == null)
                        {
                            //Не сохраняем записи с значением 0
                            if (jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeBuyer != 0)
                            {
                                Console.WriteLine("[Buy] New data " + jsonDeals.data.searchReportWoodDeal.content[i].dealNumber);
                                db.WoodBuys.Add(new WoodBuy
                                {
                                    DealNumber = jsonDeals.data.searchReportWoodDeal.content[i].dealNumber,
                                    SellerName = jsonDeals.data.searchReportWoodDeal.content[i].sellerName,
                                    SellerInn = jsonDeals.data.searchReportWoodDeal.content[i].sellerInn,
                                    BuyerName = jsonDeals.data.searchReportWoodDeal.content[i].buyerName,
                                    BuyerInn = jsonDeals.data.searchReportWoodDeal.content[i].buyerInn,
                                    DealDate = jsonDeals.data.searchReportWoodDeal.content[i].dealDate,
                                    WoodVolumeBuyer = jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeBuyer
                                });
                            }
                        }
                        else 
                        {
                            //Иначе(запись найдена) - Проверяем необходимо ли обновить запись

                            //Не обновляем записи с значением 0
                            if (jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeBuyer != 0)
                            {
                                //Собираем строку из всех данных в БД
                                string dataBaseDeal = woodDeal.DealNumber
                                    + woodDeal.SellerName
                                    + woodDeal.SellerInn
                                    + woodDeal.BuyerName
                                    + woodDeal.BuyerInn
                                    + woodDeal.DealDate
                                    + woodDeal.WoodVolumeBuyer;
                                //Собираем строку из всех данных из json
                                string jsonDeal = jsonDeals.data.searchReportWoodDeal.content[i].dealNumber
                                    + jsonDeals.data.searchReportWoodDeal.content[i].sellerName
                                    + jsonDeals.data.searchReportWoodDeal.content[i].sellerInn
                                    + jsonDeals.data.searchReportWoodDeal.content[i].buyerName
                                    + jsonDeals.data.searchReportWoodDeal.content[i].buyerInn
                                    + jsonDeals.data.searchReportWoodDeal.content[i].dealDate
                                    + jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeBuyer;

                                //Если строки не совпадают, значит обновляем всю строку
                                if (dataBaseDeal != jsonDeal)
                                {
                                    woodDeal.DealNumber = jsonDeals.data.searchReportWoodDeal.content[i].dealNumber;
                                    woodDeal.SellerName = jsonDeals.data.searchReportWoodDeal.content[i].sellerName;
                                    woodDeal.SellerInn = jsonDeals.data.searchReportWoodDeal.content[i].sellerInn;
                                    woodDeal.BuyerName = jsonDeals.data.searchReportWoodDeal.content[i].buyerName;
                                    woodDeal.BuyerInn = jsonDeals.data.searchReportWoodDeal.content[i].buyerInn;
                                    woodDeal.DealDate = jsonDeals.data.searchReportWoodDeal.content[i].dealDate;
                                    woodDeal.WoodVolumeBuyer = jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeBuyer;
                                    Console.WriteLine("[Buy] Update data: " + jsonDeals.data.searchReportWoodDeal.content[i].dealNumber);
                                }
                            }                                
                        }

                        //Получаем сохранённую запись из БД, если отсутствует вернёт null
                        var woodDeal2 = db.WoodSells.FirstOrDefault(x => x.DealNumber == jsonDeals.data.searchReportWoodDeal.content[i].dealNumber);
                        //Если записи о продаже с dealNumber не найдено, сохраняем как новую
                        if (woodDeal2 == null)
                        {
                            //Не сохраняем записи с значением 0
                            if (jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeSeller != 0)
                            {
                                Console.WriteLine("[Sell] New data " + jsonDeals.data.searchReportWoodDeal.content[i].dealNumber);
                                db.WoodSells.Add(new WoodSell
                                {
                                    DealNumber = jsonDeals.data.searchReportWoodDeal.content[i].dealNumber,
                                    SellerName = jsonDeals.data.searchReportWoodDeal.content[i].sellerName,
                                    SellerInn = jsonDeals.data.searchReportWoodDeal.content[i].sellerInn,
                                    BuyerName = jsonDeals.data.searchReportWoodDeal.content[i].buyerName,
                                    BuyerInn = jsonDeals.data.searchReportWoodDeal.content[i].buyerInn,
                                    DealDate = jsonDeals.data.searchReportWoodDeal.content[i].dealDate,
                                    WoodVolumeSeller = jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeSeller
                                });
                            }
                        }
                        else
                        {
                            //Не обновляем записи с значением 0
                            if (jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeSeller != 0)
                            {
                                //Иначе(запись найдена) - Проверяем необходимо ли обновить запись
                                //Собираем строку из всех данных в БД
                                string dataBaseDeal = woodDeal2.DealNumber
                                    + woodDeal2.SellerName
                                    + woodDeal2.SellerInn
                                    + woodDeal2.BuyerName
                                    + woodDeal2.BuyerInn
                                    + woodDeal2.DealDate
                                    + woodDeal2.WoodVolumeSeller;
                                //Собираем строку из всех данных из json
                                string jsonDeal = jsonDeals.data.searchReportWoodDeal.content[i].dealNumber
                                    + jsonDeals.data.searchReportWoodDeal.content[i].sellerName
                                    + jsonDeals.data.searchReportWoodDeal.content[i].sellerInn
                                    + jsonDeals.data.searchReportWoodDeal.content[i].buyerName
                                    + jsonDeals.data.searchReportWoodDeal.content[i].buyerInn
                                    + jsonDeals.data.searchReportWoodDeal.content[i].dealDate
                                    + jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeSeller;

                                //Если строки не совпадают, значит обновляем всю строку
                                if (dataBaseDeal != jsonDeal)
                                {
                                    woodDeal2.DealNumber = jsonDeals.data.searchReportWoodDeal.content[i].dealNumber;
                                    woodDeal2.SellerName = jsonDeals.data.searchReportWoodDeal.content[i].sellerName;
                                    woodDeal2.SellerInn = jsonDeals.data.searchReportWoodDeal.content[i].sellerInn;
                                    woodDeal2.BuyerName = jsonDeals.data.searchReportWoodDeal.content[i].buyerName;
                                    woodDeal2.BuyerInn = jsonDeals.data.searchReportWoodDeal.content[i].buyerInn;
                                    woodDeal2.DealDate = jsonDeals.data.searchReportWoodDeal.content[i].dealDate;
                                    woodDeal2.WoodVolumeSeller = jsonDeals.data.searchReportWoodDeal.content[i].woodVolumeSeller;
                                    Console.WriteLine("[Sell] Update data: " + jsonDeals.data.searchReportWoodDeal.content[i].dealNumber);
                                }
                            }                                
                        }
                    }
                    db.SaveChanges();
                    Console.WriteLine("Database saved");
                }

                pageNum++; //номер страницы
                //выходим из цикла если это последняя страница
                if (jsonDeals.data.searchReportWoodDeal.content.Count != size) 
                {
                    break;
                }
            }
        }
    }
}
